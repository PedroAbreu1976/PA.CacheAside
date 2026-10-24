using Demo.Models;

namespace Demo.Services;

public class AuthorService(ILoggerService loggerService) : IAuthorService
{
    private readonly List<Author> _authors = new();

    public Task AddAsync(Author author, CancellationToken ct = default)
    {
        loggerService.LogRepositoryCommand(nameof(AuthorService), nameof(AddAsync), author.Id);
        _authors.Add(author);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Author>> GetAllAsync(CancellationToken ct = default)
    {
        loggerService.LogRepositoryQuery(nameof(AuthorService), nameof(GetAllAsync));
        return Task.FromResult<IEnumerable<Author>>(_authors);
    }

    public Task<Author> GetByIdAsync(int id, CancellationToken ct = default)
    {
        loggerService.LogRepositoryQuery(nameof(AuthorService), nameof(GetByIdAsync), id);
        return Task.FromResult(_authors.First(author => author.Id == id));
    }

    public Task RemoveAsync(int id, CancellationToken ct = default)
    {
        loggerService.LogRepositoryCommand(nameof(AuthorService), nameof(RemoveAsync), id);
        _authors.RemoveAll(author => author.Id == id);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Author>> SearchAsync(AuthorSearchParameters parameters, CancellationToken ct = default)
    {
        loggerService.LogRepositoryQuery(nameof(AuthorService), nameof(SearchAsync), parameters.NameContains, parameters.MinimumAge, parameters.MaximumAge);
        IEnumerable<Author> results = _authors;

        if (!string.IsNullOrWhiteSpace(parameters.NameContains))
        {
            results = results.Where(author =>
                $"{author.FirstName} {author.LastName}"
                    .Contains(parameters.NameContains, StringComparison.OrdinalIgnoreCase));
        }

        if (parameters.MinimumAge.HasValue)
        {
            results = results.Where(author => author.Age >= parameters.MinimumAge.Value);
        }

        if (parameters.MaximumAge.HasValue)
        {
            results = results.Where(author => author.Age <= parameters.MaximumAge.Value);
        }

        return Task.FromResult(results);
    }

    public Task UpdateAsync(Author author, CancellationToken ct = default)
    {
        loggerService.LogRepositoryCommand(nameof(AuthorService), nameof(UpdateAsync), author.Id);
        var index = _authors.FindIndex(a => a.Id == author.Id);
        if (index >= 0)
        {
            _authors[index] = author;
        }
        return Task.CompletedTask;
    }
}
