import {
  AlertCircle,
  ArrowLeftRight,
  RefreshCw,
  SearchX,
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
  TransactionFilters as TransactionFiltersValue,
  UpdateTransactionInput,
} from '../transactionTypes'
import { useTransactions } from '../useTransactions'
import { TransactionForm } from './TransactionForm'
import { TransactionsList } from './TransactionsList'
import { TransactionFilters } from './TransactionFilters'

export function TransactionsSection() {
  const {
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
    updatingTransactionId,
    deletingTransactionId,
    updateTransaction,
    deleteTransaction,
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

  async function handleUpdate(
    id: number,
    input: UpdateTransactionInput,
  ): Promise<Transaction> {
    try {
      const updatedTransaction =
        await updateTransaction(id, input)

      toast.success('Transação atualizada', {
        description: `${updatedTransaction.description} foi atualizada com sucesso.`,
      })

      return updatedTransaction
    } catch (error) {
      toast.error(
        'Não foi possível atualizar a transação',
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
      await deleteTransaction(id)

      toast.success('Transação excluída')
    } catch (error) {
      toast.error(
        'Não foi possível excluir a transação',
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

  async function handleApplyFilters(
    nextFilters: TransactionFiltersValue,
  ) {
    try {
      await applyFilters(nextFilters)

      toast.success('Filtros aplicados')
    } catch (error) {
      toast.error(
        'Não foi possível aplicar os filtros',
        {
          description: getApiErrorMessage(
            error,
            'Verifique os filtros e tente novamente.',
          ),
        },
      )
    }
  }

  async function handleClearFilters() {
    try {
      await clearFilters()

      toast.success('Filtros removidos')
    } catch (error) {
      toast.error(
        'Não foi possível remover os filtros',
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
                : transactions.length === 0 &&
                    hasActiveFilters
                  ? 'Nenhuma transação encontrada com os filtros aplicados.'
                  : transactions.length === 0
                    ? 'Nenhuma movimentação registrada.'
                    : `${transactions.length} ${
                        transactions.length === 1
                          ? 'transação encontrada'
                          : 'transações encontradas'
                      }${
                        hasActiveFilters
                          ? ' com os filtros aplicados'
                          : ''
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
          <TransactionFilters
            people={people}
            filters={filters}
            hasActiveFilters={hasActiveFilters}
            isLoading={isLoading}
            onApply={handleApplyFilters}
            onClear={handleClearFilters}
          />

          {isLoading && transactions.length === 0 ? (
            <TransactionsListSkeleton />
          ) : loadError &&
            transactions.length === 0 ? (
            <TransactionsLoadError
              message={loadError}
              isLoading={isLoading}
              onRetry={handleRefresh}
            />
          ) : transactions.length === 0 &&
            hasActiveFilters ? (
            <TransactionsFilteredEmptyState
              isLoading={isLoading}
              onClear={handleClearFilters}
            />
          ) : (
            <TransactionsList
              transactions={transactions}
              people={people}
              updatingTransactionId={
                updatingTransactionId
              }
              deletingTransactionId={
                deletingTransactionId
              }
              onUpdate={handleUpdate}
              onDelete={handleDelete}
            />
          )}
        </CardContent>
      </Card>
    </div>
  )
}

interface TransactionsFilteredEmptyStateProps {
  isLoading: boolean
  onClear: () => Promise<void>
}

function TransactionsFilteredEmptyState({
  isLoading,
  onClear,
}: TransactionsFilteredEmptyStateProps) {
  return (
    <div className="flex min-h-72 flex-col items-center justify-center rounded-xl border border-dashed p-8 text-center">
      <div className="flex size-12 items-center justify-center rounded-2xl bg-muted text-muted-foreground">
        <SearchX
          className="size-6"
          aria-hidden="true"
        />
      </div>

      <h3 className="mt-4 font-medium">
        Nenhuma transação encontrada
      </h3>

      <p className="mt-2 max-w-md text-sm leading-6 text-muted-foreground">
        Não existem transações que atendam aos
        filtros selecionados.
      </p>

      <Button
        type="button"
        variant="outline"
        className="mt-5"
        disabled={isLoading}
        onClick={() => {
          void onClear()
        }}
      >
        Limpar filtros
      </Button>
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
        <div className="grid grid-cols-[2fr_1.3fr_1fr_1fr_80px] gap-4 border-b bg-muted/40 px-4 py-3">
          {[1, 2, 3, 4, 5].map((item) => (
            <Skeleton
              key={item}
              className="h-4 w-20"
            />
          ))}
        </div>

        {[1, 2, 3, 4, 5].map((item) => (
          <div
            key={item}
            className="grid grid-cols-[2fr_1.3fr_1fr_1fr_80px] items-center gap-4 border-b px-4 py-4 last:border-b-0"
          >
            <div className="space-y-2">
              <Skeleton className="h-4 w-40" />
              <Skeleton className="h-3 w-20" />
            </div>

            <div className="flex items-center gap-2">
              <Skeleton className="size-8 rounded-lg" />
              <Skeleton className="h-4 w-24" />
            </div>

            <div className="flex justify-end gap-1">
              <Skeleton className="size-8 rounded-md" />
              <Skeleton className="size-8 rounded-md" />
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