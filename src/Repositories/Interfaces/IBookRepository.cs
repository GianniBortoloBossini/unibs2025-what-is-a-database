using LibraryAPI.Models;

namespace LibraryAPI.Repositories.Interfaces;

/// <summary>
/// ✅ SOLUZIONE: Contratto per l'accesso ai dati dei libri
/// Definisce tutte le operazioni CRUD necessarie senza esporre dettagli implementativi
/// Facilita la testabilità e permette multiple implementazioni
/// </summary>
public interface IBookRepository
{
    /// <summary>
    /// Recupera tutti i libri dal database
    /// </summary>
    /// <returns>Lista di tutti i libri</returns>
    Task<IEnumerable<Book>> GetAllAsync();
    
    /// <summary>
    /// Recupera un libro specifico per ID
    /// </summary>
    /// <param name="id">ID del libro da recuperare</param>
    /// <returns>Il libro se trovato, null altrimenti</returns>
    Task<Book?> GetByIdAsync(int id);
    
    /// <summary>
    /// Cerca libri per titolo e/o autore
    /// </summary>
    /// <param name="title">Titolo da cercare (opzionale)</param>
    /// <param name="author">Autore da cercare (opzionale)</param>
    /// <returns>Lista dei libri che corrispondono ai criteri di ricerca</returns>
    Task<IEnumerable<Book>> SearchAsync(string? title, string? author);
    
    /// <summary>
    /// Verifica se esiste già un libro con l'ISBN specificato
    /// </summary>
    /// <param name="isbn">ISBN da verificare</param>
    /// <returns>True se esiste, false altrimenti</returns>
    Task<bool> ExistsByIsbnAsync(string isbn);
    
    /// <summary>
    /// Verifica se esiste già un libro con l'ISBN specificato, escludendo un ID specifico
    /// </summary>
    /// <param name="isbn">ISBN da verificare</param>
    /// <param name="excludeId">ID da escludere dalla ricerca</param>
    /// <returns>True se esiste un altro libro con lo stesso ISBN, false altrimenti</returns>
    Task<bool> ExistsByIsbnAsync(string isbn, int excludeId);
    
    /// <summary>
    /// Aggiunge un nuovo libro al database
    /// </summary>
    /// <param name="book">Il libro da aggiungere</param>
    /// <returns>Il libro creato con l'ID assegnato</returns>
    Task<Book> CreateAsync(Book book);
    
    /// <summary>
    /// Aggiorna un libro esistente
    /// </summary>
    /// <param name="book">Il libro con i dati aggiornati</param>
    /// <returns>True se l'aggiornamento è avvenuto con successo, false se il libro non esiste</returns>
    Task<bool> UpdateAsync(Book book);
    
    /// <summary>
    /// Elimina un libro dal database
    /// </summary>
    /// <param name="id">ID del libro da eliminare</param>
    /// <returns>True se l'eliminazione è avvenuta con successo, false se il libro non esiste</returns>
    Task<bool> DeleteAsync(int id);
    
    /// <summary>
    /// Verifica se un libro ha prestiti attivi
    /// </summary>
    /// <param name="bookId">ID del libro da controllare</param>
    /// <returns>True se ha prestiti attivi, false altrimenti</returns>
    Task<bool> HasActiveLoansAsync(int bookId);
}