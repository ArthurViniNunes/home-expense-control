import {
  BookOpen,
  FolderGit,
  House,
  WalletCards,
} from 'lucide-react'

import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { env } from '@/config/env'

export function AppHeader() {
  const scalarUrl = `${env.apiBaseUrl}/scalar`

  return (
    <header className="border-b bg-background/95 backdrop-blur">
      <div className="mx-auto flex max-w-7xl flex-col gap-5 px-4 py-6 sm:px-6 lg:flex-row lg:items-center lg:justify-between lg:px-8">
        <div className="flex items-start gap-4">
          <div className="flex size-12 shrink-0 items-center justify-center rounded-2xl bg-primary text-primary-foreground shadow-sm">
            <House
              className="size-6"
              aria-hidden="true"
            />
          </div>

          <div>
            <div className="mb-2 flex flex-wrap items-center gap-2">
              <Badge variant="secondary">
                Finanças residenciais
              </Badge>

              <Badge variant="outline">
                <WalletCards
                  className="size-3.5"
                  aria-hidden="true"
                />

                Receitas e despesas
              </Badge>
            </div>

            <h1 className="text-2xl font-semibold tracking-tight sm:text-3xl">
              Controle de Gastos Residenciais
            </h1>

            <p className="mt-2 max-w-2xl text-sm leading-6 text-muted-foreground sm:text-base">
              Cadastre moradores, registre movimentações e
              acompanhe os resultados financeiros da residência.
            </p>
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            asChild
          >
            <a
              href={scalarUrl}
              target="_blank"
              rel="noreferrer"
            >
              <BookOpen
                className="size-4"
                aria-hidden="true"
              />

              Documentação da API
            </a>
          </Button>

          <Button
            variant="outline"
            size="sm"
            asChild
          >
            <a
              href="https://github.com/ArthurViniNunes/home-expense-control/"
              target="_blank"
              rel="noreferrer"
            >
              <FolderGit
                className="size-4"
                aria-hidden="true"
              />

              Repositório
            </a>
          </Button>
        </div>
      </div>
    </header>
  )
}