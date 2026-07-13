# Arquitetura

## 1. Visão geral

O **Controle de Gastos Residenciais** é uma aplicação full stack para cadastro de pessoas, registro de receitas e despesas e consulta de totais financeiros.

O projeto está sendo desenvolvido de forma incremental. Cada commit representa uma evolução pequena e compreensível do produto, permitindo acompanhar as decisões técnicas e funcionais pelo histórico do Git.

No estado atual, o back-end está implementado com:

- .NET 10 e ASP.NET Core Web API;
- Entity Framework Core;
- SQLite;
- OpenAPI nativo do ASP.NET Core;
- Scalar para visualização e teste da API;
- xUnit;
- testes com SQLite em memória;
- testes de contrato do documento OpenAPI.

O front-end será desenvolvido com React e TypeScript após a consolidação do back-end.

## 2. Objetivos arquiteturais

A arquitetura foi definida para:

- manter as regras de negócio fáceis de localizar;
- manter os controllers focados em responsabilidades HTTP;
- separar contratos públicos das entidades internas;
- proteger o domínio contra estados inválidos;
- centralizar comportamentos compartilhados;
- permitir que cada funcionalidade evolua de forma independente;
- facilitar testes unitários, de integração e de contrato;
- evitar abstrações que não agregariam valor ao escopo atual.

## 3. Abordagem adotada

O back-end utiliza uma abordagem de **monólito modular organizado por funcionalidade**.

Essa escolha oferece separação de responsabilidades sem introduzir a complexidade de múltiplos projetos, microsserviços, barramentos de eventos ou frameworks de mediação.

```text
HomeExpenseControl.Api/
├── Common/
│   ├── Errors/
│   ├── Money/
│   ├── OpenApi/
│   └── Serialization/
├── Domain/
│   ├── Entities/
│   └── Enums/
├── Features/
│   ├── People/
│   │   ├── Contracts/
│   │   ├── PeopleController.cs
│   │   └── PeopleService.cs
│   ├── Transactions/
│   │   ├── Contracts/
│   │   ├── TransactionsController.cs
│   │   └── TransactionsService.cs
│   └── Totals/
│       ├── Contracts/
│       ├── TotalsController.cs
│       └── TotalsService.cs
├── Infrastructure/
│   └── Persistence/
│       ├── Configurations/
│       ├── Migrations/
│       └── AppDbContext.cs
└── Program.cs
```

## 4. Responsabilidades por área

### 4.1 Common

Contém comportamentos compartilhados por mais de uma funcionalidade.

Atualmente inclui:

- exceções da aplicação;
- tratamento centralizado de exceções;
- conversão de valores monetários;
- convenções para o documento OpenAPI;
- serialização do tipo de transação.

Um componente só deve ser colocado nessa área quando possuir reutilização real.

### 4.2 Domain

Contém os conceitos centrais de negócio:

- `Person`;
- `Transaction`;
- `TransactionType`.

As entidades possuem setters privados e validam os dados necessários para impedir a criação de estados inválidos.

Exemplos de regras protegidas no domínio:

- nome obrigatório;
- idade não negativa;
- descrição obrigatória;
- valor positivo;
- limite de duas casas decimais;
- tipo de transação válido;
- proibição de receitas para menores de idade.

### 4.3 Features

Cada pasta representa uma capacidade da aplicação.

#### People

Responsável por:

- cadastrar pessoas;
- listar pessoas;
- consultar uma pessoa pelo identificador;
- excluir pessoas.

#### Transactions

Responsável por:

- cadastrar transações;
- listar transações;
- consultar uma transação pelo identificador;
- verificar a existência da pessoa vinculada;
- aplicar a restrição de receita para menores de idade.

#### Totals

Responsável por:

- calcular receitas, despesas e saldo de cada pessoa;
- incluir pessoas sem transações;
- calcular receitas, despesas e saldo líquido gerais.

### 4.4 Infrastructure

Contém os detalhes de persistência:

- `AppDbContext`;
- configurações das entidades;
- migrations;
- configuração do SQLite.

As regras de negócio não devem depender diretamente de detalhes específicos do banco.

## 5. Convenção de nomenclatura

A nomenclatura distingue registros individuais de coleções e funcionalidades.

| Contexto | Nome |
|---|---|
| Entidade individual | `Person` |
| Contrato individual | `PersonResponse` |
| Requisição individual | `CreatePersonRequest` |
| Funcionalidade | `People` |
| Controller | `PeopleController` |
| Serviço | `PeopleService` |
| Coleção do contexto | `People` |
| Tabela | `People` |
| Rota REST | `/api/people` |

A mesma lógica é aplicada a transações:

- `Transaction` para uma transação;
- `Transactions` para coleção, funcionalidade, controller e rota.

## 6. Persistência

O SQLite garante que os dados permaneçam disponíveis após o encerramento da aplicação.

As migrations do Entity Framework Core são versionadas para permitir a reprodução da estrutura do banco.

