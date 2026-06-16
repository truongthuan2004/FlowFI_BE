param(
  [ValidateSet("auth", "finance", "ai", "analytics", "notification", "all")]
  [string] $Service = "all"
)

$ErrorActionPreference = "Stop"

$composeFile = "deploy/docker-compose.yml"

$postgresUser = $env:POSTGRES_USER
if (-not $postgresUser) { $postgresUser = "flowfi" }

$postgresPassword = $env:POSTGRES_PASSWORD
if (-not $postgresPassword) { $postgresPassword = "74780102" }

$authDbName = $env:AUTH_DB_NAME
if (-not $authDbName) { $authDbName = "FlowFi_AuthUser" }

$financeDbName = $env:FINANCE_DB_NAME
if (-not $financeDbName) { $financeDbName = "FlowFi_FinanceCore" }

$aiDbName = $env:AI_DB_NAME
if (-not $aiDbName) { $aiDbName = "FlowFi_AIProcessing" }

$analyticsDbName = $env:ANALYTICS_DB_NAME
if (-not $analyticsDbName) { $analyticsDbName = "FlowFi_Analytics" }

$notificationDbName = $env:NOTIFICATION_DB_NAME
if (-not $notificationDbName) { $notificationDbName = "FlowFi_Notification" }

$migrations = @(
  @{ Name = "auth"; DbService = "auth-db"; Database = $authDbName; Path = "deploy/postgres/migrations/auth" },
  @{ Name = "finance"; DbService = "finance-db"; Database = $financeDbName; Path = "deploy/postgres/migrations/finance" },
  @{ Name = "ai"; DbService = "ai-db"; Database = $aiDbName; Path = "deploy/postgres/migrations/ai" },
  @{ Name = "analytics"; DbService = "analytics-db"; Database = $analyticsDbName; Path = "deploy/postgres/migrations/analytics" },
  @{ Name = "notification"; DbService = "notification-db"; Database = $notificationDbName; Path = "deploy/postgres/migrations/notification" }
)

$selectedMigrations = if ($Service -eq "all") {
  $migrations
} else {
  $migrations | Where-Object { $_.Name -eq $Service }
}

foreach ($migrationSet in $selectedMigrations) {
  Get-ChildItem -LiteralPath $migrationSet.Path -Filter "*.sql" | Sort-Object Name | ForEach-Object {
    Write-Host "Applying $($_.Name) to $($migrationSet.Database) on $($migrationSet.DbService)"
    Get-Content -LiteralPath $_.FullName -Raw |
      docker compose -f $composeFile exec -T -e PGPASSWORD=$postgresPassword $migrationSet.DbService psql -U $postgresUser -d $migrationSet.Database
  }
}
