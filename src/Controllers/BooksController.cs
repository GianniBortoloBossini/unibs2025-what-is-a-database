using LibraryAPI.Models;
using LibraryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers;

/// <summary>
/// ✅ SOLUZIONE: Controller refactorizzato che usa il Repository Pattern
/// - Dipende solo dall'interfaccia IBookService
/// - Non conosce dettagli del database
/// - Facilmente testabile
/// - Separazione delle responsabilità: solo gestione HTTP
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IBookService bookService, ILogger<BooksController> logger)
    {
        _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// ✅ SOLUZIONE: Controller pulito che delega al service layer
    /// - Nessuna logica di business nel controller
    /// - Nessun accesso diretto al database
    /// - Facile da testare
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks()
    {
        try
        {
            _logger.LogInformation("Getting all books");
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all books");
            return StatusCode(500, "An internal server error occurred");
        }
    }

    /// <summary>
    /// ✅ SOLUZIONE: Endpoint semplificato che delega al service
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        try
        {
            _logger.LogInformation("Getting book with ID: {BookId}", id);
            var book = await _bookService.GetBookByIdAsync(id);
            
            if (book == null)
                return NotFound();
                
            return Ok(book);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting book with ID: {BookId}", id);
            return StatusCode(500, "An internal server error occurred");
        }
    }

    /// <summary>
    /// ✅ SOLUZIONE: Ricerca che delega validazione e logica al service layer
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Book>>> SearchBooks([FromQuery] string? title, [FromQuery] string? author)
    {
        try
        {
            _logger.LogInformation("Searching books with title: '{Title}', author: '{Author}'", 
                title ?? "N/A", author ?? "N/A");
            
            var result = await _bookService.SearchBooksAsync(title, author);
            
            if (!result.IsSuccess)
            {
                if (result.ValidationErrors.Any())
                {
                    return BadRequest(new { 
                        Message = result.ErrorMessage,
                        ValidationErrors = result.ValidationErrors 
                    });
                }
                return StatusCode(500, new { Message = result.ErrorMessage });
            }
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching books");
            return StatusCode(500, "An internal server error occurred");
        }
    }

    /// <summary>
    /// ✅ SOLUZIONE: Creazione che delega validazione e business logic al service
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Book>> CreateBook(Book book)
    {
        try
        {
            _logger.LogInformation("Creating new book: {Title} by {Author}", book.Title, book.Author);
            
            var result = await _bookService.CreateBookAsync(book);
            
            if (!result.IsSuccess)
            {
                if (result.ValidationErrors.Any())
                {
                    return BadRequest(new { 
                        Message = result.ErrorMessage,
                        ValidationErrors = result.ValidationErrors 
                    });
                }
                return StatusCode(500, new { Message = result.ErrorMessage });
            }
            
            return CreatedAtAction(nameof(GetBook), new { id = result.Data!.Id }, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating book");
            return StatusCode(500, "An internal server error occurred");
        }
    }

    /// <summary>
    /// ✅ SOLUZIONE: Aggiornamento che delega al service layer
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(int id, Book book)
    {
        try
        {
            _logger.LogInformation("Updating book with ID: {BookId}", id);
            
            var result = await _bookService.UpdateBookAsync(id, book);
            
            if (!result.IsSuccess)
            {
                if (result.ErrorMessage == "Book not found.")
                {
                    return NotFound();
                }
                
                if (result.ValidationErrors.Any())
                {
                    return BadRequest(new { 
                        Message = result.ErrorMessage,
                        ValidationErrors = result.ValidationErrors 
                    });
                }
                
                return StatusCode(500, new { Message = result.ErrorMessage });
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating book with ID: {BookId}", id);
            return StatusCode(500, "An internal server error occurred");
        }
    }

    /// <summary>
    /// ✅ SOLUZIONE: Eliminazione che delega business logic al service
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        try
        {
            _logger.LogInformation("Deleting book with ID: {BookId}", id);
            
            var result = await _bookService.DeleteBookAsync(id);
            
            if (!result.IsSuccess)
            {
                if (result.ErrorMessage == "Book not found.")
                {
                    return NotFound();
                }
                
                if (result.ValidationErrors.Any())
                {
                    return BadRequest(new { 
                        Message = result.ErrorMessage,
                        ValidationErrors = result.ValidationErrors 
                    });
                }
                
                return StatusCode(500, new { Message = result.ErrorMessage });
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting book with ID: {BookId}", id);
            return StatusCode(500, "An internal server error occurred");
        }
    }
}

/*
 * ✅ VANTAGGI DEL CONTROLLER REFACTORIZZATO:
 * 
 * 1. SINGLE RESPONSIBILITY PRINCIPLE
 *    - Il controller gestisce SOLO le HTTP requests
 *    - La logica di business è nel service layer
 * 
 * 2. DISACCOPPIAMENTO
 *    - Nessuna dipendenza diretta dal database
 *    - Facilmente sostituibile e testabile
 * 
 * 3. ELIMINAZIONE CODICE DUPLICATO
 *    - Logica centralizzata nel service
 *    - Validazioni uniche e riutilizzabili
 * 
 * 4. TESTABILITÀ
 *    - Facile da mockare le dipendenze
 *    - Test unitari isolati e veloci
 * 
 * 5. MANUTENIBILITÀ
 *    - Codice pulito e leggibile
 *    - Separazione chiara delle responsabilità
 * 
 * 6. SCALABILITÀ
 *    - Facile aggiungere nuovi database
 *    - Service layer riutilizzabile in altri contesti
 */