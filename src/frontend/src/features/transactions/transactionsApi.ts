import { httpClient } from '@/services/httpClient'
import type {
  CreateTransactionInput,
  Transaction,
  TransactionFilters,
  UpdateTransactionInput,
} from './transactionTypes'

function buildTransactionsQuery(
  filters: TransactionFilters,
): string {
  const searchParams = new URLSearchParams()

  if (filters.personId !== undefined) {
    searchParams.set(
      'personId',
      filters.personId.toString(),
    )
  }

  if (filters.ageGroup !== undefined) {
    searchParams.set(
      'ageGroup',
      filters.ageGroup,
    )
  }

  if (filters.type !== undefined) {
    searchParams.set(
      'type',
      filters.type,
    )
  }

  if (filters.minAmount !== undefined) {
    searchParams.set(
      'minAmount',
      filters.minAmount.toString(),
    )
  }

  if (filters.maxAmount !== undefined) {
    searchParams.set(
      'maxAmount',
      filters.maxAmount.toString(),
    )
  }

  const queryString = searchParams.toString()

  return queryString
    ? `/api/transactions?${queryString}`
    : '/api/transactions'
}

export const transactionsApi = {
  list(
    filters: TransactionFilters = {},
    signal?: AbortSignal,
  ) {
    return httpClient.get<Transaction[]>(
      buildTransactionsQuery(filters),
      signal,
    )
  },

  getById(id: number, signal?: AbortSignal) {
    return httpClient.get<Transaction>(
      `/api/transactions/${id}`,
      signal,
    )
  },

  create(
    input: CreateTransactionInput,
    signal?: AbortSignal,
  ) {
    return httpClient.post<
      Transaction,
      CreateTransactionInput
    >(
      '/api/transactions',
      input,
      signal,
    )
  },

  update(
    id: number,
    input: UpdateTransactionInput,
    signal?: AbortSignal,
  ) {
    return httpClient.put<
      Transaction,
      UpdateTransactionInput
    >(
      `/api/transactions/${id}`,
      input,
      signal,
    )
  },

  delete(
    id: number,
    signal?: AbortSignal,
  ) {
    return httpClient.delete(
      `/api/transactions/${id}`,
      signal,
    )
  },
}