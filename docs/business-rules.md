# Regras de Negócio

Este documento descreve os comportamentos que devem permanecer válidos independentemente da interface ou da tecnologia de persistência utilizada.

## Pessoas

### Dados da pessoa

Uma pessoa possui:

- Um identificador único gerado automaticamente.
- Um nome.
- Uma idade.

### Criação de pessoa

Uma pessoa poderá ser cadastrada quando:

- O nome for informado.
- O nome não for composto apenas por espaços.
- A idade for um número válido e não negativo.

Espaços no início e no final do nome devem ser removidos antes da persistência.

### Listagem de pessoas

O sistema deve retornar todas as pessoas cadastradas.

A implementação inicial deverá utilizar uma ordenação consistente, preferencialmente por nome e depois por identificador.

### Exclusão de pessoa

Uma pessoa cadastrada poderá ser excluída por meio de seu identificador.

Quando uma pessoa for excluída, todas as transações vinculadas a ela também deverão ser excluídas.

A tentativa de excluir uma pessoa inexistente não deve gerar um erro não tratado pela aplicação.

## Transações

### Dados da transação

Uma transação possui:

- Um identificador único gerado automaticamente.
- Uma descrição.
- Um valor monetário.
- Um tipo de transação.
- O identificador de uma pessoa existente.

### Tipos de transação

Os tipos permitidos são:

- Despesa.
- Receita.

Nenhum outro valor será considerado válido.

### Criação de transação

Uma transação poderá ser cadastrada quando:

- A descrição for informada.
- A descrição não for composta apenas por espaços.
- O valor monetário for maior que zero.
- O valor possuir no máximo duas casas decimais.
- O tipo da transação for válido.
- A pessoa informada existir no sistema.

Espaços no início e no final da descrição devem ser removidos antes da persistência.

### Obrigatoriedade da pessoa

Toda transação deve estar vinculada a uma pessoa previamente cadastrada.

O sistema deverá rejeitar uma transação quando o identificador informado não corresponder a uma pessoa existente.

### Restrição para menores de idade

Uma pessoa será considerada menor de idade quando possuir menos de 18 anos.

Uma pessoa menor de idade poderá cadastrar despesas.

Uma pessoa menor de idade não poderá cadastrar receitas.

Exemplos:

| Idade | Tipo da transação | Resultado |
| ----: | ----------------- | --------- |
|    17 | Despesa           | Permitido |
|    17 | Receita           | Rejeitado |
|    18 | Despesa           | Permitido |
|    18 | Receita           | Permitido |

Essa regra deverá ser obrigatoriamente protegida pelo back-end.

O front-end poderá impedir visualmente a seleção de receita para menores de idade, mas essa validação não será considerada suficiente.

### Listagem de transações

O sistema deverá listar as transações cadastradas.

Cada transação listada deverá fornecer informações suficientes para identificar:

- A transação.
- Sua descrição.
- Seu valor.
- Seu tipo.
- A pessoa vinculada.

Um filtro opcional por pessoa poderá ser implementado futuramente sem alterar o comportamento obrigatório.

## Valores monetários

Os valores monetários deverão:

- Ser maiores que zero durante o cadastro.
- Possuir no máximo duas casas decimais.
- Ser calculados sem tipos binários de ponto flutuante.
- Ser retornados para o cliente em formato decimal legível.

Exemplos de valores válidos:

```text
10
10,50
125,99
```

Exemplos de valores inválidos:

```text
0
-10
10,999
```

## Totais

A consulta de totais deverá incluir todas as pessoas cadastradas, inclusive aquelas que não possuem transações.

Para cada pessoa, o sistema deverá calcular:

- Total de receitas.
- Total de despesas.
- Saldo.

O saldo individual será calculado da seguinte forma:

```text
saldo = total de receitas - total de despesas
```

Um saldo positivo indica que as receitas são maiores que as despesas.

Um saldo negativo indica que as despesas são maiores que as receitas.

Um saldo igual a zero indica que receitas e despesas são iguais ou que a pessoa não possui transações.

## Totais gerais

Ao final da consulta, o sistema deverá apresentar:

- Total geral de receitas.
- Total geral de despesas.
- Saldo líquido geral.

O saldo geral será calculado da seguinte forma:

```text
saldo geral = total geral de receitas - total geral de despesas
```

Os valores gerais deverão corresponder à soma dos totais individuais.

## Pessoas sem transações

Uma pessoa sem transações deverá aparecer na consulta de totais com:

```text
receitas: 0,00
despesas: 0,00
saldo: 0,00
```

## Persistência dos dados

As pessoas e transações cadastradas deverão continuar disponíveis depois que a aplicação for encerrada e iniciada novamente.

Armazenamento temporário somente em memória não atende a esse requisito.

## Proteção das regras

As regras que protegem a integridade dos dados deverão ser aplicadas pelo back-end.

O front-end poderá repetir algumas validações para melhorar a experiência do usuário, mas nunca deverá ser a única camada responsável por essas regras.

## Casos extremos esperados

A implementação e os testes automatizados deverão considerar:

- Nome vazio.
- Descrição vazia.
- Idade negativa.
- Valor da transação igual a zero.
- Valor negativo.
- Valor com mais de duas casas decimais.
- Tipo de transação inválido.
- Pessoa inexistente.
- Receita cadastrada para menor de idade.
- Pessoa sem transações.
- Exclusão de pessoa com transações existentes.
- Totais contendo receitas e despesas.
- Saldo individual negativo.
- Saldo geral negativo.
