import {
  AlertCircle,
  ArrowDownRight,
  ArrowUpRight,
  RefreshCw,
  Scale,
  UserRound,
  Users,
} from 'lucide-react'
import { toast } from 'sonner'

import { MetricCard } from '@/components/dashboard/MetricCard'
import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { cn } from '@/lib/utils'
import { getApiErrorMessage } from '@/services/apiError'

import type { PersonTotals } from '@/features/totals/totalsTypes'
import { useTotals } from '@/features/totals/useTotals'

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

function getBalanceTone(
  balance: number,
): 'positive' | 'negative' | 'balance' {
  if (balance > 0) {
    return 'positive'
  }

  if (balance < 0) {
    return 'negative'
  }

  return 'balance'
}

function FinancialValue({
  value,
  type,
}: {
  value: number
  type: 'income' | 'expense' | 'balance'
}) {
  const valueClass =
    type === 'income'
      ? 'text-emerald-700 dark:text-emerald-400'
      : type === 'expense'
        ? 'text-rose-700 dark:text-rose-400'
        : value > 0
          ? 'text-emerald-700 dark:text-emerald-400'
          : value < 0
            ? 'text-rose-700 dark:text-rose-400'
            : 'text-muted-foreground'

  return (
    <span
      className={cn(
        'whitespace-nowrap font-semibold tabular-nums',
        valueClass,
      )}
    >
      {formatCurrency(value)}
    </span>
  )
}

export function TotalsOverview() {
  const {
    totals,
    isLoading,
    loadError,
    loadTotals,
  } = useTotals()

  async function handleRefresh() {
    try {
      await loadTotals()

      toast.success('Totais atualizados')
    } catch (error) {
      toast.error(
        'Não foi possível atualizar os totais',
        {
          description: getApiErrorMessage(
            error,
            'Tente novamente em alguns instantes.',
          ),
        },
      )
    }
  }

  if (isLoading && !totals) {
    return <TotalsOverviewSkeleton />
  }

  if (loadError && !totals) {
    return (
      <TotalsLoadError
        message={loadError}
        isLoading={isLoading}
        onRetry={handleRefresh}
      />
    )
  }

  if (!totals) {
    return null
  }

  const {
    people,
    general,
  } = totals

  return (
    <div className="space-y-6">
      <section
        className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4"
        aria-label="Indicadores financeiros"
      >
        <MetricCard
          title="Pessoas"
          value={String(people.length)}
          description={
            people.length === 1
              ? 'Morador considerado no consolidado'
              : 'Moradores considerados no consolidado'
          }
          icon={Users}
        />

        <MetricCard
          title="Receitas"
          value={formatCurrency(
            general.totalIncome,
          )}
          description="Total de receitas registradas"
          icon={ArrowUpRight}
          tone="positive"
        />

        <MetricCard
          title="Despesas"
          value={formatCurrency(
            general.totalExpenses,
          )}
          description="Total de despesas registradas"
          icon={ArrowDownRight}
          tone="negative"
        />

        <MetricCard
          title="Saldo líquido"
          value={formatCurrency(
            general.netBalance,
          )}
          description="Receitas menos despesas"
          icon={Scale}
          tone={getBalanceTone(
            general.netBalance,
          )}
          valueTone={getBalanceTone(
            general.netBalance,
          )}
        />
      </section>

      <Card>
        <CardHeader className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="space-y-1.5">
            <CardTitle className="flex items-center gap-2">
              <Scale
                className="size-5"
                aria-hidden="true"
              />

              Totais por pessoa
            </CardTitle>

            <CardDescription>
              {people.length === 0
                ? 'Nenhuma pessoa disponível para consolidação.'
                : people.length === 1
                  ? 'Resultado financeiro de uma pessoa.'
                  : `Resultado financeiro de ${people.length} pessoas.`}
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
          {people.length === 0 ? (
            <EmptyPersonTotals />
          ) : (
            <PersonTotalsList
              people={people}
            />
          )}
        </CardContent>
      </Card>
    </div>
  )
}

