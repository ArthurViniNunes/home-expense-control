# Controle de Gastos Residenciais

Aplicação full stack para cadastro de pessoas, registro de receitas e despesas e consulta de totais financeiros residenciais.

O projeto foi desenvolvido de forma incremental. Cada commit representa uma evolução pequena, funcional e compreensível do produto.

## Funcionalidades

### Pessoas

- cadastro de pessoas;
- listagem em ordem alfabética;
- identificação de adultos e menores de idade;
- exclusão com confirmação;
- exclusão automática das transações vinculadas.

### Transações

- cadastro de receitas e despesas;
- edição de transações existentes;
- exclusão direta com confirmação;
- associação obrigatória a uma pessoa;
- listagem responsiva;
- bloqueio de receitas para menores de 18 anos;
- filtros combináveis por pessoa, faixa etária, tipo e faixa de valor;
- identificação de maiores e menores de idade nos filtros;
- valores mínimo e máximo inclusivos;
- limpeza dos filtros aplicados;
- estado específico para consultas sem resultados;
- valores monetários formatados em reais;
- ordenação das movimentações mais recentes primeiro;
- manutenção dos filtros ativos após cadastro, edição e exclusão.

### Totais

- total de receitas;
- total de despesas;
- saldo líquido geral;
- totais individuais por pessoa;
- inclusão de pessoas sem transações;
- visualização responsiva em tabela e cartões.

### Experiência da interface

- estados de carregamento;
- tratamento de erros;
- notificações de sucesso e falha;
- confirmação de ações destrutivas;
- interface responsiva;
- documentação da API acessível pelo cabeçalho.

## Tecnologias

### Back-end

- .NET 10;
- ASP.NET Core Web API;
- Entity Framework Core;
- SQLite;
- OpenAPI nativo;
- Scalar;
- xUnit.

### Front-end

- React;
- TypeScript;
- Vite;
- Tailwind CSS;
- shadcn/ui;
- Radix UI;
- Lucide React;
- Sonner;
- ESLint.

### Qualidade e automação

- GitHub Actions;
- testes automatizados;
- verificação de formatação;
- análise de dependências vulneráveis;
- build automatizado do back-end e do front-end.

## Estrutura do repositório

```text
desafio-tecnico-TS.NET/
├── .github/
│   └── workflows/
│       └── ci.yml
├── docs/
│   ├── architecture.md
│   └── business-rules.md
├── src/
│   ├── backend/
│   │   └── HomeExpenseControl.Api/
│   └── frontend/
├── tests/
│   └── HomeExpenseControl.Api.Tests/
├── .nvmrc
├── HomeExpenseControl.sln
└── README.md
```

## Pré-requisitos

- .NET SDK 10;
- Node.js 24;
- npm;
- Git.

A versão do Node.js esperada está registrada em:
```
.nvmrc
```

Caso utilize NVM:

```bash
nvm install
nvm use
```

## Configuração inicial

Clone o repositório e entre na pasta:

```bash
git clone https://github.com/ArthurViniNunes/home-expense-control.git
cd home-expense-control
```

### Back-end

Restaure as ferramentas e dependências:

```bash
dotnet tool restore
dotnet restore
```

### Front-end

Instale as dependências:

```bash
cd src/frontend
npm ci
cd ../..
```

## Configuração do front-end

O front-end utiliza uma variável de ambiente para localizar a API.

Copie o arquivo de exemplo:

```bash
cp src/frontend/.env.example src/frontend/.env
```

Para execução local, o conteúdo esperado é:

```text
VITE_API_BASE_URL=http://localhost:5079
```

O arquivo .env é local e não deve ser versionado.

A aplicação valida essa variável ao iniciar. Caso ela não esteja configurada, o front-end exibe um erro explícito.

## Executando a API

Na raiz do repositório:

```bash
dotnet run --project src/backend/HomeExpenseControl.Api
```
Endereços locais:

```text
API:      http://localhost:5079
Scalar:   http://localhost:5079/scalar
OpenAPI:  http://localhost:5079/openapi/v1.json
```
As rotas do Scalar e do documento OpenAPI ficam disponíveis em ambiente de desenvolvimento.

## Executando o front-end

Em outro terminal:

```bash
cd src/frontend
npm run dev
```

Endereço padrão:

```text
http://localhost:5173
```

## CORS

Por padrão, a API aceita requisições destas origens locais:

```
http://localhost:5173
http://127.0.0.1:5173
```

Em ambientes externos, como GitHub Codespaces, uma origem adicional pode ser informada por variável de ambiente:

```bash
Cors__AllowedOrigins__0="http://localhost:5173" \
Cors__AllowedOrigins__1="http://127.0.0.1:5173" \
Cors__AllowedOrigins__2="https://URL-DO-FRONTEND" \
dotnet run --project src/backend/HomeExpenseControl.Api
```

A URL externa não deve ser gravada diretamente nos arquivos versionados.

## Banco de dados

O projeto utiliza SQLite.

A connection string padrão é:

```text
Data Source=home-expense-control.db
```

As migrations são versionadas e aplicadas automaticamente ao iniciar a API em ambiente de desenvolvimento.

O arquivo físico do banco não faz parte do repositório.

## Executando os testes

Na raiz:

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
dotnet list HomeExpenseControl.sln package \
  --vulnerable \
  --include-transitive
