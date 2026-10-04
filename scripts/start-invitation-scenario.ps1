param(
    [string]$Server = 'localhost',
    [string]$ScenarioName = ([Guid]::NewGuid().ToString('N')),
    [switch]$IncludeTripAudienceAdult,
    # Seed "Development Second Family" (A and B active members) for cross-Family E2E specs only (WI-009-005, OD-E).
    [switch]$IncludeSecondFamily,
    # Reuse an existing API build (dotnet run --no-build). The Playwright harness builds once, then starts each spec's API with this switch.
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
if ($ScenarioName -notmatch '^[A-Za-z0-9_]+$') {
    throw 'ScenarioName may contain letters, digits, and underscores only.'
}
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ASPNETCORE_URLS = 'http://localhost:5080'
$env:ConnectionStrings__FamilySafety = "Server=$Server;Database=FamilySafety_Invitation_$ScenarioName;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
$env:DevelopmentFixtures__Enabled = 'true'
$env:DevelopmentFixtures__InvitationScenario = 'true'
$env:DevelopmentFixtures__TripAudienceScenario = if ($IncludeTripAudienceAdult) { 'true' } else { 'false' }
$env:DevelopmentFixtures__SecondFamilyScenario = if ($IncludeSecondFamily) { 'true' } else { 'false' }
$env:DevelopmentIdentity__Enabled = 'true'
$env:DevelopmentIdentity__AllowSwitching = 'true'
$env:DevelopmentIdentity__AccountId = '11111111-1111-1111-1111-111111111111'
$env:DevelopmentInvitationOutbox__Enabled = 'true'
$env:DevelopmentInvitationOutbox__ClientBaseUrl = 'http://localhost:4200'
Write-Host "Development invitation scenario: FamilySafety_Invitation_$ScenarioName"
Write-Host 'A is the administrator. B has an Account/Profile and no membership in a new scenario.'
if ($IncludeTripAudienceAdult) {
    Write-Host 'Adult C is included as an active Family member for the Trip audience scenario.'
}
if ($IncludeSecondFamily) {
    Write-Host 'Development Second Family is included: A and B are active members there; C is not.'
}
Write-Host 'Run again without ScenarioName for a fresh database; reuse this name to resume.'
$priorWarningsNotAsErrors = $env:WarningsNotAsErrors
try {
    $env:WarningsNotAsErrors = if ($priorWarningsNotAsErrors) {
        "$priorWarningsNotAsErrors;NU1900"
    } else {
        'NU1900'
    }
    $runArguments = @('run', '--project', "$PSScriptRoot\..\src\FamilySafety.Api\FamilySafety.Api.csproj", '--no-launch-profile')
    if ($NoBuild) {
        $runArguments += '--no-build'
    }
    dotnet @runArguments
}
finally {
    $env:WarningsNotAsErrors = $priorWarningsNotAsErrors
}
