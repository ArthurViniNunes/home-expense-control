import {
  ArrowDownRight,
  ArrowUpRight,
  ReceiptText,
  UserRound,
} from 'lucide-react'

import { Badge } from '@/components/ui/badge'
import {
  Card,
  CardContent,
} from '@/components/ui/card'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { cn } from '@/lib/utils'

import type {
  Transaction,
  TransactionType,
  UpdateTransactionInput,
} from '../transactionTypes'

import type { Person } from '@/features/people/peopleTypes'

import { TransactionDeleteDialog } from './TransactionDeleteDialog'
import { TransactionEditDialog } from './TransactionEditDialog'

interface TransactionsListProps {
  transactions: Transaction[]
  people: Person[]
  updatingTransactionId: number | null
  deletingTransactionId: number | null
  onUpdate: (
    id: number,
    input: UpdateTransactionInput,
  ) => Promise<Transaction>
  onDelete: (id: number) => Promise<void>
}

const currencyFormatter = new Intl.NumberFormat(
  'pt-BR',
  {
    style: 'currency',
    currency: 'BRL',
  },
)

function formatCurrency(value: number) {
  return currencyFormatter.format(value)
}

function getTypeLabel(type: TransactionType) {
  return type === 'income'
    ? 'Receita'
    : 'Despesa'
}

function TransactionTypeBadge({
  type,
}: {
  type: TransactionType
}) {
  const isIncome = type === 'income'

  return (
    <Badge
      variant="outline"
      className={cn(
        'gap-1.5',
        isIncome
          ? 'border-emerald-500/30 bg-emerald-500/10 text-emerald-700 dark:text-emerald-400'
          : 'border-rose-500/30 bg-rose-500/10 text-rose-700 dark:text-rose-400',
      )}
    >
      {isIncome ? (
        <ArrowUpRight
          className="size-3.5"
          aria-hidden="true"
        />
      ) : (
        <ArrowDownRight
          className="size-3.5"
          aria-hidden="true"
        />
      )}

      {getTypeLabel(type)}
    </Badge>
  )
}

function EmptyTransactions() {
  return (
    <div className="flex min-h-72 flex-col items-center justify-center rounded-xl border border-dashed bg-muted/20 p-8 text-center">
      <div className="flex size-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
        <ReceiptText
          className="size-6"
          aria-hidden="true"
        />
      </div>

      <h3 className="mt-4 font-medium">
        Nenhuma transação registrada
      </h3>

      <p className="mt-2 max-w-sm text-sm leading-6 text-muted-foreground">
        Utilize o formulário para registrar a primeira
        receita ou despesa da residência.
      </p>
    </div>
  )
}

function DesktopTransactionsTable({
  transactions,
  people,
  updatingTransactionId,
  deletingTransactionId,
  onUpdate,
  onDelete,
}: TransactionsListProps) {
  return (
    <div className="hidden overflow-hidden rounded-xl border md:block">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Descrição</TableHead>
            <TableHead>Pessoa</TableHead>
            <TableHead>Tipo</TableHead>
            <TableHead className="text-right">
              Valor
            </TableHead>
            <TableHead className="w-24 text-right">
              Ações
            </TableHead>
          </TableRow>
        </TableHeader>

        <TableBody>
          {transactions.map((transaction) => {
            const isIncome =
              transaction.type === 'income'

            return (
              <TableRow key={transaction.id}>
                <TableCell>
                  <div className="min-w-0">
                    <p className="max-w-xs truncate font-medium">
                      {transaction.description}
                    </p>

                    <p className="mt-1 text-xs text-muted-foreground">
                      Transação #{transaction.id}
                    </p>
                  </div>
                </TableCell>

                <TableCell>
                  <div className="flex items-center gap-2">
                    <div className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-muted text-muted-foreground">
                      <UserRound
                        className="size-4"
                        aria-hidden="true"
                      />
                    </div>

                    <span className="max-w-44 truncate">
                      {transaction.person.name}
                    </span>
                  </div>
                </TableCell>

                <TableCell>
                  <TransactionTypeBadge
                    type={transaction.type}
                  />
                </TableCell>

                <TableCell
                  className={cn(
                    'text-right font-semibold tabular-nums',
                    isIncome
                      ? 'text-emerald-700 dark:text-emerald-400'
                      : 'text-rose-700 dark:text-rose-400',
                  )}
                >
                  {isIncome ? '+' : '-'}
                  {formatCurrency(
                    transaction.amount,
                  )}
                </TableCell>

                <TableCell>
                  <div className="flex items-center justify-end gap-1">
                    <TransactionEditDialog
                      transaction={transaction}
                      people={people}
                      isUpdating={
                        updatingTransactionId ===
                        transaction.id
                      }
                      onUpdate={onUpdate}
                    />

                    <TransactionDeleteDialog
                      transaction={transaction}
                      isDeleting={
                        deletingTransactionId ===
                        transaction.id
                      }
                      onDelete={onDelete}
                    />
                  </div>
                </TableCell>
              </TableRow>
            )
          })}
        </TableBody>
      </Table>
    </div>
  )
}

