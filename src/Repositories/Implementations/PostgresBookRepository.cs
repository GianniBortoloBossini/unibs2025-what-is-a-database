using Dapper;
using LibraryAPI.Infrastructure.Database;
using LibraryAPI.Models;
using LibraryAPI.Repositories.Interfaces;

namespace LibraryAPI.Repositories.Implementations;

/// <summary>
/// ✅ SOLUZIONE: Implementazione specifica per PostgreSQL del repository
/// Gestisce le differenze specifiche di PostgreSQL (naming conventions, case sensitivity, ILIKE)
/// </summary>
public class PostgresBookRepository : IBookRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PostgresBookRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        const string sql = """
            SELECT id as Id, title as Title, author as Author, isbn as ISBN, 
                   published_date as PublishedDate, genre as Genre, 
                   available_copies as AvailableCopies, price as Price 
            FROM books
            ORDER BY title
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Book>(sql);
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT id as Id, title as Title, author as Author, isbn as ISBN, 
                   published_date as PublishedDate, genre as Genre, 
                   available_copies as AvailableCopies, price as Price 
            FROM books 
            WHERE id = @Id
            """;

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Book>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Book>> SearchAsync(string? title, string? author)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        var sql = """
            SELECT id as Id, title as Title, author as Author, isbn as ISBN, 
                   published_date as PublishedDate, genre as Genre, 
                   available_copies as AvailableCopies, price as Price 
            FROM books 
            WHERE 1=1
            """;

        if (!string.IsNullOrWhiteSpace(title))
        {
            conditions.Add("AND title ILIKE @Title"); // PostgreSQL case-insensitive search
            parameters.Add("Title", $"%{title}%");
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            conditions.Add("AND author ILIKE @Author"); // PostgreSQL case-insensitive search
            parameters.Add("Author", $"%{author}%");
        }

        if (conditions.Any())
        {
            sql += " " + string.Join(" ", conditions);
        }

        sql += " ORDER BY title";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Book>(sql, parameters);
    }

    public async Task<bool> ExistsByIsbnAsync(string isbn)
    {
        const string sql = "SELECT COUNT(*) FROM books WHERE isbn = @ISBN";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.QuerySingleAsync<int>(sql, new { ISBN = isbn });
        return count > 0;
    }

    public async Task<bool> ExistsByIsbnAsync(string isbn, int excludeId)
    {
        const string sql = "SELECT COUNT(*) FROM books WHERE isbn = @ISBN AND id != @ExcludeId";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.QuerySingleAsync<int>(sql, new { ISBN = isbn, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<Book> CreateAsync(Book book)
    {
        const string sql = """
            INSERT INTO books (title, author, isbn, published_date, genre, available_copies, price) 
            VALUES (@Title, @Author, @ISBN, @PublishedDate, @Genre, @AvailableCopies, @Price)
            RETURNING id
            """;

        using var connection = _connectionFactory.CreateConnection();
        var newId = await connection.QuerySingleAsync<int>(sql, book);
        book.Id = newId;
        return book;
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        const string sql = """
            UPDATE books 
            SET title = @Title, 
                author = @Author, 
                isbn = @ISBN, 
                published_date = @PublishedDate, 
                genre = @Genre, 
                available_copies = @AvailableCopies, 
                price = @Price
            WHERE id = @Id
            """;

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, book);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM books WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        return affectedRows > 0;
    }

    public async Task<bool> HasActiveLoansAsync(int bookId)
    {
        const string sql = """
            SELECT COUNT(*) 
            FROM book_loans 
            WHERE book_id = @BookId AND returned_date IS NULL
            """;

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.QuerySingleAsync<int>(sql, new { BookId = bookId });
        return count > 0;
    }
}