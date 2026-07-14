export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  traceId?: string
}

export interface ValidationProblemDetails
  extends ProblemDetails {
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  readonly status: number
  readonly problemDetails?: ValidationProblemDetails

  constructor(
    status: number,
    problemDetails?: ValidationProblemDetails,
  ) {
    const message =
      problemDetails?.detail ??
      problemDetails?.title ??
      `A requisição falhou com status ${status}.`

    super(message)

    this.name = 'ApiError'
    this.status = status
    this.problemDetails = problemDetails
  }
}

export function getApiErrorMessage(
  error: unknown,
  fallbackMessage: string,
): string {
  if (error instanceof ApiError) {
    return error.message
  }

  return fallbackMessage
}