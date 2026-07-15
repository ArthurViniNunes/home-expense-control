# Regras de Negócio

## 1. Objetivo

Este documento descreve os comportamentos que devem permanecer válidos independentemente da interface, do cliente ou da tecnologia de persistência.

As regras que protegem a integridade dos dados são aplicadas pelo back-end.

O front-end poderá repetir validações para melhorar a experiência do usuário, mas nunca será a única camada responsável por elas.

## 2. Pessoas

### 2.1 Dados da pessoa

Uma pessoa possui:

- identificador único gerado automaticamente;
- nome;
- idade.

### 2.2 Cadastro de pessoa

Uma pessoa pode ser cadastrada quando:

- o nome é informado;
- o nome não contém apenas espaços;
- o nome possui no máximo 120 caracteres;
- a idade é um número inteiro;
- a idade é igual ou superior a zero.

O nome é normalizado antes da persistência, removendo espaços no início e no final.

Exemplo válido:

```json
{
  "name": "Arthur Nunes",
  "age": 22
}
```

### 2.3 Menoridade e maioridade

Uma pessoa é considerada menor de idade quando possui menos de 18 anos.

Uma pessoa é considerada maior de idade quando possui 18 anos ou mais.

A idade não impede o cadastro da pessoa.

A restrição de menoridade é aplicada durante o cadastro de transações.

A classificação também pode ser utilizada na consulta de transações:

| Valor do filtro | Regra |
|---|---|
| `minor` | Idade menor que 18 anos |
| `adult` | Idade igual ou superior a 18 anos |

### 2.4 Listagem de pessoas

A listagem deve retornar todas as pessoas cadastradas.

A ordenação é feita por:

1. nome;
2. identificador, quando os nomes forem iguais.

### 2.5 Consulta por identificador

Uma pessoa pode ser consultada por seu identificador.

Quando o identificador não corresponde a uma pessoa existente, a API retorna `404 Not Found`.

### 2.6 Exclusão de pessoa

Uma pessoa pode ser excluída por seu identificador.

Quando uma pessoa é excluída, todas as transações vinculadas a ela também são excluídas automaticamente.

A tentativa de excluir uma pessoa inexistente retorna `404 Not Found`.

A exclusão bem-sucedida retorna `204 No Content`.

## 3. Transações

### 3.1 Dados da transação

Uma transação possui:

- identificador único gerado automaticamente;
- descrição;
- valor;
- tipo;
- identificador da pessoa vinculada.

### 3.2 Tipos permitidos

Os tipos públicos da API são:

| Valor | Significado |
|---|---|
| `expense` | Despesa |
| `income` | Receita |

O tipo é enviado como texto.

Exemplo:

```json
{ "type": "expense" }
```

Valores numéricos como `1` e `2` não são aceitos no contrato JSON.

### 3.3 Cadastro de transação

Uma transação pode ser cadastrada quando:

- a descrição é informada;
- a descrição não contém apenas espaços;
- a descrição possui no máximo 200 caracteres;
- o valor é maior que zero;
- o valor possui no máximo duas casas decimais;
- o tipo é `expense` ou `income`;
- a pessoa informada existe.

A descrição é normalizada antes da persistência, removendo espaços no início e no final.

Exemplo válido:

```json
{
  "description": "Conta de energia",
  "amount": 145.34,
  "type": "expense",
  "personId": 3
}
```

### 3.4 Pessoa obrigatória

Toda transação pertence obrigatoriamente a uma pessoa cadastrada.

Quando `personId` não corresponde a uma pessoa existente, a API retorna `404 Not Found`.

O banco também protege o relacionamento por meio de chave estrangeira.

### 3.5 Restrição para menores de idade

Uma pessoa menor de 18 anos pode cadastrar despesas.

Uma pessoa menor de 18 anos não pode cadastrar receitas.

| Idade | Tipo | Resultado |
|---:|---|---|
| 17 | `expense` | Permitido |
| 17 | `income` | Rejeitado |
| 18 | `expense` | Permitido |
| 18 | `income` | Permitido |

Uma tentativa de cadastrar receita para menor de idade retorna `422 Unprocessable Entity`.

### 3.6 Valores monetários

O valor informado deve:

- ser maior que zero;
- possuir no máximo duas casas decimais;
- ser enviado como número JSON.

Valores válidos:

```text
10
10.50
125.99
```

Valores inválidos:

```text
0
-10
10.999
"125.99"
```

