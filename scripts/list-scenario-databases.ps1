<#
.SYNOPSIS
Lists FamilySafety* databases on the local SQL Server, one exact name per line. Read-only.

.DESCRIPTION
Used by the Playwright harness (WI-009-001) to snapshot the databases that exist when a run starts,
and to confirm that a new scenario name is unused. It never changes anything on the server.
Uses Windows integrated security, as the scenario script and SQL-gated tests do; no credentials are embedded.
#>
param(
    [string]$Server = 'localhost'
)

$ErrorActionPreference = 'Stop'
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$builder['Server'] = $Server
$builder['Database'] = 'master'
$builder['Integrated Security'] = $true
$builder['TrustServerCertificate'] = $true

$connection = New-Object System.Data.SqlClient.SqlConnection $builder.ConnectionString
try {
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT name FROM sys.databases WHERE name LIKE N'FamilySafety%' ORDER BY name"
    $reader = $command.ExecuteReader()
    while ($reader.Read()) {
        Write-Output $reader.GetString(0)
    }
    $reader.Close()
}
finally {
    $connection.Dispose()
}
