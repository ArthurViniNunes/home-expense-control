import type {
  LucideIcon,
} from 'lucide-react'

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'

interface FeaturePlaceholderProps {
  title: string
  description: string
  icon: LucideIcon
}

export function FeaturePlaceholder({
  title,
  description,
  icon: Icon,
}: FeaturePlaceholderProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{title}</CardTitle>

        <CardDescription>
          {description}
        </CardDescription>
      </CardHeader>

      <CardContent>
        <div className="flex min-h-64 flex-col items-center justify-center rounded-xl border border-dashed bg-muted/30 p-8 text-center">
          <div className="mb-4 flex size-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
            <Icon
              className="size-6"
              aria-hidden="true"
            />
          </div>

          <p className="font-medium">
            Funcionalidade em construção
          </p>

          <p className="mt-2 max-w-md text-sm leading-6 text-muted-foreground">
            A estrutura visual está pronta. Os dados e
            comportamentos serão conectados à API na próxima
            etapa.
          </p>
        </div>
      </CardContent>
    </Card>
  )
}