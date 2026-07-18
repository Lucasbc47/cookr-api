# Cookr API

API de receitas pessoais: cada receita tem seus ingredientes, com quantidade e unidade.

Projeto de estudo em .NET 8 com Minimal API, EF Core e SQLite.

## Stack

- .NET 8 / Minimal API
- EF Core 8 + SQLite
- Serilog
- Swagger (dev)

## Rodar

```
dotnet run --project Cookr.Api
```

- API: `http://localhost:5021`
- Swagger: `http://localhost:5021/swagger` (só em Development)
- `GET /` responde a versão em execução

Banco SQLite em `Cookr.Api/Data/cookr.db`, criado via migrations:

```
dotnet ef database update --project Cookr.Infrastructure --startup-project Cookr.Api
```

## Endpoints

### Receitas

| Método | Rota           | Faz                               |
| ------ | -------------- | --------------------------------- |
| GET    | /recipes       | Lista receitas (id + título)      |
| GET    | /recipes/{id}  | Receita completa com ingredientes |
| POST   | /recipes       | Cria receita                      |
| PUT    | /recipes/{id}  | Atualiza receita                  |
| DELETE | /recipes/{id}  | Remove receita                    |

### Ingredientes da receita

| Método | Rota                                     | Faz                                   |
| ------ | ---------------------------------------- | ------------------------------------- |
| POST   | /recipes/{id}/ingredients                | Vincula ingrediente (quantity + unit) |
| DELETE | /recipes/{id}/ingredients/{ingredientId} | Desvincula ingrediente                |

### Ingredientes

| Método | Rota               | Faz                  |
| ------ | ------------------ | -------------------- |
| GET    | /ingredients       | Lista ingredientes   |
| GET    | /ingredients/{id}  | Ingrediente por id   |
| POST   | /ingredients       | Cria ingrediente     |
| PUT    | /ingredients/{id}  | Atualiza ingrediente |
| DELETE | /ingredients/{id}  | Remove ingrediente   |

Exemplos de request prontos em `Cookr.Api/Cookr.Api.http`.

## Arquitetura

```
cookr-api/
  Cookr.Api/            -> Web API, features em vertical slice
  Cookr.Domain/         -> entidades
  Cookr.Infrastructure/ -> DbContext + migrations
```

Cada feature vive numa pasta própria com 4 arquivos:

```
Features/Recipes/
  RecipeEndpoints.cs   -> rotas (MapGroup)
  RecipeService.cs     -> interface + service (injeta DbContext direto, sem repository)
  RecipeModels.cs      -> records de request/response
  RecipeMappings.cs    -> ToEntity() / ToResponse() / ApplyTo()
```

Endpoint nunca devolve entidade crua, sempre DTO. Sem camada de repository nem Application.
