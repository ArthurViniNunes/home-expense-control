import {
  useState,
  type MouseEvent,
} from 'react'
import {
  LoaderCircle,
  Trash2,
  UserRound,
  Users,
} from 'lucide-react'

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
} from '@/components/ui/card'

import type { Person } from '../peopleTypes'

interface PeopleListProps {
  people: Person[]
  deletingPersonId: number | null
  onDelete: (id: number) => Promise<void>
}

interface PersonListItemProps {
  person: Person
  isDeleting: boolean
  isDeleteDisabled: boolean
  onDelete: (id: number) => Promise<void>
}

function PersonListItem({
  person,
  isDeleting,
  isDeleteDisabled,
  onDelete,
}: PersonListItemProps) {
  const [isDialogOpen, setIsDialogOpen] =
    useState(false)

  const isMinor = person.isMinor

  async function handleDelete(
    event: MouseEvent<HTMLButtonElement>,
  ) {
    event.preventDefault()

    try {
      await onDelete(person.id)
      setIsDialogOpen(false)
    } catch {
      // A mensagem de erro será exibida pela seção de pessoas.
      // O diálogo permanece aberto para permitir uma nova tentativa.
    }
  }

  return (
    <Card className="py-0">
      <CardContent className="flex flex-col gap-4 p-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex min-w-0 items-center gap-3">
          <div className="flex size-11 shrink-0 items-center justify-center rounded-xl bg-muted text-muted-foreground">
            <UserRound
              className="size-5"
              aria-hidden="true"
            />
          </div>

          <div className="min-w-0">
            <p className="truncate font-medium">
              {person.name}
            </p>

            <p className="text-sm text-muted-foreground">
              {person.age}{' '}
              {person.age === 1 ? 'ano' : 'anos'}
            </p>
          </div>
        </div>

        <div className="flex flex-wrap items-center justify-between gap-2 sm:justify-end">
          <Badge
            variant={
              isMinor
                ? 'secondary'
                : 'outline'
            }
          >
            {isMinor
              ? 'Menor de idade'
              : 'Adulto'}
          </Badge>

          <AlertDialog
            open={isDialogOpen}
            onOpenChange={(open) => {
              if (!isDeleting) {
                setIsDialogOpen(open)
              }
            }}
          >
            <AlertDialogTrigger asChild>
              <Button
                type="button"
                variant="outline"
                size="sm"
                disabled={isDeleteDisabled}
              >
                <Trash2
                  className="size-4"
                  aria-hidden="true"
                />

                Excluir
              </Button>
            </AlertDialogTrigger>

            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>
                  Excluir {person.name}?
                </AlertDialogTitle>

                <AlertDialogDescription>
                  Todas as transações vinculadas a essa
                  pessoa também serão excluídas. Essa ação
                  não poderá ser desfeita.
                </AlertDialogDescription>
              </AlertDialogHeader>

              <AlertDialogFooter>
                <AlertDialogCancel
                  disabled={isDeleting}
                >
                  Cancelar
                </AlertDialogCancel>

                <AlertDialogAction asChild>
                  <Button
                    type="button"
                    variant="destructive"
                    disabled={isDeleting}
                    onClick={(event) => {
                      void handleDelete(event)
                    }}
                  >
                    {isDeleting ? (
                      <>
                        <LoaderCircle
                          className="size-4 animate-spin"
                          aria-hidden="true"
                        />

                        Excluindo...
                      </>
                    ) : (
                      <>
                        <Trash2
                          className="size-4"
                          aria-hidden="true"
                        />

                        Confirmar exclusão
                      </>
                    )}
                  </Button>
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </div>
      </CardContent>
    </Card>
  )
}

export function PeopleList({
  people,
  deletingPersonId,
  onDelete,
}: PeopleListProps) {
  if (people.length === 0) {
    return (
      <div className="flex min-h-72 flex-col items-center justify-center rounded-xl border border-dashed bg-muted/20 p-8 text-center">
        <div className="flex size-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
          <Users
            className="size-6"
            aria-hidden="true"
          />
        </div>

        <h3 className="mt-4 font-medium">
          Nenhuma pessoa cadastrada
        </h3>

        <p className="mt-2 max-w-sm text-sm leading-6 text-muted-foreground">
          Utilize o formulário para cadastrar o primeiro
          morador da residência.
        </p>
      </div>
    )
  }

  return (
    <div className="grid gap-3">
      {people.map((person) => (
        <PersonListItem
          key={person.id}
          person={person}
          isDeleting={
            deletingPersonId === person.id
          }
          isDeleteDisabled={
            deletingPersonId !== null
          }
          onDelete={onDelete}
        />
      ))}
    </div>
  )
}