function PersonTotalsList({
  people,
}: {
  people: PersonTotals[]
}) {
  return (
    <>
      <div className="hidden overflow-hidden rounded-xl border md:block">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Pessoa</TableHead>

              <TableHead className="text-right">
                Receitas
              </TableHead>

              <TableHead className="text-right">
                Despesas
              </TableHead>

              <TableHead className="text-right">
                Saldo
              </TableHead>
            </TableRow>
          </TableHeader>

          <TableBody>
            {people.map((person) => (
              <TableRow key={person.personId}>
                <TableCell>
                  <div className="flex items-center gap-3">
                    <div className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-muted text-muted-foreground">
                      <UserRound
                        className="size-4"
                        aria-hidden="true"
                      />
                    </div>

                    <div className="min-w-0">
                      <p className="max-w-64 truncate font-medium">
                        {person.personName}
                      </p>

                      <p className="text-xs text-muted-foreground">
                        Pessoa #{person.personId}
                      </p>
                    </div>
                  </div>
                </TableCell>

                <TableCell className="text-right">
                  <FinancialValue
                    value={person.totalIncome}
                    type="income"
                  />
                </TableCell>

                <TableCell className="text-right">
                  <FinancialValue
                    value={person.totalExpenses}
                    type="expense"
                  />
                </TableCell>

                <TableCell className="text-right">
                  <FinancialValue
                    value={person.balance}
                    type="balance"
                  />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <div className="grid gap-3 md:hidden">
        {people.map((person) => (
          <Card
            key={person.personId}
            className="py-0"
          >
            <CardContent className="space-y-4 p-4">
              <div className="flex items-center gap-3">
                <div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-muted text-muted-foreground">
                  <UserRound
                    className="size-5"
                    aria-hidden="true"
                  />
                </div>

                <div className="min-w-0">
                  <p className="truncate font-medium">
                    {person.personName}
                  </p>

                  <p className="text-xs text-muted-foreground">
                    Pessoa #{person.personId}
                  </p>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3 border-t pt-4">
                <div>
                  <p className="text-xs text-muted-foreground">
                    Receitas
                  </p>

                  <div className="mt-1">
                    <FinancialValue
                      value={person.totalIncome}
                      type="income"
                    />
                  </div>
                </div>

                <div className="text-right">
                  <p className="text-xs text-muted-foreground">
                    Despesas
                  </p>

                  <div className="mt-1">
                    <FinancialValue
                      value={person.totalExpenses}
                      type="expense"
                    />
                  </div>
                </div>
              </div>

              <div className="flex items-center justify-between rounded-lg bg-muted/40 px-3 py-2.5">
                <span className="text-sm font-medium">
                  Saldo
                </span>

                <FinancialValue
                  value={person.balance}
                  type="balance"
                />
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </>
  )
}

function EmptyPersonTotals() {
  return (
    <div className="flex min-h-64 flex-col items-center justify-center rounded-xl border border-dashed bg-muted/20 p-8 text-center">
      <div className="flex size-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
        <Users
          className="size-6"
          aria-hidden="true"
        />
      </div>

      <h3 className="mt-4 font-medium">
        Nenhum total disponível
      </h3>

      <p className="mt-2 max-w-sm text-sm leading-6 text-muted-foreground">
        Cadastre pessoas e registre transações para
        visualizar o consolidado financeiro.
      </p>
    </div>
  )
}

function TotalsOverviewSkeleton() {
  return (
    <div className="space-y-6">
      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {[1, 2, 3, 4].map((item) => (
          <Card key={item}>
            <CardHeader className="flex flex-row items-center justify-between">
              <Skeleton className="h-4 w-20" />
              <Skeleton className="size-9 rounded-xl" />
            </CardHeader>

            <CardContent className="space-y-2">
              <Skeleton className="h-7 w-32" />
              <Skeleton className="h-3 w-40" />
            </CardContent>
          </Card>
        ))}
      </section>

      <Card>
        <CardHeader>
          <Skeleton className="h-5 w-40" />
          <Skeleton className="h-4 w-64" />
        </CardHeader>

        <CardContent className="space-y-3">
          {[1, 2, 3].map((item) => (
            <Skeleton
              key={item}
              className="h-16 w-full rounded-xl"
            />
          ))}
        </CardContent>
      </Card>
    </div>
  )
}

interface TotalsLoadErrorProps {
  message: string
  isLoading: boolean
  onRetry: () => Promise<void>
}

function TotalsLoadError({
  message,
  isLoading,
  onRetry,
}: TotalsLoadErrorProps) {
  return (
    <Card>
      <CardContent className="flex min-h-80 flex-col items-center justify-center p-8 text-center">
        <div className="flex size-12 items-center justify-center rounded-2xl bg-destructive/10 text-destructive">
          <AlertCircle
            className="size-6"
            aria-hidden="true"
          />
        </div>

        <h3 className="mt-4 font-medium">
          Não foi possível carregar os totais
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
      </CardContent>
    </Card>
  )
}