import { env } from '../config/env'
import {
  ApiError,
  type ValidationProblemDetails,
} from './apiError'

type RequestOptions = Omit<RequestInit, 'body'> & {
  body?: unknown
}

async function readProblemDetails(
  response: Response,
): Promise<ValidationProblemDetails | undefined> {
  const contentType =
    response.headers.get('content-type') ?? ''

  if (!contentType.includes('json')) {
    return undefined
  }

  try {
    return (await response.json()) as ValidationProblemDetails
  } catch {
    return undefined
  }
}

async function request<T>(
  path: string,
  options: RequestOptions = {},
): Promise<T> {
  const hasBody = options.body !== undefined

  const response = await fetch(
    `${env.apiBaseUrl}${path}`,
    {
      ...options,

      headers: {
        Accept: 'application/json',

        ...(hasBody
          ? { 'Content-Type': 'application/json' }
          : {}),

        ...options.headers,
      },

      body: hasBody
        ? JSON.stringify(options.body)
        : undefined,
    },
  )

  if (!response.ok) {
    const problemDetails =
      await readProblemDetails(response)

    throw new ApiError(
      response.status,
      problemDetails,
    )
  }

  if (response.status === 204) {
    return undefined as T
  }

  const contentType =
    response.headers.get('content-type') ?? ''

  if (!contentType.includes('json')) {
    return undefined as T
  }

  return (await response.json()) as T
}

export const httpClient = {
  get<T>(path: string, signal?: AbortSignal) {
    return request<T>(path, {
      method: 'GET',
      signal,
    })
  },

  post<TResponse, TBody>(
    path: string,
    body: TBody,
    signal?: AbortSignal,
  ) {
    return request<TResponse>(path, {
      method: 'POST',
      body,
      signal,
    })
  },

  put<TResponse, TBody>(
    path: string,
    body: TBody,
    signal?: AbortSignal,
  ) {
    return request<TResponse>(path, {
      method: 'PUT',
      body,
      signal,
    })
  },

  delete(path: string, signal?: AbortSignal) {
    return request<void>(path, {
      method: 'DELETE',
      signal,
    })
  },
}