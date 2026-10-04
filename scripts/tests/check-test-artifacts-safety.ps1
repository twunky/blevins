<#
.SYNOPSIS
Repeatable safety checks for scripts/test-artifacts.ps1 (WI-009-002 AC-19). Drops nothing.

.DESCRIPTION
Exercises the refusal and limitation cases of WI-009-002 AC-04 to AC-08, AC-12 and AC-13 against temporary repository
layouts created under %TEMP% with hand-made manifests. test-artifacts.ps1 is pointed at them with -RepositoryRoot; it
still queries the real local SQL Server (read-only) and uses the real PRE-EXISTING list and removal script.

Every database name used here is invented and does not exist, every destructive case is expected to be refused or to
touch only the temporary folder, and the FamilySafety* database list is compared before and after. The repository
itself, ai-context/, and the root "trips" file are never written. The temporary folder is removed at the end.

Cases that need a real retained database (the positive -RemoveDatabase path, "holding databases" run folders, and the
"exists" reconciliation labels) are covered by the WI-009-002 single-spec verification runs instead.

Precondition: no Playwright run is active and port 5080 is free.
Exit codes: 0 = every check passed; 1 = at least one check failed; 3 = precondition or setup failure.
#>
param(
    [string]$Server = 'localhost'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$tool = Join-Path $repositoryRoot 'scripts\test-artifacts.ps1'
$lister = Join-Path $repositoryRoot 'scripts\list-scenario-databases.ps1'
$work = Join-Path ([System.IO.Path]::GetTempPath()) ("fs-artifact-safety-" + [guid]::NewGuid().ToString('N').Substring(0, 8))
$script:failures = 0
$script:passes = 0
$fakeRunnerProcess = $null
$heldPort = $null
$lockedFiles = New-Object System.Collections.Generic.List[System.IO.FileStream]

function Note([string]$text) { Write-Host "  $text" }

function Check([string]$name, [bool]$condition, [string]$detail = '') {
    if ($condition) { $script:passes++; Write-Host "PASS  $name" }
    else { $script:failures++; Write-Host "FAIL  $name$(if ($detail) { " -- $detail" })" }
}

function Invoke-Tool([string]$root, [string[]]$arguments, [string]$sqlServer = $Server) {
    $all = @('-NoLogo', '-NoProfile', '-NonInteractive', '-ExecutionPolicy', 'Bypass', '-File', $tool, '-RepositoryRoot', $root, '-Server', $sqlServer) + $arguments
    $ErrorActionPreference = 'Continue'   # A parameter-binding error on stderr is an expected result, not a failure here.
    $output =& powershell.exe @all 2>&1 | ForEach-Object { "$_" }
    return [pscustomobject]@{ Code = $LASTEXITCODE; Text = ($output -join "`n") }
}

function Get-TreeHash([string]$path) {
    # Names and contents of everything below $path, without following links.
    if (-not (Test-Path -LiteralPath $path)) { return 'ABSENT' }
    $lines = foreach ($item in @(Get-ChildItem -LiteralPath $path -Recurse -Force -Attributes !ReparsePoint | Sort-Object FullName)) {
        if ($item.PSIsContainer) { "D $($item.FullName.Substring($path.Length))" }
        else { "F $($item.FullName.Substring($path.Length)) $((Get-FileHash -LiteralPath $item.FullName -Algorithm SHA256).Hash)" }
    }
    return ($lines -join "`n")
}

function New-Layout([string]$name) {
    # A minimal copy of the repository layout that test-artifacts.ps1 accepts as a root.
    $root = Join-Path $work $name
    foreach ($folder in 'src\FamilySafety.Web\e2e\scenario-runs', 'ai-context\planning', 'claude-handoff', 'codex-handoff', 'docs', 'scripts') {
        New-Item -ItemType Directory -Force (Join-Path $root $folder) | Out-Null
    }
    Set-Content -LiteralPath (Join-Path $root 'src\FamilySafety.Web\e2e\playwright.config.ts') -Value '// safety-check placeholder' -Encoding utf8
    Set-Content -LiteralPath (Join-Path $root 'README.md') -Value '# safety-check placeholder' -Encoding utf8
    Set-Content -LiteralPath (Join-Path $root 'ai-context\planning\notes.md') -Value 'No run is cited here.' -Encoding utf8
    return $root
}

function New-RunId {
    $stamp = (Get-Date).ToUniversalTime().AddSeconds(- (Get-Random -Maximum 100000)).ToString('yyyyMMddTHHmmss')
    return "$stamp-$([guid]::NewGuid().ToString('N').Substring(0, 8))"
}

# Random hex like real names: the citation search also matches a name's first 8 characters, so they must differ.
function New-FakeName { return "FamilySafety_Invitation_E2E_$([guid]::NewGuid().ToString('N'))" }

function New-Entry([string]$name, [string]$status, [string]$outcome = 'failed') {
    return [ordered]@{ name = $name; spec = 'safety-check.spec.ts'; scenario = @{}; createdAt = '2026-10-02T06:00:00.000Z'
        apiPid = $null; apiLog = "$name.api.log"; outcome = $outcome; status = $status }
}

function New-Run([string]$root, [object[]]$entries, [switch]$Completed, $RunnerPid = $null, [string[]]$Snapshot = @('FamilySafety'), [string]$RunId = (New-RunId)) {
    $folder = Join-Path $root "src\FamilySafety.Web\e2e\scenario-runs\$RunId"
    New-Item -ItemType Directory -Force $folder | Out-Null
    $manifest = [ordered]@{ runId = $RunId; createdAt = '2026-10-02T06:00:00.000Z'; server = $Server; preExistingListPath = 'n/a'
        preExistingAtRunStart = [object[]]$Snapshot; databases = [object[]]$entries }
    if ($null -ne $RunnerPid) { $manifest.runnerPid = $RunnerPid }
    if ($Completed) { $manifest.completedAt = '2026-10-02T06:05:00.000Z' }
    [System.IO.File]::WriteAllText((Join-Path $folder 'manifest.json'), (ConvertTo-Json -InputObject $manifest -Depth 10), (New-Object System.Text.UTF8Encoding $false))
    foreach ($entry in $entries) { Set-Content -LiteralPath (Join-Path $folder "$($entry.name).api.log") -Value 'log' -Encoding utf8 }
    return [pscustomobject]@{ RunId = $RunId; Folder = $folder; Manifest = (Join-Path $folder 'manifest.json') }
}

function Get-ServerDatabases {
    $names = & powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $lister -Server $Server
    if ($LASTEXITCODE -ne 0) { throw 'list-scenario-databases.ps1 failed.' }
    return (@($names) -join "`n")
}

function Test-PortFree([int]$port) {
    $listener = New-Object System.Net.Sockets.TcpListener ([System.Net.IPAddress]::Loopback, $port)
    try { $listener.Start(); return $true } catch { return $false } finally { $listener.Stop() }
}

# ---- Preconditions -----------------------------------------------------------------------------------------------

Write-Host "WI-009-002 test-artifacts safety checks ($work)"
if (-not (Test-PortFree 5080)) { Write-Host 'PRECONDITION: port 5080 is in use; a run may be active. Nothing was checked.'; exit 3 }
try { $databasesBefore = Get-ServerDatabases }
catch { Write-Host "PRECONDITION: SQL Server '$Server' cannot be queried: $($_.Exception.Message)"; exit 3 }
$tripsPath = Join-Path $repositoryRoot 'trips'
$tripsBefore = if (Test-Path -LiteralPath $tripsPath) { (Get-FileHash -LiteralPath $tripsPath -Algorithm SHA256).Hash } else { 'ABSENT' }
$aiContextBefore = Get-TreeHash (Join-Path $repositoryRoot 'ai-context')
$realRunsBefore = Get-TreeHash (Join-Path $repositoryRoot 'src\FamilySafety.Web\e2e\scenario-runs')
New-Item -ItemType Directory -Force $work | Out-Null

try {
    # ---- AC-05: exact selection only --------------------------------------------------------------------------
    $root = New-Layout 'selection'
    foreach ($bad in @('FamilySafety_Invitation_E2E_*', 'FamilySafety_Invitation_E2E_ab?d', 'FamilySafety_Invitation_E2E_%', 'FamilySafety_Invitation_E2E_[ab]', 'FamilySafety_Invitation_E2E_]')) {
        $result = Invoke-Tool $root @('-RemoveDatabase', $bad, '-Execute')
        Check "AC-05 wildcard rejected: $bad" ($result.Code -eq 2 -and $result.Text -match 'wildcard') $result.Text
    }
    $result = Invoke-Tool $root @('-RemoveDatabase', '-Execute')
    Check 'AC-05 -RemoveDatabase without a name is refused' ($result.Code -ne 0) $result.Text
    $result = Invoke-Tool $root @('-RemoveAllRetained', '-Execute')
    Check 'AC-05 no "all retained" option exists (parameter rejected)' ($result.Code -ne 0 -and $result.Text -match 'parameter') $result.Text
    $toolText = Get-Content -LiteralPath $tool -Raw
    Check 'AC-05 test-artifacts.ps1 contains no DROP statement' (-not ($toolText -match '(?im)^(?!\s*#).*\bDROP\s+DATABASE\b') -and -not ($toolText -match '(?i)ExecuteNonQuery'))
    Check 'AC-05 test-artifacts.ps1 delegates to remove-scenario-database.ps1 -RemoveRetained' ($toolText -match 'removalScript' -and $toolText -match '-RemoveRetained')
    $result = Invoke-Tool $root @('-RemoveRunRecords', '-RunId', '2026*', '-Execute')
    Check 'AC-12 wildcard run ID rejected' ($result.Code -eq 2) $result.Text
    $result = Invoke-Tool $root @('-RemoveRunRecords', '-RunId', '20261002T0611', '-Execute')
    Check 'AC-12 partial run ID rejected' ($result.Code -eq 2) $result.Text
    $result = Invoke-Tool $root @('-Execute')
    Check '-Execute without an action is refused' ($result.Code -eq 2) $result.Text
    $direct = & powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $tool -Execute 2>&1 | ForEach-Object { "$_" }
    Check 'Default repository root resolves when -RepositoryRoot is omitted' ($LASTEXITCODE -eq 2 -and ($direct -join "`n") -match '-Execute needs an action') ($direct -join "`n")
    $result = Invoke-Tool $root @('-RemoveRunRecords', '-ClearGenerated', '-Execute')
    Check 'Two actions at once are refused' ($result.Code -eq 2) $result.Text

    # ---- AC-06: protected names -------------------------------------------------------------------------------
    $root = New-Layout 'protected'
    $snapshotName = New-FakeName
    $claimed = New-Run $root @((New-Entry 'FamilySafety_VisualReview' 'retained')) -Completed
    $snapshotRun = New-Run $root @((New-Entry $snapshotName 'retained')) -Completed -Snapshot @('FamilySafety', $snapshotName)
    foreach ($form in @(@(), @('-Execute'))) {
        $suffix = if ($form.Count) { '-Execute' } else { 'dry run' }
        $result = Invoke-Tool $root (@('-RemoveDatabase', 'FamilySafety') + $form)
        Check "AC-06 FamilySafety refused ($suffix)" ($result.Code -eq 2 -and $result.Text -match 'FamilySafety is never removed') $result.Text
        $result = Invoke-Tool $root (@('-RemoveDatabase', 'familysafety') + $form)
        Check "AC-06 familysafety (case variant) refused ($suffix)" ($result.Code -eq 2) $result.Text
        $result = Invoke-Tool $root (@('-RemoveDatabase', 'FamilySafety_VisualReview') + $form)
        Check "AC-06 PRE-EXISTING FamilySafety_VisualReview refused despite a hand-made manifest ($suffix)" ($result.Code -eq 2) $result.Text
        $result = Invoke-Tool $root (@('-RemoveDatabase', $snapshotName) + $form)
        Check "AC-06 name in the run-start snapshot refused ($suffix)" ($result.Code -eq 2 -and $result.Text -match 'already existed when run') $result.Text
        $result = Invoke-Tool $root (@('-RemoveDatabase', "FamilySafety_Invitation_E2E_ok,FamilySafety") + $form)
        Check "AC-06 a list containing FamilySafety is refused as a whole ($suffix)" ($result.Code -eq 2) $result.Text
    }
    $result = Invoke-Tool $root @()
    Check 'AC-06 status report marks the hand-made PRE-EXISTING claim invalid (not a scenario name)' ($result.Code -eq 0 -and $result.Text -match [regex]::Escape($claimed.RunId) -and $result.Text -match 'invalid') $result.Text

    # ---- AC-07: unproven ownership ----------------------------------------------------------------------------
    $root = New-Layout 'ownership'
    $noManifestName = New-FakeName
    $result = Invoke-Tool $root @('-RemoveDatabase', $noManifestName, '-Execute')
    Check 'AC-07 name with no manifest refused' ($result.Code -eq 2 -and $result.Text -match 'unknown ownership') $result.Text

    $twiceName = New-FakeName
    New-Run $root @((New-Entry $twiceName 'retained')) -Completed | Out-Null
    New-Run $root @((New-Entry $twiceName 'retained')) -Completed | Out-Null
    $result = Invoke-Tool $root @('-RemoveDatabase', $twiceName, '-Execute')
    Check 'AC-07 name recorded in two manifests refused' ($result.Code -eq 2 -and $result.Text -match 'ambiguous') $result.Text

    $missingName = New-FakeName
    $missing = New-Run $root @((New-Entry $missingName 'retained')) -Completed
    Remove-Item -LiteralPath $missing.Manifest
    $result = Invoke-Tool $root @('-RemoveDatabase', $missingName, '-Execute')
    Check 'AC-07 missing manifest refused (its folder holds the API log)' ($result.Code -eq 2 -and $result.Text -match 'Ownership cannot be proven') $result.Text

    $malformedName = New-FakeName
    $malformed = New-Run $root @((New-Entry $malformedName 'retained')) -Completed
    [System.IO.File]::WriteAllText($malformed.Manifest, "{ `"runId`": `"$($malformed.RunId)`", `"databases`": [ { `"name`": `"$malformedName`" ")
    $result = Invoke-Tool $root @('-RemoveDatabase', $malformedName, '-Execute')
    Check 'AC-07 malformed manifest refused' ($result.Code -eq 2 -and $result.Text -match 'not valid JSON') $result.Text

    $inconsistentName = New-FakeName
    New-Run $root @((New-Entry $inconsistentName 'retained'), (New-Entry $inconsistentName 'removed')) -Completed | Out-Null
    $result = Invoke-Tool $root @('-RemoveDatabase', $inconsistentName, '-Execute')
    Check 'AC-07 internally inconsistent manifest (duplicate entry) refused' ($result.Code -eq 2 -and $result.Text -match 'recorded more than once') $result.Text

    $noStatusName = New-FakeName
    $noStatus = New-Entry $noStatusName 'retained'
    $noStatus.Remove('status')
    New-Run $root @($noStatus) -Completed | Out-Null
    $result = Invoke-Tool $root @('-RemoveDatabase', $noStatusName, '-Execute')
    Check 'AC-07 manifest entry without a status refused' ($result.Code -eq 2 -and $result.Text -match 'no valid status') $result.Text

    $unreadableName = New-FakeName
    $unreadable = New-Run $root @((New-Entry $unreadableName 'retained')) -Completed
    $lockedFiles.Add([System.IO.File]::Open($unreadable.Manifest, 'Open', 'ReadWrite', 'None'))
    $result = Invoke-Tool $root @('-RemoveDatabase', $unreadableName, '-Execute')
    Check 'AC-07 unreadable (locked) manifest refused' ($result.Code -eq 2 -and $result.Text -match 'could not be read') $result.Text
    $result = Invoke-Tool $root @('-RemoveRunRecords', '-RunId', $unreadable.RunId, '-Execute')
    Check 'AC-12 unreadable manifest folder refused even with -RunId' ($result.Code -eq 2 -and (Test-Path -LiteralPath $unreadable.Folder)) $result.Text
    $lockedFiles | ForEach-Object { $_.Dispose() }
    $lockedFiles.Clear()

    $result = Invoke-Tool $root @('-RemoveRunRecords', '-RunId', $malformed.RunId, '-Execute')
    Check 'AC-12 malformed manifest folder refused even with -RunId' ($result.Code -eq 2 -and (Test-Path -LiteralPath $malformed.Folder)) $result.Text

    # ---- AC-04: dry run by default ----------------------------------------------------------------------------
    $root = New-Layout 'dryrun'
    $resolvedRun = New-Run $root @((New-Entry (New-FakeName) 'removed' 'passed')) -Completed
    foreach ($folder in 'e2e\test-results\some-test', 'e2e\captures\concern-workflow', 'dist\verification\WI-000-000') {
        $path = Join-Path $root "src\FamilySafety.Web\$folder"
        New-Item -ItemType Directory -Force $path | Out-Null
        Set-Content -LiteralPath (Join-Path $path 'file.txt') -Value 'x' -Encoding utf8
    }
    $before = Get-TreeHash $root
    foreach ($arguments in @(@('-RemoveRunRecords'), @('-RemoveRunRecords', '-RunId', $resolvedRun.RunId), @('-ClearGenerated'))) {
        $result = Invoke-Tool $root $arguments
        Check "AC-04 dry run changes nothing: $($arguments -join ' ')" ($result.Code -eq 0 -and $result.Text -match 'DRY RUN' -and (Get-TreeHash $root) -eq $before) $result.Text
    }
    $result = Invoke-Tool $root @('-RemoveDatabase', 'FamilySafety_Invitation_E2E_doesnotexist0')
    Check 'AC-04 -RemoveDatabase dry run changes nothing' ($result.Code -eq 2 -and (Get-TreeHash $root) -eq $before) $result.Text
    $result = Invoke-Tool $root @()
    Check 'AC-04 status report changes nothing' ($result.Code -eq 0 -and (Get-TreeHash $root) -eq $before) $result.Text

    # ---- AC-08: active or unreachable --------------------------------------------------------------------------
    $root = New-Layout 'active'
    $activeName = New-FakeName
    New-Run $root @((New-Entry (New-FakeName) 'removed' 'passed')) -Completed | Out-Null
    $before = Get-TreeHash $root
    $result = Invoke-Tool $root @('-RemoveRunRecords', '-Execute') -sqlServer '127.0.0.1,1'
    Check 'AC-08 SQL Server unreachable: -RemoveRunRecords refused' ($result.Code -eq 2 -and $result.Text -match 'cannot be queried' -and (Get-TreeHash $root) -eq $before) $result.Text
    $result = Invoke-Tool $root @('-ClearGenerated', '-Execute') -sqlServer '127.0.0.1,1'
    Check 'AC-08 SQL Server unreachable: -ClearGenerated refused' ($result.Code -eq 2) $result.Text
    $result = Invoke-Tool $root @('-RemoveDatabase', $activeName, '-Execute') -sqlServer '127.0.0.1,1'
    Check 'AC-08 SQL Server unreachable: -RemoveDatabase refused' ($result.Code -eq 2) $result.Text
    $result = Invoke-Tool $root @() -sqlServer '127.0.0.1,1'
    Check 'AC-08 SQL Server unreachable: status reports it and every run counts as holding databases' ($result.Code -eq 0 -and $result.Text -match 'UNREACHABLE') $result.Text

    $heldPort = New-Object System.Net.Sockets.TcpListener ([System.Net.IPAddress]::Loopback, 5080)
    $heldPort.Start()
    foreach ($arguments in @(@('-RemoveRunRecords', '-Execute'), @('-ClearGenerated', '-Execute'), @('-RemoveDatabase', $activeName, '-Execute'))) {
        $result = Invoke-Tool $root $arguments
        Check "AC-08 port 5080 held: refused ($($arguments[0]))" ($result.Code -eq 2 -and $result.Text -match 'Port 5080 is in use' -and (Get-TreeHash $root) -eq $before) $result.Text
    }
    $heldPort.Stop()
    $heldPort = $null

    # A live process whose command line names Playwright stands in for a run's runner.
    $fakeRunnerProcess = Start-Process powershell.exe -ArgumentList '-NoProfile', '-Command', 'Start-Sleep -Seconds 300 # playwright safety-check runner' -WindowStyle Hidden -PassThru
    Start-Sleep -Milliseconds 500
    $activeRun = New-Run $root @((New-Entry $activeName 'running' 'pending')) -RunnerPid $fakeRunnerProcess.Id
    $before = Get-TreeHash $root
    foreach ($arguments in @(@('-RemoveRunRecords', '-Execute'), @('-RemoveRunRecords', '-RunId', $activeRun.RunId, '-Execute'), @('-ClearGenerated', '-Execute'), @('-RemoveDatabase', $activeName, '-Execute'))) {
        $result = Invoke-Tool $root $arguments
        Check "AC-08 live owned runner process: refused ($($arguments -join ' '))" ($result.Code -eq 2 -and $result.Text -match 'is active' -and (Get-TreeHash $root) -eq $before) $result.Text
    }
    $result = Invoke-Tool $root @()
    Check 'AC-08 status report lists the active run' ($result.Code -eq 0 -and $result.Text -match "$($activeRun.RunId)\s+\(live runner") $result.Text
    Stop-Process -Id $fakeRunnerProcess.Id -Force
    $fakeRunnerProcess = $null
    Start-Sleep -Milliseconds 500
    $result = Invoke-Tool $root @()
    Check 'AC-09 the same run without a live process is reported as interrupted' ($result.Code -eq 0 -and $result.Text -match 'INTERRUPTED RUNS[\s\S]*' + [regex]::Escape($activeRun.RunId)) $result.Text
    $result = Invoke-Tool $root @('-RemoveDatabase', $activeName, '-Execute')
    Check 'AC-09 an interrupted entry whose database is absent is not removable' ($result.Code -eq 2 -and $result.Text -match 'does not exist') $result.Text

    # ---- AC-12: run records fail closed ------------------------------------------------------------------------
    $root = New-Layout 'records'
    $resolved = New-Run $root @((New-Entry (New-FakeName) 'removed' 'passed')) -Completed
    $citedById = New-Run $root @((New-Entry (New-FakeName) 'removed' 'passed')) -Completed
    $citedName = New-FakeName
    $citedByName = New-Run $root @((New-Entry $citedName 'removed' 'passed')) -Completed
    $interruptedNoDb = New-Run $root @((New-Entry (New-FakeName) 'starting' 'pending'))
    $invalid = New-Run $root @((New-Entry (New-FakeName) 'removed' 'passed')) -Completed
    [System.IO.File]::WriteAllText($invalid.Manifest, 'not json')
    Set-Content -LiteralPath (Join-Path $root 'ai-context\planning\VER-cites-run.md') -Value "Evidence: run $($citedById.RunId)." -Encoding utf8
    Set-Content -LiteralPath (Join-Path $root 'docs\cites-database.md') -Value "Database $citedName was inspected." -Encoding utf8

    $result = Invoke-Tool $root @('-RemoveRunRecords')
    Check 'AC-12 dry run proposes only the resolved folder' ($result.Code -eq 0 -and $result.Text -match "Remove:\s+$($resolved.RunId)" -and $result.Text -notmatch "Remove:\s+($($citedById.RunId)|$($citedByName.RunId)|$($interruptedNoDb.RunId)|$($invalid.RunId))") $result.Text
    $result = Invoke-Tool $root @('-RemoveRunRecords', '-Execute')
    Check 'AC-12 -Execute removes the resolved folder' ($result.Code -eq 0 -and -not (Test-Path -LiteralPath $resolved.Folder)) $result.Text
    Check 'AC-12 folder cited by run ID kept' (Test-Path -LiteralPath $citedById.Folder)
    Check 'AC-12 folder cited by database name kept' (Test-Path -LiteralPath $citedByName.Folder)
    Check 'AC-12 interrupted folder kept' (Test-Path -LiteralPath $interruptedNoDb.Folder)
    Check 'AC-12 invalid manifest folder kept' (Test-Path -LiteralPath $invalid.Folder)
    $result = Invoke-Tool $root @('-RemoveRunRecords', '-RunId', $invalid.RunId, '-Execute')
    Check 'AC-12 invalid manifest folder refused even with -RunId' ($result.Code -eq 2 -and (Test-Path -LiteralPath $invalid.Folder)) $result.Text
    $result = Invoke-Tool $root @('-RemoveRunRecords', '-RunId', (New-RunId), '-Execute')
    Check 'AC-12 unknown -RunId refused' ($result.Code -eq 2) $result.Text
    $result = Invoke-Tool $root @('-RemoveRunRecords', '-RunId', $citedById.RunId, '-Execute')
    Check 'AC-12 cited folder removed only with its exact -RunId and -Execute' ($result.Code -eq 0 -and -not (Test-Path -LiteralPath $citedById.Folder) -and (Test-Path -LiteralPath $citedByName.Folder)) $result.Text

    # Citation uncertainty: a missing search location, then an unreadable searched file.
    $root = New-Layout 'uncertain'
    $uncertainRun = New-Run $root @((New-Entry (New-FakeName) 'removed' 'passed')) -Completed
    Remove-Item -LiteralPath (Join-Path $root 'codex-handoff')
    $result = Invoke-Tool $root @('-RemoveRunRecords', '-Execute')
    Check 'AC-12 missing search location: uncited folder protected' ($result.Code -eq 0 -and $result.Text -match 'uncertain' -and (Test-Path -LiteralPath $uncertainRun.Folder)) $result.Text
    New-Item -ItemType Directory -Force (Join-Path $root 'codex-handoff') | Out-Null
    $lockedPath = Join-Path $root 'docs\locked.md'
    Set-Content -LiteralPath $lockedPath -Value 'unreadable' -Encoding utf8
    $lockedFiles.Add([System.IO.File]::Open($lockedPath, 'Open', 'ReadWrite', 'None'))
    $result = Invoke-Tool $root @('-RemoveRunRecords', '-Execute')
    Check 'AC-12 unreadable searched file: uncited folder protected' ($result.Code -eq 0 -and $result.Text -match 'uncertain' -and (Test-Path -LiteralPath $uncertainRun.Folder)) $result.Text
    $result = Invoke-Tool $root @('-RemoveRunRecords', '-RunId', $uncertainRun.RunId, '-Execute')
    Check 'AC-12 citation-uncertain folder removed only with its exact -RunId' ($result.Code -eq 0 -and -not (Test-Path -LiteralPath $uncertainRun.Folder)) $result.Text
    $lockedFiles | ForEach-Object { $_.Dispose() }
    $lockedFiles.Clear()

    # A run folder that is a junction is never followed or removed.
    $root = New-Layout 'junction'
    $outside = Join-Path $work 'outside-target'
    New-Item -ItemType Directory -Force $outside | Out-Null
    Set-Content -LiteralPath (Join-Path $outside 'precious.txt') -Value 'must survive' -Encoding utf8
    $linkRunId = New-RunId
    $linkPath = Join-Path $root "src\FamilySafety.Web\e2e\scenario-runs\$linkRunId"
    cmd /c mklink /J "$linkPath" "$outside" | Out-Null
    $result = Invoke-Tool $root @('-RemoveRunRecords', '-RunId', $linkRunId, '-Execute')
    Check 'AC-12 junction run folder refused; its target is untouched' ($result.Code -eq 2 -and (Test-Path -LiteralPath (Join-Path $outside 'precious.txt'))) $result.Text
    [System.IO.Directory]::Delete($linkPath, $false)

    # ---- AC-13: generated cleanup is limited -------------------------------------------------------------------
    $root = New-Layout 'generated'
    $web = Join-Path $root 'src\FamilySafety.Web'
    $files = @{}
    foreach ($relative in 'e2e\test-results\t1\trace.zip', 'e2e\captures\concern-workflow\a.png', 'e2e\captures\.gitignore', 'dist\verification\WI-000-000\main.js',
        'dist\main.js', 'src\app.ts', 'e2e\scenario-runs\.gitignore', 'e2e\playwright.config.ts') {
        $path = Join-Path $web $relative
        New-Item -ItemType Directory -Force (Split-Path -Parent $path) | Out-Null
        if (-not (Test-Path -LiteralPath $path)) { Set-Content -LiteralPath $path -Value $relative -Encoding utf8 }
    }
    Set-Content -LiteralPath (Join-Path $root 'trips') -Value 'stand-in for the unclassified root file' -Encoding utf8
    New-Item -ItemType Directory -Force (Join-Path $root 'ai-context\planning\verification\evidence') | Out-Null
    Set-Content -LiteralPath (Join-Path $root 'ai-context\planning\verification\evidence\canonical.png') -Value 'canonical' -Encoding utf8
    $tripsStandIn = (Get-FileHash -LiteralPath (Join-Path $root 'trips') -Algorithm SHA256).Hash
    $keepHashes = @{}
    foreach ($relative in 'dist\main.js', 'src\app.ts', 'e2e\scenario-runs\.gitignore', 'e2e\captures\.gitignore', 'e2e\playwright.config.ts') {
        $keepHashes[$relative] = (Get-FileHash -LiteralPath (Join-Path $web $relative) -Algorithm SHA256).Hash
    }
    $aiBefore = Get-TreeHash (Join-Path $root 'ai-context')

    $result = Invoke-Tool $root @()
    Check 'AC-13 status labels the root trips file UNCLASSIFIED / OWNER REVIEW REQUIRED with no action' ($result.Code -eq 0 -and $result.Text -match 'trips\s+\(file.*UNCLASSIFIED / OWNER REVIEW REQUIRED') $result.Text
    $result = Invoke-Tool $root @('-ClearGenerated', '-Execute')
    Check 'AC-13 -ClearGenerated -Execute succeeds' ($result.Code -eq 0) $result.Text
    Check 'AC-13 e2e/test-results emptied' (@(Get-ChildItem -LiteralPath (Join-Path $web 'e2e\test-results') -Force).Count -eq 0)
    Check 'AC-13 e2e/captures emptied except its .gitignore' (@(Get-ChildItem -LiteralPath (Join-Path $web 'e2e\captures') -Force | Where-Object { $_.Name -ne '.gitignore' }).Count -eq 0)
    Check 'AC-13 dist/verification emptied' (@(Get-ChildItem -LiteralPath (Join-Path $web 'dist\verification') -Force).Count -eq 0)
    $untouched = @($keepHashes.Keys | Where-Object { (Get-FileHash -LiteralPath (Join-Path $web $_) -Algorithm SHA256).Hash -ne $keepHashes[$_] })
    Check 'AC-13 dist itself, source, scenario-runs and config untouched' ($untouched.Count -eq 0) ($untouched -join ', ')
    Check 'AC-13 root trips stand-in byte-identical' ((Get-FileHash -LiteralPath (Join-Path $root 'trips') -Algorithm SHA256).Hash -eq $tripsStandIn)
    Check 'AC-13 ai-context untouched' ((Get-TreeHash (Join-Path $root 'ai-context')) -eq $aiBefore)

    # A junction inside a fixed folder makes the whole clear refuse; the junction target survives.
    $outside2 = Join-Path $work 'outside-target-2'
    New-Item -ItemType Directory -Force $outside2 | Out-Null
    Set-Content -LiteralPath (Join-Path $outside2 'precious.txt') -Value 'must survive' -Encoding utf8
    $link2 = Join-Path $web 'e2e\captures\escape'
    cmd /c mklink /J "$link2" "$outside2" | Out-Null
    $result = Invoke-Tool $root @('-ClearGenerated', '-Execute')
    Check 'AC-13 junction inside captures: refused, target untouched' ($result.Code -eq 2 -and $result.Text -match 'junction' -and (Test-Path -LiteralPath (Join-Path $outside2 'precious.txt'))) $result.Text
    [System.IO.Directory]::Delete($link2, $false)

    # A fixed folder that is itself a junction to elsewhere is refused.
    Remove-Item -LiteralPath (Join-Path $web 'dist\verification') -Recurse -Force
    cmd /c mklink /J "$(Join-Path $web 'dist\verification')" "$outside2" | Out-Null
    $result = Invoke-Tool $root @('-ClearGenerated', '-Execute')
    Check 'AC-13 fixed folder replaced by a junction: refused, target untouched' ($result.Code -eq 2 -and (Test-Path -LiteralPath (Join-Path $outside2 'precious.txt'))) $result.Text
    [System.IO.Directory]::Delete((Join-Path $web 'dist\verification'), $false)

    # No parameter accepts a path to clear, and -RepositoryRoot must be a repository layout.
    $result = Invoke-Tool $root @('-ClearGenerated', '-Path', $root, '-Execute')
    Check 'AC-13 a crafted target path cannot be passed (no such parameter)' ($result.Code -ne 0 -and (Test-Path -LiteralPath (Join-Path $root 'trips'))) $result.Text
    $result = Invoke-Tool $outside2 @('-ClearGenerated', '-Execute')
    Check 'AC-13 a non-repository root is refused' ($result.Code -eq 2 -and (Test-Path -LiteralPath (Join-Path $outside2 'precious.txt'))) $result.Text
}
catch {
    $script:failures++
    Write-Host "FAIL  unexpected error: $($_.Exception.Message) at line $($_.InvocationInfo.ScriptLineNumber)"
}
finally {
    $lockedFiles | ForEach-Object { $_.Dispose() }
    if ($null -ne $heldPort) { $heldPort.Stop() }
    if ($null -ne $fakeRunnerProcess) { Stop-Process -Id $fakeRunnerProcess.Id -Force -ErrorAction SilentlyContinue }
}

# ---- Global invariants -----------------------------------------------------------------------------------------
$databasesAfter = Get-ServerDatabases
Check 'No database was created or dropped (FamilySafety* list identical)' ($databasesAfter -eq $databasesBefore)
$tripsAfter = if (Test-Path -LiteralPath $tripsPath) { (Get-FileHash -LiteralPath $tripsPath -Algorithm SHA256).Hash } else { 'ABSENT' }
Check 'Repository root trips file byte-identical' ($tripsAfter -eq $tripsBefore)
Check 'Repository ai-context/ unchanged' ((Get-TreeHash (Join-Path $repositoryRoot 'ai-context')) -eq $aiContextBefore)
Check 'Repository scenario-runs/ unchanged' ((Get-TreeHash (Join-Path $repositoryRoot 'src\FamilySafety.Web\e2e\scenario-runs')) -eq $realRunsBefore)

# Remove the temporary folder: junctions first (never followed), then the rest.
if (Test-Path -LiteralPath $work) {
    Get-ChildItem -LiteralPath $work -Recurse -Force -Attributes ReparsePoint -ErrorAction SilentlyContinue | ForEach-Object { [System.IO.Directory]::Delete($_.FullName, $false) }
    Remove-Item -LiteralPath $work -Recurse -Force
}

Write-Host ''
Write-Host "Safety checks: $script:passes passed, $script:failures failed."
if ($script:failures -gt 0) { exit 1 }
exit 0
