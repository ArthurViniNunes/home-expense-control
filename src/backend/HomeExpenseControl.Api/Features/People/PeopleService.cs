using HomeExpenseControl.Api.Domain.Entities;
using HomeExpenseControl.Api.Features.People.Contracts;
using HomeExpenseControl.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HomeExpenseControl.Api.Features.People;

/// <summary>
/// Coordena os casos de uso relacionados ao cadastro e à consulta de pessoas.
/// </summary>
public sealed class PeopleService
{
    private readonly AppDbContext _dbContext;

    public PeopleService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PersonResponse> CreateAsync(
        CreatePersonRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Age < 0)
        {
            throw new ArgumentException(
                "A idade não pode ser negativa.",
                nameof(request));
        }

        var person = new Person(
            request.Name,
            request.Age);

        _dbContext.People.Add(person);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(person);
    }

    public async Task<IReadOnlyList<PersonResponse>> ListAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.People
            // Objetos serão apenas consultados e não precisam ser monitorados pelo Entity Framework.
            .AsNoTracking()
            .OrderBy(person => person.Name)
            .ThenBy(person => person.Id)
            .Select(person => new PersonResponse(
                person.Id,
                person.Name,
                person.Age,
                person.Age < 18))
            .ToListAsync(cancellationToken);
    }

    public async Task<PersonResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.People
            .AsNoTracking()
            .Where(person => person.Id == id)
            .Select(person => new PersonResponse(
                person.Id,
                person.Name,
                person.Age,
                person.Age < 18))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static PersonResponse MapToResponse(Person person)
    {
        return new PersonResponse(
            person.Id,
            person.Name,
            person.Age,
            person.IsMinor);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var person = await _dbContext.People
            .SingleOrDefaultAsync(
                person => person.Id == id,
                cancellationToken);

        if (person is null)
        {
            return false;
        }

        _dbContext.People.Remove(person);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}