interface SectionPlaceholderProps {
  title: string
  description: string
}

export function SectionPlaceholder({
  title,
  description,
}: SectionPlaceholderProps) {
  return (
    <div className="section-placeholder">
      <div
        className="section-placeholder__icon"
        aria-hidden="true"
      >
        +
      </div>

      <div>
        <h3 className="section-placeholder__title">
          {title}
        </h3>

        <p className="section-placeholder__description">
          {description}
        </p>
      </div>
    </div>
  )
}