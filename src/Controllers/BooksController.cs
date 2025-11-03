using System.Data;
using Dapper;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Npgsql;

namespace LibraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly string _databaseType;
    private readonly string _connectionString;

    public BooksController(IConfiguration configuration)
    {
        _configuration = configuration;
        _databaseType = _configuration["DatabaseSettings:Type"] ?? "SqlServer";
        _connectionString = _configuration.GetConnectionString(_databaseType) ?? 
            throw new InvalidOperationException($"Connection string '{_databaseType}' not found.");
    }

    // ❌ PROBLEMA: Accesso diretto al database nel controller
    // ❌ PROBLEMA: Logica di business mista con logica di accesso ai dati
    // ❌ PROBLEMA: Difficile da testare
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks()
    {
        try
        {
            using var connection = CreateConnection();
            
            // ❌ PROBLEMA: Query SQL embedded nel controller con gestione naming specifico per database
            string sql;
            if (_databaseType == "PostgreSQL")
            {
                sql = "SELECT id, title, author, isbn, published_date, genre, available_copies, price FROM books";
            }
            else
            {
                sql = "SELECT Id, Title, Author, ISBN, PublishedDate, Genre, AvailableCopies, Price FROM Books";
            }
            
            var books = await connection.QueryAsync<Book>(sql);
            return Ok(books);
        }
        catch (Exception ex)
        {
            // ❌ PROBLEMA: Gestione errori database nel controller
            return StatusCode(500, $"Database error: {ex.Message}");
        }
    }

    // ❌ PROBLEMA: Codice duplicato per la connessione al database
    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        try
        {
            using var connection = CreateConnection();
            
            // ❌ PROBLEMA: Query SQL con parametri hardcoded e gestione naming specifico per database
            string sql;
            if (_databaseType == "PostgreSQL")
            {
                sql = "SELECT id, title, author, isbn, published_date, genre, available_copies, price FROM books WHERE id = @Id";
            }
            else
            {
                sql = "SELECT Id, Title, Author, ISBN, PublishedDate, Genre, AvailableCopies, Price FROM Books WHERE Id = @Id";
            }
            
            var book = await connection.QuerySingleOrDefaultAsync<Book>(sql, new { Id = id });
            
            if (book == null)
                return NotFound();
                
            return Ok(book);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Database error: {ex.Message}");
        }
    }

    // ❌ PROBLEMA: Logica di business nel controller (validazione, trasformazione dati)
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Book>>> SearchBooks([FromQuery] string? title, [FromQuery] string? author)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(author))
            return BadRequest("Either title or author must be provided for search.");

        try
        {
            using var connection = CreateConnection();
            
            // ❌ PROBLEMA: Costruzione dinamica di query SQL nel controller con naming specifico per database
            string sql;
            if (_databaseType == "PostgreSQL")
            {
                sql = "SELECT id, title, author, isbn, published_date, genre, available_copies, price FROM books WHERE 1=1";
            }
            else
            {
                sql = "SELECT Id, Title, Author, ISBN, PublishedDate, Genre, AvailableCopies, Price FROM Books WHERE 1=1";
            }
            
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(title))
            {
                if (_databaseType == "PostgreSQL")
                {
                    sql += " AND title ILIKE @Title"; // PostgreSQL case-insensitive
                }
                else
                {
                    sql += " AND Title LIKE @Title";
                }
                parameters.Add("Title", $"%{title}%");
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                if (_databaseType == "PostgreSQL")
                {
                    sql += " AND author ILIKE @Author"; // PostgreSQL case-insensitive
                }
                else
                {
                    sql += " AND Author LIKE @Author";
                }
                parameters.Add("Author", $"%{author}%");
            }

            var books = await connection.QueryAsync<Book>(sql, parameters);
            return Ok(books);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Database error: {ex.Message}");
        }
    }

    // ❌ PROBLEMA: Validazione business rules nel controller
    [HttpPost]
    public async Task<ActionResult<Book>> CreateBook(Book book)
    {
        // ❌ PROBLEMA: Validazione business logic nel controller
        if (string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrWhiteSpace(book.Author))
            return BadRequest("Title and Author are required.");

        if (book.AvailableCopies < 0)
            return BadRequest("Available copies cannot be negative.");

        try
        {
            using var connection = CreateConnection();
            
            // ❌ PROBLEMA: Controllo duplicati nel controller con naming specifico per database
            string checkSql;
            if (_databaseType == "PostgreSQL")
            {
                checkSql = "SELECT COUNT(*) FROM books WHERE isbn = @ISBN";
            }
            else
            {
                checkSql = "SELECT COUNT(*) FROM Books WHERE ISBN = @ISBN";
            }
            var existingCount = await connection.QuerySingleAsync<int>(checkSql, new { book.ISBN });
            
            if (existingCount > 0)
                return BadRequest("A book with this ISBN already exists.");

            // ❌ PROBLEMA: SQL di inserimento specifico per database nel controller con naming conventions diverse
            string sql;
            if (_databaseType == "SqlServer")
            {
                sql = @"INSERT INTO Books (Title, Author, ISBN, PublishedDate, Genre, AvailableCopies, Price) 
                       OUTPUT INSERTED.Id
                       VALUES (@Title, @Author, @ISBN, @PublishedDate, @Genre, @AvailableCopies, @Price)";
            }
            else if (_databaseType == "PostgreSQL")
            {
                sql = @"INSERT INTO books (title, author, isbn, published_date, genre, available_copies, price) 
                       VALUES (@Title, @Author, @ISBN, @PublishedDate, @Genre, @AvailableCopies, @Price)
                       RETURNING id";
            }
            else // SQLite
            {
                sql = @"INSERT INTO Books (Title, Author, ISBN, PublishedDate, Genre, AvailableCopies, Price) 
                       VALUES (@Title, @Author, @ISBN, @PublishedDate, @Genre, @AvailableCopies, @Price);
                       SELECT last_insert_rowid()";
            }

            var newId = await connection.QuerySingleAsync<int>(sql, book);
            book.Id = newId;
            
            return CreatedAtAction(nameof(GetBook), new { id = newId }, book);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Database error: {ex.Message}");
        }
    }

    // ❌ PROBLEMA: Stessa logica di validazione ripetuta
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(int id, Book book)
    {
        if (id != book.Id)
            return BadRequest("ID mismatch.");

        // ❌ PROBLEMA: Validazione duplicata in ogni metodo
        if (string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrWhiteSpace(book.Author))
            return BadRequest("Title and Author are required.");

        if (book.AvailableCopies < 0)
            return BadRequest("Available copies cannot be negative.");

        try
        {
            using var connection = CreateConnection();
            
            // ❌ PROBLEMA: Controllo esistenza nel controller con naming specifico per database
            string checkSql;
            if (_databaseType == "PostgreSQL")
            {
                checkSql = "SELECT COUNT(*) FROM books WHERE id = @Id";
            }
            else
            {
                checkSql = "SELECT COUNT(*) FROM Books WHERE Id = @Id";
            }
            var exists = await connection.QuerySingleAsync<int>(checkSql, new { Id = id }) > 0;
            
            if (!exists)
                return NotFound();

            // ❌ PROBLEMA: Query SQL di aggiornamento nel controller con naming specifico per database
            string sql;
            if (_databaseType == "PostgreSQL")
            {
                sql = @"UPDATE books 
                       SET title = @Title, author = @Author, isbn = @ISBN, 
                           published_date = @PublishedDate, genre = @Genre, 
                           available_copies = @AvailableCopies, price = @Price
                       WHERE id = @Id";
            }
            else
            {
                sql = @"UPDATE Books 
                       SET Title = @Title, Author = @Author, ISBN = @ISBN, 
                           PublishedDate = @PublishedDate, Genre = @Genre, 
                           AvailableCopies = @AvailableCopies, Price = @Price
                       WHERE Id = @Id";
            }

            await connection.ExecuteAsync(sql, book);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Database error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        try
        {
            using var connection = CreateConnection();
            
            // ❌ PROBLEMA: Logica business (controllo prestiti) nel controller con naming specifico per database
            string checkLoansSql;
            if (_databaseType == "PostgreSQL")
            {
                checkLoansSql = "SELECT COUNT(*) FROM book_loans WHERE book_id = @Id AND returned_date IS NULL";
            }
            else
            {
                checkLoansSql = "SELECT COUNT(*) FROM BookLoans WHERE BookId = @Id AND ReturnedDate IS NULL";
            }
            var activeLoans = await connection.QuerySingleAsync<int>(checkLoansSql, new { Id = id });
            
            if (activeLoans > 0)
                return BadRequest("Cannot delete book with active loans.");

            // ❌ PROBLEMA: Query di cancellazione nel controller con naming specifico per database
            string sql;
            if (_databaseType == "PostgreSQL")
            {
                sql = "DELETE FROM books WHERE id = @Id";
            }
            else
            {
                sql = "DELETE FROM Books WHERE Id = @Id";
            }
            var affected = await connection.ExecuteAsync(sql, new { Id = id });

            if (affected == 0)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Database error: {ex.Message}");
        }
    }

    // ❌ PROBLEMA: Factory pattern nel controller invece che in un servizio dedicato
    // ❌ PROBLEMA: Logica di connessione database nel controller
    private IDbConnection CreateConnection()
    {
        return _databaseType switch
        {
            "SqlServer" => new SqlConnection(_connectionString),
            "PostgreSQL" => new NpgsqlConnection(_connectionString),
            "SQLite" => new SqliteConnection(_connectionString),
            _ => throw new InvalidOperationException($"Unsupported database type: {_databaseType}")
        };
    }
}

/*
 * ❌ PROBLEMI EVIDENZIATI IN QUESTO CONTROLLER:
 * 
 * 1. VIOLAZIONE SINGLE RESPONSIBILITY PRINCIPLE
 *    - Il controller gestisce HTTP requests E accesso al database
 * 
 * 2. ACCOPPIAMENTO FORTE
 *    - Dipendenza diretta da Dapper e specifici database provider
 *    - Difficile cambiare database senza modificare il controller
 * 
 * 3. CODICE DUPLICATO
 *    - Logica di connessione ripetuta in ogni metodo
 *    - Validazioni duplicate
 *    - Gestione errori ripetitiva
 * 
 * 4. DIFFICOLTÀ NEI TEST
 *    - Impossibile testare senza database reale
 *    - Non si possono mockare facilmente le dipendenze
 * 
 * 5. MANUTENIBILITÀ SCARSA
 *    - Query SQL sparse in tutto il controller
 *    - Business logic mista con data access logic
 * 
 * 6. SCALABILITÀ LIMITATA
 *    - Aggiungere nuovo database richiede modifiche al controller
 *    - Impossibile riutilizzare la logica in altri contesti
 */