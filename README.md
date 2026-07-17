# Cookr.Api

API do Cookr - .NET 8, Minimal API

## Estrutura

```
cookr-api/
  Cookr.sln
  Cookr.Api/       -> Web API (Minimal API + Serilog)
  Cookr.Domain/    -> Class library (entidades)
```

## Solution (.sln)

```
dotnet new sln -n Cookr
dotnet sln add Cookr.Api/Cookr.Api.csproj
dotnet sln list
dotnet build Cookr.sln
```

## Criar projetos novos

```
dotnet new webapi -n Cookr.Api -o Cookr.Api
dotnet new webapi -n Cookr.Api -o Cookr.Api -controllers
```

## Referência entre projetos

```
dotnet add Cookr.Api/Cookr.Api.csproj reference Cookr.Domain/Cookr.Domain.csproj
```

## Pacotes (NuGet)

```
dotnet add Cookr.Api package Serilog.AspNetCore
dotnet list Cookr.Api package
dotnet remove Cookr.Api package [....]
```

## Rodar / buildar

```
dotnet run --project Cookr.Api
dotnet run
dotnet build
dotnet watch run --project Cookr.Api
```
