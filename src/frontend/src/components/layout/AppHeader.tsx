export function AppHeader() {
  return (
    <header className="app-header">
      <div className="app-header__content">
        <div>
          <p className="app-header__eyebrow">
            Gestão financeira doméstica
          </p>

          <h1 className="app-header__title">
            Controle de Gastos Residenciais
          </h1>

          <p className="app-header__description">
            Organize pessoas, registre movimentações e acompanhe
            o saldo financeiro da residência.
          </p>
        </div>

        <div className="environment-badge">
          <span
            className="environment-badge__indicator"
            aria-hidden="true"
          />

          Ambiente de desenvolvimento
        </div>
      </div>
    </header>
  )
}