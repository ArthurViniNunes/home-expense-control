export interface PersonTotals {
  personId: number
  personName: string
  totalIncome: number
  totalExpenses: number
  balance: number
}

export interface GeneralTotals {
  totalIncome: number
  totalExpenses: number
  netBalance: number
}

export interface Totals {
  people: PersonTotals[]
  general: GeneralTotals
}