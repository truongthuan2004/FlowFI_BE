# FlowFi Backend

Microservices backend for FlowFi personal finance management.

## Structure

```text
FlowFi
├── src
│   ├── FlowFi.ApiGateway
│   ├── FlowFi.WebSocketGateway
│   ├── FlowFi.AuthUserService
│   ├── FlowFi.FinanceCoreService
│   ├── FlowFi.AIProcessingService
│   ├── FlowFi.AnalyticsService
│   └── FlowFi.NotificationService
├── shared
│   ├── FlowFi.Contracts
│   ├── FlowFi.Common
│   ├── FlowFi.EventBus
│   └── FlowFi.GrpcContracts
├── deploy
│   ├── docker-compose.yml
│   ├── rabbitmq
│   ├── postgres
│   ├── redis
│   └── monitoring
├── FlowFi.sln
└── README.md
```

## Run

```powershell
Copy-Item .env.example .env
# Update every CHANGE_ME value in .env before starting the system.
.\scripts\build.ps1
docker compose --env-file .\.env -f .\deploy\docker-compose.yml up --build
.\scripts\migrate.ps1
```

All services load the root `.env` automatically when they run locally. Shared
settings use the `FLOWFI_SHARED__` prefix, while service-specific settings use
`FLOWFI_AUTH__`, `FLOWFI_FINANCE__`, `FLOWFI_AI__`, `FLOWFI_ANALYTICS__`,
`FLOWFI_NOTIFICATION__`, `FLOWFI_WEBSOCKET__`, or `FLOWFI_GATEWAY__`.
The real `.env` file is ignored by Git; only `.env.example` should be committed.

API Gateway:

```text
http://localhost:8080
```

Swagger Gateway:

```text
http://localhost:8080/docs
```

## Direct Service Testing

Each service has its own PostgreSQL container and can be tested directly:

| Service | API URL | Swagger | DB container | DB host port | DB name |
| --- | --- | --- | --- | --- | --- |
| AuthUser | `http://localhost:5101` | `http://localhost:5101/swagger` | `auth-db` | `6001` | `FlowFi_AuthUser` |
| FinanceCore | `http://localhost:5102` | `http://localhost:5102/swagger` | `finance-db` | `6002` | `FlowFi_FinanceCore` |
| AIProcessing | `http://localhost:5103` | `http://localhost:5103/swagger` | `ai-db` | `6000` | `FlowFi_AIProcessing` |
| Analytics | `http://localhost:5104` | `http://localhost:5104/swagger` | `analytics-db` | `6003` | `FlowFi_Analytics` |
| Notification | `http://localhost:5105` | `http://localhost:5105/swagger` | `notification-db` | `6004` | `FlowFi_Notification` |
| WebSocket Gateway | `http://localhost:5106` | n/a | n/a | n/a | n/a |

Run a single service for testing:

```powershell
docker compose --env-file .\.env -f .\deploy\docker-compose.yml up --build ai-db ai-service
```

Then apply migrations:

```powershell
.\scripts\migrate.ps1 -Service ai
```

Or run a service directly. It automatically loads the root `.env`:

```powershell
docker compose --env-file .\.env -f .\deploy\docker-compose.yml up ai-db
.\scripts\migrate.ps1 -Service ai
dotnet run --project .\src\FlowFi.AIProcessingService\FlowFi.AIProcessingService.csproj
```
