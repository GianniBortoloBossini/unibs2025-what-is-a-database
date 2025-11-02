-- SQLite Database Initialization Script

-- Create Books table
CREATE TABLE Books (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Author TEXT NOT NULL,
    ISBN TEXT NOT NULL UNIQUE,
    PublishedDate TEXT NOT NULL,
    Genre TEXT NOT NULL,
    AvailableCopies INTEGER NOT NULL DEFAULT 0,
    Price REAL NOT NULL DEFAULT 0.00,
    CreatedAt TEXT DEFAULT (datetime('now'))
);

-- Create BookLoans table (for business logic demonstration)
CREATE TABLE BookLoans (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    BookId INTEGER NOT NULL,
    BorrowerName TEXT NOT NULL,
    LoanDate TEXT DEFAULT (datetime('now')),
    ReturnedDate TEXT NULL,
    FOREIGN KEY (BookId) REFERENCES Books(Id)
);

-- Insert sample data - SQLITE SPECIFIC BOOKS
INSERT INTO Books (Title, Author, ISBN, PublishedDate, Genre, AvailableCopies, Price) VALUES
('Using SQLite', 'Jay Kreibich', '978-0-596-52118-9', '2010-08-18', 'SQLite', 3, 29.99),
('SQLite Database System Design and Implementation', 'Sibsankar Haldar', '978-8-1782-7679-4', '2015-11-15', 'SQLite', 4, 39.99),
('Getting Started with SQL and Databases', 'Mark Simon', '978-1-4919-3861-2', '2019-03-12', 'SQLite', 5, 24.99),
('SQLite Pocket Reference', 'Dave Kearns', '978-0-596-00663-8', '2006-02-01', 'SQLite', 2, 19.99),
('Embedded Database Programming', 'John Anderson', '978-1-4354-5512-8', '2018-07-20', 'SQLite', 6, 34.99),
('Clean Code', 'Robert C. Martin', '978-0-13-235088-4', '2008-08-01', 'Technology', 8, 42.99),
('Design Patterns', 'Gang of Four', '978-0-201-63361-0', '1994-10-21', 'Technology', 2, 54.99),
('The Pragmatic Programmer', 'Andrew Hunt', '978-0-201-61622-4', '1999-10-20', 'Technology', 5, 39.99);

-- Insert some sample loans
INSERT INTO BookLoans (BookId, BorrowerName, LoanDate, ReturnedDate) VALUES
(1, 'Mario Rossi', '2024-10-01', '2024-10-15'),
(2, 'Luigi Bianchi', '2024-10-05', NULL),
(3, 'Anna Verdi', '2024-10-10', NULL);

-- Create indexes for better performance
CREATE INDEX idx_books_author ON Books(Author);
CREATE INDEX idx_books_title ON Books(Title);
CREATE INDEX idx_books_genre ON Books(Genre);
CREATE INDEX idx_bookloans_bookid ON BookLoans(BookId);
CREATE INDEX idx_bookloans_returneddate ON BookLoans(ReturnedDate);