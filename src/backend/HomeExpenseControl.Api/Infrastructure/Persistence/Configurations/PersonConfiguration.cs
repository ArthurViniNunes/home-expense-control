using HomeExpenseControl.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeExpenseControl.Api.Infrastructure.Persistence.Configurations;

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("People");

        builder.HasKey(person => person.Id);

        builder.Property(person => person.Id)
            .ValueGeneratedOnAdd();

        builder.Property(person => person.Name)
            .HasMaxLength(Person.MaxNameLength)
            .IsRequired();

        builder.Property(person => person.Age)
            .IsRequired();

        /* Ignora a propriedade IsMinor, pois ela é calculada e não precisa ser persistida no banco de dados.
        Fazer isso evita inconsistências de dados, já que a idade da pessoa pode mudar ao longo do tempo
        (possível nova feature), mas a propriedade IsMinor não será atualizada automaticamente.
        */
        builder.Ignore(person => person.IsMinor);
    }
}