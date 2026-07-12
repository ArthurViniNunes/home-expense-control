# Arquitetura

## Visão geral

O Controle de Gastos Residenciais é uma aplicação full stack composta por:

- Uma API desenvolvida com ASP.NET Core.
- Uma aplicação React escrita em TypeScript.
- Um banco de dados SQLite.
- Testes automatizados para o back-end.

O projeto utiliza uma abordagem de monólito modular.

Essa estrutura permite separar claramente as responsabilidades sem introduzir complexidade desnecessária para o tamanho atual da aplicação.

## Objetivos arquiteturais

A arquitetura foi definida para:

- Facilitar a localização das regras de negócio.
- Manter os controllers pequenos.
- Separar os contratos da API das entidades de persistência.
- Tornar o código compreensível para novos desenvolvedores.
- Permitir que cada funcionalidade evolua de forma independente.
- Manter a aplicação simples para o escopo atual.
- Facilitar os testes automatizados das regras mais importantes.

## Organização do back-end

O back-end será organizado por funcionalidade.

```text
HomeExpenseControl.Api/
├── Common/
├── Domain/
├── Features/
│   ├── People/
│   ├── Transactions/
│   └── Totals/
├── Infrastructure/
│   └── Persistence/
└── Program.cs
```

### Common

Contém comportamentos compartilhados entre diferentes funcionalidades, como:

- Tratamento de erros.
- Conversão de valores monetários.
- Métodos de extensão.
- Exceções compartilhadas pela aplicação.

Um componente deve ser colocado nessa pasta apenas quando for realmente utilizado por mais de uma funcionalidade.

### Domain

Contém os principais conceitos de negócio:

- Pessoa.
- Transação.
- Tipo de transação.

As entidades de domínio devem proteger o próprio estado válido sempre que possível.

Setters públicos devem ser evitados quando permitirem a criação de objetos inválidos.

### Features

Cada funcionalidade contém os elementos necessários para uma capacidade específica do sistema.

Exemplo:

```text
Features/
├── People/
│   ├── Contracts/
│   ├── PeopleController.cs
│   └── PeopleService.cs
├── Transactions/
│   ├── Contracts/
│   ├── TransactionsController.cs
│   └── TransactionsService.cs
└── Totals/
    ├── Contracts/
    ├── TotalsController.cs
    └── TotalsService.cs
```

Os controllers são responsáveis por:

- Receber requisições HTTP.
- Validar o formato da requisição.
- Chamar o serviço apropriado.
- Retornar a resposta HTTP correta.

Os services são responsáveis por:

- Coordenar os casos de uso.
- Carregar os dados necessários.
- Aplicar regras que dependem de mais de uma entidade.
- Persistir alterações.
- Produzir os modelos de resposta.

### Infrastructure

Contém detalhes de implementação relacionados a recursos externos.

A camada de persistência conterá:

- O contexto do Entity Framework Core.
- As configurações das entidades.
- As migrations do banco de dados.
- A configuração do SQLite.

As regras de negócio não devem depender diretamente de comportamentos específicos do SQLite.

## Organização do front-end

O front-end também será organizado por funcionalidade.

```text
src/
├── components/
├── features/
│   ├── people/
│   ├── transactions/
│   └── totals/
├── layouts/
├── services/
├── types/
└── App.tsx
```

### Components

Contém componentes reutilizáveis da interface que não pertencem exclusivamente a uma funcionalidade.

### Features

Contém páginas, formulários, hooks e componentes relacionados a uma funcionalidade específica.

O código específico de uma funcionalidade deve permanecer dentro de sua própria pasta, exceto quando houver reutilização real.

### Services

Contém a comunicação com a API.

As requisições HTTP não devem ficar espalhadas diretamente pelos componentes de apresentação.

### Types

Contém contratos TypeScript compartilhados entre diferentes funcionalidades.

## Persistência

O SQLite será utilizado para garantir que os dados permaneçam disponíveis após o encerramento da aplicação.

As migrations do Entity Framework Core serão versionadas no repositório para permitir a reprodução da estrutura do banco.

O arquivo físico do banco de dados não deve ser versionado.

## Valores monetários

Valores monetários não devem ser representados por tipos binários de ponto flutuante.

A API receberá e retornará valores monetários como números decimais.

Internamente, os valores poderão ser armazenados como centavos inteiros para:

- Preservar valores exatos.
- Simplificar agregações.
- Evitar limitações do SQLite com operações decimais.
- Evitar erros de arredondamento.

Exemplo:

```text
R$ 125,90 = 12.590 centavos
```

A conversão entre valores decimais e centavos deve ficar centralizada em um único componente.

## Relacionamentos

Uma pessoa pode possuir nenhuma ou várias transações.

Cada transação pertence obrigatoriamente a uma pessoa.

```text
Pessoa 1 ──────── 0..N Transação
```

Ao excluir uma pessoa, todas as suas transações também devem ser excluídas.

Esse comportamento será configurado como exclusão em cascata na camada de persistência.

## Contratos da API

As entidades de persistência não devem ser retornadas diretamente pelos controllers.

Serão utilizados contratos de entrada e saída para:

* Proteger o modelo interno.
* Controlar o formato público da API.
* Evitar a serialização acidental de propriedades de navegação.
* Permitir alterações internas sem quebrar os consumidores da API.

## Tratamento de erros

A API utilizará uma representação padronizada de erros baseada em `ProblemDetails`.

As principais categorias serão:

* Dados de entrada inválidos.
* Recurso não encontrado.
* Violação de regra de negócio.
* Erro inesperado no servidor.

Informações internas detalhadas de exceções não devem ser expostas ao cliente.

## Estratégia de testes

Os testes automatizados priorizarão comportamentos com impacto direto nas regras de negócio.

Os principais cenários são:

* Criar uma pessoa válida.
* Rejeitar dados inválidos de uma pessoa.
* Excluir uma pessoa e suas transações.
* Criar receitas e despesas válidas.
* Impedir o cadastro de receitas para menores de idade.
* Rejeitar transações vinculadas a pessoas inexistentes.
* Calcular os totais de cada pessoa.
* Calcular os totais gerais.
* Tratar pessoas sem transações.

Os nomes dos testes devem descrever:

* O cenário.
* A ação executada.
* O resultado esperado.

## Comentários e documentação

O código deve ser compreendido principalmente por meio de:

* Nomes claros.
* Métodos pequenos.
* Contratos explícitos.
* Classes com responsabilidades bem definidas.
* Testes descritivos.

Os comentários devem explicar:

* Decisões arquiteturais.
* Motivos relacionados ao negócio.
* Limitações técnicas não óbvias.
* Comportamentos que poderiam ser interpretados incorretamente.

Os comentários não devem apenas repetir o que o código já demonstra.

## Complexidades evitadas

A versão inicial não utilizará:

* Repositórios genéricos.
* Abstração de Unit of Work sobre o Entity Framework Core.
* Vários projetos de aplicação sem necessidade concreta.
- Barramento de eventos.
- Frameworks CQRS.
- Frameworks de mapeamento baseados em reflexão.
- Autenticação e autorização.

Essas decisões poderão ser revistas somente quando houver um requisito real que justifique a complexidade adicional.
