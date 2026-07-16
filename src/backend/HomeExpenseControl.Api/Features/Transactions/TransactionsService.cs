using HomeExpenseControl.Api.Common.Errors;
using HomeExpenseControl.Api.Domain.Entities;
using HomeExpenseControl.Api.Features.Transactions.Contracts;
using HomeExpenseControl.Api.Infrastructure.Persistence;
using HomeExpenseControl.Api.Common.Money;
using Microsoft.EntityFrameworkCore;

namespace HomeExpenseControl.Api.Features.Transactions;

/// <summary>
/// Coordena os casos de uso relacionados às transações.
/// </summary>
public sealed class TransactionsService
{
    private readonly AppDbContext _dbContext;

    public TransactionsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TransactionResponse> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var person = await _dbContext.People
            .SingleOrDefaultAsync(
                person => person.Id == request.PersonId,
                cancellationToken);

        if (person is null)
        {
            throw new ResourceNotFoundException(
                $"Não existe uma pessoa cadastrada com o identificador {request.PersonId}.");
        }

        var transaction = new Transaction(
            request.Description ?? string.Empty,
            request.Amount,
            request.Type,
            person);

        _dbContext.Transactions.Add(transaction);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(transaction, person);
    }

    public async Task<TransactionResponse> UpdateAsync(
        int id,
        UpdateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var transaction = await _dbContext.Transactions
            .SingleOrDefaultAsync(
                transaction => transaction.Id == id,
                cancellationToken);

        if (transaction is null)
        {
            throw new ResourceNotFoundException(
                $"Não existe uma transação cadastrada com o identificador {id}.");
        }

        var person = await _dbContext.People
            .SingleOrDefaultAsync(
                person => person.Id == request.PersonId,
                cancellationToken);

        if (person is null)
        {
            throw new ResourceNotFoundException(
                $"Não existe uma pessoa cadastrada com o identificador {request.PersonId}.");
        }

        transaction.Update(
            request.Description ?? string.Empty,
            request.Amount,
            request.Type,
            person);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return MapToResponse(
            transaction,
            person);
    }

    public async Task DeleteAsync(
            int id,
            CancellationToken cancellationToken)
    {
        var transaction = await _dbContext.Transactions
            .SingleOrDefaultAsync(
                transaction => transaction.Id == id,
                cancellationToken);

        if (transaction is null)
        {
            throw new ResourceNotFoundException(
                $"Não existe uma transação cadastrada com o identificador {id}.");
        }

        _dbContext.Transactions.Remove(transaction);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
    public async Task<IReadOnlyList<TransactionResponse>> ListAsync(
        ListTransactionsQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var transactionsQuery = _dbContext.Transactions
            .AsNoTracking()
            .AsQueryable();

        if (query.PersonId.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(
                transaction =>
                    transaction.Person.Id == query.PersonId.Value);
        }

        if (query.AgeGroup == "minor")
        {
            transactionsQuery = transactionsQuery.Where(
                transaction =>
                    transaction.Person.Age < Person.AdultAge);
        }
        else if (query.AgeGroup == "adult")
        {
            transactionsQuery = transactionsQuery.Where(
                transaction =>
                    transaction.Person.Age >= Person.AdultAge);
        }

        var transactionType = query.GetTransactionType();

        if (transactionType.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(
                transaction =>
                    transaction.Type == transactionType.Value);
        }

        if (query.MinAmount.HasValue)
        {
            var minimumAmountInCents =
                MoneyConverter.ToCents(query.MinAmount.Value);

            transactionsQuery = transactionsQuery.Where(
                transaction =>
                    transaction.AmountInCents >= minimumAmountInCents);
        }

        if (query.MaxAmount.HasValue)
        {
            var maximumAmountInCents =
                MoneyConverter.ToCents(query.MaxAmount.Value);

            transactionsQuery = transactionsQuery.Where(
                transaction =>
                    transaction.AmountInCents <= maximumAmountInCents);
        }

        return await transactionsQuery
            .OrderBy(transaction => transaction.Id)
            .Select(transaction => new TransactionResponse(
                transaction.Id,
                transaction.Description,
                MoneyConverter.FromCents(
                    transaction.AmountInCents),
                transaction.Type,
                new TransactionPersonResponse(
                    transaction.Person.Id,
                    transaction.Person.Name)))
            .ToListAsync(cancellationToken);
    }

    public async Task<TransactionResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.Id == id)
            .Select(transaction => new TransactionResponse(
                transaction.Id,
                transaction.Description,
                transaction.AmountInCents / 100m,
                transaction.Type,
                new TransactionPersonResponse(
                    transaction.Person.Id,
                    transaction.Person.Name)))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static TransactionResponse MapToResponse(
        Transaction transaction,
        Person person)
    {
        return new TransactionResponse(
            transaction.Id,
            transaction.Description,
            transaction.Amount,
            transaction.Type,
            new TransactionPersonResponse(
                person.Id,
                person.Name));
    }
}