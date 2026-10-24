using Demo.Models;

namespace Demo.Services;

public class BookService(ILoggerService loggerService) : IBookService
{
    private readonly List<Book> _books = new();

    public Task AddAsync(Book book, CancellationToken ct = default)
    {
        loggerService.LogRepositoryCommand(nameof(BookService), nameof(AddAsync), book.Id);
        _books.Add(book);
        return Task.CompletedTask;
    }

    public Task<int> CountAsync(CancellationToken ct = default)
    {
        loggerService.LogRepositoryQuery(nameof(BookService), nameof(CountAsync));
        return Task.FromResult(_books.Count);
    }

    public Task<IEnumerable<Book>> GetAllAsync(CancellationToken ct = default)
    {
        loggerService.LogRepositoryQuery(nameof(BookService), nameof(GetAllAsync));
        return Task.FromResult<IEnumerable<Book>>(_books);
    }

    public async Task<IEnumerable<Book>> GetByAuthorAsync(Author author, CancellationToken ct = default)
    {
        loggerService.LogRepositoryQuery(nameof(BookService), nameof(GetByAuthorAsync), author.Id);

        return _books.Where(book => book.Author.Id == author.Id);
    }

    public Task<Book> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        loggerService.LogRepositoryQuery(nameof(BookService), nameof(GetByIdAsync), id);
        return Task.FromResult(_books.First(book => book.Id == id));
    }

    public Task RemoveAsync(Guid id, CancellationToken ct = default)
    {
        loggerService.LogRepositoryCommand(nameof(BookService), nameof(RemoveAsync), id);
        _books.RemoveAll(book => book.Id == id);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Book>> SearchAsync(BookSearchParameters parameters, CancellationToken ct = default)
    {
        loggerService.LogRepositoryQuery(nameof(BookService), nameof(SearchAsync), parameters.Title, parameters.AuthorName, parameters.PublishedAfter, parameters.PublishedBefore);
        IEnumerable<Book> results = _books;

        if (!string.IsNullOrWhiteSpace(parameters.Title))
        {
            results = results.Where(book =>
                book.Title.Contains(parameters.Title, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(parameters.AuthorName))
        {
            results = results.Where(book =>
                $"{book.Author.FirstName} {book.Author.LastName}"
                    .Contains(parameters.AuthorName, StringComparison.OrdinalIgnoreCase));
        }

        if (parameters.PublishedAfter.HasValue)
        {
            results = results.Where(book => book.PublishDate >= parameters.PublishedAfter.Value);
        }

        if (parameters.PublishedBefore.HasValue)
        {
            results = results.Where(book => book.PublishDate <= parameters.PublishedBefore.Value);
        }

        return Task.FromResult(results);
    }

    public Task UpdateAsync(Book book, CancellationToken ct = default)
    {
        loggerService.LogRepositoryCommand(nameof(BookService), nameof(UpdateAsync), book.Id);
        var index = _books.FindIndex(b => b.Id == book.Id);
        if (index >= 0)
        {
            _books[index] = book;
        }
        return Task.CompletedTask;
    }
}
