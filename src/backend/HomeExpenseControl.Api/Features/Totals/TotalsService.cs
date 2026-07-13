using HomeExpenseControl.Api.Common.Money;
using HomeExpenseControl.Api.Domain.Enums;
using HomeExpenseControl.Api.Features.Totals.Contracts;
using HomeExpenseControl.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HomeExpenseControl.Api.Features.Totals;

/// <summary>
/// Calcula os totais financeiros individuais e gerais da residência.
/// </summary>
public sealed class TotalsService
{
    private readonly AppDbContext _dbContext;

    public TotalsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TotalsResponse> GetAsync(
        CancellationToken cancellationToken)
    {
        // As pessoas são consultadas separadamente para garantir que aquelas
        // sem transações também sejam incluídas no resultado.
        var people = await _dbContext.People
            .AsNoTracking()
            .OrderBy(person => person.Name)
            .ThenBy(person => person.Id)
            .Select(person => new
            {
                person.Id,
                person.Name
            })
            .ToListAsync(cancellationToken);

        // A agregação ocorre no banco em centavos, evitando cálculos
        // monetários com tipos de ponto flutuante.
        var transactionTotals = await _dbContext.Transactions
            .AsNoTracking()
            .GroupBy(transaction => transaction.PersonId)
            .Select(group => new
            {
                PersonId = group.Key,

                TotalIncomeInCents = group.Sum(
                    transaction =>
                        transaction.Type == TransactionType.Income
                            ? transaction.AmountInCents
                            : 0L),

                TotalExpensesInCents = group.Sum(
                    transaction =>
                        transaction.Type == TransactionType.Expense
                            ? transaction.AmountInCents
                            : 0L)
            })
            .ToListAsync(cancellationToken);

        var totalsByPerson = transactionTotals.ToDictionary(
            total => total.PersonId);

        var peopleResponses = new List<PersonTotalsResponse>(
            people.Count);

        foreach (var person in people)
        {
            var totalIncomeInCents = 0L;
            var totalExpensesInCents = 0L;

            if (totalsByPerson.TryGetValue(
                person.Id,
                out var personTotals))
            {
                totalIncomeInCents =
                    personTotals.TotalIncomeInCents;

                totalExpensesInCents =
                    personTotals.TotalExpensesInCents;
            }

            var balanceInCents = checked(
                totalIncomeInCents - totalExpensesInCents);

            peopleResponses.Add(
                new PersonTotalsResponse(
                    person.Id,
                    person.Name,
                    MoneyConverter.FromCents(totalIncomeInCents),
                    MoneyConverter.FromCents(totalExpensesInCents),
                    MoneyConverter.FromCents(balanceInCents)));
        }

        var generalIncomeInCents = checked(
            transactionTotals.Sum(
                total => total.TotalIncomeInCents));

        var generalExpensesInCents = checked(
            transactionTotals.Sum(
                total => total.TotalExpensesInCents));

        var generalBalanceInCents = checked(
            generalIncomeInCents - generalExpensesInCents);

        var general = new GeneralTotalsResponse(
            MoneyConverter.FromCents(generalIncomeInCents),
            MoneyConverter.FromCents(generalExpensesInCents),
            MoneyConverter.FromCents(generalBalanceInCents));

        return new TotalsResponse(
            peopleResponses,
            general);
    }
}