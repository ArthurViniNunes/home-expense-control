import type {
  LucideIcon,
} from 'lucide-react'

import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { cn } from '@/lib/utils'

type MetricTone =
  | 'default'
  | 'positive'
  | 'negative'
  | 'balance'

interface MetricCardProps {
  title: string
  value: string
  description: string
  icon: LucideIcon
  tone?: MetricTone
  valueTone?: MetricTone
}

const toneClasses: Record<MetricTone, string> = {
  default:
    'bg-muted text-muted-foreground',

  positive:
    'bg-emerald-500/10 text-emerald-700 dark:text-emerald-400',

  negative:
    'bg-rose-500/10 text-rose-700 dark:text-rose-400',

  balance:
    'bg-blue-500/10 text-blue-700 dark:text-blue-400',
}

const textToneClasses: Record<MetricTone, string> = {
  default: 'text-foreground',

  positive:
    'text-emerald-700 dark:text-emerald-400',

  negative:
    'text-rose-700 dark:text-rose-400',

  balance:
    'text-foreground',
}

export function MetricCard({
  title,
  value,
  description,
  icon: Icon,
  tone = 'default',
  valueTone = 'default',
}: MetricCardProps) {
  return (
    <Card className="gap-4">
      <CardHeader className="flex flex-row items-center justify-between space-y-0">
        <CardTitle className="text-sm font-medium text-muted-foreground">
          {title}
        </CardTitle>

        <div
          className={cn(
            'flex size-9 items-center justify-center rounded-xl',
            toneClasses[tone],
          )}
        >
          <Icon
            className="size-4"
            aria-hidden="true"
          />
        </div>
      </CardHeader>

      <CardContent>
        <p className={cn('text-2xl font-semibold tracking-tight', textToneClasses[valueTone])}>
          {value}
        </p>

        <p className="mt-1 text-xs leading-5 text-muted-foreground">
          {description}
        </p>
      </CardContent>
    </Card>
  )
}