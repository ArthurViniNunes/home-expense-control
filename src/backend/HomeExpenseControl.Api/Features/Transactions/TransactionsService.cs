using HomeExpenseControl.Api.Common.Errors;
using HomeExpenseControl.Api.Domain.Entities;
using HomeExpenseControl.Api.Features.Transactions.Contracts;
using HomeExpenseControl.Api.Infrastructure.Persistence;
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

    public async Task<IReadOnlyList<TransactionResponse>> ListAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .OrderBy(transaction => transaction.Id)
            .Select(transaction => new TransactionResponse(
                transaction.Id,
                transaction.Description,
                transaction.AmountInCents / 100m,
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