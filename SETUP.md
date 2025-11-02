# Branch 1-myfirstlibrary - Setup Instructions

Questo branch contiene l'implementazione **SENZA Repository Pattern** per dimostrare i problemi dell'accesso diretto ai dati nei controller.

## 🚀 Avvio Rapido con Docker

### Prerequisiti
- Docker Desktop installato e in esecuzione
- Porte 5000, 5001, 1433, 5432 libere

### 1. Avvia tutti i servizi
```bash
docker-compose up --build -d
```

> **Nota:** Questo progetto usa .NET 9. Usa il file di progetto `LibraryAPI.csproj` direttamente con i comandi dotnet.

### 2. Inizializza i database
Dopo che i container sono avviati, esegui gli script di inizializzazione:

#### SQL Server
```bash
# Aspetta che SQL Server sia pronto e ricrea il database
docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -d master -Q "DROP DATABASE IF EXISTS LibraryDB" -C
docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -d master -Q "CREATE DATABASE LibraryDB" -C
docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -d LibraryDB -i /Scripts/init-sqlserver.sql -C
```

#### PostgreSQL
```bash
# Ricrea il database PostgreSQL (opzionale, se vuoi ripartire da zero)
docker-compose exec postgres psql -U postgres -c "DROP DATABASE IF EXISTS LibraryDB"
docker-compose exec postgres psql -U postgres -c "CREATE DATABASE LibraryDB"
docker-compose exec postgres psql -U postgres -d LibraryDB -f /Scripts/init-postgres.sql
```
> **Nota:** I primi due comandi sono opzionali se vuoi ripartire da zero. Se vedi errori "already exists", è normale.

#### SQLite (viene creato automaticamente al primo utilizzo)

### 3. Testa l'API
L'API sarà disponibile su: http://localhost:5000

Swagger UI: http://localhost:5000

> 💡 **Nota sui Dati**: Ogni database è inizializzato con libri specifici per rendere evidente quale database si sta utilizzando:
> - **SQL Server**: Libri su SQL Server e T-SQL
> - **PostgreSQL**: Libri su PostgreSQL 
> - **SQLite**: Libri su SQLite e database embedded
> - Tutti includono anche alcuni libri generici di programmazione

## 🔧 Cambio Database

Per testare diversi database, modifica il file `appsettings.json`:

### SQL Server
```json
{
  "DatabaseSettings": {
    "Type": "SqlServer"
  }
}
```

### PostgreSQL
```json
{
  "DatabaseSettings": {
    "Type": "PostgreSQL"
  }
}
```

### SQLite
```json
{
  "DatabaseSettings": {
    "Type": "SQLite"
  }
}
```

Dopo aver modificato la configurazione, riavvia il container:
```bash
docker-compose restart library-api
```

## 🔍 Verifica Database

Puoi verificare che i database siano attivi usando i seguenti comandi:

### SQL Server
```bash
docker-compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT name FROM sys.databases" -C
```

### PostgreSQL
```bash
docker-compose exec postgres psql -U postgres -l
```

## 🧪 Test delle API

### Esempi di chiamate API

#### Ottieni tutti i libri
```bash
curl http://localhost:5000/api/books
```

#### Verifica quale database stai usando
```bash
# Cerca libri specifici per SQL Server
curl "http://localhost:5000/api/books/search?title=SQL%20Server"

# Cerca libri specifici per PostgreSQL  
curl "http://localhost:5000/api/books/search?title=PostgreSQL"

# Cerca libri specifici per SQLite
curl "http://localhost:5000/api/books/search?title=SQLite"
```

#### Aggiungi un nuovo libro
```bash
curl -X POST http://localhost:5000/api/books \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Nuovo Libro",
    "author": "Autore Test",
    "isbn": "978-1-234-56789-0",
    "publishedDate": "2024-01-01",
    "genre": "Test",
    "availableCopies": 5,
    "price": 29.99
  }'
```

## ❌ Problemi Evidenziati in Questo Branch

### 1. **Controller Sovraccarico**
Il `BooksController` contiene:
- Logica HTTP
- Logica di business
- Logica di accesso ai dati
- Gestione errori database

### 2. **Accoppiamento Forte**
- Dipendenza diretta da Dapper
- Query SQL embedded nel controller
- Logica specifica per ogni database

### 3. **Difficoltà nei Test**
- Impossibile testare senza database reale
- Non si possono mockare le dipendenze
- Setup complesso per ogni test

### 4. **Codice Duplicato**
- Validazioni ripetute in ogni metodo
- Gestione connessioni duplicata
- Logica di errore ripetitiva

### 5. **Manutenibilità Scarsa**
- Modifiche al database richiedono modifiche al controller
- Aggiungere un nuovo database è complesso
- Business rules sparse nel controller

## 🔄 Confronto con Branch 2

Per vedere come questi problemi vengono risolti, passa al branch `2-mysecondlibrary`:
```bash
git checkout 2-mysecondlibrary
```

## �‍💻 Sviluppo Locale (senza Docker)

Se preferisci eseguire l'applicazione direttamente sul tuo sistema:

### Prerequisiti
- .NET 9 SDK installato
- Database locale (SQL Server, PostgreSQL o SQLite)

### Comandi di sviluppo
```bash
# Restore delle dipendenze
dotnet restore LibraryAPI.csproj

# Build del progetto
dotnet build LibraryAPI.csproj

# Esecuzione in modalità development
dotnet run --project LibraryAPI.csproj

# Esecuzione con hot reload
dotnet watch --project LibraryAPI.csproj
```

### Note per lo sviluppo
Il progetto usa .NET 9 e può essere compilato direttamente usando `LibraryAPI.csproj`.

## �🛑 Pulizia

Per fermare e rimuovere tutti i container:
```bash
docker-compose down -v
```