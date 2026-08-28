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
            $_.FullName -notmatch '[\\/](bin|obj|\.git|_site)[\\/]' -and
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

function Test-SpecPackages {
    $specRoot = Join-Path $repositoryRoot 'docs/specs'
    if (-not (Test-Path -LiteralPath $specRoot)) {
        Add-VerificationError 'docs/specs is missing.'
        return
    }

    $specIndexPath = Join-Path $specRoot 'README.md'
    $specIndexContent = Get-Content -LiteralPath $specIndexPath -Raw
    $specIndexPattern = [regex]'(?m)^\| \[SPEC-(?<number>\d{4})\]\((?<target>[^)]+)\) \|.*\| `(?<status>Draft|Clarifying|Approved|Implementing|Verifying|Implemented|Superseded)` \|\r?$'
    $specIndexEntries = @{}
    foreach ($match in $specIndexPattern.Matches($specIndexContent)) {
        $indexNumber = $match.Groups['number'].Value
        if ($specIndexEntries.ContainsKey($indexNumber)) {
            Add-VerificationError "docs/specs/README.md contains duplicate SPEC-$indexNumber."
            continue
        }

        $specIndexEntries[$indexNumber] = @{
            Target = $match.Groups['target'].Value
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

        $declaredNumber = [regex]::Match($specContent, '(?m)^- 编号：`SPEC-(?<number>\d{4})`$')
        if (-not $declaredNumber.Success -or $declaredNumber.Groups['number'].Value -ne $number) {
            Add-VerificationError "docs/specs/$($package.Name)/spec.md has a missing or mismatched Spec number."
        }

        $specStatusMatch = [regex]::Match($specContent, '(?m)^- 状态：`(?<status>[^`]+)`$')
        if (-not $specStatusMatch.Success -or $specStatusMatch.Groups['status'].Value -notin $specStatuses) {
            Add-VerificationError "docs/specs/$($package.Name)/spec.md has an invalid Spec status."
        }
        $specStatus = if ($specStatusMatch.Success) { $specStatusMatch.Groups['status'].Value } else { $null }

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
        }

        $planStatusMatch = [regex]::Match($planContent, '(?m)^- 状态：`(?<status>[^`]+)`$')
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

        $statusMatches = [regex]::Matches($tasksContent, '(?m)^- 状态：`(?<status>[^`]+)`$')
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

        $verificationResultMatch = [regex]::Match($verificationContent, '(?m)^- 最终结果：`(?<result>Pending|Passed|Failed)`$')
        if (-not $verificationResultMatch.Success) {
            Add-VerificationError "docs/specs/$($package.Name)/verification.md has a missing or invalid final result."
        }
        $verificationResult = if ($verificationResultMatch.Success) { $verificationResultMatch.Groups['result'].Value } else { $null }

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

            $baselineMatch = [regex]::Match($planContent, '(?m)^- Spec 基线提交：`(?<commit>[0-9a-fA-F]{7,40})`$')
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
            if ($specContent -match '(?m)^- 规格批准：—$|^- 批准日期：—$') {
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
