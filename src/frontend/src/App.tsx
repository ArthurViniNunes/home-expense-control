import { useState } from 'react'
import './App.css'
import { AppHeader } from './components/layout/AppHeader'
import {
  AppNavigation,
  type AppSection,
} from './components/layout/AppNavigation'
import { MetricCard } from './components/ui/MetricCard'
import { SectionPlaceholder } from './components/ui/SectionPlaceholder'

const sectionHeadings: Record<
  AppSection,
  {
    eyebrow: string
    title: string
    description: string
  }
> = {
  totals: {
    eyebrow: 'Resumo financeiro',
    title: 'Visão geral',
    description:
      'Acompanhe receitas, despesas e o saldo consolidado da residência.',
  },
  people: {
    eyebrow: 'Moradores',
    title: 'Pessoas',
    description:
      'Cadastre e consulte as pessoas vinculadas às movimentações.',
  },
  transactions: {
    eyebrow: 'Movimentações',
    title: 'Transações',
    description:
      'Registre receitas e despesas para as pessoas cadastradas.',
  },
}

function App() {
  const [activeSection, setActiveSection] =
    useState<AppSection>('totals')

  const heading = sectionHeadings[activeSection]

  return (
    <div className="app">
      <AppHeader />

      <div className="app__content">
        <AppNavigation
          activeSection={activeSection}
          onSectionChange={setActiveSection}
        />

        <main className="app__main">
          <header className="page-heading">
            <p className="page-heading__eyebrow">
              {heading.eyebrow}
            </p>

            <h2 className="page-heading__title">
              {heading.title}
            </h2>

            <p className="page-heading__description">
              {heading.description}
            </p>
          </header>

          {activeSection === 'totals' && (
            <>
              <section
                className="metrics-grid"
                aria-label="Indicadores financeiros"
              >
                <MetricCard
                  label="Pessoas"
                  value="—"
                  description="Total de pessoas cadastradas"
                />

                <MetricCard
                  label="Receitas"
                  value="—"
                  description="Receitas consolidadas"
                  tone="income"
                />

                <MetricCard
                  label="Despesas"
                  value="—"
                  description="Despesas consolidadas"
                  tone="expense"
                />

                <MetricCard
                  label="Saldo líquido"
                  value="—"
                  description="Receitas menos despesas"
                  tone="balance"
                />
              </section>

              <section className="content-panel">
                <div className="content-panel__heading">
                  <div>
                    <h3 className="content-panel__title">
                      Totais por pessoa
                    </h3>

                    <p className="content-panel__description">
                      Comparação individual de receitas,
                      despesas e saldos.
                    </p>
                  </div>
                </div>

                <SectionPlaceholder
                  title="Consulta de totais"
                  description="Os dados financeiros serão carregados pela API na próxima etapa."
                />
              </section>
            </>
          )}

          {activeSection === 'people' && (
            <section className="content-panel">
              <div className="content-panel__heading">
                <div>
                  <h3 className="content-panel__title">
                    Pessoas cadastradas
                  </h3>

                  <p className="content-panel__description">
                    Gerencie os moradores vinculados ao controle
                    financeiro.
                  </p>
                </div>
              </div>

              <SectionPlaceholder
                title="Cadastro de pessoas"
                description="O formulário e a listagem serão implementados na próxima funcionalidade."
              />
            </section>
          )}

          {activeSection === 'transactions' && (
            <section className="content-panel">
              <div className="content-panel__heading">
                <div>
                  <h3 className="content-panel__title">
                    Histórico de transações
                  </h3>

                  <p className="content-panel__description">
                    Consulte as receitas e despesas registradas.
                  </p>
                </div>
              </div>

              <SectionPlaceholder
                title="Cadastro de transações"
                description="O formulário e o histórico serão implementados após o cadastro de pessoas."
              />
            </section>
          )}
        </main>
      </div>
    </div>
  )
}

export default App