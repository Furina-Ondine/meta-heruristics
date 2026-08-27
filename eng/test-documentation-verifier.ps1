[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$verifier = Join-Path $PSScriptRoot 'verify-documentation.ps1'
$temporaryBase = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$testRoot = Join-Path $temporaryBase ("metaheuristics-doc-verifier-" + [guid]::NewGuid().ToString('N'))
$validRoot = Join-Path $testRoot 'valid'

function Write-Utf8File {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Content
    )

    $directory = Split-Path -Parent $Path
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
    Set-Content -LiteralPath $Path -Value $Content -Encoding utf8NoBOM
}

function Invoke-Verifier {
    param([Parameter(Mandatory)][string]$RepositoryRoot)

    $output = & pwsh -NoProfile -File $verifier -RootPath $RepositoryRoot 2>&1 | Out-String
    return @{
        ExitCode = $LASTEXITCODE
        Output = $output
    }
}

function Assert-InvalidFixture {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][scriptblock]$Mutate,
        [Parameter(Mandatory)][string]$ExpectedMessage
    )

    $fixture = Join-Path $testRoot $Name
    Copy-Item -LiteralPath $validRoot -Destination $fixture -Recurse
    & $Mutate $fixture
    $result = Invoke-Verifier -RepositoryRoot $fixture
    if ($result.ExitCode -eq 0) {
        throw "Fixture '$Name' unexpectedly passed."
    }
    if ($result.Output -notlike "*$ExpectedMessage*") {
        throw "Fixture '$Name' did not report '$ExpectedMessage'. Output: $($result.Output)"
    }
}

try {
    Write-Utf8File -Path (Join-Path $validRoot 'README.md') -Content '[Specs](docs/specs/README.md)'
    Write-Utf8File -Path (Join-Path $validRoot 'docs/specs/README.md') -Content '# Specs'
    Write-Utf8File -Path (Join-Path $validRoot 'docs/decisions/README.md') -Content @'
| 编号 | 决策 | 状态 |
| --- | --- | --- |
| [0001](0001-test.md) | Test | `Accepted` |
'@
    Write-Utf8File -Path (Join-Path $validRoot 'docs/decisions/0001-test.md') -Content @'
# ADR-0001

## 状态

Accepted
'@

    $package = Join-Path $validRoot 'docs/specs/SPEC-0001-test'
    Write-Utf8File -Path (Join-Path $package 'spec.md') -Content @'
# SPEC-0001

- 编号：`SPEC-0001`
- 状态：`Draft`

### FR-001: 可验证行为
'@
    Write-Utf8File -Path (Join-Path $package 'plan.md') -Content @'
# Plan

- 状态：`Draft`
- 覆盖需求：`FR-001`
'@
    Write-Utf8File -Path (Join-Path $package 'tasks.md') -Content @'
# Tasks

## T001：实现

- 状态：`Pending`
- 覆盖需求：`FR-001`
- 依赖：无
'@
    Write-Utf8File -Path (Join-Path $package 'verification.md') -Content @'
# Verification

| 需求 | 证据 |
| --- | --- |
| FR-001 | Pending |
'@

    $validResult = Invoke-Verifier -RepositoryRoot $validRoot
    if ($validResult.ExitCode -ne 0) {
        throw "Valid fixture failed: $($validResult.Output)"
    }

    Assert-InvalidFixture -Name 'broken-link' -ExpectedMessage 'references missing local target' -Mutate {
        param($fixture)
        Add-Content -LiteralPath (Join-Path $fixture 'README.md') -Value '[Missing](missing.md)'
    }
    Assert-InvalidFixture -Name 'duplicate-number' -ExpectedMessage 'Spec number 0001 is used by both' -Mutate {
        param($fixture)
        Copy-Item -LiteralPath (Join-Path $fixture 'docs/specs/SPEC-0001-test') -Destination (Join-Path $fixture 'docs/specs/SPEC-0001-copy') -Recurse
    }
    Assert-InvalidFixture -Name 'undefined-requirement' -ExpectedMessage 'references undefined requirement FR-999' -Mutate {
        param($fixture)
        $path = Join-Path $fixture 'docs/specs/SPEC-0001-test/plan.md'
        (Get-Content -LiteralPath $path -Raw).Replace('FR-001', 'FR-999') | Set-Content -LiteralPath $path -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'missing-evidence' -ExpectedMessage 'has no evidence row for declared requirement FR-001' -Mutate {
        param($fixture)
        $path = Join-Path $fixture 'docs/specs/SPEC-0001-test/verification.md'
        (Get-Content -LiteralPath $path -Raw).Replace('FR-001', 'none') | Set-Content -LiteralPath $path -Encoding utf8NoBOM
    }

    Write-Host 'Documentation verifier self-tests passed.' -ForegroundColor Green
}
finally {
    $resolvedTestRoot = [IO.Path]::GetFullPath($testRoot)
    if ($resolvedTestRoot.StartsWith($temporaryBase, [StringComparison]::OrdinalIgnoreCase) -and
        (Split-Path -Leaf $resolvedTestRoot).StartsWith('metaheuristics-doc-verifier-', [StringComparison]::Ordinal)) {
        Remove-Item -LiteralPath $resolvedTestRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
