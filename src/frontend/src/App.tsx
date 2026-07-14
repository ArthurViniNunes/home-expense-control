import {
  ArrowLeftRight,
  LayoutDashboard,
  Users,
} from 'lucide-react'

import { AppHeader } from '@/components/layout/AppHeader'
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from '@/components/ui/tabs'
import { TotalsOverview } from '@/features/totals/components/TotalsOverview'
import { PeopleSection } from '@/features/people/components/PeopleSection'
import { TransactionsSection } from './features/transactions/components/TransactionsSection'

function App() {
  return (
    <div className="min-h-screen bg-muted/30">
      <AppHeader />

      <main className="mx-auto w-full max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <Tabs
          defaultValue="overview"
          className="gap-6"
        >
          <TabsList className="grid w-full grid-cols-3 rounded-xl p-1 group-data-[orientation=horizontal]/tabs:h-auto sm:inline-grid sm:w-auto">
            <TabsTrigger
              value="overview"
              className="min-w-0 gap-2 rounded-lg px-2 py-2.5 sm:min-w-32 sm:px-4"
            >
              <LayoutDashboard
                className="size-4"
                aria-hidden="true"
              />

              <span className="hidden sm:inline">
                Visão geral
              </span>

              <span className="sm:hidden">
                Geral
              </span>
            </TabsTrigger>

            <TabsTrigger
              value="people"
              className="min-w-0 gap-2 rounded-lg px-2 py-2.5 sm:min-w-32 sm:px-4"
            >
              <Users
                className="size-4"
                aria-hidden="true"
              />

              Pessoas
            </TabsTrigger>

            <TabsTrigger
              value="transactions"
              className="min-w-0 gap-2 rounded-lg px-2 py-2.5 sm:min-w-32 sm:px-4"
            >
              <ArrowLeftRight
                className="size-4"
                aria-hidden="true"
              />

              <span className="hidden sm:inline">
                Transações
              </span>

              <span className="sm:hidden">
                Trans.
              </span>
            </TabsTrigger>
          </TabsList>

          <TabsContent
            value="overview"
            className="space-y-6"
          >
            <header>
              <p className="text-sm font-medium text-primary">
                Resumo financeiro
              </p>

              <h2 className="mt-1 text-2xl font-semibold tracking-tight">
                Visão geral
              </h2>

              <p className="mt-2 text-sm leading-6 text-muted-foreground">
                Acompanhe receitas, despesas e o saldo consolidado
                da residência.
              </p>
            </header>

            <TotalsOverview />
          </TabsContent>

          <TabsContent
            value="people"
            className="space-y-6"
          >
            <header>
              <p className="text-sm font-medium text-primary">
                Moradores
              </p>

              <h2 className="mt-1 text-2xl font-semibold tracking-tight">
                Pessoas
              </h2>

              <p className="mt-2 text-sm leading-6 text-muted-foreground">
                Cadastre e gerencie as pessoas vinculadas às
                movimentações financeiras.
              </p>
            </header>

            <PeopleSection/>

          </TabsContent>

          <TabsContent
            value="transactions"
            className="space-y-6"
          >
            <header>
              <p className="text-sm font-medium text-primary">
                Movimentações
              </p>

              <h2 className="mt-1 text-2xl font-semibold tracking-tight">
                Transações
              </h2>

              <p className="mt-2 text-sm leading-6 text-muted-foreground">
                Registre receitas e despesas para as pessoas
                cadastradas.
              </p>
            </header>

            <TransactionsSection />

          </TabsContent>
        </Tabs>
      </main>
    </div>
  )
}

export default App