import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'

function App() {
  return (
    <main className="flex min-h-screen items-center justify-center bg-muted/40 p-6">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>
            Controle de Gastos Residenciais
          </CardTitle>

          <CardDescription>
            O design system foi configurado com sucesso.
          </CardDescription>
        </CardHeader>

        <CardContent>
          <Button className="w-full">
            Continuar
          </Button>
        </CardContent>
      </Card>
    </main>
  )
}

export default App