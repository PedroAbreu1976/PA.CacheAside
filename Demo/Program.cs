using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Demo.Services;
using System.Reflection;
using Demo.Models;
using PA.CacheAside.Core;

var builder = Host.CreateDefaultBuilder();

var assembly = Assembly.GetEntryAssembly();
builder.ConfigureServices((hostContext, services) => {
    services.AddScoped<ILoggerService, LoggerService>();
    services.AddScoped<IBookService, BookService>();
    services.AddScoped<IAuthorService, AuthorService>();
    services.AddCacheAside()
        .WithHybridCache()
        .LoadCacheKeyGeneratorsFromAssemblies([assembly])
        .ConfigureCacheKeyFactory(s =>
        {
            s.RegisterCacheKeyProvider<Author>(a=> a.Id.ToString());
        });
});
var app = builder.Build();

ICacheKeyFactory fact = app.Services.GetRequiredService<ICacheKeyFactory>();


var bookRepository = app.Services.GetRequiredService<IBookService>();
var bookRepositoryInvalidator = app.Services.GetRequiredService<IBookServiceInvalidator>();
var authorRepository = app.Services.GetRequiredService<IAuthorService>();
var authorRepositoryInvalidator = app.Services.GetRequiredService<IAuthorServiceInvalidator>();

await LoadDataAsync();

var bookSearchByName = new BookSearchParameters {     AuthorName = "Fred",};
var bookSearchByTitle = new BookSearchParameters { Title = "1" };
var authorSearchByName = new AuthorSearchParameters { NameContains = "J" };
var authorSearchByAge = new AuthorSearchParameters { MinimumAge = 30, MaximumAge = 50 };
IEnumerable<Book> books;
IEnumerable<Author> authors;

Console.WriteLine("=============================================================================");
Console.WriteLine("Initial fetches (should hit repositories):");
Console.WriteLine("-----------------------------------------------------------------------------");
books = await FetchBooksAsync(bookSearchByName, bookSearchByTitle);
authors = await FetchAuthorsAsync(authorSearchByName, authorSearchByAge);
Console.WriteLine();

Console.WriteLine("=============================================================================");
Console.WriteLine("Second fetches (should NOT hit repositories):");
Console.WriteLine("-----------------------------------------------------------------------------");
books = await FetchBooksAsync(bookSearchByName, bookSearchByTitle);
authors = await FetchAuthorsAsync(authorSearchByName, authorSearchByAge);
Console.WriteLine();

await authorRepositoryInvalidator.InvalidateGetAllAsync();
await authorRepositoryInvalidator.InvalidateSearchAsync(authorSearchByName);
await authorRepositoryInvalidator.InvalidateGetByIdAsync(authors.First().Id);
await bookRepositoryInvalidator.InvalidateGetAllAsync();
await bookRepositoryInvalidator.InvalidateSearchAsync(bookSearchByName);
await bookRepositoryInvalidator.InvalidateGetByIdAsync(books.First().Id);
Console.WriteLine("=============================================================================");
Console.WriteLine("Third fetches should only hit repositories for invalidated items:");
Console.WriteLine("\t- GetAllAsync for both repositories");
Console.WriteLine("\t- GetByIdAsync for first author and first book");
Console.WriteLine("\t- SearchAsync for author search by name and book search by name");
Console.WriteLine("-----------------------------------------------------------------------------");
books = await FetchBooksAsync(bookSearchByName, bookSearchByTitle);
authors = await FetchAuthorsAsync(authorSearchByName, authorSearchByAge);
Console.WriteLine();

await bookRepositoryInvalidator.InvalidateAsync();
await authorRepositoryInvalidator.InvalidateAsync();
Console.WriteLine("=============================================================================");
Console.WriteLine("Fourth fetches (should hit repositories):");
Console.WriteLine("-----------------------------------------------------------------------------");
books = await FetchBooksAsync(bookSearchByName, bookSearchByTitle);
authors = await FetchAuthorsAsync(authorSearchByName, authorSearchByAge);
Console.WriteLine();


await app.RunAsync();

#region Load and Query Helpers

async Task<IEnumerable<Book>> FetchBooksAsync(BookSearchParameters bookSearchByName, BookSearchParameters bookSearchByTitle)
{
    var books = await bookRepository.GetAllAsync();
    Console.WriteLine($"All books: [{string.Join(", ", books.Select(b => b.Title))}]");
    Console.WriteLine($"Books count (Marked as Ignore - Should always hit repository): {await bookRepository.CountAsync()}");
    foreach (var book in books)
    {
        Console.WriteLine($"Book ID={book.Id}: {(await bookRepository.GetByIdAsync(book.Id)).Title}");
    }
    var booksByName = await bookRepository.SearchAsync(bookSearchByName);
    Console.WriteLine($"Books by Author [{bookSearchByName.AuthorName}]: [{string.Join(", ", booksByName.Select(b => b.Title))}]");
    var booksByTitle = await bookRepository.SearchAsync(bookSearchByTitle);
    Console.WriteLine($"Books by title [{bookSearchByTitle.Title}]: [{string.Join(", ", booksByTitle.Select(b => b.Title))}]");

    return books;
}

async Task<IEnumerable<Author>> FetchAuthorsAsync(AuthorSearchParameters authorSearchByName, AuthorSearchParameters authorSearchByAge)
{
    var authors = await authorRepository.GetAllAsync();
    Console.WriteLine($"All authors: [{string.Join(", ", authors.Select(a => $"{a.FirstName} {a.LastName}"))}]");
    foreach (var author in authors)
    {
        Console.WriteLine($"Author ID={author.Id}: {(await authorRepository.GetByIdAsync(author.Id)).FirstName} {(await authorRepository.GetByIdAsync(author.Id)).LastName}");
    }
    var authorsByName = await authorRepository.SearchAsync(authorSearchByName);
    Console.WriteLine($"Authors by name [{authorSearchByName.NameContains}: [{string.Join(", ", authorsByName.Select(a => $"{a.FirstName} {a.LastName}"))}]");
    var authorsByAge = await authorRepository.SearchAsync(authorSearchByAge);
    Console.WriteLine($"Authors by age [{authorSearchByAge.MinimumAge}-{authorSearchByAge.MaximumAge}]: [{string.Join(", ", authorsByAge.Select(a => $"{a.FirstName} {a.LastName}"))}]");

    return authors;
}

async Task LoadDataAsync()
{
    var author1 = await AddAuthor(authorRepository, 1, "Fred", "Krugger", 45);
    var author2 = await AddAuthor(authorRepository, 2, "John", "Smith", 25);
    var author3 = await AddAuthor(authorRepository, 3, "Jane", "Nobuks", 35);
    await AddBook(bookRepository, Guid.NewGuid(), "Book 1", DateTime.Now.AddYears(-5), author1);
    await AddBook(bookRepository, Guid.NewGuid(), "Book 2", DateTime.Now.AddYears(-3), author1);
    await AddBook(bookRepository, Guid.NewGuid(), "Book 3", DateTime.Now.AddYears(-1), author2);
}

async Task<Author> AddAuthor(IAuthorService authorService, int id, string firstName, string lastName, int age)
{
    var result = new Author { Id = id, FirstName = firstName, LastName = lastName, Age = age };
    await authorService.AddAsync(result);
    return result;
}
async Task<Book> AddBook(IBookService bookService, Guid id, string title, DateTime publishDate, Author author)
{
    var result = new Book { Id = id, Title = title, PublishDate = publishDate, Author = author };
    await bookService.AddAsync(result);
    return result;
}

#endregion