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
    Write-Utf8File -Path (Join-Path $validRoot 'docs/specs/README.md') -Content @'
# Specs

| 编号 | 主题 | 状态 |
| --- | --- | --- |
| [SPEC-0001](./SPEC-0001-test/spec.md) | Test | `Draft` |
'@
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
- 批准人：—
- 批准日期：—

### FR-001：可验证行为

## 批准记录

- 规格批准：—
- 批准日期：—
'@
    Write-Utf8File -Path (Join-Path $package 'plan.md') -Content @'
# Plan

- 状态：`Draft`
- Spec 基线提交：—
- 覆盖需求：`FR-001`
- 批准人：—
- 批准日期：—
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

- 最终结果：`Pending`

| 需求 | 实现位置 | 测试或基准 | 文档 | 结果 |
| --- | --- | --- | --- | --- |
| FR-001 | | | | Pending |
'@

    & git -C $validRoot init --quiet
    & git -C $validRoot config user.name 'Documentation Verifier Test'
    & git -C $validRoot config user.email 'documentation-verifier@example.invalid'
    & git -C $validRoot config core.autocrlf false
    & git -C $validRoot add --all
    & git -C $validRoot commit --quiet -m 'Create valid draft fixture'

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
    Assert-InvalidFixture -Name 'requirement-outside-task-coverage' -ExpectedMessage 'has no task for declared requirement FR-001' -Mutate {
        param($fixture)
        $path = Join-Path $fixture 'docs/specs/SPEC-0001-test/tasks.md'
        (Get-Content -LiteralPath $path -Raw).Replace('- 覆盖需求：`FR-001`', '- 说明：`FR-001`') |
            Set-Content -LiteralPath $path -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'implemented-with-draft-plan' -ExpectedMessage 'Implemented Spec requires an Approved Plan' -Mutate {
        param($fixture)
        $specPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/spec.md'
        $spec = Get-Content -LiteralPath $specPath -Raw
        $spec = $spec.Replace('- 状态：`Draft`', '- 状态：`Implemented`')
        $spec = $spec.Replace('- 批准人：—', '- 批准人：项目作者')
        $spec = $spec.Replace('- 规格批准：—', '- 规格批准：项目作者')
        $spec = $spec.Replace('- 批准日期：—', '- 批准日期：2026-08-28')
        Set-Content -LiteralPath $specPath -Value $spec -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'approved-plan-missing-approval' -ExpectedMessage 'Approved Plan lacks approval metadata' -Mutate {
        param($fixture)
        $planPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/plan.md'
        (Get-Content -LiteralPath $planPath -Raw).Replace('- 状态：`Draft`', '- 状态：`Approved`') |
            Set-Content -LiteralPath $planPath -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'plan-baseline-not-approved' -ExpectedMessage 'Spec baseline does not contain an Approved Spec' -Mutate {
        param($fixture)
        $baseline = (& git -C $fixture rev-parse HEAD).Trim()
        $planPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/plan.md'
        $plan = Get-Content -LiteralPath $planPath -Raw
        $plan = $plan.Replace('- 状态：`Draft`', '- 状态：`Approved`')
        $plan = $plan.Replace('- Spec 基线提交：—', "- Spec 基线提交：``$baseline``")
        $plan = $plan.Replace('- 批准人：—', '- 批准人：项目作者')
        $plan = $plan.Replace('- 批准日期：—', '- 批准日期：2026-08-28')
        Set-Content -LiteralPath $planPath -Value $plan -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'implemented-with-pending-task' -ExpectedMessage 'Implemented Spec requires every Task to be Completed' -Mutate {
        param($fixture)
        $specPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/spec.md'
        $spec = Get-Content -LiteralPath $specPath -Raw
        $spec = $spec.Replace('- 状态：`Draft`', '- 状态：`Implemented`')
        $spec = $spec.Replace('- 批准人：—', '- 批准人：项目作者')
        $spec = $spec.Replace('- 规格批准：—', '- 规格批准：项目作者')
        $spec = $spec.Replace('- 批准日期：—', '- 批准日期：2026-08-28')
        Set-Content -LiteralPath $specPath -Value $spec -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'implemented-with-pending-verification' -ExpectedMessage 'Implemented Spec requires Verification to be Passed' -Mutate {
        param($fixture)
        $specPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/spec.md'
        $spec = Get-Content -LiteralPath $specPath -Raw
        $spec = $spec.Replace('- 状态：`Draft`', '- 状态：`Implemented`')
        $spec = $spec.Replace('- 批准人：—', '- 批准人：项目作者')
        $spec = $spec.Replace('- 规格批准：—', '- 规格批准：项目作者')
        $spec = $spec.Replace('- 批准日期：—', '- 批准日期：2026-08-28')
        Set-Content -LiteralPath $specPath -Value $spec -Encoding utf8NoBOM

        $tasksPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/tasks.md'
        (Get-Content -LiteralPath $tasksPath -Raw).Replace('- 状态：`Pending`', '- 状态：`Completed`') |
            Set-Content -LiteralPath $tasksPath -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'implemented-with-empty-evidence' -ExpectedMessage 'has incomplete Passed evidence for requirement FR-001' -Mutate {
        param($fixture)
        $verificationPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/verification.md'
        $verification = Get-Content -LiteralPath $verificationPath -Raw
        $verification = $verification.Replace('- 最终结果：`Pending`', '- 最终结果：`Passed`')
        $verification = $verification.Replace('| FR-001 | | | | Pending |', '| FR-001 | | | | Passed |')
        Set-Content -LiteralPath $verificationPath -Value $verification -Encoding utf8NoBOM

        $specPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/spec.md'
        $spec = Get-Content -LiteralPath $specPath -Raw
        $spec = $spec.Replace('- 状态：`Draft`', '- 状态：`Implemented`')
        $spec = $spec.Replace('- 批准人：—', '- 批准人：项目作者')
        $spec = $spec.Replace('- 规格批准：—', '- 规格批准：项目作者')
        $spec = $spec.Replace('- 批准日期：—', '- 批准日期：2026-08-28')
        Set-Content -LiteralPath $specPath -Value $spec -Encoding utf8NoBOM

        $tasksPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/tasks.md'
        (Get-Content -LiteralPath $tasksPath -Raw).Replace('- 状态：`Pending`', '- 状态：`Completed`') |
            Set-Content -LiteralPath $tasksPath -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'index-status-mismatch' -ExpectedMessage 'Spec index status does not match package status' -Mutate {
        param($fixture)
        $indexPath = Join-Path $fixture 'docs/specs/README.md'
        (Get-Content -LiteralPath $indexPath -Raw).Replace('`Draft`', '`Approved`') |
            Set-Content -LiteralPath $indexPath -Encoding utf8NoBOM
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
