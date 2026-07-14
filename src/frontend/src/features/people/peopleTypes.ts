export interface Person {
  id: number
  name: string
  age: number
  isMinor: boolean
}

export interface CreatePersonInput {
  name: string
  age: number
}