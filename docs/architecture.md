# Arquitetura

## 1. Visão geral

O **Controle de Gastos Residenciais** é uma aplicação full stack para cadastro de pessoas, registro de receitas e despesas e consulta de totais financeiros.

O projeto foi desenvolvido de forma incremental. Cada commit representa uma evolução pequena, funcional e compreensível do produto, permitindo acompanhar as decisões técnicas e funcionais pelo histórico do Git.

A solução é composta por:

- uma API desenvolvida com .NET 10 e ASP.NET Core;
- uma aplicação web desenvolvida com React e TypeScript;
- um banco de dados SQLite;
- documentação OpenAPI interativa com Scalar;
- testes automatizados do back-end;
- integração contínua com GitHub Actions.

### Back-end

O back-end utiliza:

- .NET 10;
- ASP.NET Core Web API;
- Entity Framework Core;
- SQLite;
- OpenAPI nativo do ASP.NET Core;
- Scalar;
- xUnit;
- SQLite em memória nos testes de integração;
- testes HTTP;
- testes de contrato do documento OpenAPI.

### Front-end

O front-end utiliza:

- React;
- TypeScript;
- Vite;
- Tailwind CSS;
- shadcn/ui;
- Radix UI;
- Lucide React;
- Sonner;
- ESLint.

A interface permite:

- cadastrar e excluir pessoas;
- registrar receitas e despesas;
- impedir visualmente receitas para menores de idade;
- listar transações;
- consultar totais gerais;
- consultar totais individuais por pessoa;
- visualizar estados de carregamento, erro e ausência de dados.

## 2. Objetivos arquiteturais

A arquitetura foi definida para:

- manter as regras de negócio fáceis de localizar;
- manter os controllers focados em responsabilidades HTTP;
- separar contratos públicos das entidades internas;
- proteger o domínio contra estados inválidos;
- centralizar comportamentos compartilhados;
- permitir que cada funcionalidade evolua de forma independente;
- facilitar testes unitários, de integração e de contrato;
- manter o front-end organizado por funcionalidade;
- evitar abstrações que não agregariam valor ao escopo atual;
- facilitar a execução e avaliação do projeto.

## 3. Abordagem adotada

O back-end utiliza uma abordagem de **monólito modular organizado por funcionalidade**.

Essa escolha oferece separação de responsabilidades sem introduzir a complexidade de múltiplos projetos, microsserviços, barramentos de eventos ou frameworks de mediação.

```text
HomeExpenseControl.Api/
├── Common/
│   ├── Cors/
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

Cada funcionalidade concentra seus contratos, controller e serviço.

Os componentes compartilhados permanecem em `Common`, enquanto os conceitos centrais de negócio ficam em `Domain`.

## 4. Responsabilidades por área do back-end

### 4.1 Common

Contém comportamentos compartilhados por mais de uma funcionalidade.

Atualmente inclui:

- nomes das políticas de CORS;
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
- excluir pessoas;
- normalizar os dados recebidos;
- devolver contratos próprios da API.

#### Transactions

Responsável por:

- cadastrar transações;
- listar transações;
- consultar uma transação pelo identificador;
- verificar a existência da pessoa vinculada;
- aplicar a restrição de receita para menores de idade;
- converter valores monetários para a representação interna.

#### Totals

Responsável por:

- calcular receitas, despesas e saldo de cada pessoa;
- incluir pessoas sem transações;
- calcular receitas, despesas e saldo líquido gerais;
- devolver os totais em formato decimal para o cliente.

### 4.4 Infrastructure

Contém os detalhes de persistência:

- `AppDbContext`;
- configurações das entidades;
- migrations;
- configuração do SQLite;
- aplicação das migrations no início da execução.

As regras de negócio não dependem diretamente de detalhes específicos do banco.

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

No front-end, os arquivos preservam nomes em inglês para manter consistência com o ecossistema TypeScript:

```text
peopleApi.ts
peopleTypes.ts
usePeople.ts
TransactionForm.tsx
TransactionsSection.tsx
TotalsOverview.tsx
```

## 6. Persistência

O SQLite garante que os dados permaneçam disponíveis após o encerramento da aplicação.

As migrations do Entity Framework Core são versionadas para permitir a reprodução da estrutura do banco.

O arquivo físico do banco não é versionado.

A connection string padrão é:

```text
Data Source=home-expense-control.db
```

Em ambiente de desenvolvimento, as migrations são aplicadas automaticamente ao iniciar a API.

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

No front-end, os valores são:

- recebidos como números decimais;
- formatados em reais com `Intl.NumberFormat`;
- enviados novamente como números JSON;
- apresentados com destaque visual conforme receita, despesa ou saldo.

## 8. Contratos da API

As entidades de domínio não são retornadas diretamente pelos controllers.

A API utiliza contratos específicos de entrada e saída para:

- controlar o formato público;
- evitar serialização de propriedades de navegação;
- impedir acoplamento entre persistência e cliente;
- documentar cada operação;
- permitir mudanças internas sem quebrar o consumidor.

Os campos obrigatórios utilizam tipos não anuláveis e validações explícitas.

O front-end mantém contratos TypeScript equivalentes, separados por funcionalidade.

Exemplo:

```ts
export interface CreateTransactionInput {
  description: string
  amount: number
  type: 'expense' | 'income'
  personId: number
}
```

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

Exemplo inválido de identificador enviado como texto:

```json
{ "personId": "3" }
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

