import {
  ArrowDownRight,
  ArrowUpRight,
  Scale,
  Users,
} from 'lucide-react'

import { MetricCard } from '@/components/dashboard/MetricCard'
import { FeaturePlaceholder } from '@/components/layout/FeaturePlaceholder'
import { Separator } from '@/components/ui/separator'

export function TotalsOverview() {
  return (
    <div className="space-y-6">
      <section
        className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4"
        aria-label="Indicadores financeiros"
      >
        <MetricCard
          title="Pessoas"
          value="-"
          description="Total de moradores cadastrados"
          icon={Users}
        />

        <MetricCard
          title="Receitas"
          value="-"
          description="Receitas consolidadas"
          icon={ArrowUpRight}
          tone="positive"
        />

        <MetricCard
          title="Despesas"
          value="-"
          description="Despesas consolidadas"
          icon={ArrowDownRight}
          tone="negative"
        />

        <MetricCard
          title="Saldo líquido"
          value="-"
          description="Receitas menos despesas"
          icon={Scale}
          tone="balance"
        />
      </section>

      <Separator />

      <FeaturePlaceholder
        title="Totais por pessoa"
        description="Comparação individual de receitas, despesas e saldos."
        icon={Scale}
      />
    </div>
  )
}