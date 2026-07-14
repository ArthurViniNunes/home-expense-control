import {
  useState,
  type FormEvent,
} from 'react'
import {
  ArrowDownRight,
  ArrowUpRight,
  Info,
  LoaderCircle,
  ReceiptText,
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import type { Person } from '@/features/people/peopleTypes'
import { getApiErrorMessage } from '@/services/apiError'

import type {
  CreateTransactionInput,
  Transaction,
  TransactionType,
} from '../transactionTypes'

interface TransactionFormProps {
  people: Person[]
  isSubmitting: boolean
  onSubmit: (
    input: CreateTransactionInput,
  ) => Promise<Transaction>
}

interface FormErrors {
  personId?: string
  description?: string
  amount?: string
  type?: string
  form?: string
}

function parseAmount(value: string): number | null {
  const sanitizedValue = value
    .trim()
    .replace(/\s/g, '')
    .replace(/^R\$/i, '')

  if (!sanitizedValue) {
    return null
  }

  const normalizedValue =
    sanitizedValue.includes(',')
      ? sanitizedValue
          .replace(/\./g, '')
          .replace(',', '.')
      : sanitizedValue

  const parsedValue = Number(normalizedValue)

  if (!Number.isFinite(parsedValue)) {
    return null
  }

  return parsedValue
}

function formatAmount(value: number): string {
  return new Intl.NumberFormat('pt-BR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value)
}

export function TransactionForm({
  people,
  isSubmitting,
  onSubmit,
}: TransactionFormProps) {
  const [personId, setPersonId] = useState('')
  const [description, setDescription] = useState('')
  const [amount, setAmount] = useState('')
  const [type, setType] =
    useState<TransactionType>('expense')

  const [errors, setErrors] =
    useState<FormErrors>({})

  const selectedPerson = people.find(
    (person) => person.id === Number(personId),
  )

  const hasPeople = people.length > 0
  const isIncomeDisabled =
    selectedPerson?.isMinor === true

  function validate(): FormErrors {
    const validationErrors: FormErrors = {}
    const parsedAmount = parseAmount(amount)

    if (!personId) {
      validationErrors.personId =
        'Selecione uma pessoa.'
    }

    if (!description.trim()) {
      validationErrors.description =
        'Informe a descrição da transação.'
    }

    if (parsedAmount === null) {
      validationErrors.amount =
        'Informe um valor válido.'
    } else if (parsedAmount <= 0) {
      validationErrors.amount =
        'O valor deve ser maior que zero.'
    }

    if (
      selectedPerson?.isMinor &&
      type === 'income'
    ) {
      validationErrors.type =
        'Menores de idade só podem possuir despesas.'
    }

    return validationErrors
  }

  function handlePersonChange(value: string) {
    setPersonId(value)

    const person = people.find(
      (item) => item.id === Number(value),
    )

    if (
      person?.isMinor &&
      type === 'income'
    ) {
      setType('expense')
    }

    setErrors((current) => ({
      ...current,
      personId: undefined,
      type: undefined,
      form: undefined,
    }))
  }

  function handleAmountBlur() {
    const parsedAmount = parseAmount(amount)

    if (
      parsedAmount !== null &&
      parsedAmount > 0
    ) {
      setAmount(formatAmount(parsedAmount))
    }
  }

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    const validationErrors = validate()

    if (
      Object.keys(validationErrors).length > 0
    ) {
      setErrors(validationErrors)
      return
    }

    const parsedAmount = parseAmount(amount)

    if (parsedAmount === null) {
      return
    }

    setErrors({})

    try {
      await onSubmit({
        personId: Number(personId),
        description: description.trim(),
        amount:
          Math.round(parsedAmount * 100) / 100,
        type,
      })

      setDescription('')
      setAmount('')
      setType('expense')
    } catch (error) {
      setErrors({
        form: getApiErrorMessage(
          error,
          'Não foi possível registrar a transação.',
        ),
      })
    }
  }

  return (
    <Card>
      <CardHeader>
        <div className="mb-1 flex size-10 items-center justify-center rounded-xl bg-primary/10 text-primary">
          <ReceiptText
            className="size-5"
            aria-hidden="true"
          />
        </div>

        <CardTitle>Nova transação</CardTitle>

        <CardDescription>
          Registre uma receita ou despesa para uma
          pessoa cadastrada.
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

          {!hasPeople && (
            <div className="flex gap-3 rounded-lg border bg-muted/40 p-4">
              <Info
                className="mt-0.5 size-4 shrink-0 text-muted-foreground"
                aria-hidden="true"
              />

              <p className="text-sm leading-6 text-muted-foreground">
                Cadastre pelo menos uma pessoa antes de
                registrar uma transação.
              </p>
            </div>
          )}

          <div className="space-y-2">
            <Label htmlFor="transaction-person">
              Pessoa
            </Label>

            <Select
              value={personId}
              disabled={
                isSubmitting || !hasPeople
              }
              onValueChange={handlePersonChange}
            >
              <SelectTrigger
                id="transaction-person"
                className="w-full"
                aria-invalid={Boolean(
                  errors.personId,
                )}
              >
                <SelectValue placeholder="Selecione uma pessoa" />
              </SelectTrigger>

              <SelectContent>
                {people.map((person) => (
                  <SelectItem
                    key={person.id}
                    value={String(person.id)}
                  >
                    <span>{person.name}</span>

                    <span className="ml-2 text-xs text-muted-foreground">
                      {person.age} anos
                    </span>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            {errors.personId ? (
              <p className="text-xs text-destructive">
                {errors.personId}
              </p>
            ) : (
              <p className="text-xs text-muted-foreground">
                A transação será vinculada a esta
                pessoa.
              </p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="transaction-description">
              Descrição
            </Label>

            <Input
              id="transaction-description"
              value={description}
              placeholder="Ex.: Conta de energia"
              disabled={isSubmitting}
              aria-invalid={Boolean(
                errors.description,
              )}
              onChange={(event) => {
                setDescription(event.target.value)

                setErrors((current) => ({
                  ...current,
                  description: undefined,
                  form: undefined,
                }))
              }}
            />

            {errors.description ? (
              <p className="text-xs text-destructive">
                {errors.description}
              </p>
            ) : (
              <p className="text-xs text-muted-foreground">
                Identifique de forma clara a
                movimentação.
              </p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="transaction-amount">
              Valor
            </Label>

            <div className="relative">
              <span className="pointer-events-none absolute inset-y-0 left-3 flex items-center text-sm text-muted-foreground">
                R$
              </span>

              <Input
                id="transaction-amount"
                className="pl-10"
                value={amount}
                inputMode="decimal"
                placeholder="0,00"
                disabled={isSubmitting}
                aria-invalid={Boolean(errors.amount)}
                onBlur={handleAmountBlur}
                onChange={(event) => {
                  setAmount(event.target.value)

                  setErrors((current) => ({
                    ...current,
                    amount: undefined,
                    form: undefined,
                  }))
                }}
              />
            </div>

            {errors.amount ? (
              <p className="text-xs text-destructive">
                {errors.amount}
              </p>
            ) : (
              <p className="text-xs text-muted-foreground">
                Utilize vírgula ou ponto para os
                centavos.
              </p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="transaction-type">
              Tipo
            </Label>

            <Select
              value={type}
              disabled={isSubmitting}
              onValueChange={(value) => {
                setType(
                  value as TransactionType,
                )

                setErrors((current) => ({
                  ...current,
                  type: undefined,
                  form: undefined,
                }))
              }}
            >
              <SelectTrigger
                id="transaction-type"
                className="w-full"
                aria-invalid={Boolean(errors.type)}
              >
                <SelectValue />
              </SelectTrigger>

              <SelectContent>
                <SelectItem value="expense">
                  <span className="flex items-center gap-2">
                    <ArrowDownRight
                      className="size-4 text-rose-600"
                      aria-hidden="true"
                    />

                    Despesa
                  </span>
                </SelectItem>

                <SelectItem
                  value="income"
                  disabled={isIncomeDisabled}
                >
                  <span className="flex items-center gap-2">
                    <ArrowUpRight
                      className="size-4 text-emerald-600"
                      aria-hidden="true"
                    />

                    Receita
                  </span>
                </SelectItem>
              </SelectContent>
            </Select>

            {errors.type ? (
              <p className="text-xs text-destructive">
                {errors.type}
              </p>
            ) : isIncomeDisabled ? (
              <div className="flex gap-2 rounded-lg bg-muted/50 px-3 py-2">
                <Info
                  className="mt-0.5 size-4 shrink-0 text-muted-foreground"
                  aria-hidden="true"
                />

                <p className="text-xs leading-5 text-muted-foreground">
                  Como {selectedPerson?.name} é menor
                  de idade, apenas despesas podem ser
                  registradas.
                </p>
              </div>
            ) : (
              <p className="text-xs text-muted-foreground">
                Escolha como a movimentação afeta o
                saldo.
              </p>
            )}
          </div>

          <Button
            type="submit"
            className="w-full"
            disabled={
              isSubmitting || !hasPeople
            }
          >
            {isSubmitting ? (
              <>
                <LoaderCircle
                  className="size-4 animate-spin"
                  aria-hidden="true"
                />

                Registrando...
              </>
            ) : (
              <>
                <ReceiptText
                  className="size-4"
                  aria-hidden="true"
                />

                Registrar transação
              </>
            )}
          </Button>
        </form>
      </CardContent>
    </Card>
  )
}