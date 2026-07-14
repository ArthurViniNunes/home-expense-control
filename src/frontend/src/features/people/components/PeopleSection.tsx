import {
  AlertCircle,
  RefreshCw,
  Users,
} from 'lucide-react'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { getApiErrorMessage } from '@/services/apiError'

import type {
  CreatePersonInput,
} from '../peopleTypes'
import { usePeople } from '../usePeople'
import { PeopleList } from './PeopleList'
import { PersonForm } from './PersonForm'

export function PeopleSection() {
  const {
    people,
    isLoading,
    isSubmitting,
    deletingPersonId,
    loadError,
    loadPeople,
    createPerson,
    deletePerson,
  } = usePeople()

  async function handleCreate(
    input: CreatePersonInput,
  ) {
    try {
      const createdPerson =
        await createPerson(input)

      toast.success('Pessoa cadastrada', {
        description: `${createdPerson.name} foi adicionada à residência.`,
      })

      return createdPerson
    } catch (error) {
      toast.error(
        'Não foi possível cadastrar a pessoa',
        {
          description: getApiErrorMessage(
            error,
            'Verifique os dados e tente novamente.',
          ),
        },
      )

      throw error
    }
  }

  async function handleDelete(
    id: number,
  ): Promise<void> {
    try {
      const person = people.find(
        (item) => item.id === id,
      )

      await deletePerson(id)

      toast.success('Pessoa excluída', {
        description: person
          ? `${person.name} e suas transações foram removidas.`
          : 'A pessoa e suas transações foram removidas.',
      })
    } catch (error) {
      toast.error(
        'Não foi possível excluir a pessoa',
        {
          description: getApiErrorMessage(
            error,
            'Tente novamente em alguns instantes.',
          ),
        },
      )

      throw error
    }
  }

  async function handleRefresh() {
    try {
      await loadPeople()

      toast.success('Lista atualizada')
    } catch (error) {
      toast.error(
        'Não foi possível atualizar a lista',
        {
          description: getApiErrorMessage(
            error,
            'Tente novamente em alguns instantes.',
          ),
        },
      )
    }
  }

  return (
    <div className="grid items-start gap-6 lg:grid-cols-[340px_minmax(0,1fr)]">
      <aside className="lg:sticky lg:top-6">
        <PersonForm
          isSubmitting={isSubmitting}
          onSubmit={handleCreate}
        />
      </aside>

      <Card>
        <CardHeader className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="space-y-1.5">
            <CardTitle className="flex items-center gap-2">
              <Users
                className="size-5"
                aria-hidden="true"
              />

              Pessoas cadastradas
            </CardTitle>

            <CardDescription>
              {people.length === 0
                ? 'Nenhum morador cadastrado.'
                : `${people.length} ${
                    people.length === 1
                      ? 'pessoa cadastrada'
                      : 'pessoas cadastradas'
                  }.`}
            </CardDescription>
          </div>

          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={isLoading}
            onClick={() => {
              void handleRefresh()
            }}
          >
            <RefreshCw
              className={
                isLoading
                  ? 'size-4 animate-spin'
                  : 'size-4'
              }
              aria-hidden="true"
            />

            Atualizar
          </Button>
        </CardHeader>

        <CardContent>
          {isLoading && people.length === 0 ? (
            <PeopleListSkeleton />
          ) : loadError && people.length === 0 ? (
            <PeopleLoadError
              message={loadError}
              isLoading={isLoading}
              onRetry={handleRefresh}
            />
          ) : (
            <PeopleList
              people={people}
              deletingPersonId={deletingPersonId}
              onDelete={handleDelete}
            />
          )}
        </CardContent>
      </Card>
    </div>
  )
}

function PeopleListSkeleton() {
  return (
    <div
      className="grid gap-3"
      aria-label="Carregando pessoas"
    >
      {[1, 2, 3].map((item) => (
        <div
          key={item}
          className="flex items-center gap-3 rounded-xl border p-4"
        >
          <Skeleton className="size-11 rounded-xl" />

          <div className="flex-1 space-y-2">
            <Skeleton className="h-4 w-2/5" />
            <Skeleton className="h-3 w-1/5" />
          </div>

          <Skeleton className="h-8 w-24" />
        </div>
      ))}
    </div>
  )
}

interface PeopleLoadErrorProps {
  message: string
  isLoading: boolean
  onRetry: () => Promise<void>
}

function PeopleLoadError({
  message,
  isLoading,
  onRetry,
}: PeopleLoadErrorProps) {
  return (
    <div className="flex min-h-72 flex-col items-center justify-center rounded-xl border border-destructive/30 bg-destructive/5 p-8 text-center">
      <div className="flex size-12 items-center justify-center rounded-2xl bg-destructive/10 text-destructive">
        <AlertCircle
          className="size-6"
          aria-hidden="true"
        />
      </div>

      <h3 className="mt-4 font-medium">
        Não foi possível carregar as pessoas
      </h3>

      <p className="mt-2 max-w-md text-sm leading-6 text-muted-foreground">
        {message}
      </p>

      <Button
        type="button"
        variant="outline"
        className="mt-5"
        disabled={isLoading}
        onClick={() => {
          void onRetry()
        }}
      >
        <RefreshCw
          className={
            isLoading
              ? 'size-4 animate-spin'
              : 'size-4'
          }
          aria-hidden="true"
        />

        Tentar novamente
      </Button>
    </div>
  )
}