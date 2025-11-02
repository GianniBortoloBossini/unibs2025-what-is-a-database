-- SQL Server Database Initialization Script

-- Create Database (will be created automatically by connection string)
USE LibraryDB;

-- Create Books table
CREATE TABLE Books (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Author NVARCHAR(100) NOT NULL,
    ISBN NVARCHAR(20) NOT NULL UNIQUE,
    PublishedDate DATETIME2 NOT NULL,
    Genre NVARCHAR(50) NOT NULL,
    AvailableCopies INT NOT NULL DEFAULT 0,
    Price DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- Create BookLoans table (for business logic demonstration)
CREATE TABLE BookLoans (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BookId INT NOT NULL,
    BorrowerName NVARCHAR(100) NOT NULL,
    LoanDate DATETIME2 DEFAULT GETDATE(),
    ReturnedDate DATETIME2 NULL,
    FOREIGN KEY (BookId) REFERENCES Books(Id)
);

-- Insert sample data - SQL SERVER SPECIFIC BOOKS
INSERT INTO Books (Title, Author, ISBN, PublishedDate, Genre, AvailableCopies, Price) VALUES
('Programming Microsoft SQL Server 2012', 'Leonard Lobel', '978-0-7356-7674-4', '2012-11-15', 'SQL Server', 3, 59.99),
('Microsoft SQL Server 2019: A Beginners Guide', 'Dusan Petkovic', '978-1-260-45818-9', '2019-12-06', 'SQL Server', 5, 45.99),
('SQL Server Query Performance Tuning', 'Grant Fritchey', '978-1-4842-3888-2', '2018-09-15', 'SQL Server', 2, 54.99),
('Pro SQL Server Administration', 'Peter Carter', '978-1-4842-2957-6', '2016-12-27', 'SQL Server', 4, 49.99),
('T-SQL Fundamentals', 'Itzik Ben-Gan', '978-1-5093-0200-1', '2016-06-07', 'SQL Server', 6, 39.99),
('Clean Code', 'Robert C. Martin', '978-0-13-235088-4', '2008-08-01', 'Technology', 8, 42.99),
('Design Patterns', 'Gang of Four', '978-0-201-63361-0', '1994-10-21', 'Technology', 2, 54.99),
('The Pragmatic Programmer', 'Andrew Hunt', '978-0-201-61622-4', '1999-10-20', 'Technology', 5, 39.99);

-- Insert some sample loans
INSERT INTO BookLoans (BookId, BorrowerName, LoanDate, ReturnedDate) VALUES
(1, 'Mario Rossi', '2024-10-01', '2024-10-15'),
(2, 'Luigi Bianchi', '2024-10-05', NULL),
(3, 'Anna Verdi', '2024-10-10', NULL);

-- Create indexes for better performance
CREATE INDEX IX_Books_Author ON Books(Author);
CREATE INDEX IX_Books_Title ON Books(Title);
CREATE INDEX IX_Books_Genre ON Books(Genre);
CREATE INDEX IX_BookLoans_BookId ON BookLoans(BookId);
CREATE INDEX IX_BookLoans_ReturnedDate ON BookLoans(ReturnedDate);