import {
  useState,
  type FormEvent,
} from 'react'
import {
  LoaderCircle,
  UserPlus,
} from 'lucide-react'

import { Button } from '@/components/ui/button'
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

import type {
  CreatePersonInput,
} from '../peopleTypes'

interface PersonFormProps {
  isSubmitting: boolean
  onSubmit: (
    input: CreatePersonInput,
  ) => Promise<unknown>
}

interface FormErrors {
  name?: string
  age?: string
  form?: string
}

export function PersonForm({
  isSubmitting,
  onSubmit,
}: PersonFormProps) {
  const [name, setName] = useState('')
  const [age, setAge] = useState('')
  const [errors, setErrors] =
    useState<FormErrors>({})

  function validate(): FormErrors {
    const validationErrors: FormErrors = {}
    const normalizedName = name.trim()
    const numericAge = Number(age)

    if (!normalizedName) {
      validationErrors.name =
        'Informe o nome da pessoa.'
    } else if (normalizedName.length > 120) {
      validationErrors.name =
        'O nome deve possuir no máximo 120 caracteres.'
    }

    if (!age.trim()) {
      validationErrors.age =
        'Informe a idade da pessoa.'
    } else if (!Number.isInteger(numericAge)) {
      validationErrors.age =
        'A idade deve ser um número inteiro.'
    } else if (numericAge < 0) {
      validationErrors.age =
        'A idade não pode ser negativa.'
    }

    return validationErrors
  }

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    const validationErrors = validate()

    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors)
      return
    }

    setErrors({})

    try {
      await onSubmit({
        name: name.trim(),
        age: Number(age),
      })

      setName('')
      setAge('')
    } catch {
      setErrors({
        form:
          'Não foi possível cadastrar a pessoa. Verifique os dados e tente novamente.',
      })
    }
  }

  return (
    <Card>
      <CardHeader>
        <div className="mb-1 flex size-10 items-center justify-center rounded-xl bg-primary/10 text-primary">
          <UserPlus
            className="size-5"
            aria-hidden="true"
          />
        </div>

        <CardTitle>Nova pessoa</CardTitle>

        <CardDescription>
          Cadastre um morador para associar receitas
          e despesas.
        </CardDescription>
      </CardHeader>

      <CardContent>
        <form
          className="space-y-5"
          onSubmit={handleSubmit}
          noValidate
        >
          {errors.form && (
            <div
              className="rounded-lg border border-destructive/30 bg-destructive/10 px-4 py-3 text-sm text-destructive"
              role="alert"
            >
              {errors.form}
            </div>
          )}

          <div className="space-y-2">
            <div className="flex items-center justify-between gap-4">
              <Label htmlFor="person-name">
                Nome
              </Label>

              <span className="text-xs text-muted-foreground">
                {name.length}/120
              </span>
            </div>

            <Input
              id="person-name"
              name="name"
              value={name}
              maxLength={120}
              autoComplete="name"
              placeholder="Ex.: Ana Souza"
              disabled={isSubmitting}
              aria-invalid={Boolean(errors.name)}
              aria-describedby={
                errors.name
                  ? 'person-name-error'
                  : 'person-name-description'
              }
              onChange={(event) => {
                setName(event.target.value)

                if (errors.name || errors.form) {
                  setErrors((current) => ({
                    ...current,
                    name: undefined,
                    form: undefined,
                  }))
                }
              }}
            />

            {errors.name ? (
              <p
                id="person-name-error"
                className="text-xs text-destructive"
              >
                {errors.name}
              </p>
            ) : (
              <p
                id="person-name-description"
                className="text-xs text-muted-foreground"
              >
                Nome utilizado nas transações e totais.
              </p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="person-age">
              Idade
            </Label>

            <Input
              id="person-age"
              name="age"
              type="number"
              min="0"
              step="1"
              inputMode="numeric"
              value={age}
              placeholder="Ex.: 28"
              disabled={isSubmitting}
              aria-invalid={Boolean(errors.age)}
              aria-describedby={
                errors.age
                  ? 'person-age-error'
                  : 'person-age-description'
              }
              onChange={(event) => {
                setAge(event.target.value)

                if (errors.age || errors.form) {
                  setErrors((current) => ({
                    ...current,
                    age: undefined,
                    form: undefined,
                  }))
                }
              }}
            />

            {errors.age ? (
              <p
                id="person-age-error"
                className="text-xs text-destructive"
              >
                {errors.age}
              </p>
            ) : (
              <p
                id="person-age-description"
                className="text-xs text-muted-foreground"
              >
                Menores de 18 anos só podem possuir despesas.
              </p>
            )}
          </div>

          <Button
            type="submit"
            className="w-full"
            disabled={isSubmitting}
          >
            {isSubmitting ? (
              <>
                <LoaderCircle
                  className="size-4 animate-spin"
                  aria-hidden="true"
                />

                Cadastrando...
              </>
            ) : (
              <>
                <UserPlus
                  className="size-4"
                  aria-hidden="true"
                />

                Cadastrar pessoa
              </>
            )}
          </Button>
        </form>
      </CardContent>
    </Card>
  )
}