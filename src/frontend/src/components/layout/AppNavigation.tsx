export type AppSection =
  | 'totals'
  | 'people'
  | 'transactions'

interface AppNavigationProps {
  activeSection: AppSection
  onSectionChange: (section: AppSection) => void
}

const navigationItems: Array<{
  id: AppSection
  label: string
  description: string
}> = [
  {
    id: 'totals',
    label: 'Visão geral',
    description: 'Totais e saldos',
  },
  {
    id: 'people',
    label: 'Pessoas',
    description: 'Moradores cadastrados',
  },
  {
    id: 'transactions',
    label: 'Transações',
    description: 'Receitas e despesas',
  },
]

export function AppNavigation({
  activeSection,
  onSectionChange,
}: AppNavigationProps) {
  return (
    <nav
      className="app-navigation"
      aria-label="Navegação principal"
    >
      <div className="app-navigation__items">
        {navigationItems.map((item) => {
          const isActive = activeSection === item.id

          return (
            <button
              key={item.id}
              type="button"
              className={[
                'app-navigation__item',
                isActive
                  ? 'app-navigation__item--active'
                  : '',
              ]
                .filter(Boolean)
                .join(' ')}
              aria-current={isActive ? 'page' : undefined}
              onClick={() => onSectionChange(item.id)}
            >
              <span className="app-navigation__label">
                {item.label}
              </span>

              <span className="app-navigation__description">
                {item.description}
              </span>
            </button>
          )
        })}
      </div>
    </nav>
  )
}