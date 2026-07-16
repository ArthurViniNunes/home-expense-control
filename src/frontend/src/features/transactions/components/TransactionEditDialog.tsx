import {
  ArrowDownRight,
  ArrowUpRight,
  Info,
  LoaderCircle,
  Pencil,
  Save,
} from 'lucide-react'
import {
  type FormEvent,
  useEffect,
  useState,
} from 'react'

import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
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

import {
  formatTransactionAmount,
  parseTransactionAmount,
  type TransactionFormErrors,
  validateTransactionDraft,
} from '../transactionFormUtils'
import type {
  Transaction,
  TransactionType,
  UpdateTransactionInput,
} from '../transactionTypes'

interface TransactionEditDialogProps {
  transaction: Transaction
  people: Person[]
  isUpdating: boolean
  onUpdate: (
    id: number,
    input: UpdateTransactionInput,
  ) => Promise<Transaction>
}

export function TransactionEditDialog({
  transaction,
  people,
  isUpdating,
  onUpdate,
}: TransactionEditDialogProps) {
  const [open, setOpen] = useState(false)

  const [personId, setPersonId] =
    useState('')

  const [description, setDescription] =
    useState('')

  const [amount, setAmount] =
    useState('')

  const [type, setType] =
    useState<TransactionType>('expense')

  const [errors, setErrors] =
    useState<TransactionFormErrors>({})

  const selectedPerson = people.find(
    (person) =>
      person.id === Number(personId),
  )

  const isIncomeDisabled =
    selectedPerson?.isMinor === true

  useEffect(() => {
    if (!open) {
      return
    }

    setPersonId(
      transaction.person.id.toString(),
    )

    setDescription(transaction.description)

    setAmount(
      formatTransactionAmount(
        transaction.amount,
      ),
    )

    setType(transaction.type)
    setErrors({})
  }, [open, transaction])

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
    const parsedAmount =
      parseTransactionAmount(amount)

    if (
      parsedAmount !== null &&
      parsedAmount > 0
    ) {
      setAmount(
        formatTransactionAmount(
          parsedAmount,
        ),
      )
    }
  }

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    const validationErrors =
      validateTransactionDraft({
        people,
        personId,
        description,
        amount,
        type,
      })

    if (
      Object.keys(validationErrors).length >
      0
    ) {
      setErrors(validationErrors)
      return
    }

    const parsedAmount =
      parseTransactionAmount(amount)

    if (parsedAmount === null) {
      return
    }

    setErrors({})

    try {
      await onUpdate(transaction.id, {
        personId: Number(personId),
        description: description.trim(),
        amount:
          Math.round(parsedAmount * 100) /
          100,
        type,
      })

      setOpen(false)
    } catch (error) {
      setErrors({
        form: getApiErrorMessage(
          error,
          'Não foi possível atualizar a transação.',
        ),
      })
    }
  }

  return (
    <Dialog
      open={open}
      onOpenChange={(nextOpen) => {
        if (!isUpdating) {
          setOpen(nextOpen)
        }
      }}
    >
      <DialogTrigger asChild>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          className="size-8"
          aria-label={`Editar ${transaction.description}`}
        >
          <Pencil
            className="size-4"
            aria-hidden="true"
          />
        </Button>
      </DialogTrigger>

      <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>
            Editar transação
          </DialogTitle>

          <DialogDescription>
            Atualize os dados da movimentação
            selecionada.
          </DialogDescription>
        </DialogHeader>

        <form
          className="space-y-5"
          onSubmit={handleSubmit}
          noValidate
        >
          {errors.form ? (
            <div
              className="rounded-lg border border-destructive/30 bg-destructive/10 px-4 py-3 text-sm text-destructive"
              role="alert"
            >
              {errors.form}
            </div>
          ) : null}

          <div className="space-y-2">
            <Label
              htmlFor={`transaction-edit-person-${transaction.id}`}
            >
              Pessoa
            </Label>

            <Select
              value={personId}
              disabled={isUpdating}
              onValueChange={
                handlePersonChange
              }
            >
              <SelectTrigger
                id={`transaction-edit-person-${transaction.id}`}
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
                    value={person.id.toString()}
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
            ) : null}
          </div>

          <div className="space-y-2">
            <Label
              htmlFor={`transaction-edit-description-${transaction.id}`}
            >
              Descrição
            </Label>

            <Input
              id={`transaction-edit-description-${transaction.id}`}
              value={description}
              disabled={isUpdating}
              placeholder="Ex.: Conta de energia"
              aria-invalid={Boolean(
                errors.description,
              )}
              onChange={(event) => {
                setDescription(
                  event.target.value,
                )

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
            ) : null}
          </div>

          <div className="space-y-2">
            <Label
              htmlFor={`transaction-edit-amount-${transaction.id}`}
            >
              Valor
            </Label>

            <div className="relative">
              <span className="pointer-events-none absolute inset-y-0 left-3 flex items-center text-sm text-muted-foreground">
                R$
              </span>

              <Input
                id={`transaction-edit-amount-${transaction.id}`}
                className="pl-10"
                value={amount}
                disabled={isUpdating}
                inputMode="decimal"
                placeholder="0,00"
                aria-invalid={Boolean(
                  errors.amount,
                )}
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
            <Label
              htmlFor={`transaction-edit-type-${transaction.id}`}
            >
              Tipo
            </Label>

            <Select
              value={type}
              disabled={isUpdating}
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
                id={`transaction-edit-type-${transaction.id}`}
                className="w-full"
                aria-invalid={Boolean(
                  errors.type,
                )}
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
                  Como {selectedPerson?.name} é
                  menor de idade, apenas despesas
                  podem ser registradas.
                </p>
              </div>
            ) : null}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              disabled={isUpdating}
              onClick={() => {
                setOpen(false)
              }}
            >
              Cancelar
            </Button>

            <Button
              type="submit"
              disabled={isUpdating}
            >
              {isUpdating ? (
                <>
                  <LoaderCircle
                    className="size-4 animate-spin"
                    aria-hidden="true"
                  />

                  Salvando...
                </>
              ) : (
                <>
                  <Save
                    className="size-4"
                    aria-hidden="true"
                  />

                  Salvar alterações
                </>
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}