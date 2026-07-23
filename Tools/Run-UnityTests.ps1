param(
    [ValidateSet("editmode", "playmode")]
    [string]$TestPlatform = "editmode",

    [string]$TestFilter = "",

    [string]$Scope = "targeted",

    [string]$ArtifactName = "",

    [string]$UnityEditorPath = "",

    [string]$ProjectPath = "",

    [string]$ArtifactRoot = ""
)

$ErrorActionPreference = "Stop"

function Get-ScriptRoot {
    return $PSScriptRoot
}

function Resolve-UnityEditorPath {
    param(
        [string]$RequestedPath
    )

    if (-not [string]::IsNullOrWhiteSpace($RequestedPath)) {
        if (-not (Test-Path $RequestedPath)) {
            throw "Unity editor path does not exist: $RequestedPath"
        }

        return (Resolve-Path $RequestedPath).Path
    }

    if (-not [string]::IsNullOrWhiteSpace($env:UNITY_EDITOR_PATH) -and (Test-Path $env:UNITY_EDITOR_PATH)) {
        return (Resolve-Path $env:UNITY_EDITOR_PATH).Path
    }

    $hubRoot = "C:\Program Files\Unity\Hub\Editor"
    if (-not (Test-Path $hubRoot)) {
        throw "Could not find Unity Hub editor directory at $hubRoot. Pass -UnityEditorPath explicitly."
    }

    $editor = Get-ChildItem $hubRoot -Directory |
        Sort-Object @{
            Expression = {
                $match = [regex]::Match($_.Name, "^\d+\.\d+\.\d+")
                if ($match.Success) {
                    return [version]$match.Value
                }

                return [version]"0.0.0"
            }
        } -Descending |
        ForEach-Object { Join-Path $_.FullName "Editor\Unity.exe" } |
        Where-Object { Test-Path $_ } |
        Select-Object -First 1

    if (-not $editor) {
        throw "Could not find Unity.exe under $hubRoot. Pass -UnityEditorPath explicitly."
    }

    return $editor
}

function Resolve-ProjectPath {
    param(
        [string]$RequestedPath
    )

    if (-not [string]::IsNullOrWhiteSpace($RequestedPath)) {
        if (-not (Test-Path $RequestedPath)) {
            throw "Project path does not exist: $RequestedPath"
        }

        return (Resolve-Path $RequestedPath).Path
    }

    $defaultProjectPath = Join-Path (Get-ScriptRoot) ".."
    return (Resolve-Path $defaultProjectPath).Path
}

function Resolve-ArtifactRoot {
    param(
        [string]$RequestedPath
    )

    if (-not [string]::IsNullOrWhiteSpace($RequestedPath)) {
        $artifactRootPath = $RequestedPath
    }
    else {
        $artifactRootPath = Join-Path (Get-ScriptRoot) "..\..\TestResults"
    }

    New-Item -ItemType Directory -Force -Path $artifactRootPath | Out-Null
    return (Resolve-Path $artifactRootPath).Path
}

function Get-SafeArtifactToken {
    param(
        [string]$Value,
        [string]$Fallback
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $Fallback
    }

    $safeValue = $Value -replace "[^A-Za-z0-9._-]+", "-"
    $safeValue = $safeValue.Trim("-")

    if ([string]::IsNullOrWhiteSpace($safeValue)) {
        return $Fallback
    }

    return $safeValue
}

function Get-XmlAttributeValue {
    param(
        [System.Xml.XmlElement]$Element,
        [string]$Name
    )

    if ($null -eq $Element) {
        return $null
    }

    if ($Element.HasAttribute($Name)) {
        return $Element.GetAttribute($Name)
    }

    return $null
}

function Convert-UnityXmlToSummary {
    param(
        [string]$XmlPath
    )

    if (-not (Test-Path $XmlPath)) {
        return $null
    }

    try {
        [xml]$xml = Get-Content -Path $XmlPath
    }
    catch {
        return @{
            parseError = $_.Exception.Message
        }
    }

    $root = $xml.SelectSingleNode("/test-run")
    if ($null -eq $root) {
        $root = $xml.DocumentElement
    }

    $rootElement = [System.Xml.XmlElement]$root
    $testCases = @($xml.SelectNodes("//test-case"))

    $summary = [ordered]@{
        xmlPath = $XmlPath
        result = Get-XmlAttributeValue -Element $rootElement -Name "result"
        total = Get-XmlAttributeValue -Element $rootElement -Name "total"
        passed = Get-XmlAttributeValue -Element $rootElement -Name "passed"
        failed = Get-XmlAttributeValue -Element $rootElement -Name "failed"
        inconclusive = Get-XmlAttributeValue -Element $rootElement -Name "inconclusive"
        skipped = Get-XmlAttributeValue -Element $rootElement -Name "skipped"
        duration = Get-XmlAttributeValue -Element $rootElement -Name "duration"
    }

    if ($testCases.Count -gt 0) {
        $summary["tests"] = @(
            $testCases | ForEach-Object {
                [ordered]@{
                    name = $_.GetAttribute("fullname")
                    result = $_.GetAttribute("result")
                    duration = $_.GetAttribute("duration")
                }
            }
        )
    }

    return $summary
}