O valor é persistido internamente em centavos inteiros.

Exemplo:

```text
145.34 = 14.534 centavos
```

Os filtros monetários da listagem possuem regras diferentes do cadastro.

Nos parâmetros `minAmount` e `maxAmount`:

- zero é permitido;
- valores negativos não são permitidos;
- são aceitas no máximo duas casas decimais;
- os limites são inclusivos;
- o valor mínimo não pode ser maior que o valor máximo.

Exemplo:

```text
minAmount=100
maxAmount=500
```

Nesse caso, são retornadas também as transações com valor exatamente igual a `100` ou `500`.

### 3.7 Listagem de transações

Sem filtros, a listagem deve retornar todas as transações cadastradas.

Cada item apresenta:

- identificador;
- descrição;
- valor;
- tipo;
- identificador da pessoa;
- nome da pessoa.

A API ordena a listagem pelo identificador da transação em ordem crescente.

O front-end pode reorganizar os resultados para apresentar as movimentações mais recentes primeiro.

Quando nenhuma transação está cadastrada, a API retorna uma lista vazia.

Essa distinção é importante porque o serviço da API ordena por `Id` crescente, enquanto a interface reorganiza por `Id` decrescente.

### 3.8 Filtros de transações

A listagem de transações aceita filtros opcionais pela query string.

Filtros disponíveis:

| Parâmetro | Regra |
|---|---|
| `personId` | Identificador da pessoa, maior que zero |
| `ageGroup` | `adult` ou `minor` |
| `type` | `expense` ou `income` |
| `minAmount` | Valor mínimo inclusivo |
| `maxAmount` | Valor máximo inclusivo |

Os filtros:

- são opcionais;
- podem ser utilizados isoladamente;
- podem ser combinados;
- são aplicados simultaneamente quando mais de um é informado.

Exemplo por pessoa:

```http
GET /api/transactions?personId=1
```

Exemplo por faixa etária:

```http
GET /api/transactions?ageGroup=minor
```

Exemplo por tipo:

```http
GET /api/transactions?type=expense
```

Exemplo por faixa de valor:

```http
GET /api/transactions?minAmount=100&maxAmount=500
```

Exemplo combinando todos os filtros:

```http
GET /api/transactions?personId=1&ageGroup=adult&type=expense&minAmount=100&maxAmount=500
```

Para uma transação ser retornada, ela deve atender a todos os filtros informados.

Quando nenhum registro atende aos filtros, a API retorna:

```json
[]
```

Uma consulta por `personId` inexistente também retorna uma lista vazia.

Esse comportamento não retorna `404 Not Found`, pois o endpoint está consultando uma coleção e não um recurso individual.

### 3.9 Validação dos filtros

Filtros inválidos retornam `400 Bad Request`.

São considerados inválidos:

- `personId` igual ou inferior a zero;
- `ageGroup` diferente de `adult` ou `minor`;
- `type` diferente de `expense` ou `income`;
- valor mínimo negativo;
- valor máximo negativo;
- valores com mais de duas casas decimais;
- valor mínimo maior que o valor máximo;
- parâmetros enviados em formato incompatível com o contrato.

Exemplo inválido:

```http
GET /api/transactions?minAmount=500&maxAmount=100
```

A resposta informa:

```text
O valor mínimo não pode ser maior que o valor máximo.
```

A validação realizada pelo front-end melhora a experiência do usuário, mas o back-end continua sendo responsável por proteger definitivamente essas regras.

### 3.10 Consulta por identificador

Uma transação pode ser consultada por seu identificador.

Quando o identificador não corresponde a uma transação existente, a API retorna `404 Not Found`.

### 3.11 Edição e exclusão

A especificação atual não exige:

- edição de transações;
- exclusão direta de transações.

As transações são excluídas automaticamente quando sua pessoa é removida.

## 4. Totais

### 4.1 Pessoas incluídas

A consulta de totais deve incluir todas as pessoas cadastradas, inclusive aquelas sem transações.

### 4.2 Totais individuais

Para cada pessoa, o sistema calcula:

- total de receitas;
- total de despesas;
- saldo.

O saldo individual é:

```text
saldo = receitas - despesas
```

Interpretação:

- saldo positivo: receitas maiores que despesas;
- saldo negativo: despesas maiores que receitas;
- saldo zero: valores iguais ou ausência de transações.

### 4.3 Pessoas sem transações

