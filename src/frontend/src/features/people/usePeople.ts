import {
  useCallback,
  useEffect,
  useState,
} from 'react'
import { getApiErrorMessage } from '../../services/apiError'
import { peopleApi } from './peopleApi'
import type {
  CreatePersonInput,
  Person,
} from './peopleTypes'

interface Feedback {
  tone: 'success' | 'error'
  message: string
}

function orderPeople(people: Person[]) {
  return [...people].sort((first, second) => {
    const nameComparison = first.name.localeCompare(
      second.name,
      'pt-BR',
      {
        sensitivity: 'base',
      },
    )

    if (nameComparison !== 0) {
      return nameComparison
    }

    return first.id - second.id
  })
}

async function fetchPeople(
  signal?: AbortSignal,
): Promise<Person[]> {
  const response = await peopleApi.list(signal)

  return orderPeople(response)
}

export function usePeople() {
  const [people, setPeople] = useState<Person[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] =
    useState(false)

  const [deletingPersonId, setDeletingPersonId] =
    useState<number | null>(null)

  const [loadError, setLoadError] =
    useState<string | null>(null)

  const [feedback, setFeedback] =
    useState<Feedback | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    fetchPeople(controller.signal)
      .then((response) => {
        if (controller.signal.aborted) {
          return
        }

        setPeople(response)
      })
      .catch((error: unknown) => {
        if (controller.signal.aborted) {
          return
        }

        setLoadError(
          getApiErrorMessage(
            error,
            'Não foi possível carregar as pessoas.',
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

  const loadPeople = useCallback(
    async (signal?: AbortSignal) => {
      setIsLoading(true)
      setLoadError(null)

      try {
        const response = await fetchPeople(signal)

        setPeople(response)
      } catch (error) {
        if (signal?.aborted) {
          return
        }

        setLoadError(
          getApiErrorMessage(
            error,
            'Não foi possível carregar as pessoas.',
          ),
        )
      } finally {
        if (!signal?.aborted) {
          setIsLoading(false)
        }
      }
    },
    [],
  )

  async function createPerson(
    input: CreatePersonInput,
  ): Promise<Person> {
    setIsSubmitting(true)
    setFeedback(null)

    try {
      const createdPerson =
        await peopleApi.create(input)

      setPeople((currentPeople) =>
        orderPeople([
          ...currentPeople,
          createdPerson,
        ]),
      )

      setFeedback({
        tone: 'success',
        message: `${createdPerson.name} foi cadastrado com sucesso.`,
      })

      return createdPerson
    } catch (error) {
      setFeedback({
        tone: 'error',
        message: getApiErrorMessage(
          error,
          'Não foi possível cadastrar a pessoa.',
        ),
      })

      throw error
    } finally {
      setIsSubmitting(false)
    }
  }

  async function deletePerson(id: number) {
    setDeletingPersonId(id)
    setFeedback(null)

    try {
      const person = people.find(
        (item) => item.id === id,
      )

      await peopleApi.delete(id)

      setPeople((currentPeople) =>
        currentPeople.filter(
          (item) => item.id !== id,
        ),
      )

      setFeedback({
        tone: 'success',
        message: person
          ? `${person.name} foi excluído com sucesso.`
          : 'A pessoa foi excluída com sucesso.',
      })
    } catch (error) {
      setFeedback({
        tone: 'error',
        message: getApiErrorMessage(
          error,
          'Não foi possível excluir a pessoa.',
        ),
      })

      throw error
    } finally {
      setDeletingPersonId(null)
    }
  }

  function clearFeedback() {
    setFeedback(null)
  }

  return {
    people,
    isLoading,
    isSubmitting,
    deletingPersonId,
    loadError,
    feedback,
    loadPeople,
    createPerson,
    deletePerson,
    clearFeedback,
  }
}