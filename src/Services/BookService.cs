using LibraryAPI.Models;
using LibraryAPI.Repositories.Interfaces;
using LibraryAPI.Services.Interfaces;

namespace LibraryAPI.Services;

/// <summary>
/// ✅ SOLUZIONE: Application Service che contiene tutta la logica di business
/// Orchestrata le operazioni sui repository e applica le business rules
/// Completamente testabile grazie alla dipendenza da interfacce
/// </summary>
public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<BookService> _logger;

    public BookService(IBookRepository bookRepository, ILogger<BookService> logger)
    {
        _bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all books");
            var books = await _bookRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {BookCount} books", books.Count());
            return books;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all books");
            throw; // Re-throw per permettere al controller di gestire l'errore appropriatamente
        }
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid book ID provided: {BookId}", id);
                return null;
            }

            _logger.LogInformation("Retrieving book with ID: {BookId}", id);
            var book = await _bookRepository.GetByIdAsync(id);
            
            if (book == null)
            {
                _logger.LogInformation("Book with ID {BookId} not found", id);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved book: {BookTitle}", book.Title);
            }
            
            return book;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving book with ID: {BookId}", id);
            throw;
        }
    }

    public async Task<ServiceResult<IEnumerable<Book>>> SearchBooksAsync(string? title, string? author)
    {
        try
        {
            // ✅ BUSINESS RULE: Almeno uno dei criteri di ricerca deve essere fornito
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(author))
            {
                _logger.LogWarning("Search attempted without any criteria");
                return ServiceResult<IEnumerable<Book>>.ValidationError(
                    "Either title or author must be provided for search.");
            }

            // ✅ BUSINESS RULE: Validazione lunghezza minima per evitare ricerche troppo generiche
            if (!string.IsNullOrWhiteSpace(title) && title.Trim().Length < 2)
            {
                return ServiceResult<IEnumerable<Book>>.ValidationError(
                    "Title search must be at least 2 characters long.");
            }

            if (!string.IsNullOrWhiteSpace(author) && author.Trim().Length < 2)
            {
                return ServiceResult<IEnumerable<Book>>.ValidationError(
                    "Author search must be at least 2 characters long.");
            }

            _logger.LogInformation("Searching books with title: '{Title}', author: '{Author}'", 
                title ?? "N/A", author ?? "N/A");

            var books = await _bookRepository.SearchAsync(title?.Trim(), author?.Trim());
            
            _logger.LogInformation("Search returned {BookCount} books", books.Count());
            return ServiceResult<IEnumerable<Book>>.Success(books);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching books");
            return ServiceResult<IEnumerable<Book>>.Error($"An error occurred while searching books: {ex.Message}");
        }
    }

    public async Task<ServiceResult<Book>> CreateBookAsync(Book book)
    {
        try
        {
            // ✅ BUSINESS RULE: Validazione dei dati di input
            var validationErrors = ValidateBookData(book);
            if (validationErrors.Any())
            {
                _logger.LogWarning("Book creation failed validation: {@ValidationErrors}", validationErrors);
                return ServiceResult<Book>.ValidationError(validationErrors);
            }

            // ✅ BUSINESS RULE: ISBN deve essere unico
            if (await _bookRepository.ExistsByIsbnAsync(book.ISBN))
            {
                _logger.LogWarning("Attempt to create book with duplicate ISBN: {ISBN}", book.ISBN);
                return ServiceResult<Book>.ValidationError("A book with this ISBN already exists.");
            }

            // ✅ BUSINESS RULE: Normalizzazione dei dati
            NormalizeBookData(book);

            _logger.LogInformation("Creating new book: {Title} by {Author}", book.Title, book.Author);
            var createdBook = await _bookRepository.CreateAsync(book);
            
            _logger.LogInformation("Successfully created book with ID: {BookId}", createdBook.Id);
            return ServiceResult<Book>.Success(createdBook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating book: {Title}", book?.Title ?? "Unknown");
            return ServiceResult<Book>.Error($"An error occurred while creating the book: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> UpdateBookAsync(int id, Book book)
    {
        try
        {
            // ✅ BUSINESS RULE: ID deve corrispondere
            if (id != book.Id)
            {
                _logger.LogWarning("ID mismatch in book update: URL ID {UrlId} vs Book ID {BookId}", id, book.Id);
                return ServiceResult<bool>.ValidationError("ID mismatch between URL and book data.");
            }

            // ✅ BUSINESS RULE: Il libro deve esistere
            var existingBook = await _bookRepository.GetByIdAsync(id);
            if (existingBook == null)
            {
                _logger.LogWarning("Attempt to update non-existent book with ID: {BookId}", id);
                return ServiceResult<bool>.Error("Book not found.");
            }

            // ✅ BUSINESS RULE: Validazione dei dati
            var validationErrors = ValidateBookData(book);
            if (validationErrors.Any())
            {
                _logger.LogWarning("Book update failed validation for ID {BookId}: {@ValidationErrors}", 
                    id, validationErrors);
                return ServiceResult<bool>.ValidationError(validationErrors);
            }

            // ✅ BUSINESS RULE: ISBN deve essere unico (escludendo se stesso)
            if (await _bookRepository.ExistsByIsbnAsync(book.ISBN, id))
            {
                _logger.LogWarning("Attempt to update book ID {BookId} with duplicate ISBN: {ISBN}", id, book.ISBN);
                return ServiceResult<bool>.ValidationError("Another book with this ISBN already exists.");
            }

            // ✅ BUSINESS RULE: Normalizzazione dei dati
            NormalizeBookData(book);

            _logger.LogInformation("Updating book ID {BookId}: {Title}", id, book.Title);
            var updated = await _bookRepository.UpdateAsync(book);
            
            if (updated)
            {
                _logger.LogInformation("Successfully updated book ID {BookId}", id);
                return ServiceResult<bool>.Success(true);
            }
            else
            {
                _logger.LogWarning("Book update failed for ID {BookId} - no rows affected", id);
                return ServiceResult<bool>.Error("Failed to update book.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating book ID: {BookId}", id);
            return ServiceResult<bool>.Error($"An error occurred while updating the book: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteBookAsync(int id)
    {
        try
        {
            // ✅ BUSINESS RULE: Il libro deve esistere
            var existingBook = await _bookRepository.GetByIdAsync(id);
            if (existingBook == null)
            {
                _logger.LogWarning("Attempt to delete non-existent book with ID: {BookId}", id);
                return ServiceResult<bool>.Error("Book not found.");
            }

            // ✅ BUSINESS RULE: Non è possibile eliminare libri con prestiti attivi
            if (await _bookRepository.HasActiveLoansAsync(id))
            {
                _logger.LogWarning("Attempt to delete book ID {BookId} with active loans", id);
                return ServiceResult<bool>.ValidationError("Cannot delete book with active loans.");
            }

            _logger.LogInformation("Deleting book ID {BookId}: {Title}", id, existingBook.Title);
            var deleted = await _bookRepository.DeleteAsync(id);
            
            if (deleted)
            {
                _logger.LogInformation("Successfully deleted book ID {BookId}", id);
                return ServiceResult<bool>.Success(true);
            }
            else
            {
                _logger.LogWarning("Book deletion failed for ID {BookId} - no rows affected", id);
                return ServiceResult<bool>.Error("Failed to delete book.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting book ID: {BookId}", id);
            return ServiceResult<bool>.Error($"An error occurred while deleting the book: {ex.Message}");
        }
    }

    /// <summary>
    /// ✅ BUSINESS RULES: Validazione centralizzata dei dati del libro
    /// </summary>
    private static List<string> ValidateBookData(Book book)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(book.Title))
        {
            errors.Add("Title is required.");
        }
        else if (book.Title.Trim().Length > 200)
        {
            errors.Add("Title cannot exceed 200 characters.");
        }

        if (string.IsNullOrWhiteSpace(book.Author))
        {
            errors.Add("Author is required.");
        }
        else if (book.Author.Trim().Length > 100)
        {
            errors.Add("Author cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(book.ISBN))
        {
            errors.Add("ISBN is required.");
        }
        else if (!IsValidIsbn(book.ISBN))
        {
            errors.Add("ISBN format is invalid.");
        }

        if (book.PublishedDate > DateTime.Now)
        {
            errors.Add("Published date cannot be in the future.");
        }

        if (book.PublishedDate < new DateTime(1400, 1, 1)) // Prima stampa di Gutenberg
        {
            errors.Add("Published date is too far in the past.");
        }

        if (book.AvailableCopies < 0)
        {
            errors.Add("Available copies cannot be negative.");
        }

        if (book.AvailableCopies > 10000) // Business rule ragionevole per una biblioteca
        {
            errors.Add("Available copies cannot exceed 10,000.");
        }

        if (book.Price < 0)
        {
            errors.Add("Price cannot be negative.");
        }

        if (book.Price > 10000) // Business rule ragionevole
        {
            errors.Add("Price cannot exceed $10,000.");
        }

        if (!string.IsNullOrWhiteSpace(book.Genre) && book.Genre.Length > 50)
        {
            errors.Add("Genre cannot exceed 50 characters.");
        }

        return errors;
    }

    /// <summary>
    /// ✅ BUSINESS RULE: Validazione formato ISBN (semplificata)
    /// </summary>
    private static bool IsValidIsbn(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return false;

        // Rimuovi spazi e trattini
        var cleanIsbn = isbn.Replace("-", "").Replace(" ", "");

        // ISBN-10 o ISBN-13
        return (cleanIsbn.Length == 10 && cleanIsbn.All(c => char.IsDigit(c) || c == 'X')) ||
               (cleanIsbn.Length == 13 && cleanIsbn.All(char.IsDigit));
    }

    /// <summary>
    /// ✅ BUSINESS RULE: Normalizzazione dei dati
    /// </summary>
    private static void NormalizeBookData(Book book)
    {
        book.Title = book.Title.Trim();
        book.Author = book.Author.Trim();
        book.ISBN = book.ISBN.Replace("-", "").Replace(" ", "").ToUpperInvariant();
        book.Genre = book.Genre?.Trim() ?? string.Empty;
    }
}