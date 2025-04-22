# CallRecordIntelligenceBE

- [Local environment](#local-environment)

## Local environment
1. Use docker compose to setup a local database:
```
docker-compose -f dev/docker-compose.yaml up -d
```

2. Apply the latest database migrations:
```
dotnet ef database update --project CallRecordIntelligence.EF --startup-project CallRecordIntelligence.API

```