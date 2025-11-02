# 🏗️ Architettura Repository Pattern + Application Service

## 📋 Panoramica dell'Implementazione

Questa implementazione dimostra l'evoluzione da un'architettura monolitica con accesso diretto al database (Branch 1) a un'architettura pulita con separazione delle responsabilità (Branch 2).

## 🎯 Problemi Risolti

### ❌ Problemi del Branch 1 (senza pattern)
1. **Violazione Single Responsibility Principle**
   - Controller gestiva HTTP requests E accesso al database
2. **Accoppiamento Forte**
   - Dipendenza diretta da Dapper e provider specifici
3. **Codice Duplicato**
   - Logica di connessione ripetuta in ogni metodo
4. **Difficoltà nei Test**
   - Impossibile testare senza database reale
5. **Manutenibilità Scarsa**
   - Query SQL sparse nel controller
6. **Scalabilità Limitata**
   - Aggiungere nuovo database richiedeva modifiche al controller

### ✅ Soluzioni del Branch 2 (con Repository Pattern)
1. **Separazione delle Responsabilità**
   - Controller: solo gestione HTTP
   - Service: logica di business
   - Repository: accesso ai dati
2. **Disaccoppiamento**
   - Dipendenze tramite interfacce
   - Inversione delle dipendenze
3. **Eliminazione Duplicazione**
   - Logica centralizzata nei servizi appropriati
4. **Testabilità**
   - Mock/Stub delle dipendenze
   - Test unitari isolati e veloci
5. **Manutenibilità**
   - Codice pulito e organizzato
6. **Scalabilità**
   - Facile aggiungere nuovi database
   - Riutilizzo dei servizi

## 🏛️ Architettura a Livelli

```
┌─────────────────────────────────────────────────┐
│                PRESENTATION LAYER               │
│  ┌─────────────────┐                           │
│  │  Controllers/   │ ← HTTP Endpoints          │
│  │ BooksController │                           │
│  └─────────────────┘                           │
└─────────────────┬───────────────────────────────┘
                  │ IBookService
┌─────────────────▼───────────────────────────────┐
│               APPLICATION LAYER                 │
│  ┌─────────────────┐  ┌─────────────────┐      │
│  │   Services/     │  │ Services/       │      │
│  │   IBookService  │  │ BookService     │      │
│  │   (Interface)   │  │ (Implementation)│      │
│  └─────────────────┘  └─────────────────┘      │
└─────────────────┬───────────────────────────────┘
                  │ IBookRepository
┌─────────────────▼───────────────────────────────┐
│                DATA ACCESS LAYER                │
│  ┌─────────────────┐  ┌─────────────────┐      │
│  │ Repositories/   │  │ Repositories/   │      │
│  │ IBookRepository │  │ Implementations │      │
│  │ (Interface)     │  │ • SqlServer     │      │
│  └─────────────────┘  │ • PostgreSQL    │      │
│                       │ • SQLite        │      │
│                       └─────────────────┘      │
└─────────────────┬───────────────────────────────┘
                  │ IDbConnectionFactory
┌─────────────────▼───────────────────────────────┐
│              INFRASTRUCTURE LAYER               │
│  ┌─────────────────┐  ┌─────────────────┐      │
│  │ Infrastructure/ │  │ Infrastructure/ │      │
│  │ IDbConnection   │  │ DbConnection    │      │
│  │ Factory         │  │ Factory         │      │
│  │ (Interface)     │  │ (Implementation)│      │
│  └─────────────────┘  └─────────────────┘      │
└─────────────────┬───────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────┐
│                   DATABASE                      │
│        SQL Server / PostgreSQL / SQLite        │
└─────────────────────────────────────────────────┘
```

## 📁 Struttura delle Cartelle

```
src/
├── Controllers/
│   └── BooksController.cs          ← HTTP endpoints puliti
├── Services/
│   ├── Interfaces/
│   │   └── IBookService.cs         ← Contratto business logic
│   └── BookService.cs              ← Implementazione business logic
├── Repositories/
│   ├── Interfaces/
│   │   └── IBookRepository.cs      ← Contratto accesso dati
│   └── Implementations/
│       ├── SqlServerBookRepository.cs
│       ├── PostgresBookRepository.cs
│       └── SqliteBookRepository.cs ← Implementazioni specifiche DB
├── Infrastructure/
│   └── Database/
│       ├── IDbConnectionFactory.cs ← Contratto connessioni
│       └── DbConnectionFactory.cs  ← Factory connessioni
└── Models/
    └── Book.cs                     ← Entità dominio

tests/
└── Services/
    └── BookServiceTests.cs         ← Test unitari completi
```

## 🔧 Componenti Chiave

