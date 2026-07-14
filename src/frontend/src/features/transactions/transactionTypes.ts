export type TransactionType =
  | 'expense'
  | 'income'

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