Uma pessoa sem transações aparece com:

```json
{
  "totalIncome": 0,
  "totalExpenses": 0,
  "balance": 0
}
```

### 4.4 Totais gerais

A resposta também apresenta:

- total geral de receitas;
- total geral de despesas;
- saldo líquido geral.

O saldo geral é:

```text
saldo líquido geral = receitas gerais - despesas gerais
```

Os valores gerais devem corresponder à soma dos totais individuais.

### 4.5 Ordenação

As pessoas na consulta de totais são ordenadas por:

1. nome;
2. identificador, quando os nomes forem iguais.

## 5. Persistência e integridade

Os dados devem permanecer disponíveis depois que a aplicação for encerrada e iniciada novamente.

Armazenamento somente em memória não atende ao requisito.

A integridade é protegida em diferentes níveis.

### API

- validação de formato;
- propriedades obrigatórias;
- limites de valor;
- tipos permitidos.
- validação dos filtros da listagem;
- validação da relação entre valor mínimo e valor máximo;
- validação das classificações `adult`, `minor`, `expense` e `income`;

### Domínio

- proteção contra estados inválidos;
- normalização;
- limite de casas decimais;
- regra de menoridade.
- definição centralizada da idade de maioridade;

### Banco de dados

- chave primária;
- chave estrangeira;
- exclusão em cascata;
- restrição de valor positivo;
- restrição de tipo válido.

## 6. Convenções HTTP

| Situação | Status |
|---|---:|
| Recurso criado | `201 Created` |
| Consulta concluída | `200 OK` |
| Exclusão concluída | `204 No Content` |
| Dados ou formato inválido | `400 Bad Request` |
| Pessoa ou transação inexistente | `404 Not Found` |
| Regra de negócio violada | `422 Unprocessable Entity` |
| Erro inesperado | `500 Internal Server Error` |

Na listagem de transações:

- filtros válidos retornam `200 OK`;
- filtros válidos sem correspondência retornam `200 OK` com uma lista vazia;
- `personId` inexistente no filtro retorna `200 OK` com uma lista vazia;
- filtros inválidos retornam `400 Bad Request`;
- a consulta de uma transação individual inexistente continua retornando `404 Not Found`.

As respostas de erro seguem os padrões `ProblemDetails` e `ValidationProblemDetails`.

## 7. Casos cobertos pelos testes

### Pessoas

- criação com dados válidos;
- normalização do nome;
- nome vazio;
- nome acima do limite;
- idade negativa;
- menoridade;
- listagem vazia;
- ordenação;
- consulta por identificador;
- exclusão;
- exclusão de pessoa inexistente;
- exclusão em cascata.

### Transações

- criação de despesa válida;
- criação de receita válida para adulto;
- despesa para menor;
- receita para menor;
- descrição inválida;
- descrição acima do limite;
- valor igual a zero;
- valor negativo;
- valor com mais de duas casas decimais;
- tipo inválido;
- pessoa inexistente;
- listagem;
- consulta por identificador;
- serialização textual do tipo;
- filtro por pessoa;
- filtro por receita ou despesa;
- filtro por maiores de idade;
- filtro por menores de idade;
- valor mínimo inclusivo;
- valor máximo inclusivo;
- combinação de pessoa, faixa etária, tipo e valores;
- consulta filtrada sem resultados;
- pessoa inexistente no filtro;
- identificador inválido no filtro;
- faixa etária inválida;
- tipo inválido na query string;
- valores negativos nos filtros;
- filtros monetários com mais de duas casas decimais;
- valor mínimo maior que o valor máximo;
- fluxo HTTP completo com filtros combinados.

### Totais

- ausência de pessoas;
- pessoa sem transações;
- receitas e despesas da mesma pessoa;
- múltiplas pessoas;
- saldo individual positivo;
- saldo individual negativo;
- saldo geral;
- ordenação por nome.

### Contrato OpenAPI

- identificadores representados como inteiros;
- ausência de pattern numérico indevido;
- tipo de transação representado como string;
- valores permitidos `expense` e `income`;
- presença dos endpoints documentados;
- presença dos parâmetros `personId`, `ageGroup`, `type`, `minAmount` e `maxAmount`;
- descrição dos filtros da listagem de transações;
- documentação dos valores `adult` e `minor`;
- documentação dos valores `expense` e `income`;
- indicação de que os limites monetários são inclusivos;
- resposta `400 Bad Request` para filtros inválidos.