### 1. **IBookService & BookService**
- **Responsabilità**: Logica di business e validazioni
- **Vantaggi**: 
  - Business rules centralizzate
  - Facilmente testabile
  - Riutilizzabile in diversi contesti

### 2. **IBookRepository & Implementazioni**
- **Responsabilità**: Accesso ai dati specifico per database
- **Vantaggi**:
  - Astrazione del database
  - Supporto multi-database
  - Sostituibilità delle implementazioni

### 3. **IDbConnectionFactory & DbConnectionFactory**
- **Responsabilità**: Gestione delle connessioni database
- **Vantaggi**:
  - Centralizzazione logica connessione
  - Configurazione dinamica del database
  - Facilmente mockabile per i test

### 4. **ServiceResult<T>**
- **Responsabilità**: Wrapper per risultati operazioni
- **Vantaggi**:
  - Gestione uniforme di successi/errori
  - Trasporto di errori di validazione
  - API pulita e consistente

## 🧪 Strategia di Testing

### Test Unitari (25 test implementati)
```csharp
// Esempio di test con mock
[Fact]
public async Task CreateBookAsync_ValidBook_ReturnsSuccess()
{
    // Arrange
    var book = CreateTestBook(0, "New Book", "New Author");
    _mockRepository.ExistsByIsbnAsync(book.ISBN).Returns(false);
    _mockRepository.CreateAsync(Arg.Any<Book>()).Returns(createdBook);

    // Act
    var result = await _bookService.CreateBookAsync(book);

    // Assert
    result.IsSuccess.Should().BeTrue();
    await _mockRepository.Received(1).CreateAsync(Arg.Any<Book>());
}
```

### Vantaggi dei Test
- **Isolamento**: Test senza dipendenze esterne
- **Velocità**: Esecuzione rapida
- **Controllo**: Mock permettono ogni scenario
- **Copertura**: Test di tutti i path del codice
- **Documentazione**: I test documentano il comportamento

## 🔄 Dependency Injection

```csharp
// Registrazione servizi in Program.cs
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

builder.Services.AddScoped<IBookRepository>(serviceProvider =>
{
    var connectionFactory = serviceProvider.GetRequiredService<IDbConnectionFactory>();
    return connectionFactory.DatabaseType switch
    {
        "SqlServer" => new SqlServerBookRepository(connectionFactory),
        "PostgreSQL" => new PostgresBookRepository(connectionFactory),
        "SQLite" => new SqliteBookRepository(connectionFactory),
        _ => throw new InvalidOperationException($"Unsupported database type")
    };
});

builder.Services.AddScoped<IBookService, BookService>();
```

## 📈 Vantaggi Misurabili

### 1. **Testabilità**
- ❌ Branch 1: 0 test possibili senza database
- ✅ Branch 2: 25 test unitari completi

### 2. **Accoppiamento**
- ❌ Branch 1: Controller dipende da 4+ classi concrete
- ✅ Branch 2: Controller dipende da 1 interfaccia

### 3. **Responsabilità**
- ❌ Branch 1: Controller ha 6+ responsabilità
- ✅ Branch 2: Ogni classe ha 1 responsabilità

### 4. **Manutenibilità**
- ❌ Branch 1: Aggiungere DB = modificare controller
- ✅ Branch 2: Aggiungere DB = nuova implementazione repository

## 🚀 Scalabilità Futura

### Facile aggiungere:
1. **Nuovo Database** (MongoDB, Redis, etc.)
   ```csharp
   public class MongoBookRepository : IBookRepository
   {
       // Implementazione specifica MongoDB
   }
   ```

2. **Nuovi Servizi** (prestiti, utenti, etc.)
   ```csharp
   public interface ILoanService
   {
       Task<ServiceResult<Loan>> CreateLoanAsync(int bookId, int userId);
   }
   ```

3. **Caching Layer**
   ```csharp
   public class CachedBookRepository : IBookRepository
   {
       private readonly IBookRepository _repository;
       private readonly IMemoryCache _cache;
       // Implementazione con cache
   }
   ```

4. **Validazioni Avanzate**
   ```csharp
   public class BookValidationService : IBookValidationService
   {
       // Validazioni complesse estratte in servizio dedicato
   }
   ```

## 🎓 Concetti Didattici Dimostrati

1. **SOLID Principles**
   - Single Responsibility Principle
   - Open/Closed Principle  
   - Dependency Inversion Principle

2. **Design Patterns**
   - Repository Pattern
   - Factory Pattern
   - Dependency Injection

3. **Clean Architecture**
   - Separazione dei layer
   - Flusso delle dipendenze
   - Testabilità

4. **Best Practices**
   - Unit Testing
   - Error Handling
   - Logging
   - Validation

Questa architettura fornisce una base solida per applicazioni scalabili e mantenibili, dimostrando chiaramente i vantaggi del Repository Pattern rispetto all'accesso diretto ai dati.