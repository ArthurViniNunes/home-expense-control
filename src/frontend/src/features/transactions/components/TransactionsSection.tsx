import {
  AlertCircle,
  ArrowLeftRight,
  RefreshCw,
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
  CreateTransactionInput,
  Transaction,
} from '../transactionTypes'
import { useTransactions } from '../useTransactions'
import { TransactionForm } from './TransactionForm'
import { TransactionsList } from './TransactionsList'

export function TransactionsSection() {
  const {
    transactions,
    people,
    isLoading,
    isSubmitting,
    loadError,
    loadData,
    createTransaction,
  } = useTransactions()

  async function handleCreate(
    input: CreateTransactionInput,
  ): Promise<Transaction> {
    try {
      const createdTransaction =
        await createTransaction(input)

      toast.success('Transação registrada', {
        description: `${createdTransaction.description} foi adicionada com sucesso.`,
      })

      return createdTransaction
    } catch (error) {
      toast.error(
        'Não foi possível registrar a transação',
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

  async function handleRefresh() {
    try {
      await loadData()

      toast.success('Transações atualizadas')
    } catch (error) {
      toast.error(
        'Não foi possível atualizar as transações',
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
    <div className="grid items-start gap-6 lg:grid-cols-[360px_minmax(0,1fr)]">
      <aside className="lg:sticky lg:top-6">
        <TransactionForm
          people={people}
          isSubmitting={isSubmitting}
          onSubmit={handleCreate}
        />
      </aside>

      <Card>
        <CardHeader className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="space-y-1.5">
            <CardTitle className="flex items-center gap-2">
              <ArrowLeftRight
                className="size-5"
                aria-hidden="true"
              />

              Histórico de transações
            </CardTitle>

            <CardDescription>
              {isLoading && transactions.length === 0
                ? 'Carregando movimentações...'
                : transactions.length === 0
                  ? 'Nenhuma movimentação registrada.'
                  : `${transactions.length} ${
                      transactions.length === 1
                        ? 'transação registrada'
                        : 'transações registradas'
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
          {isLoading && transactions.length === 0 ? (
            <TransactionsListSkeleton />
          ) : loadError &&
            transactions.length === 0 ? (
            <TransactionsLoadError
              message={loadError}
              isLoading={isLoading}
              onRetry={handleRefresh}
            />
          ) : (
            <TransactionsList
              transactions={transactions}
            />
          )}
        </CardContent>
      </Card>
    </div>
  )
}

function TransactionsListSkeleton() {
  return (
    <div
      className="space-y-3"
      aria-label="Carregando transações"
    >
      <div className="hidden overflow-hidden rounded-xl border md:block">
        <div className="grid grid-cols-[2fr_1.3fr_1fr_1fr] gap-4 border-b bg-muted/40 px-4 py-3">
          {[1, 2, 3, 4].map((item) => (
            <Skeleton
              key={item}
              className="h-4 w-20"
            />
          ))}
        </div>

        {[1, 2, 3, 4].map((item) => (
          <div
            key={item}
            className="grid grid-cols-[2fr_1.3fr_1fr_1fr] items-center gap-4 border-b px-4 py-4 last:border-b-0"
          >
            <div className="space-y-2">
              <Skeleton className="h-4 w-40" />
              <Skeleton className="h-3 w-20" />
            </div>

            <div className="flex items-center gap-2">
              <Skeleton className="size-8 rounded-lg" />
              <Skeleton className="h-4 w-24" />
            </div>

            <Skeleton className="h-6 w-20 rounded-full" />

            <Skeleton className="ml-auto h-6 w-24" />
          </div>
        ))}
      </div>

      <div className="grid gap-3 md:hidden">
        {[1, 2, 3].map((item) => (
          <div
            key={item}
            className="space-y-4 rounded-xl border p-4"
          >
            <div className="flex justify-between gap-4">
              <div className="flex-1 space-y-2">
                <Skeleton className="h-4 w-3/5" />
                <Skeleton className="h-3 w-1/4" />
              </div>

              <Skeleton className="h-6 w-20 rounded-full" />
            </div>

            <div className="flex items-center justify-between border-t pt-4">
              <Skeleton className="h-8 w-28" />
              <Skeleton className="h-6 w-24" />
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}

interface TransactionsLoadErrorProps {
  message: string
  isLoading: boolean
  onRetry: () => Promise<void>
}

function TransactionsLoadError({
  message,
  isLoading,
  onRetry,
}: TransactionsLoadErrorProps) {
  return (
    <div className="flex min-h-72 flex-col items-center justify-center rounded-xl border border-destructive/30 bg-destructive/5 p-8 text-center">
      <div className="flex size-12 items-center justify-center rounded-2xl bg-destructive/10 text-destructive">
        <AlertCircle
          className="size-6"
          aria-hidden="true"
        />
      </div>

      <h3 className="mt-4 font-medium">
        Não foi possível carregar as transações
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