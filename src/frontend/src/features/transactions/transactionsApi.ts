import { httpClient } from '../../services/httpClient'
import type {
  CreateTransactionInput,
  Transaction,
} from './transactionTypes'

export const transactionsApi = {
  list(signal?: AbortSignal) {
    return httpClient.get<Transaction[]>(
      '/api/transactions',
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
}