function Get-UnityLogSummary {
    param(
        [string[]]$LogPaths
    )

    $existingLogPaths = @($LogPaths | Where-Object { -not [string]::IsNullOrWhiteSpace($_) -and (Test-Path $_) })
    if ($existingLogPaths.Count -eq 0) {
        return $null
    }

    $content = foreach ($path in $existingLogPaths) {
        Get-Content -Path $path
    }
    $diagnosticLines = @(
        $content |
            Select-String -Pattern "Aborting batchmode due to fatal error", "Multiple Unity instances cannot open the same project", "error CS[0-9]+", "(?i)exception", "(?i)failed" |
            Select-Object -ExpandProperty Line -Unique |
            Select-Object -First 20
    )

    $failureCategory = $null
    if ($content -match "Multiple Unity instances cannot open the same project") {
        $failureCategory = "project_lock"
    }
    elseif ($content -match "error CS[0-9]+") {
        $failureCategory = "compile_error"
    }
    elseif ($content -match "(?i)exception") {
        $failureCategory = "runtime_exception"
    }
    elseif ($content -match "(?i)failed") {
        $failureCategory = "test_or_runner_failure"
    }

    if ($null -eq $failureCategory -and $diagnosticLines.Count -eq 0) {
        return $null
    }

    return [ordered]@{
        failureCategory = $failureCategory
        diagnosticLines = $diagnosticLines
    }
}

$resolvedUnityEditorPath = Resolve-UnityEditorPath -RequestedPath $UnityEditorPath
$resolvedProjectPath = Resolve-ProjectPath -RequestedPath $ProjectPath
$resolvedArtifactRoot = Resolve-ArtifactRoot -RequestedPath $ArtifactRoot
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

$artifactToken = Get-SafeArtifactToken -Value $ArtifactName -Fallback (
    Get-SafeArtifactToken -Value $TestFilter -Fallback $Scope
)

$artifactBase = "$timestamp-$TestPlatform-$artifactToken"
$xmlPath = Join-Path $resolvedArtifactRoot "$artifactBase.xml"
$logPath = Join-Path $resolvedArtifactRoot "$artifactBase.log"
$stdoutPath = Join-Path $resolvedArtifactRoot "$artifactBase.stdout.log"
$stderrPath = Join-Path $resolvedArtifactRoot "$artifactBase.stderr.log"
$jsonPath = Join-Path $resolvedArtifactRoot "$artifactBase.json"
$lockFilePath = Join-Path $resolvedProjectPath "Temp\UnityLockfile"

$unityArguments = @(
    "-batchmode",
    "-nographics",
    "-quit",
    "-projectPath", $resolvedProjectPath,
    "-runTests",
    "-testPlatform", $TestPlatform,
    "-testResults", $xmlPath,
    "-logFile", $logPath
)

if (-not [string]::IsNullOrWhiteSpace($TestFilter)) {
    $unityArguments += @("-testFilter", $TestFilter)
}

Write-Host "Running Unity tests..."
Write-Host "  Scope: $Scope"
Write-Host "  Platform: $TestPlatform"
if (-not [string]::IsNullOrWhiteSpace($TestFilter)) {
    Write-Host "  Filter: $TestFilter"
}
Write-Host "  Unity: $resolvedUnityEditorPath"
Write-Host "  Project: $resolvedProjectPath"
Write-Host "  XML: $xmlPath"
Write-Host "  Log: $logPath"
Write-Host "  StdOut: $stdoutPath"
Write-Host "  StdErr: $stderrPath"

$preflightWarnings = @()
if (Test-Path $lockFilePath) {
    $preflightWarnings += "Project lockfile exists at $lockFilePath. Batch tests can fail while the Unity editor is already open."
}

$process = Start-Process -FilePath $resolvedUnityEditorPath -ArgumentList $unityArguments -Wait -NoNewWindow -PassThru -RedirectStandardOutput $stdoutPath -RedirectStandardError $stderrPath
$testSummary = Convert-UnityXmlToSummary -XmlPath $xmlPath
$logSummary = Get-UnityLogSummary -LogPaths @($logPath, $stdoutPath, $stderrPath)
$status = if ($process.ExitCode -eq 0) { "passed" } else { "failed" }

$report = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    status = $status
    exitCode = $process.ExitCode
    scope = $Scope
    testPlatform = $TestPlatform
    testFilter = if ([string]::IsNullOrWhiteSpace($TestFilter)) { $null } else { $TestFilter }
    unityEditorPath = $resolvedUnityEditorPath
    projectPath = $resolvedProjectPath
    artifacts = [ordered]@{
        xml = $xmlPath
        log = $logPath
        stdout = $stdoutPath
        stderr = $stderrPath
        json = $jsonPath
    }
    preflightWarnings = $preflightWarnings
    notes = @()
}

if ($null -ne $testSummary) {
    $report["unityXmlSummary"] = $testSummary
}
else {
    $report.notes += "Unity did not produce an XML result file."
}

if (-not (Test-Path $logPath)) {
    $report.notes += "Unity did not produce a log file."
}
elseif ($null -ne $logSummary) {
    $report["logSummary"] = $logSummary

    if ($logSummary.failureCategory -eq "project_lock") {
        $report.notes += "The Unity project is already open in another running Unity instance."
    }
    elseif ($logSummary.failureCategory -eq "compile_error") {
        $report.notes += "Unity reported compile errors before the test runner could finish."
    }
}

if ($process.ExitCode -ne 0) {
    $report.notes += "Unity returned a non-zero exit code. Check the log artifact for details."
}

$report | ConvertTo-Json -Depth 8 | Set-Content -Path $jsonPath -Encoding ASCII

Write-Host "Finished Unity tests."
Write-Host "  Status: $status"
Write-Host "  ExitCode: $($process.ExitCode)"
Write-Host "  JSON: $jsonPath"

if ($null -ne $testSummary) {
    Write-Host "  XML Result: $($testSummary.result)"
    Write-Host "  Total: $($testSummary.total) Passed: $($testSummary.passed) Failed: $($testSummary.failed) Skipped: $($testSummary.skipped)"
}

exit $process.ExitCode
