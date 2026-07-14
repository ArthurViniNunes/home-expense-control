import { httpClient } from '../../services/httpClient'
import type {
  CreatePersonInput,
  Person,
} from './peopleTypes'

export const peopleApi = {
  list(signal?: AbortSignal) {
    return httpClient.get<Person[]>(
      '/api/people',
      signal,
    )
  },

  getById(id: number, signal?: AbortSignal) {
    return httpClient.get<Person>(
      `/api/people/${id}`,
      signal,
    )
  },

  create(
    input: CreatePersonInput,
    signal?: AbortSignal,
  ) {
    return httpClient.post<Person, CreatePersonInput>(
      '/api/people',
      input,
      signal,
    )
  },

  delete(id: number, signal?: AbortSignal) {
    return httpClient.delete(
      `/api/people/${id}`,
      signal,
    )
  },
}