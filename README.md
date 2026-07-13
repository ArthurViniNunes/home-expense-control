# Controle de Gastos Residenciais

Aplicação full stack para cadastro de pessoas, registro de receitas e despesas e consulta de totais financeiros residenciais.

O projeto é desenvolvido de forma incremental. Cada commit representa uma evolução pequena, funcional e compreensível do produto.

## Status

### Back-end

Implementado:

- cadastro, listagem, consulta e exclusão de pessoas;
- cadastro, listagem e consulta de transações;
- exclusão automática das transações ao remover uma pessoa;
- restrição de receitas para menores de 18 anos;
- totais individuais e gerais;
- persistência com SQLite;
- documentação OpenAPI;
- interface interativa com Scalar;
- testes automatizados de domínio, serviços, persistência e contrato OpenAPI.

### Front-end

Próxima etapa:

- React;
- TypeScript;
- Vite;
- ESLint;
- integração com a API.

## Tecnologias

### Back-end

- .NET 10;
- ASP.NET Core Web API;
- Entity Framework Core;
- SQLite;
- OpenAPI;
- Scalar;
- xUnit.

### Front-end

- React;
- TypeScript;
- Vite;
- ESLint.

## Estrutura do repositório

```text
home-expense-control/
├── src/
│   ├── backend/
│   │   └── HomeExpenseControl.Api/
│   └── frontend/
├── tests/
│   └── HomeExpenseControl.Api.Tests/
├── docs/
│   ├── architecture.md
│   └── business-rules.md
├── HomeExpenseControl.sln
└── README.md
```

## Pré-requisitos

- .NET SDK 10;
- Node.js;
- npm.

## Configuração inicial

Restaure as ferramentas e dependências do back-end:

```bash
dotnet tool restore
dotnet restore
```

Instale as dependências do front-end:

```bash
cd src/frontend
npm install
cd ../..
```

## Banco de dados

O projeto utiliza SQLite.

As migrations são versionadas, mas o arquivo físico do banco não faz parte do repositório.

Para aplicar as migrations:

```bash
dotnet ef database update   --project src/backend/HomeExpenseControl.Api   --startup-project src/backend/HomeExpenseControl.Api
```

## Executando a API

Na raiz do repositório:

```bash
dotnet run --project src/backend/HomeExpenseControl.Api
```

O endereço utilizado será exibido no terminal.

Em ambiente de desenvolvimento:

```text
/scalar
/openapi/v1.json
```

## Executando os testes

```bash
dotnet test
```

Verificações completas do back-end:

```bash
dotnet format --verify-no-changes
dotnet build
dotnet test
```

Verificação de dependências vulneráveis:

```bash
dotnet list HomeExpenseControl.sln package --vulnerable --include-transitive
```

## Executando o front-end

```bash
cd src/frontend
npm run dev
```

Verificações do front-end:

```bash
npm run lint
npm run build
```

## Endpoints

### Pessoas

```http
POST   /api/people
GET    /api/people
GET    /api/people/{id}
DELETE /api/people/{id}
```

### Transações

```http
POST /api/transactions
GET  /api/transactions
GET  /api/transactions/{id}
```

### Totais

```http
GET /api/totals
```

## Exemplos

### Cadastrar pessoa

```json
{
  "name": "Arthur Nunes",
  "age": 22
}
```

### Cadastrar despesa

```json
{
  "description": "Conta de energia",
  "amount": 145.34,
  "type": "expense",
  "personId": 1
}
```

### Cadastrar receita

```json
{
  "description": "Salário",
  "amount": 3500,
  "type": "income",
  "personId": 1
}
```

## Regras principais

- O nome da pessoa é obrigatório.
- A idade não pode ser negativa.
- Toda transação deve pertencer a uma pessoa existente.
- O valor da transação deve ser maior que zero.
- O valor pode possuir no máximo duas casas decimais.
- Menores de 18 anos podem registrar somente despesas.
- Ao excluir uma pessoa, suas transações são excluídas automaticamente.
- Pessoas sem transações aparecem na consulta de totais com valores zerados.
- O saldo é calculado como receitas menos despesas.

## Valores monetários

A API trabalha com valores decimais, mas os dados são persistidos como centavos inteiros.

```text
R$ 145,34 = 14.534 centavos
```

Essa decisão evita perda de precisão e mantém as agregações consistentes.

## Contrato JSON

O tipo da transação é textual:

```text
expense
income
```

Valores numéricos como `1` e `2` são rejeitados.

Números também devem ser enviados como números JSON, não como strings.

## Tratamento de erros

As respostas de erro seguem o padrão `ProblemDetails`.

| Situação | Status |
|---|---:|
| Dados inválidos | `400 Bad Request` |
| Recurso não encontrado | `404 Not Found` |
| Regra de negócio violada | `422 Unprocessable Entity` |
| Erro inesperado | `500 Internal Server Error` |

## Documentação técnica

- [`docs/architecture.md`](docs/architecture.md)
- [`docs/business-rules.md`](docs/business-rules.md)

## Princípios de desenvolvimento

- Regras de negócio protegidas pelo back-end.
- Entidades não expostas diretamente na API.
- Controllers pequenos.
- Organização por funcionalidade.
- Comentários para explicar decisões, não linhas óbvias.
- Testes com nomes descritivos.
- Commits pequenos e com responsabilidade única.
- Código executável ao longo do histórico do Git.