O arquivo físico do banco não é versionado.

### Relacionamento

Uma pessoa pode possuir nenhuma ou várias transações.

Cada transação pertence obrigatoriamente a uma pessoa.

```text
Person 1 ──────── 0..N Transaction
```

A chave estrangeira fica em `Transaction.PersonId`.

A exclusão de uma pessoa está configurada com `DeleteBehavior.Cascade`, removendo automaticamente suas transações.

## 7. Valores monetários

A API recebe e retorna valores monetários como números decimais.

Internamente, os valores são persistidos em centavos inteiros:

```text
R$ 125,90 = 12.590 centavos
```

A entidade `Transaction` armazena `AmountInCents`, e a conversão é centralizada no `MoneyConverter`.

Essa decisão:

- preserva valores exatos;
- evita operações monetárias com ponto flutuante;
- simplifica agregações;
- evita limitações do SQLite com operações decimais;
- mantém os cálculos de totais consistentes.

O banco também possui restrição para impedir valores iguais ou inferiores a zero.

## 8. Contratos da API

As entidades de domínio não são retornadas diretamente pelos controllers.

A API utiliza contratos específicos de entrada e saída para:

- controlar o formato público;
- evitar serialização de propriedades de navegação;
- impedir acoplamento entre persistência e cliente;
- documentar cada operação;
- permitir mudanças internas sem quebrar o consumidor.

Os campos obrigatórios utilizam tipos não anuláveis e validações explícitas.

## 9. Convenções JSON

A API utiliza as seguintes convenções:

- números devem ser enviados como números JSON;
- números enviados como texto são rejeitados;
- o tipo de transação é representado como string;
- valores numéricos do enum não são aceitos.

Valores aceitos:

```json
{ "type": "expense" }
```

```json
{ "type": "income" }
```

Valor rejeitado:

```json
{ "type": 1 }
```

## 10. Tratamento de erros

O tratamento de exceções é centralizado, e as respostas seguem o padrão `ProblemDetails`.

| Situação | Status |
|---|---:|
| Dados ou formato inválido | `400 Bad Request` |
| Recurso inexistente | `404 Not Found` |
| Violação de regra de negócio | `422 Unprocessable Entity` |
| Erro inesperado | `500 Internal Server Error` |

Exceções internas e stack traces não são expostos ao cliente.

## 11. OpenAPI e Scalar

O documento OpenAPI é gerado pelo ASP.NET Core.

O Scalar é disponibilizado no ambiente de desenvolvimento para:

- consultar a documentação;
- visualizar schemas;
- executar os endpoints;
- verificar exemplos;
- analisar códigos de resposta.

O projeto utiliza:

- comentários XML;
- exemplos de requisição e resposta;
- descrição de propriedades;
- enum textual para transações;
- números estritos;
- transformer de schema;
- teste automatizado do contrato OpenAPI.

Rotas de desenvolvimento:

```text
/openapi/v1.json
/scalar
```

## 12. Endpoints atuais

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

Os endpoints de consulta por identificador são auxiliares e permitem que operações de criação retornem `201 Created` com uma localização válida para o recurso.

## 13. Estratégia de testes

O projeto utiliza três tipos principais de teste.

### 13.1 Testes de domínio

Validam as regras protegidas pelas entidades.

Exemplos:

- nome inválido;
- idade negativa;
- valor inválido;
- tipo inválido;
- receita para menor de idade.

### 13.2 Testes de serviço e persistência

Utilizam SQLite em memória para validar comportamentos próximos ao ambiente real.

Exemplos:

- persistência de pessoas;
- persistência de transações;
- ordenação;
- consulta por identificador;
- exclusão em cascata;
- cálculo de totais;
- inclusão de pessoas sem transações.

### 13.3 Testes de contrato OpenAPI

Validam que o documento público permanece coerente.

Exemplos:

- `personId` é representado somente como inteiro;
- `TransactionType` é representado como string;
- os valores permitidos são `expense` e `income`;
- os endpoints esperados aparecem no documento.

## 14. Organização prevista do front-end

O front-end será organizado por funcionalidade.

```text
src/
├── app/
├── components/
├── features/
│   ├── people/
│   ├── transactions/
│   └── totals/
├── services/
├── types/
└── main.tsx
```

### app

Configurações globais, rotas, providers e estrutura principal.

### components

Componentes visuais reutilizáveis e independentes de uma funcionalidade específica.

### features

Páginas, componentes, hooks e lógica específica de cada funcionalidade.

### services

Cliente HTTP e comunicação com a API.

### types

Contratos TypeScript compartilhados.

## 15. Complexidades evitadas

A versão atual não utiliza:

- repositório genérico;
- Unit of Work adicional sobre o Entity Framework Core;
- múltiplos projetos sem necessidade;
- CQRS;
- MediatR;
- AutoMapper;
- barramento de eventos;
- autenticação;
- autorização;
- microsserviços.

Essas decisões só devem ser revistas quando um requisito concreto justificar a complexidade.