using HomeExpenseControl.Api.Domain.Entities;
using HomeExpenseControl.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeExpenseControl.Api.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration
    : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable(
            "Transactions",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Transactions_AmountInCents_Positive",
                    "\"AmountInCents\" > 0");

                tableBuilder.HasCheckConstraint(
                    "CK_Transactions_Type_Valid",
                    "\"Type\" IN (1, 2)");
            });

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Id)
            .ValueGeneratedOnAdd();

        builder.Property(transaction => transaction.Description)
            .HasMaxLength(Transaction.MaxDescriptionLength)
            .IsRequired();

        builder.Property(transaction => transaction.AmountInCents)
            .HasColumnType("INTEGER")
            .IsRequired();

        builder.Property(transaction => transaction.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(transaction => transaction.PersonId)
            .IsRequired();

        builder.Ignore(transaction => transaction.Amount);

        builder.HasOne(transaction => transaction.Person)
            .WithMany(person => person.Transactions)
            .HasForeignKey(transaction => transaction.PersonId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasIndex(transaction => transaction.PersonId);
    }
}