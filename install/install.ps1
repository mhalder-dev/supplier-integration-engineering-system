<#
.SYNOPSIS
  Makes the supplier-integration skills available to Claude Code in EVERY project.

.DESCRIPTION
  Claude Code discovers skills from ~/.claude/skills/. This repository's skills live
  here, so without this step a developer working in a supplier repository cannot see
  them. This script links them into ~/.claude/skills/ using directory junctions, so
  they stay in sync with the repository - pull the repo, get the updated skills.

  Junctions do not require administrator rights on Windows.

.EXAMPLE
  .\install\install.ps1
  .\install\install.ps1 -Uninstall
#>
param(
    [switch]$Uninstall,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

function Remove-LinkSafely {
    <# Deletes a junction/symlink without prompting and without touching its target. #>
    param([string]$Path)
    [System.IO.Directory]::Delete($Path, $false)
}


$RepoRoot  = Split-Path -Parent $PSScriptRoot
$SkillsSrc = Join-Path $RepoRoot '.claude\skills'
$SkillsDst = Join-Path $HOME '.claude\skills'

$Skills = @(
    'supplier-development',
    'supplier-bootstrap',
    'supplier-feature',
    'aggregator-wiring'
)

if (-not (Test-Path $SkillsSrc)) {
    throw "Skills not found at $SkillsSrc. Run this from inside the engineering-system repository."
}

if (-not (Test-Path $SkillsDst)) {
    New-Item -ItemType Directory -Path $SkillsDst -Force | Out-Null
    Write-Host "Created $SkillsDst"
}

foreach ($skill in $Skills) {
    $src = Join-Path $SkillsSrc $skill
    $dst = Join-Path $SkillsDst $skill

    if ($Uninstall) {
        if (Test-Path $dst) {
            $item = Get-Item $dst -Force
            if ($item.LinkType) {
                # Only remove links we created - never delete a real directory.
                Remove-LinkSafely $dst
                Write-Host "  removed link  $skill"
            } else {
                Write-Warning "  SKIPPED $skill - it is a real directory, not a link. Remove it by hand if you meant to."
            }
        }
        continue
    }

    if (-not (Test-Path $src)) {
        Write-Warning "  missing in repo: $skill"
        continue
    }

    if (Test-Path $dst) {
        $item = Get-Item $dst -Force
        if ($item.LinkType) {
            Remove-LinkSafely $dst
        } elseif ($Force) {
            Write-Warning "  replacing real directory $skill (-Force given)"
            Remove-Item $dst -Recurse -Force -Confirm:$false
        } else {
            Write-Warning "  SKIPPED $skill - a real directory already exists there. Re-run with -Force to replace it."
            continue
        }
    }

    New-Item -ItemType Junction -Path $dst -Target $src | Out-Null
    Write-Host "  linked  $skill"
}

# Remove links into this repo for skills it no longer ships - otherwise a renamed
# skill leaves a dangling command behind.
Get-ChildItem $SkillsDst -Force -ErrorAction SilentlyContinue | Where-Object {
    $_.LinkType -and $_.Target -and (($_.Target -join '') -like "$SkillsSrc*") -and ($Skills -notcontains $_.Name)
} | ForEach-Object {
    Remove-LinkSafely $_.FullName
    Write-Host "  removed stale $($_.Name)"
}

if ($Uninstall) {
    Write-Host "`nUninstalled. Claude will no longer offer the supplier skills."
    return
}

Write-Host @"

Done. These skills are now available in every project:

  /supplier-development   route to the right stage
  /supplier-bootstrap     create a new supplier service
  /supplier-feature       implement one feature end to end
  /aggregator-wiring      wire a feature into the aggregator

They are junctions into this repository, so 'git pull' here updates them everywhere.

Next: read $RepoRoot\docs\developer-onboarding.md
"@
