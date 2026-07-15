export type TransactionType =
  | 'expense'
  | 'income'

export type TransactionAgeGroup =
  | 'minor'
  | 'adult'

export interface TransactionPerson {
  id: number
  name: string
}

export interface Transaction {
  id: number
  description: string
  amount: number
  type: TransactionType
  person: TransactionPerson
}

export interface CreateTransactionInput {
  description: string
  amount: number
  type: TransactionType
  personId: number
}

export interface TransactionFilters {
  personId?: number
  ageGroup?: TransactionAgeGroup
  type?: TransactionType
  minAmount?: number
  maxAmount?: number
}