```

## Verificações do front-end

```bash
cd src/frontend
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
POST   /api/transactions
GET    /api/transactions
GET    /api/transactions/{id}
PUT    /api/transactions/{id}
DELETE /api/transactions/{id}
```

#### Filtros de transações

O endpoint de listagem aceita filtros opcionais pela query string:

| Parâmetro | Descrição | Valores aceitos |
|---|---|---|
| `personId` | Identificador da pessoa | inteiro maior que zero |
| `ageGroup` | Faixa etária da pessoa | `adult` ou `minor` |
| `type` | Tipo da transação | `expense` ou `income` |
| `minAmount` | Valor mínimo da transação, inclusive | zero ou valor positivo |
| `maxAmount` | Valor máximo da transação, inclusive | zero ou valor positivo |

Os filtros são opcionais e podem ser combinados.

A classificação etária considera:

- `adult`: pessoa com 18 anos ou mais;
- `minor`: pessoa com menos de 18 anos.

Quando nenhum filtro é informado, todas as transações são retornadas.

Quando os filtros não encontram correspondências, a API retorna uma lista vazia.

Informar o identificador de uma pessoa inexistente também resulta em uma lista vazia, e não em `404 Not Found`.

### Totais

```http
GET /api/totals
```

## Exemplos de requisição

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

### Editar transação

```http
PUT /api/transactions/1
```

```json
{
  "description": "Conta de energia atualizada",
  "amount": 160.75,
  "type": "expense",
  "personId": 1
}
```

A edição reaplica todas as regras do cadastro e retorna a transação atualizada com `200 OK`.

### Excluir transação

```http
DELETE /api/transactions/1
```

A exclusão direta retorna `204 No Content` e remove somente a transação. A pessoa vinculada permanece cadastrada.

### Filtrar transações

Filtrar por pessoa:

```http
GET /api/transactions?personId=1
```

Filtrar por faixa etária:

```http
GET /api/transactions?ageGroup=minor
GET /api/transactions?ageGroup=adult
```

Filtrar por tipo:

```http
GET /api/transactions?type=expense
GET /api/transactions?type=income
```

Filtrar por faixa de valor:

```http
GET /api/transactions?minAmount=100&maxAmount=500
```

Combinar todos os filtros:

```http
GET /api/transactions?personId=1&ageGroup=adult&type=expense&minAmount=100&maxAmount=500
```
Os limites de valor são inclusivos. Portanto, uma transação de 100.00 será retornada quando minAmount=100.

## Regras principais
- o nome da pessoa é obrigatório;
- a idade não pode ser negativa;
- toda transação deve pertencer a uma pessoa existente;
- o valor da transação deve ser maior que zero;
- o valor pode possuir no máximo duas casas decimais;
- menores de 18 anos podem possuir somente despesas;
- as regras de cadastro são reaplicadas integralmente durante a edição;
- uma edição inválida não altera os dados anteriores da transação;
- a exclusão direta remove somente a transação e preserva a pessoa;
- ao excluir uma pessoa, suas transações são excluídas automaticamente;
- pessoas sem transações aparecem nos totais com valores zerados;
- os filtros de transações são opcionais e podem ser combinados;
- `adult` representa pessoas com 18 anos ou mais;
- `minor` representa pessoas com menos de 18 anos;
- os valores mínimo e máximo dos filtros são inclusivos;
- os valores dos filtros monetários devem possuir no máximo duas casas decimais;
- o valor mínimo do filtro não pode ser maior que o valor máximo;
- filtros inválidos retornam `400 Bad Request`;
- filtros sem correspondência retornam uma lista vazia;
- o saldo é calculado como receitas menos despesas.

## Valores monetários

A API recebe e retorna valores decimais, mas os dados são persistidos como centavos inteiros.

```text
R$ 145,34 = 14.534 centavos
```

Essa decisão evita perda de precisão e mantém as agregações consistentes.

## Contrato JSON

O tipo da transação é representado como texto:

```text
expense
income
```

Valores numéricos como `1` e `2` são rejeitados.

Os números também devem ser enviados como números JSON, e não como strings.

## Tratamento de erros

As respostas de erro seguem o padrão `ProblemDetails`.

| Situação | Status |
|---|---:|
| Dados inválidos | `400 Bad Request` |
| Atualização concluída | `200 OK` |
| Exclusão concluída | `204 No Content` |
| Recurso não encontrado | `404 Not Found` |
| Regra de negócio violada | `422 Unprocessable Entity` |
| Erro inesperado | `500 Internal Server Error` |

## Integração contínua

O workflow localizado em:

```text
.github/workflows/ci.yml
```

é executado em pushes e pull requests para a branch `main`.

### Back-end
- restauração das ferramentas;
- restauração das dependências;
- auditoria de pacotes NuGet;
- verificação de formatação;
- build em Release;
- execução dos testes.

### Front-end

- configuração do Node.js pela .nvmrc;
- instalação com npm ci;
- execução do ESLint;
- geração do build de produção.

## Documentação técnica

- [Arquitetura](docs/architecture.md)
- [Regras de negócio](docs/business-rules.md)

A documentação interativa da API pode ser consultada pelo Scalar durante a execução local:

```text
http://localhost:5079/scalar
```

## Decisões de desenvolvimento
- regras de negócio protegidas pelo back-end;
- entidades internas não expostas diretamente pela API;
- controllers pequenos;
- organização por funcionalidade;
- valores monetários persistidos em centavos;
- tratamento centralizado de exceções;
- interface organizada por features;
- componentes visuais reutilizáveis;
- commits pequenos e com responsabilidade única;
- código executável ao longo do histórico do Git;
- filtros executados no banco de dados por composição de consultas;
- comparações monetárias realizadas em centavos inteiros;
- validação dos filtros protegida tanto no front-end quanto no back-end;
- filtros da interface refletidos diretamente nos parâmetros da API;
- validações monetárias e de negócio compartilhadas entre cadastro e edição;
- atualização e exclusão recarregando a listagem com os filtros ativos;
- operações destrutivas protegidas por confirmação explícita.