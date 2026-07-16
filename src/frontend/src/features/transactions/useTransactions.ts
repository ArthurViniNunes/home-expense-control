import {
  useCallback,
  useEffect,
  useState,
} from 'react'

import { peopleApi } from '@/features/people/peopleApi'
import type { Person } from '@/features/people/peopleTypes'
import { getApiErrorMessage } from '@/services/apiError'

import { transactionsApi } from './transactionsApi'
import type {
  CreateTransactionInput,
  Transaction,
  TransactionFilters,
  UpdateTransactionInput,
} from './transactionTypes'

interface TransactionsData {
  transactions: Transaction[]
  people: Person[]
}

const EMPTY_FILTERS: TransactionFilters = {}

function orderPeople(people: Person[]) {
  return [...people].sort((first, second) =>
    first.name.localeCompare(
      second.name,
      'pt-BR',
      {
        sensitivity: 'base',
      },
    ),
  )
}

function orderTransactions(
  transactions: Transaction[],
) {
  return [...transactions].sort(
    (first, second) => second.id - first.id,
  )
}

async function fetchTransactions(
  filters: TransactionFilters,
  signal?: AbortSignal,
): Promise<Transaction[]> {
  const transactions =
    await transactionsApi.list(
      filters,
      signal,
    )

  return orderTransactions(transactions)
}

async function fetchTransactionsData(
  filters: TransactionFilters,
  signal?: AbortSignal,
): Promise<TransactionsData> {
  const [transactions, people] =
    await Promise.all([
      fetchTransactions(filters, signal),
      peopleApi.list(signal),
    ])

  return {
    transactions,
    people: orderPeople(people),
  }
}

export function useTransactions() {
  const [transactions, setTransactions] =
    useState<Transaction[]>([])

  const [people, setPeople] =
    useState<Person[]>([])

  const [filters, setFilters] =
    useState<TransactionFilters>(
      EMPTY_FILTERS,
    )

  const [isLoading, setIsLoading] =
    useState(true)

  const [isSubmitting, setIsSubmitting] =
    useState(false)

  const [
    updatingTransactionId,
    setUpdatingTransactionId,
  ] = useState<number | null>(null)

  const [
    deletingTransactionId,
    setDeletingTransactionId,
  ] = useState<number | null>(null)

  const [loadError, setLoadError] =
    useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    fetchTransactionsData(
      EMPTY_FILTERS,
      controller.signal,
    )
      .then((data) => {
        if (controller.signal.aborted) {
          return
        }

        setTransactions(data.transactions)
        setPeople(data.people)
      })
      .catch((error: unknown) => {
        if (controller.signal.aborted) {
          return
        }

        setLoadError(
          getApiErrorMessage(
            error,
            'Não foi possível carregar as transações.',
          ),
        )
      })
      .finally(() => {
        if (!controller.signal.aborted) {
          setIsLoading(false)
        }
      })

    return () => {
      controller.abort()
    }
  }, [])

  const loadData = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)

    try {
      const data =
        await fetchTransactionsData(filters)

      setTransactions(data.transactions)
      setPeople(data.people)
    } catch (error) {
      setLoadError(
        getApiErrorMessage(
          error,
          'Não foi possível carregar as transações.',
        ),
      )

      throw error
    } finally {
      setIsLoading(false)
    }
  }, [filters])

  const applyFilters = useCallback(
    async (
      nextFilters: TransactionFilters,
    ) => {
      setIsLoading(true)
      setLoadError(null)

      try {
        const filteredTransactions =
          await fetchTransactions(nextFilters)

        setTransactions(filteredTransactions)
        setFilters({ ...nextFilters })
      } catch (error) {
        setLoadError(
          getApiErrorMessage(
            error,
            'Não foi possível aplicar os filtros.',
          ),
        )

        throw error
      } finally {
        setIsLoading(false)
      }
    },
    [],
  )

  const clearFilters = useCallback(
    async () => {
      await applyFilters({})
    },
    [applyFilters],
  )

  async function createTransaction(
    input: CreateTransactionInput,
  ): Promise<Transaction> {
    setIsSubmitting(true)

    try {
      const createdTransaction =
        await transactionsApi.create(input)

      /*
       * Recarrega a listagem respeitando os filtros
       * aplicados. Assim, uma nova transação que não
       * corresponda ao filtro atual não aparece
       * incorretamente no histórico.
       */
      try {
        const refreshedTransactions =
          await fetchTransactions(filters)

        setTransactions(refreshedTransactions)
        setLoadError(null)
      } catch (error) {
        setLoadError(
          getApiErrorMessage(
            error,
            'A transação foi criada, mas a listagem não pôde ser atualizada.',
          ),
        )
      }

      return createdTransaction
    } finally {
      setIsSubmitting(false)
    }
  }

  async function updateTransaction(
    id: number,
    input: UpdateTransactionInput,
  ): Promise<Transaction> {
    setUpdatingTransactionId(id)

    try {
      const updatedTransaction =
        await transactionsApi.update(
          id,
          input,
        )

      /*
      * A transação pode deixar de atender aos
      * filtros atuais depois da edição.
      *
      * Por isso, recarregamos a consulta no
      * servidor em vez de apenas substituir
      * o item localmente.
      */
      try {
        const refreshedTransactions =
          await fetchTransactions(filters)

        setTransactions(
          refreshedTransactions,
        )

        setLoadError(null)
      } catch (error) {
        setLoadError(
          getApiErrorMessage(
            error,
            'A transação foi atualizada, mas a listagem não pôde ser recarregada.',
          ),
        )
      }

      return updatedTransaction
    } finally {
      setUpdatingTransactionId(null)
    }
  }
  async function deleteTransaction(
    id: number,
  ): Promise<void> {
    setDeletingTransactionId(id)

    try {
      await transactionsApi.delete(id)

      /*
      * Recarrega os dados mantendo os filtros
      * atualmente aplicados.
      */
      try {
        const refreshedTransactions =
          await fetchTransactions(filters)

        setTransactions(
          refreshedTransactions,
        )

        setLoadError(null)
      } catch (error) {
        setLoadError(
          getApiErrorMessage(
            error,
            'A transação foi excluída, mas a listagem não pôde ser recarregada.',
          ),
        )
      }
    } finally {
      setDeletingTransactionId(null)
    }
  }

  const hasActiveFilters =
    Object.values(filters).some(
      (value) => value !== undefined,
    )

  return {
    transactions,
    people,
    filters,
    hasActiveFilters,
    isLoading,
    isSubmitting,
    loadError,
    loadData,
    applyFilters,
    clearFilters,
    createTransaction,
    updateTransaction,
    deleteTransaction,
    updatingTransactionId,
    deletingTransactionId,
  }
}