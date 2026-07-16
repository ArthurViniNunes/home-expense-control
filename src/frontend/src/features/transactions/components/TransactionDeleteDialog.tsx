import {
  LoaderCircle,
  Trash2,
} from 'lucide-react'
import { useState } from 'react'

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
import { Button } from '@/components/ui/button'
import { getApiErrorMessage } from '@/services/apiError'

import type { Transaction } from '../transactionTypes'

interface TransactionDeleteDialogProps {
  transaction: Transaction
  isDeleting: boolean
  onDelete: (id: number) => Promise<void>
}

export function TransactionDeleteDialog({
  transaction,
  isDeleting,
  onDelete,
}: TransactionDeleteDialogProps) {
  const [open, setOpen] = useState(false)

  const [error, setError] =
    useState<string | null>(null)

  async function handleDelete() {
    setError(null)

    try {
      await onDelete(transaction.id)
      setOpen(false)
    } catch (deleteError) {
      setError(
        getApiErrorMessage(
          deleteError,
          'Não foi possível excluir a transação.',
        ),
      )
    }
  }

  return (
    <AlertDialog
      open={open}
      onOpenChange={(nextOpen) => {
        if (isDeleting) {
          return
        }

        setOpen(nextOpen)

        if (!nextOpen) {
          setError(null)
        }
      }}
    >
      <AlertDialogTrigger asChild>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          className="size-8 text-destructive hover:bg-destructive/10 hover:text-destructive"
          disabled={isDeleting}
          aria-label={`Excluir ${transaction.description}`}
        >
          <Trash2
            className="size-4"
            aria-hidden="true"
          />
        </Button>
      </AlertDialogTrigger>

      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>
            Excluir transação?
          </AlertDialogTitle>

          <AlertDialogDescription>
            A transação{' '}
            <strong className="font-medium text-foreground">
              {transaction.description}
            </strong>
            , vinculada a{' '}
            <strong className="font-medium text-foreground">
              {transaction.person.name}
            </strong>
            , será removida permanentemente.
            Essa ação não poderá ser desfeita.
          </AlertDialogDescription>
        </AlertDialogHeader>

        {error ? (
          <div
            className="rounded-lg border border-destructive/30 bg-destructive/10 px-4 py-3 text-sm text-destructive"
            role="alert"
          >
            {error}
          </div>
        ) : null}

        <AlertDialogFooter>
          <AlertDialogCancel
            disabled={isDeleting}
          >
            Cancelar
          </AlertDialogCancel>

          <AlertDialogAction
            disabled={isDeleting}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            onClick={(event) => {
              event.preventDefault()
              void handleDelete()
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

                Excluir transação
              </>
            )}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}