using Dapper;
using LibraryAPI.Infrastructure.Database;
using LibraryAPI.Models;
using LibraryAPI.Repositories.Interfaces;

namespace LibraryAPI.Repositories.Implementations;

/// <summary>
/// ✅ SOLUZIONE: Implementazione specifica per SQLite del repository
/// Gestisce le specifiche di SQLite per l'inserimento con auto-increment
/// </summary>
public class SqliteBookRepository : IBookRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SqliteBookRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        const string sql = """
            SELECT Id, Title, Author, ISBN, PublishedDate, Genre, AvailableCopies, Price 
            FROM Books
            ORDER BY Title
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Book>(sql);
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT Id, Title, Author, ISBN, PublishedDate, Genre, AvailableCopies, Price 
            FROM Books 
            WHERE Id = @Id
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Book>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Book>> SearchAsync(string? title, string? author)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        var sql = """
            SELECT Id, Title, Author, ISBN, PublishedDate, Genre, AvailableCopies, Price 
            FROM Books 
            WHERE 1=1
            """;

        if (!string.IsNullOrWhiteSpace(title))
        {
            conditions.Add("AND Title LIKE @Title COLLATE NOCASE"); // SQLite case-insensitive search
            parameters.Add("Title", $"%{title}%");
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            conditions.Add("AND Author LIKE @Author COLLATE NOCASE"); // SQLite case-insensitive search
            parameters.Add("Author", $"%{author}%");
        }

        if (conditions.Any())
        {
            sql += " " + string.Join(" ", conditions);
        }

        sql += " ORDER BY Title";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Book>(sql, parameters);
    }

    public async Task<bool> ExistsByIsbnAsync(string isbn)
    {
        const string sql = "SELECT COUNT(*) FROM Books WHERE ISBN = @ISBN";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.QuerySingleAsync<int>(sql, new { ISBN = isbn });
        return count > 0;
    }

    public async Task<bool> ExistsByIsbnAsync(string isbn, int excludeId)
    {
        const string sql = "SELECT COUNT(*) FROM Books WHERE ISBN = @ISBN AND Id != @ExcludeId";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.QuerySingleAsync<int>(sql, new { ISBN = isbn, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<Book> CreateAsync(Book book)
    {
        const string sql = """
            INSERT INTO Books (Title, Author, ISBN, PublishedDate, Genre, AvailableCopies, Price) 
            VALUES (@Title, @Author, @ISBN, @PublishedDate, @Genre, @AvailableCopies, @Price);
            SELECT last_insert_rowid()
            """;

        using var connection = _connectionFactory.CreateConnection();
        var newId = await connection.QuerySingleAsync<int>(sql, book);
        book.Id = newId;
        return book;
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        const string sql = """
            UPDATE Books 
            SET Title = @Title, 
                Author = @Author, 
                ISBN = @ISBN, 
                PublishedDate = @PublishedDate, 
                Genre = @Genre, 
                AvailableCopies = @AvailableCopies, 
                Price = @Price
            WHERE Id = @Id
            """;

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, book);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Books WHERE Id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }

    public async Task<bool> HasActiveLoansAsync(int bookId)
    {
        const string sql = """
            SELECT COUNT(*) 
            FROM BookLoans 
            WHERE BookId = @BookId AND ReturnedDate IS NULL
            """;

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.QuerySingleAsync<int>(sql, new { BookId = bookId });
        return count > 0;
    }
}