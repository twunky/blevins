<#
.SYNOPSIS
Drops ONE Playwright scenario database, only when the given run manifest proves this run created it.

.DESCRIPTION
WI-009-001 guarded cleanup. Every check below must pass, or nothing is dropped and the script exits non-zero:

  1. -DatabaseName is supplied and not blank.
  2. The name matches ^FamilySafety_Invitation_E2E_[A-Za-z0-9]+$ exactly (no wildcard, no prefix).
  3. The name is not FamilySafety.
  4. The PRE-EXISTING list file is readable and does not contain the name.
  5. The run manifest is supplied, readable, and well formed (fail closed otherwise).
  6. The name is not in the manifest's run-start snapshot (preExistingAtRunStart).
  7. The manifest records the name exactly once (unambiguous ownership).
  8. Automatic cleanup: the recorded spec outcome is "passed" and the status is still "running" (cleanup has not
     been attempted). A database recorded as "retained" or "cleanup-failed" is removed only by a person passing
     -RemoveRetained, with the manifest of the run that created it; it is never retried automatically.
  9. The database exists exactly once on the server and has no other open sessions.
 10. The exact name is logged before ALTER/DROP runs.

The name is passed to SQL Server as a parameter and quoted there with QUOTENAME; it is never concatenated
into SQL text. Windows integrated security is used; no credentials are embedded.

After a confirmed drop (WI-009-002), the manifest entry's current status becomes "removed", with removedAt,
removedBy ("automatic", or "manual" with -RemoveRetained), and an appended history entry that keeps the previous
status. The manifest is written atomically (temporary file, then replace).

Exit codes: 0 = dropped and confirmed gone; 2 = refused (unsafe input or ownership not proven); 3 = SQL or drop failure;
4 = dropped and confirmed gone, but the manifest could not be updated (the name is printed).

.EXAMPLE
.\scripts\remove-scenario-database.ps1 -DatabaseName FamilySafety_Invitation_E2E_0123abcd -ManifestPath <run>\manifest.json -RemoveRetained
#>
param(
    [string]$DatabaseName,
    [string]$ManifestPath,
    [string]$PreExistingListPath = (Join-Path $PSScriptRoot 'pre-existing-scenario-databases.txt'),
    [string]$Server = 'localhost',
    [switch]$RemoveRetained
)

$ErrorActionPreference = 'Stop'
$approvedPattern = '^FamilySafety_Invitation_E2E_[A-Za-z0-9]+$'

function Refuse([string]$reason) {
    Write-Host "REFUSED: $reason Nothing was dropped."
    exit 2
}

function Fail([string]$reason) {
    Write-Host "FAILED: $reason"
    exit 3
}

# 1. Explicit, non-blank name.
if ([string]::IsNullOrWhiteSpace($DatabaseName)) {
    Refuse 'No database name was supplied.'
}
# 3. Never the ordinary development database (checked first so the refusal names it explicitly).
if ($DatabaseName.Trim() -ieq 'FamilySafety') {
    Refuse 'FamilySafety is never removed.'
}
# 2. Exact approved pattern (case-sensitive, anchored).
if ($DatabaseName -cnotmatch $approvedPattern) {
    Refuse "'$DatabaseName' does not match the approved pattern $approvedPattern."
}

# 4. PRE-EXISTING list: required, and the name must not be on it.
if ([string]::IsNullOrWhiteSpace($PreExistingListPath) -or -not (Test-Path -LiteralPath $PreExistingListPath -PathType Leaf)) {
    Refuse "The PRE-EXISTING list '$PreExistingListPath' is missing or unreadable."
}
try {
    $preExisting = @(Get-Content -LiteralPath $PreExistingListPath | ForEach-Object { $_.Trim() } | Where-Object { $_ -and -not $_.StartsWith('#') })
}
catch {
    Refuse "The PRE-EXISTING list '$PreExistingListPath' could not be read."
}
if ($preExisting.Count -eq 0) {
    Refuse "The PRE-EXISTING list '$PreExistingListPath' is empty; it must be enumerated first."
}
if ($preExisting -icontains $DatabaseName) {
    Refuse "'$DatabaseName' is PRE-EXISTING."
}

