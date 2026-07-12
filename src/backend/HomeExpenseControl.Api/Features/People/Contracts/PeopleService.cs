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

        if (request.Age is null)
        {
            throw new ArgumentException(
                "A idade é obrigatória.",
                nameof(request));
        }

        var person = new Person(
            request.Name ?? string.Empty,
            request.Age.Value);

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
}