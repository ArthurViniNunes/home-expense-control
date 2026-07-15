import {
  Filter,
  RotateCcw,
} from 'lucide-react'
import {
  type FormEvent,
  useEffect,
  useState,
} from 'react'

import { Button } from '@/components/ui/button'
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

import type {
  TransactionAgeGroup,
  TransactionFilters as TransactionFiltersValue,
  TransactionType,
} from '../transactionTypes'

interface TransactionFiltersProps {
  people: Person[]
  filters: TransactionFiltersValue
  hasActiveFilters: boolean
  isLoading: boolean
  onApply: (
    filters: TransactionFiltersValue,
  ) => Promise<void>
  onClear: () => Promise<void>
}

const ALL_VALUE = 'all'

export function TransactionFilters({
  people,
  filters,
  hasActiveFilters,
  isLoading,
  onApply,
  onClear,
}: TransactionFiltersProps) {
  const [personId, setPersonId] =
    useState<string>(ALL_VALUE)

  const [ageGroup, setAgeGroup] =
    useState<string>(ALL_VALUE)

  const [type, setType] =
    useState<string>(ALL_VALUE)

  const [minAmount, setMinAmount] =
    useState('')

  const [maxAmount, setMaxAmount] =
    useState('')

  const [validationError, setValidationError] =
    useState<string | null>(null)

  useEffect(() => {
    setPersonId(
      filters.personId?.toString() ??
        ALL_VALUE,
    )

    setAgeGroup(
      filters.ageGroup ?? ALL_VALUE,
    )

    setType(filters.type ?? ALL_VALUE)

    setMinAmount(
      filters.minAmount?.toString() ?? '',
    )

    setMaxAmount(
      filters.maxAmount?.toString() ?? '',
    )
  }, [filters])

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    const parsedMinimum =
      parseOptionalAmount(minAmount)

    const parsedMaximum =
      parseOptionalAmount(maxAmount)

    if (!parsedMinimum.isValid) {
      setValidationError(
        'Informe um valor mínimo válido, positivo e com até duas casas decimais.',
      )

      return
    }

    if (!parsedMaximum.isValid) {
      setValidationError(
        'Informe um valor máximo válido, positivo e com até duas casas decimais.',
      )

      return
    }

    if (
      parsedMinimum.value !== undefined &&
      parsedMaximum.value !== undefined &&
      parsedMinimum.value >
        parsedMaximum.value
    ) {
      setValidationError(
        'O valor mínimo não pode ser maior que o valor máximo.',
      )

      return
    }

    setValidationError(null)

    const nextFilters: TransactionFiltersValue =
      {}

    if (personId !== ALL_VALUE) {
      nextFilters.personId =
        Number(personId)
    }

    if (ageGroup !== ALL_VALUE) {
      nextFilters.ageGroup =
        ageGroup as TransactionAgeGroup
    }

    if (type !== ALL_VALUE) {
      nextFilters.type =
        type as TransactionType
    }

    if (
      parsedMinimum.value !== undefined
    ) {
      nextFilters.minAmount =
        parsedMinimum.value
    }

    if (
      parsedMaximum.value !== undefined
    ) {
      nextFilters.maxAmount =
        parsedMaximum.value
    }

    await onApply(nextFilters)
  }

  async function handleClear() {
    await onClear()

    setPersonId(ALL_VALUE)
    setAgeGroup(ALL_VALUE)
    setType(ALL_VALUE)
    setMinAmount('')
    setMaxAmount('')
    setValidationError(null)
  }

  return (
    <form
      className="mb-6 rounded-xl border bg-muted/20 p-4"
      onSubmit={(event) => {
        void handleSubmit(event)
      }}
    >
      <div className="mb-4 flex items-center gap-2">
        <Filter
          className="size-4"
          aria-hidden="true"
        />

        <h3 className="font-medium">
          Filtrar transações
        </h3>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-5">
        <div className="space-y-2">
          <Label htmlFor="transaction-person-filter">
            Pessoa
          </Label>

          <Select
            value={personId}
            disabled={isLoading}
            onValueChange={setPersonId}
          >
            <SelectTrigger
              id="transaction-person-filter"
              className="w-full"
            >
              <SelectValue placeholder="Todas" />
            </SelectTrigger>

            <SelectContent>
              <SelectItem value={ALL_VALUE}>
                Todas as pessoas
              </SelectItem>

              {people.map((person) => (
                <SelectItem
                  key={person.id}
                  value={person.id.toString()}
                >
                  {person.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-2">
          <Label htmlFor="transaction-age-filter">
            Faixa etária
          </Label>

          <Select
            value={ageGroup}
            disabled={isLoading}
            onValueChange={setAgeGroup}
          >
            <SelectTrigger
              id="transaction-age-filter"
              className="w-full"
            >
              <SelectValue placeholder="Todas" />
            </SelectTrigger>

            <SelectContent>
              <SelectItem value={ALL_VALUE}>
                Todas
              </SelectItem>

              <SelectItem value="adult">
                Maiores de idade
              </SelectItem>

              <SelectItem value="minor">
                Menores de idade
              </SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-2">
          <Label htmlFor="transaction-type-filter">
            Tipo
          </Label>

          <Select
            value={type}
            disabled={isLoading}
            onValueChange={setType}
          >
            <SelectTrigger
              id="transaction-type-filter"
              className="w-full"
            >
              <SelectValue placeholder="Todos" />
            </SelectTrigger>

            <SelectContent>
              <SelectItem value={ALL_VALUE}>
                Todos os tipos
              </SelectItem>

              <SelectItem value="expense">
                Despesas
              </SelectItem>

              <SelectItem value="income">
                Receitas
              </SelectItem>
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-2">
          <Label htmlFor="transaction-min-amount">
            Valor mínimo
          </Label>

          <Input
            id="transaction-min-amount"
            value={minAmount}
            disabled={isLoading}
            inputMode="decimal"
            placeholder="0,00"
            onChange={(event) => {
              setMinAmount(
                event.target.value,
              )
            }}
          />
        </div>

        <div className="space-y-2">
          <Label htmlFor="transaction-max-amount">
            Valor máximo
          </Label>

          <Input
            id="transaction-max-amount"
            value={maxAmount}
            disabled={isLoading}
            inputMode="decimal"
            placeholder="0,00"
            onChange={(event) => {
              setMaxAmount(
                event.target.value,
              )
            }}
          />
        </div>
      </div>

      {validationError ? (
        <p
          className="mt-3 text-sm text-destructive"
          role="alert"
        >
          {validationError}
        </p>
      ) : null}

      <div className="mt-4 flex flex-wrap gap-2">
        <Button
          type="submit"
          size="sm"
          disabled={isLoading}
        >
          <Filter
            className="size-4"
            aria-hidden="true"
          />

          Aplicar filtros
        </Button>

        <Button
          type="button"
          size="sm"
          variant="outline"
          disabled={
            isLoading ||
            !hasActiveFilters
          }
          onClick={() => {
            void handleClear()
          }}
        >
          <RotateCcw
            className="size-4"
            aria-hidden="true"
          />

          Limpar filtros
        </Button>
      </div>
    </form>
  )
}

interface ParsedAmount {
  isValid: boolean
  value?: number
}

function parseOptionalAmount(
  input: string,
): ParsedAmount {
  const normalizedInput = input
    .trim()
    .replace(',', '.')

  if (normalizedInput === '') {
    return {
      isValid: true,
    }
  }

  if (
    !/^\d+(?:\.\d{1,2})?$/.test(
      normalizedInput,
    )
  ) {
    return {
      isValid: false,
    }
  }

  const value = Number(normalizedInput)

  if (
    !Number.isFinite(value) ||
    value < 0
  ) {
    return {
      isValid: false,
    }
  }

  return {
    isValid: true,
    value,
  }
}