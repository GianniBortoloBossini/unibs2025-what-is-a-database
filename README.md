# What is a Database? - Repository Pattern Demo

Questo repository è stato creato per dimostrare agli studenti universitari l'utilità e l'importanza del **Repository Pattern** nello sviluppo di applicazioni moderne.

## 🎯 Obiettivi del Progetto

Attraverso due implementazioni diverse della stessa applicazione, gli studenti potranno comprendere:

- Le problematiche dell'accesso diretto ai dati nei controller
- I vantaggi dell'astrazione del livello di accesso ai dati
- Come il Repository Pattern facilita i test unitari
- Come cambiare facilmente l'implementazione del database
- Le best practices nell'architettura delle applicazioni

## 📚 Scenario: Gestione Biblioteca

L'applicazione simula un semplice sistema di gestione biblioteca con le seguenti funzionalità:
- Visualizzazione elenco libri
- Ricerca libri per titolo/autore
- Aggiunta di nuovi libri
- Aggiornamento informazioni libro
- Eliminazione libro

## 🌳 Struttura dei Branch

### 📂 Branch: `1-myfirstlibrary`

**Implementazione SENZA Repository Pattern**

```
❌ Problemi evidenziati:
- Query SQL scritte direttamente nei controller
- Accoppiamento forte con il database specifico
- Difficoltà nei test unitari
- Codice ripetitivo e poco mantenibile
- Impossibilità di cambiare facilmente database
```

**Caratteristiche:**
- Controller che accedono direttamente al database
- Query SQL embedded nel codice business
- Dipendenze hard-coded
- Test difficili da scrivere e mantenere

### 📂 Branch: `2-mysecondlibrary`

**Implementazione CON Repository Pattern + Application Service**

```
✅ Vantaggi dimostrati:
- Separazione delle responsabilità
- Astrazione del livello di accesso ai dati
- Facilità di testing con mock/stub
- Possibilità di cambiare database senza modificare business logic
- Codice più pulito e mantenibile
```

**Caratteristiche:**
- **Repository Interface**: Contratto per l'accesso ai dati
- **Repository Implementations**: 
  - SQL Server Implementation
  - SQLite Implementation
  - Postgres Implementation
  - (Opzionale) In-Memory Implementation per test
- **Application Service**: Logica di business
- **Dependency Injection**: Configurazione flessibile
- **Unit Tests**: Test completi e isolati

## 🏗️ Architettura del Progetto (Branch 2)

```
┌─────────────────┐
│   Controllers   │  ← API Endpoints
└─────────┬───────┘
          │
┌─────────▼───────┐
│ Application     │  ← Business Logic
│ Services        │
└─────────┬───────┘
          │
┌─────────▼───────┐
│ IRepository     │  ← Abstract Interface
│ Interface       │
└─────────┬───────┘
          │
    ┌─────┴─────┐─────────┐
    │           │         │
┌───▼──┐   ┌───▼──┐   ┌───▼──┐
│SQL   │   │SQLite│   │ Post |  ← Concrete Implementations
│Server│   │ Impl │   │ gres |
│ Impl │   │      │   | Impl |
└──────┘   └──────┘   └──────┘
```

## 🚀 Come Utilizzare il Repository

### 1. Clona il repository
```bash
git clone https://github.com/GianniBortoloBossini/unibs2025-what-is-a-database.git
cd unibs2025-what-is-a-database
```

### 2. Esplora il primo approccio (senza pattern)
```bash
git checkout 1-myfirstlibrary
# Esplora il codice e nota i problemi
```

### 3. Esplora il secondo approccio (con Repository Pattern)
```bash
git checkout 2-mysecondlibrary
# Confronta con il primo approccio
```

## 🔧 Tecnologie Utilizzate

- **Framework**: ASP.NET Core Web API
- **Database**: SQL Server / Postgres / SQLite
- **ORM**: Dapper
- **Testing**: xUnit, NSubstitute
- **Dependency Injection**: Built-in ASP.NET Core DI

## 📖 Concetti Didattici Illustrati

### 1. **Separation of Concerns**
Come separare la logica di business dall'accesso ai dati

### 2. **Dependency Inversion Principle**
Dipendere dalle astrazioni, non dalle implementazioni concrete

### 3. **Testability**
Come rendere il codice facilmente testabile

### 4. **Flexibility**
Come supportare multiple implementazioni dello stesso contratto

### 5. **Maintainability**
Come strutturare il codice per facilitarne la manutenzione

## 🧪 Esempi di Test

Nel branch `2-mysecondlibrary` troverete esempi di:
- **Unit Tests** per Application Services
- **Integration Tests** per i Repository
- **Mock/Stub** per isolare le dipendenze
- **Test con database in-memory**

## 📝 Esercizi per gli Studenti

1. **Analisi Comparativa**: Confronta il codice nei due branch e identifica le differenze
2. **Refactoring**: Prova a refactorare il branch 1 applicando il Repository Pattern
3. **Nuova Implementazione**: Aggiungi una nuova implementazione (es. MongoDB, MySQL)
4. **Test Writing**: Scrivi test aggiuntivi per nuove funzionalità
5. **Performance Analysis**: Confronta le performance delle diverse implementazioni

## 🎓 Risorse Aggiuntive

- [Repository Pattern Documentation](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Dependency Injection in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

## 👨‍🏫 Per i Docenti

Questo repository può essere utilizzato come:
- Esempio pratico nelle lezioni di Ingegneria del Software
- Base per esercitazioni di laboratorio
- Riferimento per progetti di corso
- Dimostrazione di clean architecture principles

---

**Università degli Studi di Brescia - 2025**  
*Workshop: What is a Database?*