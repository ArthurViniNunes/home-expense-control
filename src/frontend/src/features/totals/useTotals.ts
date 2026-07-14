import {
  useCallback,
  useEffect,
  useState,
} from 'react'

import { getApiErrorMessage } from '@/services/apiError'

import { totalsApi } from './totalsApi'
import type { Totals } from './totalsTypes'

function orderTotals(totals: Totals): Totals {
  return {
    ...totals,
    people: [...totals.people].sort(
      (first, second) =>
        first.personName.localeCompare(
          second.personName,
          'pt-BR',
          {
            sensitivity: 'base',
          },
        ),
    ),
  }
}

async function fetchTotals(
  signal?: AbortSignal,
): Promise<Totals> {
  const totals = await totalsApi.get(signal)

  return orderTotals(totals)
}

export function useTotals() {
  const [totals, setTotals] =
    useState<Totals | null>(null)

  const [isLoading, setIsLoading] =
    useState(true)

  const [loadError, setLoadError] =
    useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    fetchTotals(controller.signal)
      .then((response) => {
        if (!controller.signal.aborted) {
          setTotals(response)
        }
      })
      .catch((error: unknown) => {
        if (!controller.signal.aborted) {
          setLoadError(
            getApiErrorMessage(
              error,
              'Não foi possível carregar os totais.',
            ),
          )
        }
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

  const loadTotals = useCallback(async () => {
    setIsLoading(true)
    setLoadError(null)

    try {
      const response = await fetchTotals()

      setTotals(response)
    } catch (error) {
      setLoadError(
        getApiErrorMessage(
          error,
          'Não foi possível carregar os totais.',
        ),
      )

      throw error
    } finally {
      setIsLoading(false)
    }
  }, [])

  return {
    totals,
    isLoading,
    loadError,
    loadTotals,
  }
}