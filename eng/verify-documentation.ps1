[CmdletBinding()]
param(
    [string]$RootPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = if ([string]::IsNullOrWhiteSpace($RootPath)) {
    [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
}
else {
    [IO.Path]::GetFullPath($RootPath)
}
$errors = [Collections.Generic.List[string]]::new()

function Add-VerificationError {
    param([Parameter(Mandatory)][string]$Message)

    $errors.Add($Message)
}

function Get-RepositoryRelativePath {
    param([Parameter(Mandatory)][string]$Path)

    return [IO.Path]::GetRelativePath($repositoryRoot, $Path).Replace('\', '/')
}

function Get-MarkdownLineNumber {
    param(
        [Parameter(Mandatory)][string]$Content,
        [Parameter(Mandatory)][int]$Index
    )

    if ($Index -eq 0) {
        return 1
    }

    return ([regex]::Matches($Content.Substring(0, $Index), "`n").Count + 1)
}

function Test-MarkdownLinks {
    $markdownFiles = Get-ChildItem -LiteralPath $repositoryRoot -Recurse -File -Filter '*.md' |
        Where-Object {
            $_.FullName -notmatch '[\\/](bin|obj|\.git|_site|\.artifacts)[\\/]' -and
            $_.FullName -notmatch '[\\/]BenchmarkDotNet\.Artifacts[\\/]'
        }

    $linkPattern = [regex]'\[[^\]]+\]\((?<target>[^)]+)\)'
    foreach ($file in $markdownFiles) {
        $content = Get-Content -LiteralPath $file.FullName -Raw
        foreach ($match in $linkPattern.Matches($content)) {
            $target = $match.Groups['target'].Value.Trim().Trim('<', '>')
            if ($target -match '^(https?://|mailto:|xref:|#)') {
                continue
            }

            $pathPart = ($target -split '#', 2)[0]
            if ([string]::IsNullOrWhiteSpace($pathPart)) {
                continue
            }

            $decodedPath = [Uri]::UnescapeDataString($pathPart)
            if ($decodedPath -match '(?i)(^|[\\/])(?:\.artifacts|BenchmarkDotNet\.Artifacts)(?:[\\/]|$)') {
                $relativePath = Get-RepositoryRelativePath $file.FullName
                $line = Get-MarkdownLineNumber -Content $content -Index $match.Index
                Add-VerificationError "$relativePath`:$line links to temporary artifact path '$target'."
                continue
            }

            $resolvedPath = [IO.Path]::GetFullPath((Join-Path $file.DirectoryName $decodedPath))
            if (-not (Test-Path -LiteralPath $resolvedPath)) {
                $relativePath = Get-RepositoryRelativePath $file.FullName
                $line = Get-MarkdownLineNumber -Content $content -Index $match.Index
                Add-VerificationError "$relativePath`:$line references missing local target '$target'."
            }
        }
    }
}

function Get-DocumentStatus {
    param([Parameter(Mandatory)][string]$Content)

    $match = [regex]::Match($Content, '(?ms)^## 状态\s*\r?\n\s*(?<status>[^\r\n]+)')
    if (-not $match.Success) {
        return $null
    }

    $value = $match.Groups['status'].Value.Trim().Trim('`')
    if ($value.StartsWith('状态：', [StringComparison]::Ordinal)) {
        $value = $value.Substring('状态：'.Length).Trim()
    }
    foreach ($allowed in @('Accepted', 'Superseded', 'Rejected')) {
        if ($value.StartsWith($allowed, [StringComparison]::Ordinal)) {
            return $allowed
        }
    }

    return $value
}

function Test-AdrIndex {
    $decisionDirectory = Join-Path $repositoryRoot 'docs/decisions'
    $indexPath = Join-Path $decisionDirectory 'README.md'
    $indexContent = Get-Content -LiteralPath $indexPath -Raw
    $indexPattern = [regex]'(?m)^\| \[(?<number>\d{4})\]\((?<file>[^)]+)\) \|.*\| `(?<status>Accepted|Superseded|Rejected)` \|\r?$'
    $indexEntries = @{}

    foreach ($match in $indexPattern.Matches($indexContent)) {
        $number = $match.Groups['number'].Value
        if ($indexEntries.ContainsKey($number)) {
            Add-VerificationError "docs/decisions/README.md contains duplicate ADR $number."
            continue
        }

        $indexEntries[$number] = @{
            File = $match.Groups['file'].Value
            Status = $match.Groups['status'].Value
        }
    }

    $adrFiles = Get-ChildItem -LiteralPath $decisionDirectory -File -Filter '*.md' |
        Where-Object { $_.Name -match '^(?<number>\d{4})-' }

    foreach ($file in $adrFiles) {
        $number = [regex]::Match($file.Name, '^(\d{4})-').Groups[1].Value
        if (-not $indexEntries.ContainsKey($number)) {
            Add-VerificationError "$(Get-RepositoryRelativePath $file.FullName) is missing from the ADR index."
            continue
        }

        $entry = $indexEntries[$number]
        if ($entry.File -ne $file.Name) {
            Add-VerificationError "ADR $number index target '$($entry.File)' does not match '$($file.Name)'."
        }

        $content = Get-Content -LiteralPath $file.FullName -Raw
        $status = Get-DocumentStatus $content
        if ($null -eq $status) {
            Add-VerificationError "$(Get-RepositoryRelativePath $file.FullName) has no '## 状态' section."
        }
        elseif ($status -ne $entry.Status) {
            Add-VerificationError "ADR $number status '$status' does not match index status '$($entry.Status)'."
        }

        if ($status -eq 'Superseded') {
            $statusSection = [regex]::Match($content, '(?ms)^## 状态\s*\r?\n(?<body>.*?)(?=^## |\z)').Groups['body'].Value
            if ($statusSection -notmatch '\[[^\]]+\]\([^)]+\)') {
                Add-VerificationError "ADR $number is Superseded but its status section does not link the replacement."
            }
        }
    }

    foreach ($number in $indexEntries.Keys) {
        $target = Join-Path $decisionDirectory $indexEntries[$number].File
        if (-not (Test-Path -LiteralPath $target)) {
            Add-VerificationError "ADR index entry $number points to missing file '$($indexEntries[$number].File)'."
        }
    }
}

function Test-VerificationTemporaryReferences {
    param(
        [Parameter(Mandatory)][string]$PackageName,
        [Parameter(Mandatory)][string]$Content
    )

    $verificationPath = "docs/specs/$PackageName/verification.md"
    $forbiddenReferences = @(
        @{
            Pattern = '(?i)(?<![A-Za-z0-9_])[A-Z]:[\\/]'
            Description = 'absolute drive path'
        },
        @{
            Pattern = '(?i)file://'
            Description = 'file://'
        },
        @{
            Pattern = '(?i)/tmp/'
            Description = '/tmp/'
        },
        @{
            Pattern = '(?i)%TEMP%|%LOCALAPPDATA%'
            Description = '%TEMP% or %LOCALAPPDATA%'
        }
    )

    foreach ($forbiddenReference in $forbiddenReferences) {
        if ([regex]::IsMatch($Content, $forbiddenReference.Pattern)) {
            Add-VerificationError "$verificationPath contains forbidden $($forbiddenReference.Description) reference."
        }
    }
}

function Test-TrackedTemporaryArtifacts {
    $gitOutput = @()
    try {
        $gitOutput = @(& git -C $repositoryRoot ls-files --cached -- 2>&1)
        $gitExitCode = $LASTEXITCODE
    }
    catch {
        Add-VerificationError "Unable to inspect tracked files with 'git ls-files --cached': $($_.Exception.Message)"
        return
    }

    if ($gitExitCode -ne 0) {
        $detail = ($gitOutput | ForEach-Object { $_.ToString().Trim() } | Where-Object { $_ }) -join ' '
        if ([string]::IsNullOrWhiteSpace($detail)) {
            $detail = "git exited with code $gitExitCode"
        }
        Add-VerificationError "Unable to inspect tracked files with 'git ls-files --cached': $detail"
        return
    }

    foreach ($trackedPath in $gitOutput) {
        $normalizedPath = $trackedPath.ToString()
        if ($normalizedPath -match '(?i)(^|[\\/])(?:\.artifacts|BenchmarkDotNet\.Artifacts)(?:[\\/]|$)') {
            Add-VerificationError "Tracked file '$normalizedPath' is under forbidden temporary artifact directory."
        }
    }
}

# Migration snapshot: completed packages before the 2026-09-18 document-review gate.
# These are recorded Plan baselines, not reconstructed approval or measurement identities.
# Do not extend this list for newly completed packages; rebasing an old package removes its exemption.
$legacyDocumentReviewBaselines = @{
    'SPEC-0001-evaluation-special-values' = 'af81f402e42ee8e2b9d2454ffe69e8a1f9580dee'
    'SPEC-0002-continuous-algorithm-migration' = '5caccf473e2bd96e18209a0518a5c6476744ed60'
    'SPEC-0003-simd-repairs' = 'ff613be61efc8e39050f18e2151ce45ce9afe2c0'
    'SPEC-0004-masked-simd-reflect' = 'a87df18f4602f9917c217073a726fe80ed165abf'
    'SPEC-0005-algorithm-private-simd' = '1f4b98fb40930f331b5c00783c5394cb440e1548'
    'SPEC-0006-zero-overhead-simd-cascade' = '351e57d057604b2a9637e9a9ee883e32edd91ceb'
    'SPEC-0007-repair-boundary-shape-specialization' = 'e41d33f08a53ad3ecbfcfaa5982419e017ac5e8c'
    'SPEC-0009-high-performance-random-sampling' = '8ac50110830519dd874e16d055258f8b7cdab621'
    'SPEC-0010-simd-random-sampling' = '56c1f57340e74060b938fe2e431ff77b948328da'
    'SPEC-0011-bat-batched-simd' = 'ce43725516d70d70edbe000ffc713e073adb0b94'
    'SPEC-0012-cuckoo-batched-simd' = 'ce43725516d70d70edbe000ffc713e073adb0b94'
    'SPEC-0013-pso-simd-refinement' = 'ce43725516d70d70edbe000ffc713e073adb0b94'
    'SPEC-0014-firefly-simd-refinement' = 'ce43725516d70d70edbe000ffc713e073adb0b94'
}

function Test-CurrentDocumentReview {
    param(
        [Parameter(Mandatory)][string]$PackageName,
        [Parameter(Mandatory)][string]$PlanContent,
        [Parameter(Mandatory)][string]$Content
    )

    $relativePath = "docs/specs/$PackageName/verification.md"
    # Examples and comments must not stand in for an actual review record.
    $reviewContent = [regex]::Replace($Content, '(?ms)^```[^\r\n]*\r?\n.*?^```[ \t]*\r?$|^~~~[^\r\n]*\r?\n.*?^~~~[ \t]*\r?$|<!--.*?-->', '')
    $sections = [regex]::Matches($reviewContent, '(?ms)^## 现状文档核对[ \t]*\r?\n(?<body>.*?)(?=^## |\z)')
    $baseline = [regex]::Match($PlanContent, '(?m)^- Spec 基线提交：`(?<commit>[0-9a-fA-F]{7,40})`\r?$')
    if ($sections.Count -eq 0 -and $legacyDocumentReviewBaselines.ContainsKey($PackageName) -and
        $baseline.Success -and $legacyDocumentReviewBaselines[$PackageName].StartsWith($baseline.Groups['commit'].Value, [StringComparison]::OrdinalIgnoreCase)) {
        return
    }
    if ($sections.Count -ne 1) {
        Add-VerificationError "$relativePath requires one '## 现状文档核对' section before completion."
        return
    }

    $hasOverview = $false
    $rowCount = 0
    $packagePath = Join-Path $repositoryRoot "docs/specs/$PackageName"
    $overviewPath = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'docs/architecture/overview.md'))
    foreach ($line in ($sections[0].Groups['body'].Value -split '\r?\n')) {
        if (-not $line.Trim().StartsWith('|')) { continue }
        $cells = @($line.Trim().Split('|') | ForEach-Object { $_.Trim() })
        if ($cells.Count -ne 5 -or $cells[0] -ne '' -or $cells[4] -ne '') {
            Add-VerificationError "$relativePath document review requires three-column table rows."
            continue
        }
        if ($cells[1] -eq '文档/章节' -and $cells[2] -eq '最终处理' -and $cells[3] -eq '更新位置或无需更新的理由') { continue }
        if ($cells[1] -match '^:?-{3,}:?$' -and $cells[2] -match '^:?-{3,}:?$' -and $cells[3] -match '^:?-{3,}:?$') { continue }
        $rowCount++
        $outcome = $cells[2].Trim('`')
        $reason = $cells[3].Trim('`', '*', ' ')
        if ($outcome -notin @('已更新', '无需更新')) {
            Add-VerificationError "$relativePath document review has an incomplete or invalid outcome."
        }
        if ($reason -match '^(?:[—–\-….\s]*|待定|无影响|已同步|已更新|无需更新|不适用|无)[。.!！]?$' -or
            $reason -match '(?i)\b(?:TODO|TBD|Pending)\b|待填写|待补充|待核对') {
            Add-VerificationError "$relativePath document review requires a concrete update location or no-change reason."
        }

        $hasLocalDocument = $false
        foreach ($link in [regex]::Matches($cells[1], '\[[^\]]+\]\((?<target>[^)]+)\)')) {
            $target = $link.Groups['target'].Value.Trim().Trim('<', '>')
            $pathPart = [Uri]::UnescapeDataString(($target -split '#', 2)[0])
            if ([string]::IsNullOrWhiteSpace($pathPart) -or $pathPart -match '^([a-zA-Z][a-zA-Z0-9+.-]*:|[/\\])') { continue }
            $resolvedPath = [IO.Path]::GetFullPath((Join-Path $packagePath $pathPart))
            $relativeTarget = Get-RepositoryRelativePath $resolvedPath
            if ($relativeTarget -match '^\.\.(?:/|$)' -or -not [IO.File]::Exists($resolvedPath)) { continue }
            $hasLocalDocument = $true
            if ($resolvedPath -eq $overviewPath) { $hasOverview = $true }
        }
        if (-not $hasLocalDocument) {
            Add-VerificationError "$relativePath document review must link each row to an existing local document."
        }
    }
    if ($rowCount -eq 0 -or -not $hasOverview) {
        Add-VerificationError "$relativePath document review must include docs/architecture/overview.md."
    }
}

function Test-SpecPackages {
    $specRoot = Join-Path $repositoryRoot 'docs/specs'
    if (-not (Test-Path -LiteralPath $specRoot)) {
        Add-VerificationError 'docs/specs is missing.'
        return
    }

    $specIndexPath = Join-Path $specRoot 'README.md'
    $specIndexContent = Get-Content -LiteralPath $specIndexPath -Raw
    $specIndexPattern = [regex]'(?m)^\|\s*\[SPEC-(?<number>\d{4})\]\((?<target>[^)]+)\)\s*\|\s*[^|]*\|\s*`(?<applicability>Current|Partial|Superseded)`\s*\|\s*`(?<status>Draft|Clarifying|Approved|Implementing|Verifying|Implemented|Superseded)`\s*\|\r?$'
    $specIndexEntries = @{}
    foreach ($match in $specIndexPattern.Matches($specIndexContent)) {
        $indexNumber = $match.Groups['number'].Value
        if ($specIndexEntries.ContainsKey($indexNumber)) {
            Add-VerificationError "docs/specs/README.md contains duplicate SPEC-$indexNumber."
            continue
        }

        $specIndexEntries[$indexNumber] = @{
            Target = $match.Groups['target'].Value
            Applicability = $match.Groups['applicability'].Value
            Status = $match.Groups['status'].Value
        }
    }

    $packageDirectories = Get-ChildItem -LiteralPath $specRoot -Directory |
        Where-Object { $_.Name -ne '_templates' }
    $seenNumbers = @{}
    $specStatuses = @('Draft', 'Clarifying', 'Approved', 'Implementing', 'Verifying', 'Implemented', 'Superseded')
    $planStatuses = @('Draft', 'Approved', 'Superseded')
    $taskStatuses = @('Pending', 'InProgress', 'Completed', 'Blocked')

    foreach ($package in $packageDirectories) {
        if ($package.Name -notmatch '^SPEC-(?<number>\d{4})-[a-z0-9]+(?:-[a-z0-9]+)*$') {
            Add-VerificationError "docs/specs/$($package.Name) does not follow SPEC-NNNN-kebab-case."
            continue
        }

        $number = $Matches['number']
        if ($seenNumbers.ContainsKey($number)) {
            Add-VerificationError "Spec number $number is used by both '$($seenNumbers[$number])' and '$($package.Name)'."
        }
        else {
            $seenNumbers[$number] = $package.Name
        }

        $requiredFiles = @('spec.md', 'plan.md', 'tasks.md', 'verification.md')
        foreach ($requiredFile in $requiredFiles) {
            if (-not (Test-Path -LiteralPath (Join-Path $package.FullName $requiredFile))) {
                Add-VerificationError "docs/specs/$($package.Name) is missing $requiredFile."
            }
        }
        $missingRequiredFiles = @($requiredFiles | Where-Object {
                -not (Test-Path -LiteralPath (Join-Path $package.FullName $_))
            })
        if ($missingRequiredFiles.Count -gt 0) {
            continue
        }

        $specContent = Get-Content -LiteralPath (Join-Path $package.FullName 'spec.md') -Raw
        $planContent = Get-Content -LiteralPath (Join-Path $package.FullName 'plan.md') -Raw
        $tasksContent = Get-Content -LiteralPath (Join-Path $package.FullName 'tasks.md') -Raw
        $verificationContent = Get-Content -LiteralPath (Join-Path $package.FullName 'verification.md') -Raw

        Test-VerificationTemporaryReferences -PackageName $package.Name -Content $verificationContent

        $declaredNumber = [regex]::Match($specContent, '(?m)^- 编号：`SPEC-(?<number>\d{4})`\r?$')
        if (-not $declaredNumber.Success -or $declaredNumber.Groups['number'].Value -ne $number) {
            Add-VerificationError "docs/specs/$($package.Name)/spec.md has a missing or mismatched Spec number."
        }

        $specStatusMatch = [regex]::Match($specContent, '(?m)^- 状态：`(?<status>[^`]+)`\r?$')
        if (-not $specStatusMatch.Success -or $specStatusMatch.Groups['status'].Value -notin $specStatuses) {
            Add-VerificationError "docs/specs/$($package.Name)/spec.md has an invalid Spec status."
        }
        $specStatus = if ($specStatusMatch.Success) { $specStatusMatch.Groups['status'].Value } else { $null }

        $specApplicabilityMatch = [regex]::Match($specContent, '(?m)^- 当前适用性：`(?<applicability>[^`]+)`\r?$')
        $specApplicabilityValues = @('Current', 'Partial', 'Superseded')
        if (-not $specApplicabilityMatch.Success -or
            $specApplicabilityMatch.Groups['applicability'].Value -notin $specApplicabilityValues) {
            Add-VerificationError "docs/specs/$($package.Name)/spec.md has a missing or invalid current applicability."
        }
        $specApplicability = if ($specApplicabilityMatch.Success) {
            $specApplicabilityMatch.Groups['applicability'].Value
        }
        else {
            $null
        }

        if (-not $specIndexEntries.ContainsKey($number)) {
            Add-VerificationError "docs/specs/$($package.Name) is missing from the Spec index."
        }
        else {
            $indexEntry = $specIndexEntries[$number]
            $expectedTarget = "./$($package.Name)/spec.md"
            if ($indexEntry.Target -ne $expectedTarget) {
                Add-VerificationError "Spec index target for SPEC-$number does not match '$expectedTarget'."
            }
            if ($null -ne $specStatus -and $indexEntry.Status -ne $specStatus) {
                Add-VerificationError "Spec index status does not match package status for SPEC-$number."
            }
            if ($null -ne $specApplicability -and $indexEntry.Applicability -ne $specApplicability) {
                Add-VerificationError "Spec index applicability does not match package applicability for SPEC-$number."
            }
        }

        if ($specStatus -eq 'Superseded' -and $specApplicability -ne 'Superseded') {
            Add-VerificationError "docs/specs/$($package.Name)/spec.md Superseded Spec requires Superseded current applicability."
        }

        if ($specApplicability -in @('Partial', 'Superseded')) {
            $scopeMatch = [regex]::Match($specContent, '(?ms)^## 当前适用范围\s*\r?\n(?<body>.*?)(?=^## |\z)')
            $scopeBody = if ($scopeMatch.Success) { $scopeMatch.Groups['body'].Value } else { $null }
            if (-not $scopeMatch.Success -or [string]::IsNullOrWhiteSpace($scopeBody)) {
                Add-VerificationError "docs/specs/$($package.Name)/spec.md current applicability '$specApplicability' requires a non-empty '## 当前适用范围' section."
            }
            else {
                if ($specApplicability -eq 'Partial' -and $scopeBody -notmatch '保留') {
                    Add-VerificationError "docs/specs/$($package.Name)/spec.md Partial current applicability requires '保留' in '## 当前适用范围'."
                }

                $currentSpecPath = [IO.Path]::GetFullPath((Join-Path $package.FullName 'spec.md'))
                $specRootWithSeparator = ([IO.Path]::GetFullPath($specRoot)).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
                $hasReplacementSpecLink = $false
                $scopeLinks = [regex]::Matches($scopeBody, '\[[^\]]+\]\((?<target>[^)]+)\)')
                foreach ($scopeLink in $scopeLinks) {
                    $linkTarget = $scopeLink.Groups['target'].Value.Trim().Trim('<', '>')
                    $linkPath = ($linkTarget -split '#', 2)[0].Trim()
                    if ([string]::IsNullOrWhiteSpace($linkPath) -or
                        $linkPath -match '(?i)^(?:[A-Z]:[\\/]|[\\/]|https?://|mailto:)') {
                        continue
                    }
                    if ($linkPath -notmatch '(?i)(?:^|[\\/])spec\.md$') {
                        continue
                    }

                    $resolvedTarget = [IO.Path]::GetFullPath((Join-Path $package.FullName ([Uri]::UnescapeDataString($linkPath))))
                    if ($resolvedTarget -eq $currentSpecPath -or
                        -not $resolvedTarget.StartsWith($specRootWithSeparator, [StringComparison]::OrdinalIgnoreCase) -or
                        -not (Test-Path -LiteralPath $resolvedTarget -PathType Leaf)) {
                        continue
                    }

                    $targetPackageName = Split-Path -Leaf (Split-Path -Parent $resolvedTarget)
                    if ($targetPackageName -match '^SPEC-\d{4}-[a-z0-9]+(?:-[a-z0-9]+)*$' -and
                        $targetPackageName -ne $package.Name) {
                        $targetSpecContent = Get-Content -LiteralPath $resolvedTarget -Raw
                        $targetStatusMatch = [regex]::Match($targetSpecContent, '(?m)^- 状态：`(?<status>[^`]+)`\r?$')
                        $targetApplicabilityMatch = [regex]::Match($targetSpecContent, '(?m)^- 当前适用性：`(?<applicability>[^`]+)`\r?$')
                        $targetStatus = if ($targetStatusMatch.Success) { $targetStatusMatch.Groups['status'].Value } else { $null }
                        $targetApplicability = if ($targetApplicabilityMatch.Success) {
                            $targetApplicabilityMatch.Groups['applicability'].Value
                        }
                        else {
                            $null
                        }
                        if ($targetStatus -in @('Approved', 'Implementing', 'Verifying', 'Implemented') -and
                            $targetApplicability -in @('Current', 'Partial')) {
                            $hasReplacementSpecLink = $true
                            break
                        }
                    }
                }

                if (-not $hasReplacementSpecLink) {
                    Add-VerificationError "docs/specs/$($package.Name)/spec.md current applicability '$specApplicability' must link to another SPEC spec.md in '## 当前适用范围'."
                }
            }
        }

        $planStatusMatch = [regex]::Match($planContent, '(?m)^- 状态：`(?<status>[^`]+)`\r?$')
        if (-not $planStatusMatch.Success -or $planStatusMatch.Groups['status'].Value -notin $planStatuses) {
            Add-VerificationError "docs/specs/$($package.Name)/plan.md has an invalid Plan status."
        }
        $planStatus = if ($planStatusMatch.Success) { $planStatusMatch.Groups['status'].Value } else { $null }

        $requirementMatches = [regex]::Matches($specContent, '(?m)^### (?<id>(?:FR|NFR)-\d{3})：')
        foreach ($match in [regex]::Matches($specContent, '(?m)^### (?<id>(?:FR|NFR)-\d{3}):')) {
            Add-VerificationError "docs/specs/$($package.Name)/spec.md requirement $($match.Groups['id'].Value) must use the full-width colon '：'."
        }
        $requirements = @($requirementMatches | ForEach-Object { $_.Groups['id'].Value } | Sort-Object -Unique)
        if ($requirements.Count -eq 0) {
            Add-VerificationError "docs/specs/$($package.Name)/spec.md defines no FR/NFR requirements."
        }

        foreach ($artifact in @{
                'plan.md' = $planContent
                'tasks.md' = $tasksContent
                'verification.md' = $verificationContent
            }.GetEnumerator()) {
            $references = [regex]::Matches($artifact.Value, '\b(?:FR|NFR)-\d{3}\b') |
                ForEach-Object { $_.Value } |
                Sort-Object -Unique
            foreach ($reference in $references) {
                if ($reference -notin $requirements) {
                    Add-VerificationError "docs/specs/$($package.Name)/$($artifact.Key) references undefined requirement $reference."
                }
            }
        }

        $taskIdMatches = [regex]::Matches($tasksContent, '(?m)^## (?<id>T\d{3})：')
        foreach ($match in [regex]::Matches($tasksContent, '(?m)^## (?<id>T\d{3}):')) {
            Add-VerificationError "docs/specs/$($package.Name)/tasks.md task $($match.Groups['id'].Value) must use the full-width colon '：'."
        }
        $taskIds = @($taskIdMatches | ForEach-Object { $_.Groups['id'].Value })
        if (@($taskIds | Sort-Object -Unique).Count -ne $taskIds.Count) {
            Add-VerificationError "docs/specs/$($package.Name)/tasks.md contains duplicate task IDs."
        }

        $taskSections = [regex]::Matches($tasksContent, '(?ms)^## (?<id>T\d{3})：.*?(?=^## T\d{3}：|\z)')
        $taskRequirementReferences = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($taskSection in $taskSections) {
            $coverageMatch = [regex]::Match($taskSection.Value, '(?m)^- 覆盖需求：(?<value>.+)$')
            $taskRequirements = if ($coverageMatch.Success) {
                [regex]::Matches($coverageMatch.Groups['value'].Value, '\b(?:FR|NFR)-\d{3}\b') |
                ForEach-Object { $_.Value } |
                Sort-Object -Unique
            }
            else {
                @()
            }
            if (@($taskRequirements).Count -eq 0) {
                Add-VerificationError "docs/specs/$($package.Name)/tasks.md task $($taskSection.Groups['id'].Value) has no requirement source."
            }
            foreach ($taskRequirement in $taskRequirements) {
                [void]$taskRequirementReferences.Add($taskRequirement)
            }
        }

        $statusMatches = [regex]::Matches($tasksContent, '(?m)^- 状态：`(?<status>[^`]+)`\r?$')
        $inProgressCount = 0
        $allTasksCompleted = $statusMatches.Count -gt 0
        foreach ($statusMatch in $statusMatches) {
            $status = $statusMatch.Groups['status'].Value
            if ($status -notin $taskStatuses) {
                Add-VerificationError "docs/specs/$($package.Name)/tasks.md uses invalid task status '$status'."
            }
            if ($status -eq 'InProgress') {
                $inProgressCount++
            }
            if ($status -ne 'Completed') {
                $allTasksCompleted = $false
            }
        }
        if ($inProgressCount -gt 1) {
            Add-VerificationError "docs/specs/$($package.Name)/tasks.md has more than one InProgress task."
        }

        $dependencyLines = [regex]::Matches($tasksContent, '(?m)^- 依赖：(?<value>.+)$')
        foreach ($dependencyLine in $dependencyLines) {
            $dependencyIds = [regex]::Matches($dependencyLine.Groups['value'].Value, '\bT\d{3}\b') |
                ForEach-Object { $_.Value }
            foreach ($dependencyId in $dependencyIds) {
                if ($dependencyId -notin $taskIds) {
                    Add-VerificationError "docs/specs/$($package.Name)/tasks.md depends on undefined task $dependencyId."
                }
            }
        }

        $verificationResultMatch = [regex]::Match($verificationContent, '(?m)^- 最终结果：`(?<result>Pending|Passed|Failed)`\r?$')
        if (-not $verificationResultMatch.Success) {
            Add-VerificationError "docs/specs/$($package.Name)/verification.md has a missing or invalid final result."
        }
        $verificationResult = if ($verificationResultMatch.Success) { $verificationResultMatch.Groups['result'].Value } else { $null }

        if ($specStatus -eq 'Implemented' -or $verificationResult -eq 'Passed') {
            Test-CurrentDocumentReview -PackageName $package.Name -PlanContent $planContent -Content $verificationContent
        }

        $verificationRowPattern = [regex]'(?m)^\|\s*(?<requirement>(?:FR|NFR)-\d{3})\s*\|\s*(?<implementation>[^|]*)\|\s*(?<tests>[^|]*)\|\s*(?<documentation>[^|]*)\|\s*(?<result>[^|]*)\|\s*$'
        $verificationRows = @{}
        foreach ($verificationRowMatch in $verificationRowPattern.Matches($verificationContent)) {
            $requirementId = $verificationRowMatch.Groups['requirement'].Value
            if ($verificationRows.ContainsKey($requirementId)) {
                Add-VerificationError "docs/specs/$($package.Name)/verification.md contains duplicate evidence rows for $requirementId."
                continue
            }

            $verificationRows[$requirementId] = @{
                Implementation = $verificationRowMatch.Groups['implementation'].Value.Trim()
                Tests = $verificationRowMatch.Groups['tests'].Value.Trim()
                Documentation = $verificationRowMatch.Groups['documentation'].Value.Trim()
                Result = $verificationRowMatch.Groups['result'].Value.Trim()
            }
        }

        foreach ($requirement in $requirements) {
            if ($planContent -notmatch "\b$([regex]::Escape($requirement))\b") {
                Add-VerificationError "docs/specs/$($package.Name)/plan.md does not cover declared requirement $requirement."
            }
            if (-not $taskRequirementReferences.Contains($requirement)) {
                Add-VerificationError "docs/specs/$($package.Name)/tasks.md has no task for declared requirement $requirement."
            }
            if (-not $verificationRows.ContainsKey($requirement)) {
                Add-VerificationError "docs/specs/$($package.Name)/verification.md has no evidence row for declared requirement $requirement."
                continue
            }

            $evidence = $verificationRows[$requirement]
            if ($evidence.Result -notin @('Pending', 'Passed', 'Failed')) {
                Add-VerificationError "docs/specs/$($package.Name)/verification.md has an invalid evidence result for $requirement."
            }
            if ($specStatus -eq 'Implemented' -and
                ($evidence.Result -ne 'Passed' -or
                    [string]::IsNullOrWhiteSpace($evidence.Implementation) -or
                    [string]::IsNullOrWhiteSpace($evidence.Tests) -or
                    [string]::IsNullOrWhiteSpace($evidence.Documentation))) {
                Add-VerificationError "docs/specs/$($package.Name)/verification.md has incomplete Passed evidence for requirement $requirement."
            }
        }

        if ($planStatus -in @('Approved', 'Superseded')) {
            $planApprover = [regex]::Match($planContent, '(?m)^- 批准人：(?<value>.+)$')
            $planApprovalDate = [regex]::Match($planContent, '(?m)^- 批准日期：(?<value>.+)$')
            if (-not $planApprover.Success -or $planApprover.Groups['value'].Value.Trim() -eq '—' -or
                -not $planApprovalDate.Success -or $planApprovalDate.Groups['value'].Value.Trim() -eq '—') {
                Add-VerificationError "docs/specs/$($package.Name)/plan.md Approved Plan lacks approval metadata."
            }

            $baselineMatch = [regex]::Match($planContent, '(?m)^- Spec 基线提交：`(?<commit>[0-9a-fA-F]{7,40})`\r?$')
            if (-not $baselineMatch.Success) {
                Add-VerificationError "docs/specs/$($package.Name)/plan.md Approved Plan lacks a Spec baseline commit."
            }
            else {
                $baselineCommit = $baselineMatch.Groups['commit'].Value
                $relativeSpecPath = Get-RepositoryRelativePath (Join-Path $package.FullName 'spec.md')
                $baselineSpec = & git -C $repositoryRoot show "${baselineCommit}:$relativeSpecPath" 2>$null | Out-String
                if ($LASTEXITCODE -ne 0) {
                    Add-VerificationError "docs/specs/$($package.Name)/plan.md Spec baseline commit '$baselineCommit' cannot be resolved."
                }
                elseif ($baselineSpec -notmatch '(?m)^- 状态：`Approved`\r?$') {
                    Add-VerificationError "docs/specs/$($package.Name)/plan.md Spec baseline does not contain an Approved Spec."
                }
            }
        }

        if ($specStatus -in @('Implementing', 'Verifying', 'Implemented') -and $planStatus -ne 'Approved') {
            Add-VerificationError "docs/specs/$($package.Name) $specStatus Spec requires an Approved Plan."
        }
        if ($specStatus -in @('Verifying', 'Implemented') -and -not $allTasksCompleted) {
            Add-VerificationError "docs/specs/$($package.Name) $specStatus Spec requires every Task to be Completed."
        }
        if ($specStatus -eq 'Implemented' -and $verificationResult -ne 'Passed') {
            Add-VerificationError "docs/specs/$($package.Name) Implemented Spec requires Verification to be Passed."
        }

        if ($specStatusMatch.Success -and $specStatusMatch.Groups['status'].Value -in @('Approved', 'Implementing', 'Verifying', 'Implemented')) {
            if ($specContent -match '(?im)\b(TODO|TBD)\b|待定|尚未决定') {
                Add-VerificationError "docs/specs/$($package.Name)/spec.md is approved or later but still contains a placeholder."
            }
            if ($specContent -match '(?m)^- 规格批准：—\r?$|^- 批准日期：—\r?$') {
                Add-VerificationError "docs/specs/$($package.Name)/spec.md is approved or later but lacks approval metadata."
            }
        }
    }

    foreach ($indexNumber in $specIndexEntries.Keys) {
        if (-not $seenNumbers.ContainsKey($indexNumber)) {
            Add-VerificationError "Spec index entry SPEC-$indexNumber has no matching package."
        }
    }
}

Test-TrackedTemporaryArtifacts
Test-MarkdownLinks
Test-AdrIndex
Test-SpecPackages

if ($errors.Count -gt 0) {
    Write-Host "Documentation verification failed with $($errors.Count) error(s):" -ForegroundColor Red
    foreach ($verificationError in $errors) {
        Write-Host "- $verificationError" -ForegroundColor Red
    }
    exit 1
}

Write-Host 'Documentation verification passed.' -ForegroundColor Green
