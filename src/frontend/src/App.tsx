import { useEffect, useState } from 'react'
import { peopleApi } from './features/people/peopleApi'
import type { Person } from './features/people/peopleTypes'
import { ApiError } from './services/apiError'
import './App.css'

function App() {
  const [people, setPeople] = useState<Person[]>([])
  const [message, setMessage] = useState(
    'Carregando pessoas...',
  )

  useEffect(() => {
    const controller = new AbortController()

    async function loadPeople() {
      try {
        const response = await peopleApi.list(
          controller.signal,
        )

        setPeople(response)
        setMessage(
          response.length === 0
            ? 'Nenhuma pessoa cadastrada.'
            : '',
        )
      } catch (error) {
        if (controller.signal.aborted) {
          return
        }

        setMessage(
          error instanceof ApiError
            ? error.message
            : 'Não foi possível acessar a API.',
        )
      }
    }

    void loadPeople()

    return () => {
      controller.abort()
    }
  }, [])

  return (
    <main>
      <h1>Controle de Gastos Residenciais</h1>

      {message && <p>{message}</p>}

      <ul>
        {people.map((person) => (
          <li key={person.id}>
            {person.name} ({person.age} anos)
          </li>
        ))}
      </ul>
    </main>
  )
}

export default App
