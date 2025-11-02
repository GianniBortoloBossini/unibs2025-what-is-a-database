using LibraryAPI.Models;

namespace LibraryAPI.Services.Interfaces;

/// <summary>
/// ✅ SOLUZIONE: Contratto per il service layer
/// Definisce la logica di business per la gestione dei libri
/// Separato dal controller e dal repository per massimizzare la testabilità
/// </summary>
public interface IBookService
{
    /// <summary>
    /// Recupera tutti i libri
    /// </summary>
    /// <returns>Lista di tutti i libri</returns>
    Task<IEnumerable<Book>> GetAllBooksAsync();
    
    /// <summary>
    /// Recupera un libro per ID
    /// </summary>
    /// <param name="id">ID del libro da recuperare</param>
    /// <returns>Il libro se trovato, null altrimenti</returns>
    Task<Book?> GetBookByIdAsync(int id);
    
    /// <summary>
    /// Cerca libri per titolo e/o autore con validazione dei parametri
    /// </summary>
    /// <param name="title">Titolo da cercare (opzionale)</param>
    /// <param name="author">Autore da cercare (opzionale)</param>
    /// <returns>Risultato dell'operazione con lista dei libri o errore di validazione</returns>
    Task<ServiceResult<IEnumerable<Book>>> SearchBooksAsync(string? title, string? author);
    
    /// <summary>
    /// Crea un nuovo libro con validazione completa delle business rules
    /// </summary>
    /// <param name="book">Il libro da creare</param>
    /// <returns>Risultato dell'operazione con il libro creato o errori di validazione</returns>
    Task<ServiceResult<Book>> CreateBookAsync(Book book);
    
    /// <summary>
    /// Aggiorna un libro esistente con validazione completa
    /// </summary>
    /// <param name="id">ID del libro da aggiornare</param>
    /// <param name="book">I dati aggiornati del libro</param>
    /// <returns>Risultato dell'operazione</returns>
    Task<ServiceResult<bool>> UpdateBookAsync(int id, Book book);
    
    /// <summary>
    /// Elimina un libro dopo aver verificato le business rules
    /// </summary>
    /// <param name="id">ID del libro da eliminare</param>
    /// <returns>Risultato dell'operazione</returns>
    Task<ServiceResult<bool>> DeleteBookAsync(int id);
}

/// <summary>
/// ✅ SOLUZIONE: Wrapper per i risultati delle operazioni del service layer
/// Incapsula il risultato dell'operazione e eventuali errori di validazione o business logic
/// </summary>
public class ServiceResult<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public List<string> ValidationErrors { get; }

    private ServiceResult(bool isSuccess, T? data, string? errorMessage, List<string>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        ValidationErrors = validationErrors ?? new List<string>();
    }

    /// <summary>
    /// Crea un risultato di successo
    /// </summary>
    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>(true, data, null);
    }

    /// <summary>
    /// Crea un risultato di errore con messaggio
    /// </summary>
    public static ServiceResult<T> Error(string errorMessage)
    {
        return new ServiceResult<T>(false, default, errorMessage);
    }

    /// <summary>
    /// Crea un risultato di errore con errori di validazione
    /// </summary>
    public static ServiceResult<T> ValidationError(List<string> validationErrors)
    {
        return new ServiceResult<T>(false, default, "Validation failed", validationErrors);
    }

    /// <summary>
    /// Crea un risultato di errore con singolo errore di validazione
    /// </summary>
    public static ServiceResult<T> ValidationError(string validationError)
    {
        return ValidationError(new List<string> { validationError });
    }
}