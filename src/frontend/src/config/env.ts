const apiBaseUrl = import.meta.env.VITE_API_BASE_URL?.trim()

if (!apiBaseUrl) {
  throw new Error(
    'A variável VITE_API_BASE_URL não foi configurada.',
  )
}

export const env = {
  apiBaseUrl: apiBaseUrl.replace(/\/+$/, ''),
} as const