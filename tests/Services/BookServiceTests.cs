using FluentAssertions;
using LibraryAPI.Models;
using LibraryAPI.Repositories.Interfaces;
using LibraryAPI.Services;
using LibraryAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace LibraryAPI.Tests.Services;

/// <summary>
/// ✅ SOLUZIONE: Unit tests completi per BookService
/// Dimostra come testare facilmente la business logic quando è separata dal data access
/// Utilizza mock per isolare le dipendenze e testare solo la logica del service
/// </summary>
public class BookServiceTests
{
    private readonly IBookRepository _mockRepository;
    private readonly ILogger<BookService> _mockLogger;
    private readonly BookService _bookService;

    public BookServiceTests()
    {
        _mockRepository = Substitute.For<IBookRepository>();
        _mockLogger = Substitute.For<ILogger<BookService>>();
        _bookService = new BookService(_mockRepository, _mockLogger);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var action = () => new BookService(null!, _mockLogger);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("bookRepository");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var action = () => new BookService(_mockRepository, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    #endregion

    #region GetAllBooksAsync Tests

    [Fact]
    public async Task GetAllBooksAsync_ReturnsAllBooks()
    {
        // Arrange
        var expectedBooks = new List<Book>
        {
            CreateTestBook(1, "Book 1", "Author 1"),
            CreateTestBook(2, "Book 2", "Author 2")
        };
        _mockRepository.GetAllAsync().Returns(expectedBooks);

        // Act
        var result = await _bookService.GetAllBooksAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedBooks);
        await _mockRepository.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task GetAllBooksAsync_RepositoryThrows_PropagatesException()
    {
        // Arrange
        _mockRepository.GetAllAsync().Returns(Task.FromException<IEnumerable<Book>>(new InvalidOperationException("Database error")));

        // Act & Assert
        var action = async () => await _bookService.GetAllBooksAsync();
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    #endregion

    #region GetBookByIdAsync Tests

    [Fact]
    public async Task GetBookByIdAsync_ValidId_ReturnsBook()
    {
        // Arrange
        var bookId = 1;
        var expectedBook = CreateTestBook(bookId, "Test Book", "Test Author");
        _mockRepository.GetByIdAsync(bookId).Returns(expectedBook);

        // Act
        var result = await _bookService.GetBookByIdAsync(bookId);

        // Assert
        result.Should().BeEquivalentTo(expectedBook);
        await _mockRepository.Received(1).GetByIdAsync(bookId);
    }

    [Fact]
    public async Task GetBookByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var invalidId = -1;

        // Act
        var result = await _bookService.GetBookByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
        await _mockRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task GetBookByIdAsync_BookNotFound_ReturnsNull()
    {
        // Arrange
        var bookId = 999;
        _mockRepository.GetByIdAsync(bookId).Returns((Book?)null);

        // Act
        var result = await _bookService.GetBookByIdAsync(bookId);

        // Assert
        result.Should().BeNull();
        await _mockRepository.Received(1).GetByIdAsync(bookId);
    }

    #endregion

    #region SearchBooksAsync Tests

    [Fact]
    public async Task SearchBooksAsync_ValidCriteria_ReturnsSuccess()
    {
        // Arrange
        var title = "Test";
        var author = "Author";
        var expectedBooks = new List<Book> { CreateTestBook(1, "Test Book", "Test Author") };
        _mockRepository.SearchAsync(title, author).Returns(expectedBooks);

        // Act
        var result = await _bookService.SearchBooksAsync(title, author);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedBooks);
        await _mockRepository.Received(1).SearchAsync(title, author);
    }

    [Fact]
    public async Task SearchBooksAsync_NoCriteria_ReturnsValidationError()
    {
        // Arrange & Act
        var result = await _bookService.SearchBooksAsync(null, null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Either title or author must be provided for search.");
    }

    [Theory]
    [InlineData("a", null)] // Title too short
    [InlineData(null, "b")] // Author too short
    public async Task SearchBooksAsync_TooShortCriteria_ReturnsValidationError(string? title, string? author)
    {
        // Act
        var result = await _bookService.SearchBooksAsync(title, author);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().NotBeEmpty();
    }

    #endregion

    #region CreateBookAsync Tests

    [Fact]
    public async Task CreateBookAsync_ValidBook_ReturnsSuccess()
    {
        // Arrange
        var book = CreateTestBook(0, "New Book", "New Author", "1234567890");
        var createdBook = CreateTestBook(1, book.Title, book.Author, book.ISBN);
        
        _mockRepository.ExistsByIsbnAsync(book.ISBN).Returns(false);
        _mockRepository.CreateAsync(Arg.Any<Book>()).Returns(createdBook);

        // Act
        var result = await _bookService.CreateBookAsync(book);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(createdBook);
        await _mockRepository.Received(1).ExistsByIsbnAsync(book.ISBN);
        await _mockRepository.Received(1).CreateAsync(Arg.Any<Book>());
    }

    [Fact]
    public async Task CreateBookAsync_DuplicateIsbn_ReturnsValidationError()
    {
        // Arrange
        var book = CreateTestBook(0, "New Book", "New Author", "1234567890");
        _mockRepository.ExistsByIsbnAsync(book.ISBN).Returns(true);

        // Act
        var result = await _bookService.CreateBookAsync(book);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain("A book with this ISBN already exists.");
        await _mockRepository.DidNotReceive().CreateAsync(Arg.Any<Book>());
    }

    [Theory]
    [InlineData("", "Author", "1234567890")] // Empty title
    [InlineData("Title", "", "1234567890")] // Empty author
    [InlineData("Title", "Author", "")] // Empty ISBN
    public async Task CreateBookAsync_InvalidData_ReturnsValidationError(string title, string author, string isbn)
    {
        // Arrange
        var book = CreateTestBook(0, title, author, isbn);

        // Act
        var result = await _bookService.CreateBookAsync(book);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateBookAsync_NegativeCopies_ReturnsValidationError()
    {
        // Arrange
        var book = CreateTestBook(0, "Title", "Author", "1234567890");
        book.AvailableCopies = -1;

        // Act
        var result = await _bookService.CreateBookAsync(book);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Available copies cannot be negative.");
    }

    [Fact]
    public async Task CreateBookAsync_FuturePublishedDate_ReturnsValidationError()
    {
        // Arrange
        var book = CreateTestBook(0, "Title", "Author", "1234567890");
        book.PublishedDate = DateTime.Now.AddDays(1);

        // Act
        var result = await _bookService.CreateBookAsync(book);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Published date cannot be in the future.");
    }

    #endregion

    #region UpdateBookAsync Tests

    [Fact]
    public async Task UpdateBookAsync_ValidUpdate_ReturnsSuccess()
    {
        // Arrange
        var bookId = 1;
        var book = CreateTestBook(bookId, "Updated Title", "Updated Author", "1234567890");
        var existingBook = CreateTestBook(bookId, "Original Title", "Original Author", "0987654321");

        _mockRepository.GetByIdAsync(bookId).Returns(existingBook);
        _mockRepository.ExistsByIsbnAsync(book.ISBN, bookId).Returns(false);
        _mockRepository.UpdateAsync(Arg.Any<Book>()).Returns(true);

        // Act
        var result = await _bookService.UpdateBookAsync(bookId, book);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        await _mockRepository.Received(1).UpdateAsync(Arg.Any<Book>());
    }

    [Fact]
    public async Task UpdateBookAsync_IdMismatch_ReturnsValidationError()
    {
        // Arrange
        var urlId = 1;
        var book = CreateTestBook(2, "Title", "Author", "1234567890"); // Different ID

        // Act
        var result = await _bookService.UpdateBookAsync(urlId, book);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain("ID mismatch between URL and book data.");
    }

    [Fact]
    public async Task UpdateBookAsync_BookNotFound_ReturnsError()
    {
        // Arrange
        var bookId = 999;
        var book = CreateTestBook(bookId, "Title", "Author", "1234567890");
        _mockRepository.GetByIdAsync(bookId).Returns((Book?)null);

        // Act
        var result = await _bookService.UpdateBookAsync(bookId, book);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Book not found.");
    }

    [Fact]
    public async Task UpdateBookAsync_DuplicateIsbn_ReturnsValidationError()
    {
        // Arrange
        var bookId = 1;
        var book = CreateTestBook(bookId, "Title", "Author", "1234567890");
        var existingBook = CreateTestBook(bookId, "Original Title", "Original Author", "0987654321");

        _mockRepository.GetByIdAsync(bookId).Returns(existingBook);
        _mockRepository.ExistsByIsbnAsync(book.ISBN, bookId).Returns(true); // Duplicate ISBN

        // Act
        var result = await _bookService.UpdateBookAsync(bookId, book);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Another book with this ISBN already exists.");
    }

    #endregion

    #region DeleteBookAsync Tests

    [Fact]
    public async Task DeleteBookAsync_ValidDeletion_ReturnsSuccess()
    {
        // Arrange
        var bookId = 1;
        var existingBook = CreateTestBook(bookId, "Title", "Author", "1234567890");

        _mockRepository.GetByIdAsync(bookId).Returns(existingBook);
        _mockRepository.HasActiveLoansAsync(bookId).Returns(false);
        _mockRepository.DeleteAsync(bookId).Returns(true);

        // Act
        var result = await _bookService.DeleteBookAsync(bookId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        await _mockRepository.Received(1).DeleteAsync(bookId);
    }

    [Fact]
    public async Task DeleteBookAsync_BookNotFound_ReturnsError()
    {
        // Arrange
        var bookId = 999;
        _mockRepository.GetByIdAsync(bookId).Returns((Book?)null);

        // Act
        var result = await _bookService.DeleteBookAsync(bookId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Book not found.");
        await _mockRepository.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task DeleteBookAsync_HasActiveLoans_ReturnsValidationError()
    {
        // Arrange
        var bookId = 1;
        var existingBook = CreateTestBook(bookId, "Title", "Author", "1234567890");

        _mockRepository.GetByIdAsync(bookId).Returns(existingBook);
        _mockRepository.HasActiveLoansAsync(bookId).Returns(true); // Has active loans

        // Act
        var result = await _bookService.DeleteBookAsync(bookId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Cannot delete book with active loans.");
        await _mockRepository.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }

    #endregion

    #region Helper Methods

    private static Book CreateTestBook(int id, string title, string author, string isbn = "1234567890")
    {
        return new Book
        {
            Id = id,
            Title = title,
            Author = author,
            ISBN = isbn,
            PublishedDate = DateTime.Now.AddYears(-1),
            Genre = "Test Genre",
            AvailableCopies = 5,
            Price = 19.99m
        };
    }

    #endregion
}

/// <summary>
/// ✅ VANTAGGI DEI TEST UNITARI CON REPOSITORY PATTERN:
/// 
/// 1. ISOLAMENTO COMPLETO
///    - Test veloci senza dipendenze esterne
///    - Nessun database reale coinvolto
/// 
/// 2. CONTROLLO TOTALE
///    - Mock permettono di simulare qualsiasi scenario
///    - Test deterministici e ripetibili
/// 
/// 3. COPERTURA COMPLETA
///    - Test di tutti i path del codice
///    - Validazione di business rules
///    - Gestione errori
/// 
/// 4. FEEDBACK RAPIDO
///    - Esecuzione veloce
///    - Identificazione rapida di regressioni
/// 
/// 5. DOCUMENTAZIONE VIVENTE
///    - I test documentano il comportamento atteso
///    - Esempi di utilizzo del codice
/// </summary>