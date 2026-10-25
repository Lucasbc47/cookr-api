<div align="center">

# Cookr API

**API REST do Cookr, um app de receitas pessoais.**

![Build](https://github.com/Lucasbc47/cookr-api/actions/workflows/build.yml/badge.svg)

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![Minimal API](https://img.shields.io/badge/Minimal_API-ASP.NET_Core-512BD4)
![EF Core](https://img.shields.io/badge/EF_Core-10-512BD4)
![SQLite](https://img.shields.io/badge/SQLite-003B57?logo=sqlite&logoColor=white)
![FluentValidation](https://img.shields.io/badge/FluentValidation-2C3E50)
![Serilog](https://img.shields.io/badge/Serilog-1E88E5)
![Scalar](https://img.shields.io/badge/Docs-Scalar-1A1A1A)

</div>

## Rodar

Requer o .NET 10 SDK (versão definida no `global.json`).

```
dotnet run --project Cookr.Api
```

> _Em Development, as migrations são aplicadas sozinhas ao subir a API (`MigrateAndSeedAsync` no `Program.cs`)._

- API: `http://localhost:5021`
- Scalar: `http://localhost:5021/scalar`

## Testes

```
dotnet test
```

## Estrutura

```
Cookr.Domain/          entidades e suas regras
Cookr.Infrastructure/  DbContext e migrations
Cookr.Api/             endpoints, services, DTOs e validação, por feature
Cookr.Api.Tests/       testes de domínio, validação e integração
```

## Endpoints

Exemplo `GET /recipes/1`:

```json
{
  "id": 1,
  "title": "Bolo de cenoura",
  "instructions": "Bata cenoura, ovos e óleo no liquidificador...",
  "prepTimeMinutes": 60,
  "servings": 8,
  "ingredients": [
    { "ingredientId": 1, "name": "Cenoura", "quantity": 3, "unit": "unit" },
    { "ingredientId": 3, "name": "Farinha de trigo", "quantity": 2, "unit": "cup" }
  ]
}
```

<details>
<summary><b>Receitas</b></summary>

| Método | Rota | Ação |
| --- | --- | --- |
| `GET` | `/recipes` | Lista receitas |
| `GET` | `/recipes/{id}` | Receita com ingredientes |
| `POST` | `/recipes` | Cria receita |
| `PUT` | `/recipes/{id}` | Atualiza receita |
| `DELETE` | `/recipes/{id}` | Remove receita |
| `POST` | `/recipes/{id}/ingredients` | Adiciona ingrediente |
| `DELETE` | `/recipes/{id}/ingredients/{ingredientId}` | Remove ingrediente da receita |

</details>

<details>
<summary><b>Ingredientes</b></summary>

| Método | Rota | Ação |
| --- | --- | --- |
| `GET` | `/ingredients` | Lista ingredientes |
| `GET` | `/ingredients/{id}` | Ingrediente por id |
| `POST` | `/ingredients` | Cria ingrediente |
| `PUT` | `/ingredients/{id}` | Atualiza ingrediente |
| `DELETE` | `/ingredients/{id}` | Remove ingrediente (409 se estiver em uso) |

</details>
