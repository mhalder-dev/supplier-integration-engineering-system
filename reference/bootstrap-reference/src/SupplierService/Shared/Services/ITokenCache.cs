using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using SupplierService.Shared.DTOs;
using SupplierService.Shared.Options;

namespace SupplierService.Shared.Services;

public interface ITokenCache
{
    ValueTask<string> GetTokenAsync(
        SupplierCredential credential,
        Func<SupplierCredential, CancellationToken, Task<(string Token, int ExpiresInSeconds)>> fetch,
        CancellationToken ct = default);
}

/// <summary>
/// Per-credential token cache with a gate per key and double-checked locking.
/// See docs/supplier-patterns.md section 3 for why this shape is required.
///
/// Known limitation: process-local. Tokens are lost on restart and are not shared between
/// instances. Fine at one instance per supplier; needs a distributed cache if scaled out.
/// </summary>
public sealed class TokenCache(IOptions<SupplierOptions> options) : ITokenCache
{
    private sealed record CachedToken(string Token, DateTimeOffset ExpiresAtUtc)
    {
        public bool IsUsable => DateTimeOffset.UtcNow < ExpiresAtUtc;
    }

    private static readonly ConcurrentDictionary<string, CachedToken> Tokens = new();
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates = new();

    private readonly SupplierOptions _options = options.Value;

    public async ValueTask<string> GetTokenAsync(
        SupplierCredential credential,
        Func<SupplierCredential, CancellationToken, Task<(string Token, int ExpiresInSeconds)>> fetch,
        CancellationToken ct = default)
    {
        var key = credential.CacheKey();

        // 1. Fast path - no lock taken when a usable token is already cached.
        if (Tokens.TryGetValue(key, out var cached) && cached.IsUsable)
            return cached.Token;

        // 2. Gate per credential key, so one account's fetch never blocks another's.
        var gate = Gates.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(ct);
        try
        {
            // 3. Re-check inside the gate: without this every concurrent caller still fetches.
            if (Tokens.TryGetValue(key, out cached) && cached.IsUsable)
                return cached.Token;

            var (token, expiresIn) = await fetch(credential, ct);

            // 4. Expire early so a token cannot lapse mid-request.
            var lifetime = expiresIn > 0 ? expiresIn : _options.TokenLifetimeSeconds;
            var margin = Math.Min(_options.TokenSafetyMarginSeconds, Math.Max(lifetime / 5, 1));
            Tokens[key] = new CachedToken(token, DateTimeOffset.UtcNow.AddSeconds(lifetime - margin));

            return token;
        }
        finally
        {
            gate.Release();
        }
    }
}