# 5. Run manifest: required and well formed.
if ([string]::IsNullOrWhiteSpace($ManifestPath) -or -not (Test-Path -LiteralPath $ManifestPath -PathType Leaf)) {
    Refuse "The run ownership manifest '$ManifestPath' is missing or unreadable."
}
try {
    $manifest = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
}
catch {
    Refuse "The run ownership manifest '$ManifestPath' is not valid JSON."
}
if ($null -eq $manifest -or [string]::IsNullOrWhiteSpace([string]$manifest.runId) `
        -or $null -eq $manifest.databases -or $null -eq $manifest.preExistingAtRunStart) {
    Refuse "The run ownership manifest '$ManifestPath' is not a WI-009-001 run manifest."
}

# 6. Not present when the run started.
if (@($manifest.preExistingAtRunStart) -icontains $DatabaseName) {
    Refuse "'$DatabaseName' already existed when run $($manifest.runId) started."
}

# 7. Recorded exactly once, with the exact spelling.
$entries = @(@($manifest.databases) | Where-Object { [string]$_.name -ieq $DatabaseName })
if ($entries.Count -ne 1) {
    Refuse "Run $($manifest.runId) records '$DatabaseName' $($entries.Count) time(s); ownership requires exactly one record."
}
$entry = $entries[0]
if ([string]$entry.name -cne $DatabaseName) {
    Refuse "The manifest spelling '$($entry.name)' differs from '$DatabaseName'."
}

# 8. Spec outcome.
if ($entry.status -eq 'removed') {
    Refuse "'$DatabaseName' is already recorded as removed."
}
if ($RemoveRetained) {
    if ($entry.status -ne 'retained' -and $entry.status -ne 'cleanup-failed') {
        Refuse "-RemoveRetained applies only to a database recorded as retained or cleanup-failed (status is '$($entry.status)')."
    }
}
else {
    if ($entry.outcome -ne 'passed') {
        Refuse "The spec '$($entry.spec)' outcome is '$($entry.outcome)', not 'passed'. Failed-spec databases are retained."
    }
    # Automatic cleanup happens once, right after the spec's API stops. Anything retained, or a failed cleanup,
    # is removed only deliberately with -RemoveRetained; it is never retried automatically.
    if ($entry.status -ne 'running') {
        Refuse "'$DatabaseName' is recorded as '$($entry.status)'. Use -RemoveRetained to remove a retained database deliberately."
    }
}

# 9 and 10. Exists once, no other sessions, log exact name, drop, confirm.
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$builder['Server'] = $Server
$builder['Database'] = 'master'
$builder['Integrated Security'] = $true
$builder['TrustServerCertificate'] = $true
$connection = New-Object System.Data.SqlClient.SqlConnection $builder.ConnectionString

function Invoke-Scalar([string]$sql) {
    $command = $connection.CreateCommand()
    $command.CommandText = $sql
    $null = $command.Parameters.Add('@name', [System.Data.SqlDbType]::NVarChar, 128)
    $command.Parameters['@name'].Value = $DatabaseName
    return $command.ExecuteScalar()
}

try {
    $connection.Open()

    $matching = [int](Invoke-Scalar 'SELECT COUNT(*) FROM sys.databases WHERE name = @name')
    if ($matching -ne 1) {
        Fail "'$DatabaseName' was found $matching time(s) on $Server; expected exactly one. Nothing was dropped."
    }

    # The spec's API must already be stopped. Allow a short drain for closed connections; never kill them here first.
    $sessions = 0
    for ($attempt = 0; $attempt -lt 15; $attempt++) {
        $sessions = [int](Invoke-Scalar 'SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE database_id = DB_ID(@name) AND session_id <> @@SPID')
        if ($sessions -eq 0) { break }
        Start-Sleep -Seconds 1
    }
    if ($sessions -ne 0) {
        Fail "'$DatabaseName' still has $sessions open session(s); is its API still running? Nothing was dropped."
    }

    Write-Host "Dropping scenario database: $DatabaseName (server $Server, run $($manifest.runId), spec $($entry.spec))"
    $null = Invoke-Scalar @'
SET NOCOUNT ON;
DECLARE @quoted nvarchar(260) = QUOTENAME(@name);
IF @quoted IS NULL OR DB_ID(@name) IS NULL
    THROW 50001, N'Scenario database not found; nothing dropped.', 1;
EXEC (N'ALTER DATABASE ' + @quoted + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE;');
EXEC (N'DROP DATABASE ' + @quoted + N';');
SELECT 1;
'@

    $remaining = [int](Invoke-Scalar 'SELECT COUNT(*) FROM sys.databases WHERE name = @name')
    if ($remaining -ne 0) {
        Fail "'$DatabaseName' still exists after DROP DATABASE."
    }
    Write-Host "Removed scenario database: $DatabaseName"
}
catch {
    Fail "SQL error while removing '$DatabaseName': $($_.Exception.Message)"
}
finally {
    $connection.Dispose()
}

# WI-009-002: record the confirmed drop in the manifest. Earlier fields are kept; the previous status goes to history.
try {
    $removedBy = if ($RemoveRetained) { 'manual' } else { 'automatic' }
    $now = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fffZ')
    $fullManifestPath = (Resolve-Path -LiteralPath $ManifestPath).ProviderPath
    $current = [System.IO.File]::ReadAllText($fullManifestPath, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
    $recorded = @(@($current.databases) | Where-Object { [string]$_.name -ceq $DatabaseName })
    if ($recorded.Count -ne 1) {
        throw "the manifest now records '$DatabaseName' $($recorded.Count) time(s)"
    }
    $record = $recorded[0]
    $history = @()
    if ($null -ne $record.PSObject.Properties['history']) { $history = @($record.history) }
    $history += [pscustomobject]@{
        at   = $now
        from = [string]$record.status
        to   = 'removed'
        by   = $removedBy
        note = 'dropped by remove-scenario-database.ps1 and confirmed absent'
    }
    $fields = [ordered]@{ status = 'removed'; removedAt = $now; removedBy = $removedBy; history = [object[]]$history }
    foreach ($field in $fields.Keys) {
        # Existing fields keep their position; new fields are appended.
        if ($null -ne $record.PSObject.Properties[$field]) { $record.$field = $fields[$field] }
        else { $record | Add-Member -NotePropertyName $field -NotePropertyValue $fields[$field] }
    }

    $temporary = "$fullManifestPath.tmp"
    [System.IO.File]::WriteAllText($temporary, (ConvertTo-Json -InputObject $current -Depth 20), (New-Object System.Text.UTF8Encoding $false))
    # [NullString]::Value: a plain $null would reach .NET as "" ("The path is not of a legal form").
    [System.IO.File]::Replace($temporary, $fullManifestPath, [NullString]::Value)
    Write-Host "Manifest updated: $DatabaseName recorded as removed ($removedBy)."
    exit 0
}
catch {
    if ($temporary -and (Test-Path -LiteralPath $temporary -PathType Leaf)) { Remove-Item -LiteralPath $temporary -Force -ErrorAction SilentlyContinue }
    Write-Host "WARNING: '$DatabaseName' was dropped and confirmed gone, but the manifest '$ManifestPath' could not be updated: $($_.Exception.Message)"
    exit 4
}
