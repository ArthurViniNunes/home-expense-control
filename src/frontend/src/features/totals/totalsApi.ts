import { httpClient } from '@/services/httpClient'

import type { Totals } from './totalsTypes'

export const totalsApi = {
  get(signal?: AbortSignal) {
    return httpClient.get<Totals>(
      '/api/totals',
      signal,
    )
  },
}