No front-end, os erros da API são convertidos para uma representação própria e apresentados por meio de:

- mensagens junto aos campos;
- estados de erro;
- botões de nova tentativa;
- notificações com Sonner.

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
- testes automatizados do contrato OpenAPI.

Rotas de desenvolvimento:

```text
/openapi/v1.json
/scalar
```

Em execução local:

```text
http://localhost:5079/openapi/v1.json
http://localhost:5079/scalar
```

O cabeçalho do front-end possui um link para a documentação da API, formado a partir da URL configurada em `VITE_API_BASE_URL`.

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

O projeto utiliza três tipos principais de teste no back-end.

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

### 13.3 Testes HTTP e de contrato

Validam o comportamento exposto publicamente pela aplicação.

Exemplos:

- fluxo crítico de criação e consulta;
- política de CORS;
- respostas HTTP;
- `personId` representado somente como inteiro;
- `TransactionType` representado como string;
- valores permitidos `expense` e `income`;
- presença dos endpoints esperados no OpenAPI.

### 13.4 Verificações do front-end

O front-end é validado por:

- TypeScript;
- ESLint;
- build de produção com Vite.

Os comandos utilizados são:

```bash
npm run lint
npm run build
```

## 14. Arquitetura do front-end

O front-end também é organizado por funcionalidade.

```text
src/frontend/src/
├── components/
│   ├── dashboard/
│   │   └── MetricCard.tsx
│   ├── layout/
│   │   ├── AppHeader.tsx
│   │   └── FeaturePlaceholder.tsx
│   └── ui/
│       └── componentes do shadcn/ui
├── config/
│   └── env.ts
├── features/
│   ├── people/
│   │   ├── components/
│   │   │   ├── PeopleList.tsx
│   │   │   ├── PeopleSection.tsx
│   │   │   └── PersonForm.tsx
│   │   ├── peopleApi.ts
│   │   ├── peopleTypes.ts
│   │   └── usePeople.ts
│   ├── transactions/
│   │   ├── components/
│   │   │   ├── TransactionForm.tsx
│   │   │   ├── TransactionsList.tsx
│   │   │   └── TransactionsSection.tsx
│   │   ├── transactionsApi.ts
│   │   ├── transactionTypes.ts
│   │   └── useTransactions.ts
│   └── totals/
│       ├── components/
│       │   └── TotalsOverview.tsx
│       ├── totalsApi.ts
│       ├── totalsTypes.ts
│       └── useTotals.ts
├── lib/
│   └── utils.ts
├── services/
│   ├── apiError.ts
│   └── httpClient.ts
├── App.tsx
├── index.css
├── main.tsx
└── vite-env.d.ts
```

### 14.1 Components

A pasta `components` contém elementos reutilizados por diferentes funcionalidades.

#### dashboard

Contém componentes voltados para indicadores e apresentação de métricas.

#### layout

Contém componentes estruturais da aplicação, como o cabeçalho.

#### ui

Contém os componentes do design system gerados pelo shadcn/ui.

Esses componentes permanecem dentro do próprio projeto e podem ser adaptados conforme a necessidade da aplicação.

### 14.2 Config

Centraliza configurações do front-end.

Atualmente, valida a variável de ambiente:

```text
VITE_API_BASE_URL
```

A URL base da API não é espalhada pelos componentes.

Ela é lida uma única vez em `env.ts` e disponibilizada de forma tipada.

O arquivo real `.env` é local e ignorado pelo Git.

O repositório mantém apenas:

```text
src/frontend/.env.example
```

### 14.3 Features

Cada funcionalidade concentra seus próprios:

- contratos TypeScript;
- cliente de API;
- hook de estado;
- componentes visuais;
- comportamentos de carregamento e erro.

#### People

Responsável por:

- cadastro de pessoas;
- listagem;
- identificação de menores de idade;
- confirmação de exclusão;
- exclusão da pessoa e das transações vinculadas.

#### Transactions

Responsável por:

