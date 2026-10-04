<#
.SYNOPSIS
Shows the local test artifacts that automated verification leaves behind, and removes only those whose ownership
is proven. Read-only unless an action and -Execute are both given.

.DESCRIPTION
WI-009-002 developer entry point. Windows PowerShell 5.1, System.Data.SqlClient, Windows integrated security.

  .\scripts\test-artifacts.ps1                                         Read-only status report (the default).
  .\scripts\test-artifacts.ps1 -RemoveDatabase <name>[,<name>...]      Dry run: ownership proof and proposed action.
  .\scripts\test-artifacts.ps1 -RemoveDatabase <name> -Execute         Removes each named database through
                                                                       remove-scenario-database.ps1 -RemoveRetained.
  .\scripts\test-artifacts.ps1 -RemoveRunRecords                       Dry run: run folders that may be removed.
  .\scripts\test-artifacts.ps1 -RemoveRunRecords [-RunId <id>] -Execute
                                                                       Without -RunId removes only "Resolved" folders
                                                                       (uncited, completed reference search). A cited,
                                                                       citation-uncertain, or interrupted folder needs
                                                                       its exact -RunId.
  .\scripts\test-artifacts.ps1 -ClearGenerated                         Dry run: contents of the fixed transient folders.
  .\scripts\test-artifacts.ps1 -ClearGenerated -Execute                Empties only those fixed folders.

Safeguards (WI-009-002 D1-D10):
  - This script never removes a database itself. Every database removal is delegated to
    remove-scenario-database.ps1 with the manifest of the run that owns it, so every WI-009-001 check still runs.
  - Databases are selected by exact name only. Wildcard characters are rejected. There is no "all retained" option,
    and nothing is chosen by name prefix or age.
  - An interrupted run's database is never removed automatically. With its exact name and -Execute, the entry is first
    recorded as "retained" (the earlier status is kept in history), and then the removal is delegated.
  - A run folder cited in the project records (by run ID or by a database name it records) is removed only with its
    exact -RunId and -Execute. If the reference search is uncertain, the folder is protected.
  - Every action refuses while a run is active (port 5080 in use or a live owned process) or SQL Server cannot be queried.
  - File removal is limited to scenario-runs/<run id> folders and the three fixed transient folders, each confirmed to
    lie inside src/FamilySafety.Web/ and to contain no junction or symbolic link. The repository root is never a target.
  - The status report changes nothing, and manifests are never rewritten to match the server.

-RepositoryRoot exists for scripts/tests/check-test-artifacts-safety.ps1, which points it at a temporary copy of the
layout. The PRE-EXISTING list and the removal script always come from this scripts folder.

