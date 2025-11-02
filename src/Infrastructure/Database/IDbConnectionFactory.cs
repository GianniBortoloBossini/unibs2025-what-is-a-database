using System.Data;

namespace LibraryAPI.Infrastructure.Database;

/// <summary>
/// ✅ SOLUZIONE: Factory pattern per la creazione delle connessioni al database
/// Astrae la creazione delle connessioni e permette di cambiarle facilmente
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Crea una connessione al database configurato
    /// </summary>
    /// <returns>Una connessione aperta al database</returns>
    IDbConnection CreateConnection();
    
    /// <summary>
    /// Restituisce il tipo di database attualmente configurato
    /// </summary>
    string DatabaseType { get; }
}