- seleção da pessoa;
- cadastro de receita ou despesa;
- entrada e formatação de valores;
- restrição visual de receita para menores;
- listagem responsiva das transações.

A validação visual não substitui a regra definitiva aplicada pelo back-end.

#### Totals

Responsável por:

- indicadores gerais;
- totais individuais;
- estados vazios;
- apresentação responsiva;
- atualização manual dos dados.

### 14.4 Services

A pasta `services` contém elementos compartilhados de comunicação com a API.

O cliente HTTP centraliza:

- URL base;
- serialização JSON;
- leitura das respostas;
- tratamento de respostas sem conteúdo;
- conversão de erros da API.

Os erros seguem o contrato `ProblemDetails` retornado pelo back-end.

### 14.5 Estado e comunicação

O projeto utiliza hooks específicos para cada funcionalidade:

```text
usePeople
useTransactions
useTotals
```

Essa decisão mantém o estado próximo da funcionalidade que o utiliza, sem introduzir uma biblioteca global de estado para um escopo que ainda não exige essa complexidade.

Os hooks são responsáveis por:

- carregamento inicial;
- atualização manual;
- estado de envio;
- erros de comunicação;
- atualização local após operações bem-sucedidas;
- cancelamento de requisições quando o componente é desmontado.

### 14.6 Navegação

A interface principal utiliza abas para alternar entre:

- visão geral;
- pessoas;
- transações.

Como a aplicação atual possui uma única tela e poucos contextos de navegação, não foi necessário introduzir um roteador.

Essa decisão pode ser revista caso novas páginas independentes sejam adicionadas.

### 14.7 Design system

O front-end utiliza shadcn/ui com componentes baseados em Radix UI.

Entre os componentes adotados estão:

- botões;
- cartões;
- campos;
- seletores;
- tabelas;
- badges;
- abas;
- diálogos de confirmação;
- skeletons;
- notificações.

O Tailwind CSS é utilizado para composição responsiva e ajustes visuais.

Os ícones são fornecidos pelo Lucide React.

### 14.8 Responsividade

As listagens possuem apresentações diferentes conforme o espaço disponível:

- tabelas em telas médias e grandes;
- cartões em telas pequenas.

Formulários e conteúdos são reorganizados em uma única coluna em dispositivos menores.

### 14.9 Feedback ao usuário

A interface apresenta:

- skeletons durante o carregamento;
- estados vazios;
- mensagens de erro;
- botões de nova tentativa;
- notificações com Sonner;
- confirmação antes da exclusão de uma pessoa;
- indicadores visuais durante operações assíncronas.

## 15. Configuração de ambiente e CORS

### 15.1 Front-end

O front-end utiliza:

```env
VITE_API_BASE_URL=http://localhost:5079
```

A variável é validada durante a inicialização.

A URL também é reutilizada para montar o link do Scalar no cabeçalho.

### 15.2 Back-end

As origens permitidas são configuradas em:

```text
Cors:AllowedOrigins
```

Por padrão, o ambiente local permite:

```text
http://localhost:5173
http://127.0.0.1:5173
```

Ambientes externos podem sobrescrever a lista por variáveis de ambiente:

```bash
Cors__AllowedOrigins__0="http://localhost:5173" \
Cors__AllowedOrigins__1="http://127.0.0.1:5173" \
Cors__AllowedOrigins__2="https://URL-DO-FRONTEND" \
dotnet run --project src/backend/HomeExpenseControl.Api
```

URLs dinâmicas de ambientes como Codespaces não devem ser versionadas.

## 16. Integração contínua

O GitHub Actions executa verificações em pushes e pull requests para a branch `main`.

### Back-end

O workflow executa:

- restauração das ferramentas locais;
- restauração das dependências;
- auditoria de pacotes NuGet;
- verificação de formatação;
- build em Release;
- testes automatizados.

### Front-end

O workflow executa:

- configuração do Node.js a partir da `.nvmrc`;
- instalação com `npm ci`;
- ESLint;
- build de produção.

A execução concorrente é cancelada quando um novo commit substitui uma execução anterior da mesma branch.

## 17. Complexidades evitadas

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
- microsserviços;
- biblioteca global de estado no front-end;
- roteador sem necessidade concreta;
- abstrações adicionais sobre o cliente HTTP.

Essas decisões só devem ser revistas quando um requisito concreto justificar a complexidade.

## 18. Evoluções futuras

Possíveis evoluções incluem:

- filtros de transações por pessoa;
- filtros por tipo de transação;
- filtros por faixa de valor;
- filtros consistentes nas consultas de pessoas e totais;
- testes automatizados específicos do front-end;
- paginação;
- ordenação configurável;
- autenticação e autorização;
- observabilidade;
- publicação em ambiente de produção.