function MobileTransactionsCards({
  transactions,
  people,
  updatingTransactionId,
  deletingTransactionId,
  onUpdate,
  onDelete,
}: TransactionsListProps) {
  return (
    <div className="grid gap-3 md:hidden">
      {transactions.map((transaction) => {
        const isIncome =
          transaction.type === 'income'

        return (
          <Card
            key={transaction.id}
            className="py-0"
          >
            <CardContent className="space-y-4 p-4">
              <div className="flex items-start justify-between gap-4">
                <div className="min-w-0">
                  <p className="truncate font-medium">
                    {transaction.description}
                  </p>

                  <p className="mt-1 text-xs text-muted-foreground">
                    Transação #{transaction.id}
                  </p>
                </div>

                <TransactionTypeBadge
                  type={transaction.type}
                />
              </div>

              <div className="flex items-center justify-between gap-4 border-t pt-4">
                <div className="flex min-w-0 items-center gap-2">
                  <div className="flex size-8 shrink-0 items-center justify-center rounded-lg bg-muted text-muted-foreground">
                    <UserRound
                      className="size-4"
                      aria-hidden="true"
                    />
                  </div>

                  <span className="truncate text-sm text-muted-foreground">
                    {transaction.person.name}
                  </span>
                </div>

                <p
                  className={cn(
                    'shrink-0 font-semibold tabular-nums',
                    isIncome
                      ? 'text-emerald-700 dark:text-emerald-400'
                      : 'text-rose-700 dark:text-rose-400',
                  )}
                >
                  {isIncome ? '+' : '-'}
                  {formatCurrency(
                    transaction.amount,
                  )}
                </p>
              </div>

              <div className="flex items-center justify-end gap-1 border-t pt-3">
                <TransactionEditDialog
                  transaction={transaction}
                  people={people}
                  isUpdating={
                    updatingTransactionId ===
                    transaction.id
                  }
                  onUpdate={onUpdate}
                />

                <TransactionDeleteDialog
                  transaction={transaction}
                  isDeleting={
                    deletingTransactionId ===
                    transaction.id
                  }
                  onDelete={onDelete}
                />
              </div>
            </CardContent>
          </Card>
        )
      })}
    </div>
  )
}

export function TransactionsList({
  transactions,
  people,
  updatingTransactionId,
  deletingTransactionId,
  onUpdate,
  onDelete,
}: TransactionsListProps) {
  if (transactions.length === 0) {
    return <EmptyTransactions />
  }

  return (
    <>
      <DesktopTransactionsTable
        transactions={transactions}
        people={people}
        updatingTransactionId={
          updatingTransactionId
        }
        deletingTransactionId={
          deletingTransactionId
        }
        onUpdate={onUpdate}
        onDelete={onDelete}
      />

      <MobileTransactionsCards
        transactions={transactions}
        people={people}
        updatingTransactionId={
          updatingTransactionId
        }
        deletingTransactionId={
          deletingTransactionId
        }
        onUpdate={onUpdate}
        onDelete={onDelete}
      />
    </>
  )
}