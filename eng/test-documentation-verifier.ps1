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

function Add-ReplacementFixturePackage {
    param([Parameter(Mandatory)][string]$RepositoryRoot)

    $package = Join-Path $RepositoryRoot 'docs/specs/SPEC-0002-replacement'
    Write-Utf8File -Path (Join-Path $package 'spec.md') -Content @'
# SPEC-0002

- 编号：`SPEC-0002`
- 状态：`Approved`
- 当前适用性：`Current`
- 批准人：项目作者
- 批准日期：2026-09-18

### FR-001：替代规格

## 批准记录

- 规格批准：项目作者
- 批准日期：2026-09-18
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

    $indexPath = Join-Path $RepositoryRoot 'docs/specs/README.md'
    Add-Content -LiteralPath $indexPath -Value '| [SPEC-0002](./SPEC-0002-replacement/spec.md) | Replacement | `Current` | `Approved` |'
}

function Assert-InvalidFixture {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][scriptblock]$Mutate,
        [Parameter(Mandatory)][string]$ExpectedMessage,
        [string]$SourceRoot = $validRoot
    )

    $fixture = Join-Path $testRoot $Name
    Copy-Item -LiteralPath $SourceRoot -Destination $fixture -Recurse
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

| 编号 | 主题 | 当前适用性 | 状态 |
| --- | --- | --- | --- |
| [SPEC-0001](./SPEC-0001-test/spec.md) | Test | `Current` | `Draft` |
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
- 当前适用性：`Current`
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
    Write-Utf8File -Path (Join-Path $validRoot '.artifacts/temporary.md') -Content '[Missing](missing.md)'

    $validResult = Invoke-Verifier -RepositoryRoot $validRoot
    if ($validResult.ExitCode -ne 0) {
        throw "Valid fixture failed: $($validResult.Output)"
    }

    Assert-InvalidFixture -Name 'formal-link-to-artifacts' -ExpectedMessage 'links to temporary artifact path' -Mutate {
        param($fixture)
        Write-Utf8File -Path (Join-Path $fixture '.artifacts/result.md') -Content 'temporary result'
        Add-Content -LiteralPath (Join-Path $fixture 'README.md') -Value '[Temporary result](.artifacts/result.md)'
    }
    Assert-InvalidFixture -Name 'formal-link-to-benchmark-artifacts' -ExpectedMessage 'links to temporary artifact path' -Mutate {
        param($fixture)
        Write-Utf8File -Path (Join-Path $fixture 'BenchmarkDotNet.Artifacts/result.md') -Content 'temporary result'
        Add-Content -LiteralPath (Join-Path $fixture 'README.md') -Value '[Benchmark result](BenchmarkDotNet.Artifacts/result.md)'
    }
    Assert-InvalidFixture -Name 'tracked-artifacts-file' -ExpectedMessage 'is under forbidden temporary artifact directory' -Mutate {
        param($fixture)
        $trackedPath = Join-Path $fixture '.artifacts/tracked.md'
        Write-Utf8File -Path $trackedPath -Content 'tracked temporary result'
        & git -C $fixture add --force -- '.artifacts/tracked.md'
    }
    Assert-InvalidFixture -Name 'tracked-benchmark-artifacts-file' -ExpectedMessage 'is under forbidden temporary artifact directory' -Mutate {
        param($fixture)
        $trackedPath = Join-Path $fixture 'BenchmarkDotNet.Artifacts/tracked.md'
        Write-Utf8File -Path $trackedPath -Content 'tracked temporary result'
        & git -C $fixture add --force -- 'BenchmarkDotNet.Artifacts/tracked.md'
    }

    Assert-InvalidFixture -Name 'missing-applicability' -ExpectedMessage 'has a missing or invalid current applicability' -Mutate {
        param($fixture)
        $specPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/spec.md'
        (Get-Content -LiteralPath $specPath -Raw).Replace('- 当前适用性：`Current`', '') |
            Set-Content -LiteralPath $specPath -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'index-applicability-mismatch' -ExpectedMessage 'Spec index applicability does not match package applicability' -Mutate {
        param($fixture)
        $indexPath = Join-Path $fixture 'docs/specs/README.md'
        (Get-Content -LiteralPath $indexPath -Raw).Replace('`Current`', '`Partial`') |
            Set-Content -LiteralPath $indexPath -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'superseded-with-current-applicability' -ExpectedMessage 'Superseded Spec requires Superseded current applicability' -Mutate {
        param($fixture)
        $specPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/spec.md'
        (Get-Content -LiteralPath $specPath -Raw).Replace('- 状态：`Draft`', '- 状态：`Superseded`') |
            Set-Content -LiteralPath $specPath -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'partial-without-scope' -ExpectedMessage "requires a non-empty '## 当前适用范围' section" -Mutate {
        param($fixture)
        $specPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/spec.md'
        $spec = (Get-Content -LiteralPath $specPath -Raw).Replace('- 当前适用性：`Current`', '- 当前适用性：`Partial`')
        Set-Content -LiteralPath $specPath -Value $spec -Encoding utf8NoBOM

        $indexPath = Join-Path $fixture 'docs/specs/README.md'
        (Get-Content -LiteralPath $indexPath -Raw).Replace('`Current`', '`Partial`') |
            Set-Content -LiteralPath $indexPath -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'partial-without-replacement-link' -ExpectedMessage 'must link to another SPEC spec.md' -Mutate {
        param($fixture)
        $specPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/spec.md'
        $spec = (Get-Content -LiteralPath $specPath -Raw).Replace('- 当前适用性：`Current`', '- 当前适用性：`Partial`')
        $spec = $spec.Replace('## 批准记录', "## 当前适用范围`n`n保留现有范围。`n`n## 批准记录")
        Set-Content -LiteralPath $specPath -Value $spec -Encoding utf8NoBOM

        $indexPath = Join-Path $fixture 'docs/specs/README.md'
        (Get-Content -LiteralPath $indexPath -Raw).Replace('`Current`', '`Partial`') |
            Set-Content -LiteralPath $indexPath -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'partial-with-draft-replacement' -ExpectedMessage 'must link to another SPEC spec.md' -Mutate {
        param($fixture)
        Add-ReplacementFixturePackage -RepositoryRoot $fixture
        $replacementSpecPath = Join-Path $fixture 'docs/specs/SPEC-0002-replacement/spec.md'
        (Get-Content -LiteralPath $replacementSpecPath -Raw).Replace('- 状态：`Approved`', '- 状态：`Draft`') |
            Set-Content -LiteralPath $replacementSpecPath -Encoding utf8NoBOM

        $specPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/spec.md'
        $spec = (Get-Content -LiteralPath $specPath -Raw).Replace('- 当前适用性：`Current`', '- 当前适用性：`Partial`')
        $spec = $spec.Replace('## 批准记录', "## 当前适用范围`n`n保留现有范围；详见[替代规格](../SPEC-0002-replacement/spec.md)。`n`n## 批准记录")
        Set-Content -LiteralPath $specPath -Value $spec -Encoding utf8NoBOM

        $indexPath = Join-Path $fixture 'docs/specs/README.md'
        (Get-Content -LiteralPath $indexPath -Raw).Replace('| [SPEC-0001](./SPEC-0001-test/spec.md) | Test | `Current` | `Draft` |', '| [SPEC-0001](./SPEC-0001-test/spec.md) | Test | `Partial` | `Draft` |') |
            Set-Content -LiteralPath $indexPath -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'partial-with-self-link' -ExpectedMessage 'must link to another SPEC spec.md' -Mutate {
        param($fixture)
        $specPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/spec.md'
        $spec = (Get-Content -LiteralPath $specPath -Raw).Replace('- 当前适用性：`Current`', '- 当前适用性：`Partial`')
        $spec = $spec.Replace('## 批准记录', "## 当前适用范围`n`n保留现有范围；见[本规格](./spec.md)。`n`n## 批准记录")
        Set-Content -LiteralPath $specPath -Value $spec -Encoding utf8NoBOM

        $indexPath = Join-Path $fixture 'docs/specs/README.md'
        (Get-Content -LiteralPath $indexPath -Raw).Replace('`Current`', '`Partial`') |
            Set-Content -LiteralPath $indexPath -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'partial-without-retained-marker' -ExpectedMessage "Partial current applicability requires '保留'" -Mutate {
        param($fixture)
        Add-ReplacementFixturePackage -RepositoryRoot $fixture
        $specPath = Join-Path $fixture 'docs/specs/SPEC-0001-test/spec.md'
        $spec = (Get-Content -LiteralPath $specPath -Raw).Replace('- 当前适用性：`Current`', '- 当前适用性：`Partial`')
        $spec = $spec.Replace('## 批准记录', "## 当前适用范围`n`n范围转由替代规格说明。`n`n[替代规格](../SPEC-0002-replacement/spec.md)`n`n## 批准记录")
        Set-Content -LiteralPath $specPath -Value $spec -Encoding utf8NoBOM

        $indexPath = Join-Path $fixture 'docs/specs/README.md'
        (Get-Content -LiteralPath $indexPath -Raw).Replace('| [SPEC-0001](./SPEC-0001-test/spec.md) | Test | `Current` | `Draft` |', '| [SPEC-0001](./SPEC-0001-test/spec.md) | Test | `Partial` | `Draft` |') |
            Set-Content -LiteralPath $indexPath -Encoding utf8NoBOM
    }
    Assert-InvalidFixture -Name 'verification-drive-path' -ExpectedMessage 'contains forbidden absolute drive path reference' -Mutate {
        param($fixture)
        Add-Content -LiteralPath (Join-Path $fixture 'docs/specs/SPEC-0001-test/verification.md') -Value 'Measured source: C:\temp\candidate.cs'
    }
    Assert-InvalidFixture -Name 'verification-file-url' -ExpectedMessage 'contains forbidden file:// reference' -Mutate {
        param($fixture)
        Add-Content -LiteralPath (Join-Path $fixture 'docs/specs/SPEC-0001-test/verification.md') -Value 'Measured source: file:///tmp/candidate.cs'
    }
    Assert-InvalidFixture -Name 'verification-tmp-path' -ExpectedMessage 'contains forbidden /tmp/ reference' -Mutate {
        param($fixture)
        Add-Content -LiteralPath (Join-Path $fixture 'docs/specs/SPEC-0001-test/verification.md') -Value 'Measured source: /tmp/candidate.cs'
    }
    Assert-InvalidFixture -Name 'verification-environment-path' -ExpectedMessage 'contains forbidden %TEMP% or %LOCALAPPDATA% reference' -Mutate {
        param($fixture)
        Add-Content -LiteralPath (Join-Path $fixture 'docs/specs/SPEC-0001-test/verification.md') -Value 'Measured source: %LOCALAPPDATA%\Temp\candidate.cs'
    }

    $partialFixture = Join-Path $testRoot 'implemented-partial'
    Copy-Item -LiteralPath $validRoot -Destination $partialFixture -Recurse
    Remove-Item -LiteralPath (Join-Path $partialFixture '.artifacts/temporary.md') -Force
    Add-ReplacementFixturePackage -RepositoryRoot $partialFixture
    $partialSpecPath = Join-Path $partialFixture 'docs/specs/SPEC-0001-test/spec.md'
    $partialSpec = Get-Content -LiteralPath $partialSpecPath -Raw
    $partialSpec = $partialSpec.Replace('- 状态：`Draft`', '- 状态：`Approved`')
    $partialSpec = $partialSpec.Replace('- 批准人：—', '- 批准人：项目作者')
    $partialSpec = $partialSpec.Replace('- 规格批准：—', '- 规格批准：项目作者')
    $partialSpec = $partialSpec.Replace('- 批准日期：—', '- 批准日期：2026-09-18')
    $partialSpec = $partialSpec.Replace('## 批准记录', "## 当前适用范围`n`n保留现有需求；实现细节由[替代规格](../SPEC-0002-replacement/spec.md)接替。`n`n## 批准记录")
    $partialSpec = $partialSpec.Replace('- 当前适用性：`Current`', '- 当前适用性：`Partial`')
    Set-Content -LiteralPath $partialSpecPath -Value $partialSpec -Encoding utf8NoBOM
    $partialIndexPath = Join-Path $partialFixture 'docs/specs/README.md'
    $partialIndex = Get-Content -LiteralPath $partialIndexPath -Raw
    $partialIndex = $partialIndex.Replace('| [SPEC-0001](./SPEC-0001-test/spec.md) | Test | `Current` | `Draft` |', '| [SPEC-0001](./SPEC-0001-test/spec.md) | Test | `Partial` | `Approved` |')
    Set-Content -LiteralPath $partialIndexPath -Value $partialIndex -Encoding utf8NoBOM
    & git -C $partialFixture add --all
    & git -C $partialFixture commit --quiet -m 'Create approved partial Spec baseline'
    $partialBaseline = (& git -C $partialFixture rev-parse HEAD).Trim()

    $partialSpec = (Get-Content -LiteralPath $partialSpecPath -Raw).Replace('- 状态：`Approved`', '- 状态：`Implemented`')
    Set-Content -LiteralPath $partialSpecPath -Value $partialSpec -Encoding utf8NoBOM
    $partialPlanPath = Join-Path $partialFixture 'docs/specs/SPEC-0001-test/plan.md'
    $partialPlan = Get-Content -LiteralPath $partialPlanPath -Raw
    $partialPlan = $partialPlan.Replace('- 状态：`Draft`', '- 状态：`Approved`')
    $partialPlan = $partialPlan.Replace('- Spec 基线提交：—', "- Spec 基线提交：``$partialBaseline``")
    $partialPlan = $partialPlan.Replace('- 批准人：—', '- 批准人：项目作者')
    $partialPlan = $partialPlan.Replace('- 批准日期：—', '- 批准日期：2026-09-18')
    Set-Content -LiteralPath $partialPlanPath -Value $partialPlan -Encoding utf8NoBOM
    $partialTasksPath = Join-Path $partialFixture 'docs/specs/SPEC-0001-test/tasks.md'
    (Get-Content -LiteralPath $partialTasksPath -Raw).Replace('- 状态：`Pending`', '- 状态：`Completed`') |
        Set-Content -LiteralPath $partialTasksPath -Encoding utf8NoBOM
    $partialVerificationPath = Join-Path $partialFixture 'docs/specs/SPEC-0001-test/verification.md'
    $partialVerification = Get-Content -LiteralPath $partialVerificationPath -Raw
    $partialVerification = $partialVerification.Replace('- 最终结果：`Pending`', '- 最终结果：`Passed`')
    $partialVerification = $partialVerification.Replace('| FR-001 | | | | Pending |', '| FR-001 | implementation | tests | docs | Passed |')
    Set-Content -LiteralPath $partialVerificationPath -Value $partialVerification -Encoding utf8NoBOM
    $partialIndex = (Get-Content -LiteralPath $partialIndexPath -Raw).Replace('| [SPEC-0001](./SPEC-0001-test/spec.md) | Test | `Partial` | `Approved` |', '| [SPEC-0001](./SPEC-0001-test/spec.md) | Test | `Partial` | `Implemented` |')
    Set-Content -LiteralPath $partialIndexPath -Value $partialIndex -Encoding utf8NoBOM
    $missingReviewResult = Invoke-Verifier -RepositoryRoot $partialFixture
    if ($missingReviewResult.ExitCode -eq 0 -or $missingReviewResult.Output -notlike "*requires one '## 现状文档核对' section*") {
        throw "Completed fixture accepted missing current-document review: $($missingReviewResult.Output)"
    }
    Write-Utf8File -Path (Join-Path $partialFixture 'docs/architecture/overview.md') -Content '# 当前系统概览'
    $reviewTable = @'

## 现状文档核对

| 文档/章节 | 最终处理 | 更新位置或无需更新的理由 |
| --- | --- | --- |
| [架构概览](../../architecture/overview.md) | 无需更新 | 仅替换内部计算，现有职责、生命周期和公开能力表述仍然成立。 |
| [入口](../../../README.md) | 已更新 | 入口导航已链接新的规格索引。 |
'@
    Add-Content -LiteralPath $partialVerificationPath -Value $reviewTable -Encoding utf8NoBOM
    $partialResult = Invoke-Verifier -RepositoryRoot $partialFixture
    if ($partialResult.ExitCode -ne 0) {
        throw "Implemented+Partial fixture failed: $($partialResult.Output)"
    }

    Assert-InvalidFixture -Name 'document-review-pending' -SourceRoot $partialFixture -ExpectedMessage 'document review has an incomplete or invalid outcome' -Mutate {
        param($fixture)
        $path = Join-Path $fixture 'docs/specs/SPEC-0001-test/verification.md'
        (Get-Content -LiteralPath $path -Raw).Replace('| 无需更新 |', '| Pending |') | Set-Content -LiteralPath $path
    }
    foreach ($reason in @('', '—', 'TODO', '无影响', '已同步。', '更新位置待补充')) {
        Assert-InvalidFixture -Name ("document-review-reason-" + [guid]::NewGuid().ToString('N')) -SourceRoot $partialFixture -ExpectedMessage 'document review requires a concrete update location or no-change reason' -Mutate {
            param($fixture)
            $path = Join-Path $fixture 'docs/specs/SPEC-0001-test/verification.md'
            (Get-Content -LiteralPath $path -Raw).Replace('仅替换内部计算，现有职责、生命周期和公开能力表述仍然成立。', $reason) |
                Set-Content -LiteralPath $path
        }
    }
    Assert-InvalidFixture -Name 'document-review-missing-link' -SourceRoot $partialFixture -ExpectedMessage 'document review must link each row to an existing local document' -Mutate {
        param($fixture)
        $path = Join-Path $fixture 'docs/specs/SPEC-0001-test/verification.md'
        (Get-Content -LiteralPath $path -Raw).Replace('[架构概览](../../architecture/overview.md)', '架构概览') | Set-Content -LiteralPath $path
    }
    Assert-InvalidFixture -Name 'document-review-broken-target' -SourceRoot $partialFixture -ExpectedMessage 'references missing local target' -Mutate {
        param($fixture)
        $path = Join-Path $fixture 'docs/specs/SPEC-0001-test/verification.md'
        (Get-Content -LiteralPath $path -Raw).Replace('../../architecture/overview.md', '../../architecture/missing.md') | Set-Content -LiteralPath $path
    }
    Assert-InvalidFixture -Name 'document-review-no-overview' -SourceRoot $partialFixture -ExpectedMessage 'document review must include docs/architecture/overview.md' -Mutate {
        param($fixture)
        $path = Join-Path $fixture 'docs/specs/SPEC-0001-test/verification.md'
        (Get-Content -LiteralPath $path -Raw).Replace('../../architecture/overview.md', '../../../README.md') | Set-Content -LiteralPath $path
    }
    Assert-InvalidFixture -Name 'document-review-placeholder-extra-row' -SourceRoot $partialFixture -ExpectedMessage 'document review has an incomplete or invalid outcome' -Mutate {
        param($fixture)
        Add-Content -LiteralPath (Join-Path $fixture 'docs/specs/SPEC-0001-test/verification.md') -Value '| | Pending | |'
    }
    Assert-InvalidFixture -Name 'document-review-comment-only' -SourceRoot $partialFixture -ExpectedMessage "requires one '## 现状文档核对' section" -Mutate {
        param($fixture)
        $path = Join-Path $fixture 'docs/specs/SPEC-0001-test/verification.md'
        ((Get-Content -LiteralPath $path -Raw).Replace('## 现状文档核对', "<!--`n## 现状文档核对") + "`n-->") | Set-Content -LiteralPath $path
    }
    Assert-InvalidFixture -Name 'document-review-passed-before-implemented' -SourceRoot $partialFixture -ExpectedMessage "requires one '## 现状文档核对' section" -Mutate {
        param($fixture)
        $path = Join-Path $fixture 'docs/specs/SPEC-0001-test/spec.md'
        (Get-Content -LiteralPath $path -Raw).Replace('`Implemented`', '`Verifying`') | Set-Content -LiteralPath $path
        $path = Join-Path $fixture 'docs/specs/README.md'
        (Get-Content -LiteralPath $path -Raw).Replace('`Implemented`', '`Verifying`') | Set-Content -LiteralPath $path
        $path = Join-Path $fixture 'docs/specs/SPEC-0001-test/verification.md'
        ((Get-Content -LiteralPath $path -Raw) -split '## 现状文档核对', 2)[0] | Set-Content -LiteralPath $path
    }

    # A draft report may keep the template's Pending review without pretending to have completed it.
    $pendingReviewFixture = Join-Path $testRoot 'document-review-draft'
    Copy-Item -LiteralPath $validRoot -Destination $pendingReviewFixture -Recurse
    Write-Utf8File -Path (Join-Path $pendingReviewFixture 'docs/architecture/overview.md') -Content '# 当前系统概览'
    Add-Content -LiteralPath (Join-Path $pendingReviewFixture 'docs/specs/SPEC-0001-test/verification.md') -Value $reviewTable.Replace('| 无需更新 |', '| Pending |')
    $pendingReviewResult = Invoke-Verifier -RepositoryRoot $pendingReviewFixture
    if ($pendingReviewResult.ExitCode -ne 0) {
        throw "Draft document-review template unexpectedly failed: $($pendingReviewResult.Output)"
    }

    $partialVerification = (Get-Content -LiteralPath $partialVerificationPath -Raw).Replace('| FR-001 | implementation | tests | docs | Passed |', '| FR-001 | implementation | tests | docs | Pending |')
    Set-Content -LiteralPath $partialVerificationPath -Value $partialVerification -Encoding utf8NoBOM
    $partialEvidenceResult = Invoke-Verifier -RepositoryRoot $partialFixture
    if ($partialEvidenceResult.ExitCode -eq 0 -or
        $partialEvidenceResult.Output -notlike '*has incomplete Passed evidence for requirement FR-001*') {
        throw "Implemented+Partial fixture accepted incomplete historical evidence. Output: $($partialEvidenceResult.Output)"
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
