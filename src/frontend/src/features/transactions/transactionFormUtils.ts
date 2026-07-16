import type { Person } from '@/features/people/peopleTypes'

import type { TransactionType } from './transactionTypes'

export interface TransactionFormErrors {
  personId?: string
  description?: string
  amount?: string
  type?: string
  form?: string
}

interface ValidateTransactionDraftInput {
  people: Person[]
  personId: string
  description: string
  amount: string
  type: TransactionType
}

export function parseTransactionAmount(
  value: string,
): number | null {
  const sanitizedValue = value
    .trim()
    .replace(/\s/g, '')
    .replace(/^R\$/i, '')

  if (!sanitizedValue) {
    return null
  }

  const normalizedValue =
    sanitizedValue.includes(',')
      ? sanitizedValue
          .replace(/\./g, '')
          .replace(',', '.')
      : sanitizedValue

  const parsedValue = Number(normalizedValue)

  if (!Number.isFinite(parsedValue)) {
    return null
  }

  return parsedValue
}

export function formatTransactionAmount(
  value: number,
): string {
  return new Intl.NumberFormat('pt-BR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value)
}

export function validateTransactionDraft({
  people,
  personId,
  description,
  amount,
  type,
}: ValidateTransactionDraftInput): TransactionFormErrors {
  const errors: TransactionFormErrors = {}

  const selectedPerson = people.find(
    (person) =>
      person.id === Number(personId),
  )

  const parsedAmount =
    parseTransactionAmount(amount)

  if (!personId) {
    errors.personId =
      'Selecione uma pessoa.'
  }

  if (!description.trim()) {
    errors.description =
      'Informe a descrição da transação.'
  }

  if (parsedAmount === null) {
    errors.amount =
      'Informe um valor válido.'
  } else if (parsedAmount <= 0) {
    errors.amount =
      'O valor deve ser maior que zero.'
  } else if (
    Math.round(parsedAmount * 100) !==
    parsedAmount * 100
  ) {
    errors.amount =
      'O valor deve possuir no máximo duas casas decimais.'
  }

  if (
    selectedPerson?.isMinor &&
    type === 'income'
  ) {
    errors.type =
      'Menores de idade só podem possuir despesas.'
  }

  return errors
}