Exit codes: 0 = report, dry run, or done; 2 = refused (nothing changed); 3 = failure;
4 = a delegated removal dropped the database but could not update its manifest.
#>
[CmdletBinding()]   # Unknown parameters (for example an "all" switch or a target path) are rejected, not ignored.
param(
    [string[]]$RemoveDatabase,
    [switch]$RemoveRunRecords,
    [string[]]$RunId,
    [switch]$ClearGenerated,
    [switch]$Execute,
    [string]$Server = 'localhost',
    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
# Resolved here: Windows PowerShell 5.1 leaves $PSScriptRoot empty in an advanced script's parameter defaults.
if (-not $PSBoundParameters.ContainsKey('RepositoryRoot')) { $RepositoryRoot = Split-Path -Parent $PSScriptRoot }
$scenarioPattern = '^FamilySafety_Invitation_E2E_[A-Za-z0-9]+$'
$runIdPattern = '^\d{8}T\d{6}-[0-9a-f]{8}$'
$wildcardCharacters = '[\*\?%\[\]]'
$knownStatuses = @('starting', 'running', 'removed', 'retained', 'cleanup-failed')
$knownOutcomes = @('pending', 'passed', 'failed')
$removalScript = Join-Path $PSScriptRoot 'remove-scenario-database.ps1'
$preExistingListPath = Join-Path $PSScriptRoot 'pre-existing-scenario-databases.txt'
$citationLocations = @('ai-context', 'claude-handoff', 'codex-handoff', 'docs', 'scripts', 'README.md')
$binaryExtensions = @('.png', '.jpg', '.jpeg', '.gif', '.webp', '.bmp', '.ico')
$expectedRootEntries = @('.claude', '.config', '.editorconfig', '.git', '.gitattributes', '.gitignore', '.vs', '.vscode',
    'ai-context', 'claude-handoff', 'codex-handoff', 'Directory.Build.props', 'docs', 'FamilySafety.slnx', 'global.json',
    'README.md', 'scripts', 'src', 'tests')
$apiPort = 5080

function Refuse([string]$reason) {
    Write-Host "REFUSED: $reason Nothing was changed."
    exit 2
}

function Fail([string]$reason) {
    Write-Host "FAILED: $reason"
    exit 3
}

function Format-Size([double]$bytes) {
    if ($bytes -ge 1MB) { return '{0:N1} MB' -f ($bytes / 1MB) }
    if ($bytes -ge 1KB) { return '{0:N1} KB' -f ($bytes / 1KB) }
    return '{0} B' -f [long]$bytes
}

function Get-ItemsBelow([string]$path) {
    # Lists every file and folder below $path without ever descending into a junction or symbolic link.
    $items = New-Object System.Collections.Generic.List[System.IO.FileSystemInfo]
    $pending = New-Object System.Collections.Generic.Stack[string]
    $pending.Push($path)
    while ($pending.Count -gt 0) {
        $directory = New-Object System.IO.DirectoryInfo ($pending.Pop())
        foreach ($item in $directory.EnumerateFileSystemInfos()) {
            $items.Add($item)
            $isLink = ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0
            if (($item.Attributes -band [System.IO.FileAttributes]::Directory) -and -not $isLink) {
                $pending.Push($item.FullName)
            }
        }
    }
    return $items.ToArray()   # Callers wrap the result in @(...), so an empty or single result is still a list.
}

function Test-IsLink([System.IO.FileSystemInfo]$item) {
    return ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0
}

# ---- Arguments ------------------------------------------------------------------------------------------------

$databaseNames = @()
if ($PSBoundParameters.ContainsKey('RemoveDatabase')) {
    $databaseNames = @(@($RemoveDatabase) | ForEach-Object { "$_" -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ } | Select-Object -Unique)
}
$runIds = @()
if ($PSBoundParameters.ContainsKey('RunId')) {
    $runIds = @(@($RunId) | ForEach-Object { "$_" -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ } | Select-Object -Unique)
}
# Built explicitly: Windows PowerShell 5.1 can drop values from an array literal of empty $(...) subexpressions.
$actions = New-Object System.Collections.Generic.List[string]
if ($PSBoundParameters.ContainsKey('RemoveDatabase')) { $actions.Add('RemoveDatabase') }
if ($RemoveRunRecords) { $actions.Add('RemoveRunRecords') }
if ($ClearGenerated) { $actions.Add('ClearGenerated') }
if ($actions.Count -gt 1) { Refuse "Choose one action at a time ($($actions -join ', '))." }
$action = if ($actions.Count -eq 1) { $actions[0] } else { 'Status' }
if ($Execute -and $action -eq 'Status') { Refuse '-Execute needs an action: -RemoveDatabase, -RemoveRunRecords, or -ClearGenerated.' }
if ($PSBoundParameters.ContainsKey('RunId') -and $action -ne 'RemoveRunRecords') { Refuse '-RunId is used only with -RemoveRunRecords.' }
if ($action -eq 'RemoveDatabase') {
    if ($databaseNames.Count -eq 0) { Refuse '-RemoveDatabase needs at least one exact database name.' }
    foreach ($name in $databaseNames) {
        if ($name -match $wildcardCharacters) { Refuse "'$name' contains a wildcard character (* ? % [ ]). Databases are selected by exact name only." }
    }
}
if ($PSBoundParameters.ContainsKey('RunId')) {
    if ($runIds.Count -eq 0) { Refuse '-RunId needs at least one exact run ID.' }
    foreach ($id in $runIds) {
        if ($id -match $wildcardCharacters) { Refuse "'$id' contains a wildcard character. Run folders are selected by exact run ID only." }
        if ($id -cnotmatch $runIdPattern) { Refuse "'$id' is not a run ID (expected the form 20261002T061132-1247b7e7)." }
    }
}

try {
    $root = [System.IO.Path]::GetFullPath($RepositoryRoot).TrimEnd('\')
}
catch {
    Refuse "The repository root '$RepositoryRoot' is not a valid path."
}
$webRoot = Join-Path $root 'src\FamilySafety.Web'
if (-not (Test-Path -LiteralPath (Join-Path $webRoot 'e2e\playwright.config.ts') -PathType Leaf)) {
    Refuse "'$root' is not the repository root (src\FamilySafety.Web\e2e\playwright.config.ts not found)."
}
$runsDirectory = Join-Path $webRoot 'e2e\scenario-runs'

# ---- Inventory: PRE-EXISTING list, server, processes, port ----------------------------------------------------

$preExisting = $null
if (Test-Path -LiteralPath $preExistingListPath -PathType Leaf) {
    try {
        $preExisting = @(Get-Content -LiteralPath $preExistingListPath | ForEach-Object { $_.Trim() } | Where-Object { $_ -and -not $_.StartsWith('#') })
        if ($preExisting.Count -eq 0) { $preExisting = $null }
    }
    catch { $preExisting = $null }
}

$sqlError = $null
$serverDatabases = @()
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$builder['Server'] = $Server
$builder['Database'] = 'master'
$builder['Integrated Security'] = $true
$builder['TrustServerCertificate'] = $true
$builder['Connect Timeout'] = 5
$connection = New-Object System.Data.SqlClient.SqlConnection $builder.ConnectionString
try {
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT name FROM sys.databases WHERE name LIKE N'FamilySafety%' ORDER BY name"
    $reader = $command.ExecuteReader()
    $list = New-Object System.Collections.Generic.List[string]
    while ($reader.Read()) { $list.Add($reader.GetString(0)) }
    $reader.Close()
    $serverDatabases = @($list)
}
catch {
    $sqlError = $_.Exception.Message
}
finally {
    $connection.Dispose()
}
$sqlOk = $null -eq $sqlError
$serverSet = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
foreach ($name in $serverDatabases) { $null = $serverSet.Add($name) }

function Get-ProcessTable {
    try {
        $table = @{}
        foreach ($row in @(Get-CimInstance Win32_Process -ErrorAction Stop)) { $table[[int]$row.ProcessId] = $row }
        return $table
    }
    catch { return $null }
}

function Test-PortInUse {
    foreach ($address in @([System.Net.IPAddress]::Loopback, [System.Net.IPAddress]::IPv6Loopback)) {
        $listener = New-Object System.Net.Sockets.TcpListener ($address, $apiPort)
        $listener.ExclusiveAddressUse = $true
        try { $listener.Start() }
        catch [System.Net.Sockets.SocketException] {
            if ($_.Exception.SocketErrorCode -eq 'AddressAlreadyInUse' -or $_.Exception.SocketErrorCode -eq 'AccessDenied') { return $true }
        }
        finally { $listener.Stop() }
    }
    try {
        if (@(Get-NetTCPConnection -LocalPort $apiPort -State Listen -ErrorAction Stop).Count -gt 0) { return $true }
    }
    catch {
        # No listener found (the cmdlet reports "no matching objects" as an error).
    }
    return $false
}

$processes = Get-ProcessTable
$portInUse = Test-PortInUse

# ---- Inventory: run manifests ----------------------------------------------------------------------------------

function Get-ManifestProblem($manifest, [string]$folderName) {
    if ($null -eq $manifest -or $manifest -isnot [System.Management.Automation.PSCustomObject]) { return 'not a JSON object' }
    if ($manifest.runId -isnot [string] -or $manifest.runId -cnotmatch $runIdPattern) { return 'runId is missing or not a run ID' }
    if ($manifest.runId -cne $folderName) { return "runId '$($manifest.runId)' does not match its folder" }
    if ($manifest.createdAt -isnot [string] -or -not $manifest.createdAt) { return 'createdAt is missing' }
    if ($manifest.preExistingAtRunStart -isnot [array]) { return 'preExistingAtRunStart is missing or not a list' }
    if ($manifest.databases -isnot [array]) { return 'databases is missing or not a list' }
    $completed = $manifest.PSObject.Properties['completedAt']
    if ($null -ne $completed -and ($completed.Value -isnot [string] -or -not $completed.Value)) { return 'completedAt is not a timestamp' }
    $runner = $manifest.PSObject.Properties['runnerPid']
    if ($null -ne $runner -and $runner.Value -isnot [int] -and $runner.Value -isnot [long]) { return 'runnerPid is not a process ID' }
    $seen = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($entry in @($manifest.databases)) {
        if ($null -eq $entry -or $entry -isnot [System.Management.Automation.PSCustomObject]) { return 'a database entry is not an object' }
        if ($entry.name -isnot [string] -or $entry.name -cnotmatch $scenarioPattern) { return "an entry name is missing or not a scenario database name ('$($entry.name)')" }
        if (-not $seen.Add($entry.name)) { return "'$($entry.name)' is recorded more than once" }
        if ($entry.status -isnot [string] -or $knownStatuses -cnotcontains $entry.status) { return "'$($entry.name)' has no valid status ('$($entry.status)')" }
        if ($entry.outcome -isnot [string] -or $knownOutcomes -cnotcontains $entry.outcome) { return "'$($entry.name)' has no valid outcome ('$($entry.outcome)')" }
        if ($entry.spec -isnot [string] -or -not $entry.spec) { return "'$($entry.name)' has no spec" }
        if ($entry.createdAt -isnot [string] -or -not $entry.createdAt) { return "'$($entry.name)' has no createdAt" }
        $history = $entry.PSObject.Properties['history']
        if ($null -ne $history) {
            if ($history.Value -isnot [array]) { return "'$($entry.name)' history is not a list" }
            foreach ($change in @($history.Value)) {
                if ($null -eq $change -or $change.from -isnot [string] -or $change.to -isnot [string]) { return "'$($entry.name)' has a malformed history entry" }
            }
        }
    }
    return $null
}

function Test-ProcessOwned($processId, [string]$mustContain) {
    if ($null -eq $processId -or $null -eq $processes) { return $false }
    $row = $processes[[int]$processId]
    return $null -ne $row -and ([string]$row.CommandLine).IndexOf($mustContain, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
}

$runs = New-Object System.Collections.Generic.List[object]
$runsDirectoryProblem = $null
if (Test-Path -LiteralPath $runsDirectory -PathType Container) {
    try { $runFolders = @((New-Object System.IO.DirectoryInfo $runsDirectory).GetDirectories() | Sort-Object Name) }
    catch { $runFolders = @(); $runsDirectoryProblem = $_.Exception.Message }
}
else { $runFolders = @() }

foreach ($folder in $runFolders) {
    $run = [pscustomobject]@{
        RunId = $folder.Name; Path = $folder.FullName; ManifestPath = (Join-Path $folder.FullName 'manifest.json')
        Manifest = $null; RawText = $null; RawReadable = $false; ManifestHash = $null; Problem = $null
        Entries = @(); CompletedAt = $null; Active = $false; State = $null; StateDetail = ''; Citations = @()
        Bytes = 0; FileNames = @()
    }
    try {
        $files = @(Get-ItemsBelow $folder.FullName | Where-Object { -not ($_.Attributes -band [System.IO.FileAttributes]::Directory) })
        $run.Bytes = [long](($files | Measure-Object Length -Sum).Sum)
        $run.FileNames = @($files | ForEach-Object { $_.Name })
    }
    catch { $run.Problem = "folder could not be listed: $($_.Exception.Message)" }

    if (Test-IsLink $folder) { $run.Problem = 'the folder is a junction or symbolic link' }
    elseif ($folder.Name -cnotmatch $runIdPattern) { $run.Problem = 'the folder name is not a run ID' }
    elseif ($null -eq $run.Problem) {
        if (-not (Test-Path -LiteralPath $run.ManifestPath -PathType Leaf)) { $run.Problem = 'manifest.json is missing' }
        else {
            try {
                $run.RawText = [System.IO.File]::ReadAllText($run.ManifestPath, [System.Text.Encoding]::UTF8)
                $run.RawReadable = $true
                $run.ManifestHash = (Get-FileHash -LiteralPath $run.ManifestPath -Algorithm SHA256).Hash
            }
            catch { $run.Problem = "manifest.json could not be read: $($_.Exception.Message)" }
            if ($run.RawReadable) {
                try { $run.Manifest = $run.RawText | ConvertFrom-Json }
                catch { $run.Problem = 'manifest.json is not valid JSON' }
                if ($null -eq $run.Problem) { $run.Problem = Get-ManifestProblem $run.Manifest $folder.Name }
            }
        }
    }
    if ($null -eq $run.Problem) {
        $run.Entries = @($run.Manifest.databases)
        $completed = $run.Manifest.PSObject.Properties['completedAt']
        if ($null -ne $completed) { $run.CompletedAt = [string]$completed.Value }
        if (-not $run.CompletedAt) {
            if ($null -eq $processes) { $run.Active = $true }   # Cannot prove the run is over: fail closed.
            else {
                $runner = $run.Manifest.PSObject.Properties['runnerPid']
                if ($null -ne $runner -and (Test-ProcessOwned $runner.Value 'playwright')) { $run.Active = $true }
                foreach ($entry in $run.Entries) {
                    if (Test-ProcessOwned $entry.apiPid ($entry.name.Substring('FamilySafety_Invitation_'.Length))) { $run.Active = $true }
                }
            }
        }
    }
    $runs.Add($run)
}
$validRuns = @($runs | Where-Object { $null -eq $_.Problem })
$invalidRuns = @($runs | Where-Object { $null -ne $_.Problem })
$anyActive = $portInUse -or @($validRuns | Where-Object { $_.Active }).Count -gt 0

# Ownership index: every valid record of every database name, across all manifests.
$owners = @{}
foreach ($run in $validRuns) {
    foreach ($entry in $run.Entries) {
        $key = $entry.name.ToLowerInvariant()
        # Plain arrays: Windows PowerShell 5.1 fails on @($genericList) of these objects ("Argument types do not match").
        if (-not $owners.ContainsKey($key)) { $owners[$key] = @() }
        $owners[$key] += [pscustomobject]@{ Run = $run; Entry = $entry }
    }
}

function Get-UnprovenClaims([string]$name) {
    # Invalid or unreadable run folders that might also record $name. Any of them makes ownership unprovable.
    $claims = @()
    foreach ($run in $invalidRuns) {
        if (-not $run.RawReadable) {
            if ($run.FileNames -contains "$name.api.log") { $claims += "$($run.RunId) ($($run.Problem); its folder holds $name.api.log)" }
            elseif ($run.Problem -notlike 'manifest.json is missing*') { $claims += "$($run.RunId) ($($run.Problem); its records cannot be inspected)" }
        }
        elseif ($run.RawText.IndexOf($name, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            $claims += "$($run.RunId) ($($run.Problem); it mentions $name)"
        }
    }
    return , $claims
}

# ---- Reconciliation labels and run states (D5) ----------------------------------------------------------------

function Get-RunInterrupted($run) {
    if ($run.Active -or $run.CompletedAt) { return $false }
    if (@($run.Entries | Where-Object { $_.status -eq 'starting' -or $_.status -eq 'running' }).Count -gt 0) { return $true }
    # A manifest written by the WI-009-002 harness records runnerPid; without completedAt its teardown never ran.
    return $null -ne $run.Manifest.PSObject.Properties['runnerPid']
}

function Get-EntryLabel($run, $entry) {
    if (-not $sqlOk) { return 'unknown — server not queried' }
    $present = $serverSet.Contains($entry.name)
    $removedAt = $entry.PSObject.Properties['removedAt']
    switch ($entry.status) {
        'removed' { if ($present) { return 'removed in manifest — PRESENT on server' } else { return 'removed — absent' } }
        { $_ -eq 'retained' -or $_ -eq 'cleanup-failed' } {
            if ($present) { return 'retained — exists' }
            return 'retained in manifest — absent on server'
        }
        { $_ -eq 'starting' -or $_ -eq 'running' } {
            if ($run.Active) { if ($present) { return 'active — in use' } else { return 'active — not created yet' } }
            if ($present) { return 'interrupted — exists' }
            if ($entry.status -eq 'starting') { return 'creation never completed — absent' }
            if ($null -eq $removedAt) { return 'retained in manifest — absent on server' }
        }
    }
    return "unexpected status '$($entry.status)'"
}

foreach ($run in $validRuns) {
    $run | Add-Member -NotePropertyName Interrupted -NotePropertyValue (Get-RunInterrupted $run)
    $run | Add-Member -NotePropertyName Present -NotePropertyValue @($run.Entries | Where-Object { $sqlOk -and $serverSet.Contains($_.name) })
    foreach ($entry in $run.Entries) {
        $entry | Add-Member -NotePropertyName __label -NotePropertyValue (Get-EntryLabel $run $entry) -Force
    }
}

# ---- Citation search (D2): fails closed --------------------------------------------------------------------------

$citationUncertain = New-Object System.Collections.Generic.List[string]
$searchTexts = New-Object System.Collections.Generic.List[object]
if ($validRuns.Count -gt 0) {
    foreach ($location in $citationLocations) {
        $path = Join-Path $root $location
        if (-not (Test-Path -LiteralPath $path)) { $citationUncertain.Add("search location '$location' is missing"); continue }
        try {
            if (Test-Path -LiteralPath $path -PathType Leaf) { $candidates = @(New-Object System.IO.FileInfo $path) }
            else { $candidates = @(Get-ItemsBelow $path | Where-Object { -not ($_.Attributes -band [System.IO.FileAttributes]::Directory) }) }
        }
        catch { $citationUncertain.Add("search location '$location' could not be listed: $($_.Exception.Message)"); continue }
        foreach ($file in $candidates) {
            if ($binaryExtensions -contains $file.Extension.ToLowerInvariant()) { continue }
            try {
                $text = [System.IO.File]::ReadAllText($file.FullName).ToLowerInvariant()
                $searchTexts.Add([pscustomobject]@{ Path = $file.FullName.Substring($root.Length + 1); Text = $text })
            }
            catch { $citationUncertain.Add("'$($file.FullName.Substring($root.Length + 1))' could not be read") }
        }
    }
}

function Get-Citations($run) {
    # A run is cited by its run ID or its 8-character suffix, or by a database name it records or that name's first
    # 8 hexadecimal characters (records abbreviate names as …0c1229b7…). Short forms may over-protect; that is safe.
    $tokens = @($run.RunId, $run.RunId.Substring($run.RunId.Length - 8))
    foreach ($entry in $run.Entries) {
        $suffix = $entry.name.Substring('FamilySafety_Invitation_E2E_'.Length)
        $tokens += $entry.name
        if ($suffix.Length -ge 8) { $tokens += $suffix.Substring(0, 8) }
    }
    $found = @()
    foreach ($token in ($tokens | ForEach-Object { $_.ToLowerInvariant() } | Select-Object -Unique)) {
        foreach ($source in $searchTexts) {
            if ($source.Text.IndexOf($token, [System.StringComparison]::Ordinal) -ge 0) { $found += "$token in $($source.Path)"; break }
        }
    }
    return , $found
}

foreach ($run in $validRuns) {
    if ($run.Active) { $run.State = 'Active'; continue }
    $run.Citations = Get-Citations $run
    if ($run.Interrupted) {
        if ($run.Present.Count -gt 0 -or -not $sqlOk) { $run.State = 'InterruptedHoldingDatabases' } else { $run.State = 'InterruptedNoDatabase' }
        continue
    }
    if (-not $sqlOk -or $run.Present.Count -gt 0) { $run.State = 'HoldingDatabases'; continue }
    if ($citationUncertain.Count -gt 0) { $run.State = 'CitationUncertain' }
    elseif ($run.Citations.Count -gt 0) { $run.State = 'Cited' }
    else { $run.State = 'Resolved' }
}
foreach ($run in $invalidRuns) { $run.State = 'Invalid' }

# ---- Database candidates (§ 4) ------------------------------------------------------------------------------------

function Get-DatabaseDecision([string]$name) {
    # Returns [pscustomobject]@{ Allowed; Reason; Run; Entry; Interrupted }. Checks run in the order of the WI-009-001 script.
    $decision = [pscustomobject]@{ Name = $name; Allowed = $false; Reason = ''; Run = $null; Entry = $null; Interrupted = $false }
    if ($name.Trim() -ieq 'FamilySafety') { $decision.Reason = 'FamilySafety is never removed.'; return $decision }
    if ($name -cnotmatch $scenarioPattern) { $decision.Reason = "'$name' does not match the approved pattern $scenarioPattern."; return $decision }
    if ($null -eq $preExisting) { $decision.Reason = "The PRE-EXISTING list '$preExistingListPath' is missing, unreadable, or empty."; return $decision }
    if ($preExisting -icontains $name) { $decision.Reason = "'$name' is PRE-EXISTING (protected)."; return $decision }
    $unproven = Get-UnprovenClaims $name
    if ($unproven.Count -gt 0) { $decision.Reason = "Ownership cannot be proven: $($unproven -join '; ')."; return $decision }
    $records = @()
    if ($owners.ContainsKey($name.ToLowerInvariant())) { $records = @($owners[$name.ToLowerInvariant()]) }
    if ($records.Count -eq 0) { $decision.Reason = "No run manifest records '$name' (unknown ownership)."; return $decision }
    if ($records.Count -gt 1) { $decision.Reason = "'$name' is recorded in $($records.Count) manifests ($(($records | ForEach-Object { $_.Run.RunId }) -join ', ')); ownership is ambiguous."; return $decision }
    $run = $records[0].Run
    $entry = $records[0].Entry
    $decision.Run = $run
    $decision.Entry = $entry
    if ($entry.name -cne $name) { $decision.Reason = "The manifest spelling '$($entry.name)' differs from '$name'."; return $decision }
    if (@($run.Manifest.preExistingAtRunStart) -icontains $name) { $decision.Reason = "'$name' already existed when run $($run.RunId) started."; return $decision }
    if ($run.Active) { $decision.Reason = "Run $($run.RunId) is active."; return $decision }
    switch ($entry.status) {
        { $_ -eq 'retained' -or $_ -eq 'cleanup-failed' } { break }
        { $_ -eq 'starting' -or $_ -eq 'running' } {
            if (-not $run.Interrupted) { $decision.Reason = "'$name' is '$($entry.status)' in a run that is not proven interrupted."; return $decision }
            $decision.Interrupted = $true
            break
        }
        'removed' { $decision.Reason = "'$name' is recorded as removed ($($entry.__label)); no action is offered."; return $decision }
        default { $decision.Reason = "'$name' has status '$($entry.status)'."; return $decision }
    }
    if (-not $sqlOk) { $decision.Reason = "SQL Server '$Server' cannot be queried."; return $decision }
    if (-not $serverSet.Contains($name)) { $decision.Reason = "'$name' does not exist on $Server ($($entry.__label)); nothing to remove."; return $decision }
    $decision.Allowed = $true
    return $decision
}

# ---- Generated folders (§ 5, D8) -----------------------------------------------------------------------------------

$generatedTargets = @(
    [pscustomobject]@{ Label = 'e2e/test-results'; Path = (Join-Path $webRoot 'e2e\test-results'); Keep = @() },
    [pscustomobject]@{ Label = 'e2e/captures'; Path = (Join-Path $webRoot 'e2e\captures'); Keep = @('.gitignore') },
    [pscustomobject]@{ Label = 'dist/verification'; Path = (Join-Path $webRoot 'dist\verification'); Keep = @() }
)

function Get-TargetProblem([string]$path, [string]$containerRoot) {
    # The path must lie inside $containerRoot and no folder from $containerRoot down to it, or below it, may be a link.
    $full = [System.IO.Path]::GetFullPath($path).TrimEnd('\')
    $container = [System.IO.Path]::GetFullPath($containerRoot).TrimEnd('\')
    if (-not $full.StartsWith($container + '\', [System.StringComparison]::OrdinalIgnoreCase)) { return "'$full' is outside '$container'" }
    $step = $full
    while ($step.Length -gt $container.Length) {
        if (Test-Path -LiteralPath $step) {
            $item = Get-Item -LiteralPath $step -Force
            if (Test-IsLink $item) { return "'$step' is a junction or symbolic link" }
        }
        $step = Split-Path -Parent $step
    }
    if (Test-Path -LiteralPath $full -PathType Container) {
        $links = @(Get-ItemsBelow $full | Where-Object { Test-IsLink $_ })
        if ($links.Count -gt 0) { return "'$($links[0].FullName)' inside it is a junction or symbolic link" }
    }
    return $null
}

function Get-FolderSummary([string]$path, [string[]]$keep) {
    if (-not (Test-Path -LiteralPath $path -PathType Container)) { return [pscustomobject]@{ Exists = $false; Files = 0; Folders = 0; Bytes = 0; Items = @() } }
    $all = @(Get-ItemsBelow $path)
    $top = @((New-Object System.IO.DirectoryInfo $path).EnumerateFileSystemInfos() | Where-Object { $keep -notcontains $_.Name })
    $files = @($all | Where-Object { -not ($_.Attributes -band [System.IO.FileAttributes]::Directory) -and -not ($keep -contains $_.Name -and (Split-Path -Parent $_.FullName) -eq $path) })
    $folders = @($all | Where-Object { $_.Attributes -band [System.IO.FileAttributes]::Directory })
    return [pscustomobject]@{ Exists = $true; Files = $files.Count; Folders = $folders.Count; Bytes = [long](($files | Measure-Object Length -Sum).Sum); Items = $top }
}

function Remove-FolderContents([string]$path, [string[]]$keep) {
    # Deletes bottom-up with non-recursive calls, so a link can never be followed. Callers check for links first.
    foreach ($item in @((New-Object System.IO.DirectoryInfo $path).EnumerateFileSystemInfos())) {
        if ($keep -contains $item.Name) { continue }
        Remove-Tree $item.FullName
    }
}

function Remove-Tree([string]$path) {
    $item = Get-Item -LiteralPath $path -Force
    if (Test-IsLink $item) { throw "'$path' is a junction or symbolic link; it is not removed." }
    if ($item.Attributes -band [System.IO.FileAttributes]::Directory) {
        foreach ($child in @((New-Object System.IO.DirectoryInfo $path).EnumerateFileSystemInfos())) { Remove-Tree $child.FullName }
        [System.IO.Directory]::Delete($path, $false)
    }
    else {
        if ($item.Attributes -band [System.IO.FileAttributes]::ReadOnly) { $item.Attributes = $item.Attributes -bxor [System.IO.FileAttributes]::ReadOnly }
        [System.IO.File]::Delete($path)
    }
}

function Write-ManifestAtomically([string]$manifestPath, $manifest) {
    $temporary = "$manifestPath.tmp"
    try {
        [System.IO.File]::WriteAllText($temporary, (ConvertTo-Json -InputObject $manifest -Depth 20), (New-Object System.Text.UTF8Encoding $false))
        # [NullString]::Value: a plain $null would reach .NET as "" ("The path is not of a legal form").
        [System.IO.File]::Replace($temporary, $manifestPath, [NullString]::Value)
    }
    catch {
        if (Test-Path -LiteralPath $temporary -PathType Leaf) { Remove-Item -LiteralPath $temporary -Force -ErrorAction SilentlyContinue }
        throw
    }
}

# ---- Guards shared by every action ---------------------------------------------------------------------------------

function Assert-SafeToAct {
    if (-not $sqlOk) { Refuse "SQL Server '$Server' cannot be queried ($sqlError). Ownership and absence cannot be proven." }
    if ($null -eq $processes) { Refuse 'The process list could not be read, so an active run cannot be ruled out.' }
    if ($portInUse) { Refuse "Port $apiPort is in use; a run may be active." }
    $active = @($validRuns | Where-Object { $_.Active })
    if ($active.Count -gt 0) { Refuse "Run $(($active | ForEach-Object { $_.RunId }) -join ', ') is active." }
    if ($null -ne $runsDirectoryProblem) { Refuse "The run folder '$runsDirectory' could not be listed ($runsDirectoryProblem)." }
}

$mode = if ($Execute) { 'EXECUTE' } else { 'DRY RUN (nothing is changed; add -Execute to act)' }

# ---- -RemoveDatabase ----------------------------------------------------------------------------------------------

if ($action -eq 'RemoveDatabase') {
    Assert-SafeToAct
    Write-Host "REMOVE DATABASE - $mode"
    $decisions = @($databaseNames | ForEach-Object { Get-DatabaseDecision $_ })
    foreach ($decision in $decisions) {
        Write-Host ''
        Write-Host "  Database:      $($decision.Name)"
        if ($null -ne $decision.Entry) {
            $entry = $decision.Entry
            Write-Host "  Owning run:    $($decision.Run.RunId) (manifest $($decision.Run.ManifestPath))"
            Write-Host "  Manifest hash: $($decision.Run.ManifestHash)"
            Write-Host "  Spec:          $($entry.spec); outcome $($entry.outcome); status $($entry.status); created $($entry.createdAt)"
            if ($entry.note) { Write-Host "  Note:          $($entry.note)" }
            Write-Host "  On server:     $(if ($sqlOk) { $serverSet.Contains($decision.Name) } else { 'unknown' })"
            Write-Host "  Label:         $($entry.__label)$(if ($decision.Interrupted) { ' — retained diagnostic / interrupted' })"
            Write-Host "  Run-start snapshot excludes it: $(-not (@($decision.Run.Manifest.preExistingAtRunStart) -icontains $decision.Name))"
        }
        if ($decision.Allowed) {
            if ($decision.Interrupted) {
                Write-Host "  Proposed:      record the entry as 'retained' (note 'run interrupted; marked retained by test-artifacts.ps1'; earlier status kept in history), then"
            }
            else { Write-Host '  Proposed:' -NoNewline }
            Write-Host "  remove-scenario-database.ps1 -DatabaseName $($decision.Name) -ManifestPath `"$($decision.Run.ManifestPath)`" -RemoveRetained"
        }
        else { Write-Host "  REFUSED:       $($decision.Reason)" }
    }
    $refused = @($decisions | Where-Object { -not $_.Allowed })
    if ($refused.Count -gt 0) { Refuse "$($refused.Count) of $($decisions.Count) name(s) cannot be removed (see above)." }
    if (-not $Execute) { Write-Host ''; Write-Host 'Dry run complete. Nothing was changed.'; exit 0 }

    foreach ($decision in $decisions) {
        $name = $decision.Name
        $manifestPath = $decision.Run.ManifestPath
        Write-Host ''
        if ($decision.Interrupted) {
            # Prove again, immediately before acting, that nothing of this run is alive and the manifest is unchanged.
            $processes = Get-ProcessTable
            if ($null -eq $processes) { Refuse 'The process list could not be read.' }
            if (Test-PortInUse) { Refuse "Port $apiPort is in use." }
            $runner = $decision.Run.Manifest.PSObject.Properties['runnerPid']
            if ($null -ne $runner -and (Test-ProcessOwned $runner.Value 'playwright')) { Refuse "The runner of $($decision.Run.RunId) is alive." }
            foreach ($other in $decision.Run.Entries) {
                if (Test-ProcessOwned $other.apiPid ($other.name.Substring('FamilySafety_Invitation_'.Length))) { Refuse "An API process of $($decision.Run.RunId) is alive (pid $($other.apiPid))." }
            }
            if ((Get-FileHash -LiteralPath $manifestPath -Algorithm SHA256).Hash -ne $decision.Run.ManifestHash) { Refuse "The manifest of $($decision.Run.RunId) changed during this command." }
            try {
                $manifest = [System.IO.File]::ReadAllText($manifestPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
                $record = @(@($manifest.databases) | Where-Object { [string]$_.name -ceq $name })[0]
                $note = 'run interrupted; marked retained by test-artifacts.ps1'
                $change = [ordered]@{ at = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fffZ'); from = [string]$record.status; to = 'retained'; by = 'test-artifacts.ps1'; note = $note }
                if ($null -ne $record.PSObject.Properties['note'] -and $record.note) { $change['previousNote'] = [string]$record.note }
                $history = @()
                if ($null -ne $record.PSObject.Properties['history']) { $history = @($record.history) }
                $history += [pscustomobject]$change
                $record.status = 'retained'
                foreach ($field in @('note', 'history')) {
                    $value = if ($field -eq 'note') { $note } else { [object[]]$history }
                    if ($null -ne $record.PSObject.Properties[$field]) { $record.$field = $value }
                    else { $record | Add-Member -NotePropertyName $field -NotePropertyValue $value }
                }
                Write-ManifestAtomically $manifestPath $manifest
                Write-Host "Recorded $name as retained (was '$($change.from)'; kept in history) in $manifestPath"
            }
            catch { Fail "Could not record $name as retained in $manifestPath ($($_.Exception.Message)). Nothing was removed." }
        }

        Write-Host "Delegating removal of $name to remove-scenario-database.ps1 (run $($decision.Run.RunId))"
        $output = & powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $removalScript `
            -DatabaseName $name -ManifestPath $manifestPath -Server $Server -RemoveRetained 2>&1
        $code = $LASTEXITCODE
        $output | ForEach-Object { Write-Host "  | $_" }
        if ($code -eq 2) { Write-Host "REFUSED by remove-scenario-database.ps1. '$name' was not removed."; exit 2 }
        if ($code -ne 0 -and $code -ne 4) { Fail "remove-scenario-database.ps1 exited $code for '$name'." }
        $check = & powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'list-scenario-databases.ps1') -Server $Server
        if ($LASTEXITCODE -ne 0) { Fail "Could not confirm that '$name' is gone." }
        if (@($check | Where-Object { "$_".Trim() -ieq $name }).Count -ne 0) { Fail "'$name' is still present after removal." }
        Write-Host "Confirmed absent: $name"
        if ($code -eq 4) { Write-Host "WARNING: the manifest of $($decision.Run.RunId) could not be updated."; exit 4 }
    }
    exit 0
}

# ---- -RemoveRunRecords -----------------------------------------------------------------------------------------------

if ($action -eq 'RemoveRunRecords') {
    Assert-SafeToAct
    Write-Host "REMOVE RUN RECORDS - $mode"
    if ($citationUncertain.Count -gt 0) {
        Write-Host "  Citation search uncertain, so every run counts as protected: $($citationUncertain[0])$(if ($citationUncertain.Count -gt 1) { " (+$($citationUncertain.Count - 1) more)" })"
    }
    $byId = @{}
    foreach ($run in $runs) { $byId[$run.RunId] = $run }
    $targets = @()
    if ($runIds.Count -gt 0) {
        foreach ($id in $runIds) {
            if (-not $byId.ContainsKey($id)) { Refuse "No run folder '$id' exists under $runsDirectory." }
            $run = $byId[$id]
            switch ($run.State) {
                'Invalid' { Refuse "Run $id has an unreadable or invalid manifest ($($run.Problem)); the tool never removes it." }
                'Active' { Refuse "Run $id is active." }
                'HoldingDatabases' { Refuse "Run $id still owns a database that exists ($(($run.Present | ForEach-Object { $_.name }) -join ', ')); its manifest is the ownership proof." }
                'InterruptedHoldingDatabases' { Refuse "Run $id was interrupted and its database still exists; remove the database first with -RemoveDatabase." }
            }
            $targets += $run
        }
    }
    else {
        $targets = @($runs | Where-Object { $_.State -eq 'Resolved' })
        foreach ($run in @($runs | Where-Object { $_.State -ne 'Resolved' })) {
            $why = switch ($run.State) {
                'Cited' { "cited ($($run.Citations[0])); exact -RunId + -Execute only" }
                'CitationUncertain' { 'citation search uncertain; exact -RunId + -Execute only' }
                'InterruptedNoDatabase' { 'interrupted run (diagnostic); exact -RunId + -Execute only' }
                'InterruptedHoldingDatabases' { 'interrupted run holding a database; kept' }
                'HoldingDatabases' { 'owns a database that exists; kept' }
                'Invalid' { "unreadable or invalid manifest ($($run.Problem)); kept" }
                'Active' { 'active; kept' }
            }
            Write-Host "  Kept:     $($run.RunId)  $why"
        }
    }
    foreach ($run in $targets) {
        $problem = Get-TargetProblem $run.Path $runsDirectory
        if ($null -ne $problem) { Refuse "Run folder $($run.RunId) is not safe to remove: $problem." }
        if ((Split-Path -Parent ([System.IO.Path]::GetFullPath($run.Path))) -ne [System.IO.Path]::GetFullPath($runsDirectory).TrimEnd('\')) { Refuse "Run folder $($run.RunId) is not directly inside $runsDirectory." }
        Write-Host "  Remove:   $($run.RunId)  [$($run.State)]  $(Format-Size $run.Bytes)  $($run.Path)"
    }
    if ($targets.Count -eq 0) { Write-Host '  Nothing to remove.'; exit 0 }
    if (-not $Execute) { Write-Host ''; Write-Host "Dry run complete. $($targets.Count) run folder(s) would be removed. Nothing was changed."; exit 0 }
    foreach ($run in $targets) {
        Write-Host "Removing run folder: $($run.Path)"
        try { Remove-Tree $run.Path }
        catch { Fail "Could not remove $($run.Path): $($_.Exception.Message)" }
        if (Test-Path -LiteralPath $run.Path) { Fail "$($run.Path) still exists after removal." }
        Write-Host "Confirmed absent: $($run.Path)"
    }
    exit 0
}

# ---- -ClearGenerated ---------------------------------------------------------------------------------------------------

if ($action -eq 'ClearGenerated') {
    Assert-SafeToAct
    Write-Host "CLEAR GENERATED FOLDERS - $mode"
    foreach ($target in $generatedTargets) {
        $problem = Get-TargetProblem $target.Path $webRoot
        if ($null -ne $problem) { Refuse "$($target.Label) is not safe to clear: $problem." }
    }
    $work = @()
    foreach ($target in $generatedTargets) {
        $summary = Get-FolderSummary $target.Path $target.Keep
        if (-not $summary.Exists) { Write-Host "  $($target.Label): absent"; continue }
        Write-Host ("  {0}: {1} file(s), {2} folder(s), {3}{4}" -f $target.Label, $summary.Files, $summary.Folders, (Format-Size $summary.Bytes), $(if ($target.Keep.Count) { " (keeps $($target.Keep -join ', '))" }))
        if ($summary.Items.Count -gt 0) { $work += $target }
    }
    if ($work.Count -eq 0) { Write-Host '  Nothing to clear.'; exit 0 }
    if (-not $Execute) { Write-Host ''; Write-Host 'Dry run complete. Nothing was changed.'; exit 0 }
    foreach ($target in $work) {
        Write-Host "Clearing: $($target.Path)"
        try { Remove-FolderContents $target.Path $target.Keep }
        catch { Fail "Could not clear $($target.Path): $($_.Exception.Message)" }
        $left = @((New-Object System.IO.DirectoryInfo $target.Path).EnumerateFileSystemInfos() | Where-Object { $target.Keep -notcontains $_.Name })
        if ($left.Count -gt 0) { Fail "$($target.Path) is not empty after clearing." }
        Write-Host "Confirmed empty: $($target.Path)"
    }
    exit 0
}

# ---- Status report (read-only) --------------------------------------------------------------------------------------------

$now = Get-Date
function Format-Age([string]$timestamp) {
    try {
        $at = [DateTime]::Parse($timestamp, [System.Globalization.CultureInfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::RoundtripKind).ToLocalTime()
        $span = $now - $at
        $age = if ($span.TotalDays -ge 1) { '{0:N0} d' -f [math]::Floor($span.TotalDays) } elseif ($span.TotalHours -ge 1) { '{0:N0} h' -f [math]::Floor($span.TotalHours) } else { '{0:N0} min' -f [math]::Floor($span.TotalMinutes) }
        return '{0} ({1})' -f $at.ToString('yyyy-MM-dd HH:mm'), $age
    }
    catch { return $timestamp }
}

Write-Host ("TEST ARTIFACT STATUS  (read-only; nothing was changed)        {0}" -f $now.ToString('yyyy-MM-dd HH:mm'))
$serverLine = if ($sqlOk) { "$($serverDatabases.Count) FamilySafety* databases" } else { "SQL SERVER UNREACHABLE ($sqlError) - every run counts as holding databases; no action is possible" }
Write-Host ("Server {0} - {1} - {2} run records - port {3} {4}" -f $Server, $serverLine, $runs.Count, $apiPort, $(if ($portInUse) { 'IN USE' } else { 'free' }))
Write-Host ''

Write-Host 'ACTIVE RUNS'
$activeRuns = @($validRuns | Where-Object { $_.Active })
if ($activeRuns.Count -eq 0 -and -not $portInUse) { Write-Host '  none' }
foreach ($run in $activeRuns) { Write-Host "  $($run.RunId)  (live runner or API process; every action refuses)" }
if ($portInUse) { Write-Host "  port $apiPort is in use (every action refuses)" }
if ($null -eq $processes) { Write-Host '  process list unavailable: runs without completedAt are treated as active' }
Write-Host ''

$finished = @($validRuns | Where-Object { -not $_.Active -and -not $_.Interrupted })
$successful = @($finished | Where-Object { $_.Present.Count -eq 0 -and $_.Entries.Count -gt 0 -and @($_.Entries | Where-Object { $_.outcome -ne 'passed' -or $_.status -ne 'removed' }).Count -eq 0 })
$failedHolding = @($finished | Where-Object { $_.Present.Count -gt 0 })
$otherFinished = @($finished | Where-Object { $successful -notcontains $_ -and $failedHolding -notcontains $_ })
$interruptedRuns = @($validRuns | Where-Object { $_.Interrupted })
Write-Host 'RUNS'
Write-Host ("  Successful, completed (no remaining database) ............ {0,3}" -f $successful.Count)
Write-Host ("  Finished with failed, kept or stale entries, no database .. {0,3}" -f $otherFinished.Count)
Write-Host ("  Holding retained databases ............................... {0,3}" -f $failedHolding.Count)
Write-Host ("  Interrupted .............................................. {0,3}" -f $interruptedRuns.Count)
Write-Host ("  Active ................................................... {0,3}" -f $activeRuns.Count)
Write-Host ("  Unreadable or invalid manifest ........................... {0,3}" -f $invalidRuns.Count)
Write-Host ''

Write-Host 'RETAINED DATABASES (proven run ownership; removed only by exact name + -Execute)'
$retainedRows = @()
foreach ($run in $validRuns) {
    foreach ($entry in $run.Entries) {
        if ($sqlOk -and $serverSet.Contains($entry.name) -and $entry.status -ne 'removed') {
            $decision = Get-DatabaseDecision $entry.name
            $label = $entry.__label
            if ($label -eq 'interrupted — exists') { $label = 'retained diagnostic / interrupted' }
            $retainedRows += [pscustomobject]@{ Run = $run.RunId; Database = $entry.name; Spec = ($entry.spec -replace '\.spec\.ts$', ''); Result = $entry.outcome
                Label = $label; Reason = $entry.note; Created = (Format-Age $entry.createdAt); Action = $(if ($decision.Allowed) { 'removable on request' } else { "no action: $($decision.Reason)" }); Allowed = $decision.Allowed }
        }
    }
}
if ($retainedRows.Count -eq 0) { Write-Host '  none' }
foreach ($row in $retainedRows) {
    Write-Host "  $($row.Database)"
    Write-Host "      run $($row.Run) - $($row.Spec) - result $($row.Result) - $($row.Label)$(if ($row.Reason) { " - reason: $($row.Reason)" })"
    Write-Host "      created $($row.Created) - $($row.Action)"
}
Write-Host ''

Write-Host 'INTERRUPTED RUNS (retained diagnostic / interrupted; never removed automatically)'
if ($interruptedRuns.Count -eq 0) { Write-Host '  none' }
foreach ($run in $interruptedRuns) {
    Write-Host "  $($run.RunId)  manifest $($run.ManifestPath)"
    Write-Host "      ownership evidence: manifest SHA-256 $($run.ManifestHash); no completedAt; runner and recorded API processes not alive; port $apiPort $(if ($portInUse) { 'in use' } else { 'free' })"
    foreach ($entry in $run.Entries) {
        $exists = if ($sqlOk) { $serverSet.Contains($entry.name) } else { 'unknown' }
        Write-Host "      $($entry.name)  spec $($entry.spec)  status $($entry.status)  exists: $exists  label: $($entry.__label)"
        if ($exists -eq $true -and $entry.status -ne 'removed') { Write-Host "        cleanup (dry run first): .\scripts\test-artifacts.ps1 -RemoveDatabase $($entry.name)   then add -Execute" }
    }
}
Write-Host ''

Write-Host 'MANIFEST / SERVER MISMATCHES (reported only; manifests are not rewritten)'
$mismatchLabels = @('retained in manifest — absent on server', 'creation never completed — absent', 'removed in manifest — PRESENT on server')
$mismatches = @()
foreach ($run in $validRuns) { foreach ($entry in $run.Entries) { if ($mismatchLabels -contains $entry.__label) { $mismatches += "  $($run.RunId)  $($entry.name)  $($entry.spec -replace '\.spec\.ts$', '')  $($entry.__label)" } } }
if ($mismatches.Count -eq 0) { Write-Host '  none' } else { $mismatches | ForEach-Object { Write-Host $_ } }
Write-Host ''

Write-Host 'RECONCILIATION LABELS (all recorded databases)'
$labelCounts = @{}
foreach ($run in $validRuns) { foreach ($entry in $run.Entries) { $labelCounts[$entry.__label] = 1 + [int]$labelCounts[$entry.__label] } }
if ($labelCounts.Count -eq 0) { Write-Host '  none' }
foreach ($label in ($labelCounts.Keys | Sort-Object)) { Write-Host ("  {0,-45} {1,3}" -f $label, $labelCounts[$label]) }
Write-Host ''

Write-Host 'RUN RECORDS'
$groups = [ordered]@{
    'Resolved, removable (stale) ..........................' = @($runs | Where-Object { $_.State -eq 'Resolved' })
    'Cited (protected; exact -RunId + -Execute only) .......' = @($runs | Where-Object { $_.State -eq 'Cited' })
    'Citation uncertain (protected; exact -RunId only) .....' = @($runs | Where-Object { $_.State -eq 'CitationUncertain' })
    'Interrupted, no database (exact -RunId only) ..........' = @($runs | Where-Object { $_.State -eq 'InterruptedNoDatabase' })
    'Holding databases (kept for ownership proof) ..........' = @($runs | Where-Object { $_.State -eq 'HoldingDatabases' -or $_.State -eq 'InterruptedHoldingDatabases' })
    'Unreadable or invalid (kept; never removed by the tool)' = @($invalidRuns)
    'Active (kept) .........................................' = @($activeRuns)
}
foreach ($group in $groups.Keys) {
    $members = @($groups[$group])
    Write-Host ("  {0} {1,3}   ({2})" -f $group, $members.Count, (Format-Size ([double](($members | Measure-Object Bytes -Sum).Sum))))
}
if ($citationUncertain.Count -gt 0) { Write-Host "  Citation search uncertain: $($citationUncertain -join '; ')" }
Write-Host ''
Write-Host 'STALE REMOVABLE RUN RECORDS'
$resolved = @($runs | Where-Object { $_.State -eq 'Resolved' })
if ($resolved.Count -eq 0) { Write-Host '  none' }
foreach ($run in $resolved) { Write-Host "  $($run.RunId)  $(Format-Size $run.Bytes)" }
foreach ($run in @($runs | Where-Object { $_.State -eq 'Cited' })) { Write-Host "  (protected) $($run.RunId)  cited: $($run.Citations[0])" }
foreach ($run in $invalidRuns) { Write-Host "  (kept) $($run.RunId)  invalid: $($run.Problem)" }
Write-Host ''

$protected = @($serverDatabases | Where-Object { $_ -ieq 'FamilySafety' -or ($null -ne $preExisting -and $preExisting -icontains $_) })
$recordedAnywhere = @($serverDatabases | Where-Object { $owners.ContainsKey($_.ToLowerInvariant()) })
$unknown = @($serverDatabases | Where-Object { $protected -notcontains $_ -and $recordedAnywhere -notcontains $_ })
$ambiguous = @($recordedAnywhere | Where-Object { $owners[$_.ToLowerInvariant()].Count -gt 1 })
Write-Host 'PROTECTED DATABASES (never removed)'
if ($sqlOk) {
    Write-Host ("  {0} present: FamilySafety{1} + {2} PRE-EXISTING" -f $protected.Count, $(if ($protected -icontains 'FamilySafety') { '' } else { ' (absent)' }), @($protected | Where-Object { $_ -ine 'FamilySafety' }).Count)
    $protected | ForEach-Object { Write-Host "    $_" }
}
else { Write-Host '  unknown (server not queried)' }
if ($null -eq $preExisting) { Write-Host "  WARNING: the PRE-EXISTING list is missing or empty: $preExistingListPath" }
Write-Host 'UNKNOWN OWNERSHIP (no action offered)'
if ($unknown.Count -eq 0) { Write-Host '  none' }
foreach ($name in $unknown) {
    $claims = Get-UnprovenClaims $name
    Write-Host "  $name$(if ($claims.Count) { "  (unproven: $($claims -join '; '))" })"
}
if ($ambiguous.Count -gt 0) { Write-Host 'AMBIGUOUS OWNERSHIP (recorded by several manifests; no action offered)'; $ambiguous | ForEach-Object { Write-Host "  $_" } }
Write-Host ''

Write-Host 'GENERATED FOLDERS (fixed approved locations only)'
foreach ($target in $generatedTargets) {
    $summary = Get-FolderSummary $target.Path $target.Keep
    if (-not $summary.Exists) { Write-Host ("  {0,-20} absent" -f $target.Label); continue }
    $problem = Get-TargetProblem $target.Path $webRoot
    Write-Host ("  {0,-20} {1} file(s), {2} folder(s), {3}  {4}" -f $target.Label, $summary.Files, $summary.Folders, (Format-Size $summary.Bytes), $(if ($problem) { "NOT clearable: $problem" } else { 'clearable' }))
}
$dist = Get-FolderSummary (Join-Path $webRoot 'dist') @()
Write-Host ("  {0,-20} {1}" -f 'dist', $(if ($dist.Exists) { "$($dist.Files) file(s), $(Format-Size $dist.Bytes)  (npm run build output; reported only, never cleared)" } else { 'absent' }))
Write-Host ("  {0,-20} {1} run folder(s), {2}  (see RUN RECORDS)" -f 'e2e/scenario-runs', $runs.Count, (Format-Size ([double](($runs | Measure-Object Bytes -Sum).Sum))))
Write-Host ''

Write-Host 'UNEXPECTED REPOSITORY-ROOT ARTIFACTS (never deleted)'
$rootEntries = @((New-Object System.IO.DirectoryInfo $root).EnumerateFileSystemInfos() | Where-Object { $expectedRootEntries -notcontains $_.Name } | Sort-Object Name)
if ($rootEntries.Count -eq 0) { Write-Host '  none' }
foreach ($item in $rootEntries) {
    $isFolder = [bool]($item.Attributes -band [System.IO.FileAttributes]::Directory)
    $size = if ($isFolder) { 'folder' } else { "file, $(Format-Size $item.Length)" }
    $label = if ($item.Name -ceq 'trips') { 'UNCLASSIFIED / OWNER REVIEW REQUIRED' } else { 'unclassified — review manually' }
    Write-Host ("  {0}   ({1}, {2})   {3}" -f $item.Name, $size, $item.LastWriteTime.ToString('yyyy-MM-dd'), $label)
}
Write-Host ''

Write-Host 'SAFE NEXT STEPS (dry run first; add -Execute only for what you intend)'
$steps = @()
foreach ($row in @($retainedRows | Where-Object { $_.Allowed })) { $steps += ".\scripts\test-artifacts.ps1 -RemoveDatabase $($row.Database)" }
if ($resolved.Count -gt 0) { $steps += '.\scripts\test-artifacts.ps1 -RemoveRunRecords' }
if (@($generatedTargets | Where-Object { (Get-FolderSummary $_.Path $_.Keep).Items.Count -gt 0 }).Count -gt 0) { $steps += '.\scripts\test-artifacts.ps1 -ClearGenerated' }
if ($steps.Count -eq 0) { Write-Host '  none' } else { $steps | ForEach-Object { Write-Host "  $_" } }
exit 0
