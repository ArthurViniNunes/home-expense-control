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
} from './transactionTypes'

interface TransactionsData {
  transactions: Transaction[]
  people: Person[]
}

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

async function fetchTransactionsData(
  signal?: AbortSignal,
): Promise<TransactionsData> {
  const [transactions, people] =
    await Promise.all([
      transactionsApi.list(signal),
      peopleApi.list(signal),
    ])

  return {
    transactions:
      orderTransactions(transactions),
    people: orderPeople(people),
  }
}

export function useTransactions() {
  const [transactions, setTransactions] =
    useState<Transaction[]>([])

  const [people, setPeople] =
    useState<Person[]>([])

  const [isLoading, setIsLoading] =
    useState(true)

  const [isSubmitting, setIsSubmitting] =
    useState(false)

  const [loadError, setLoadError] =
    useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    fetchTransactionsData(controller.signal)
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
        await fetchTransactionsData()

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
  }, [])

  async function createTransaction(
    input: CreateTransactionInput,
  ): Promise<Transaction> {
    setIsSubmitting(true)

    try {
      const createdTransaction =
        await transactionsApi.create(input)

      setTransactions(
        (currentTransactions) =>
          orderTransactions([
            ...currentTransactions,
            createdTransaction,
          ]),
      )

      return createdTransaction
    } finally {
      setIsSubmitting(false)
    }
  }

  return {
    transactions,
    people,
    isLoading,
    isSubmitting,
    loadError,
    loadData,
    createTransaction,
  }
}