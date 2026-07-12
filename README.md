# Controle de Gastos Residenciais

Aplicação full stack para gerenciamento de pessoas, transações e totais financeiros residenciais.

O projeto está sendo desenvolvido de forma incremental. Cada commit do Git representa uma etapa pequena, funcional e compreensível da evolução do produto.

## Status do projeto

Etapa atual: Implementação da funcionalidade de cadastro, listagem e exclusão de pessoas.

As funcionalidades de negócio serão implementadas nos próximos commits.

## Funcionalidades previstas

A aplicação permitirá:

- Cadastro, listagem e exclusão de pessoas.
- Exclusão automática das transações vinculadas a uma pessoa removida.
- Cadastro e listagem de transações.
- Classificação das transações como receita ou despesa.
- Restrição que impede menores de idade de cadastrarem receitas.
- Consulta dos totais financeiros de cada pessoa.
- Consulta dos totais gerais da residência.
- Persistência local dos dados.

## Tecnologias

### Back-end

- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQLite
- OpenAPI com Scalar
- xUnit

### Front-end

- React
- TypeScript
- Vite
- ESLint

## Estrutura do repositório

```txt
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

Antes de executar o projeto, instale:

- SDK do .NET compatível com o framework definido no projeto.
- Node.js.
- npm.

## Executando o back-end

Na raiz do repositório:

```bash
dotnet restore
dotnet run --project src/backend/HomeExpenseControl.Api
```

O terminal exibirá o endereço local utilizado pela API.

## Executando o front-end

Na raiz do repositório:

```bash
cd src/frontend
npm install
npm run dev
```

O terminal exibirá o endereço local utilizado pela aplicação React.

## Executando os testes

Na raiz do repositório:

```bash
dotnet test
```

## Executando as verificações de qualidade

Back-end:

```bash
dotnet build
dotnet test
```

Front-end:

```bash
cd src/frontend
npm run lint
npm run build
```

## Documentação

A documentação técnica adicional está disponível em:

- [`docs/architecture.md`](docs/architecture.md)
- [`docs/business-rules.md`](docs/business-rules.md)

## Princípios de desenvolvimento

Este projeto segue os seguintes princípios:

- As regras de negócio devem ser protegidas pelo back-end.
- Os controllers devem permanecer focados em responsabilidades HTTP.
- Cálculos monetários não devem utilizar tipos de ponto flutuante.
- Os contratos públicos da API não devem expor diretamente as entidades de persistência.
- Os nomes devem comunicar intenção sem exigir comentários desnecessários.
- Os comentários devem explicar decisões e comportamentos não óbvios.
- Cada commit deve possuir uma única responsabilidade clara.
- A aplicação deve permanecer executável durante sua evolução no histórico do Git.
