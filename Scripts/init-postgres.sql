-- PostgreSQL Database Initialization Script

-- Create books table (using PostgreSQL naming conventions: snake_case)
CREATE TABLE books (
    id SERIAL PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    author VARCHAR(100) NOT NULL,
    isbn VARCHAR(20) NOT NULL UNIQUE,
    published_date TIMESTAMP NOT NULL,
    genre VARCHAR(50) NOT NULL,
    available_copies INTEGER NOT NULL DEFAULT 0,
    price DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create book_loans table (for business logic demonstration)
CREATE TABLE book_loans (
    id SERIAL PRIMARY KEY,
    book_id INTEGER NOT NULL,
    borrower_name VARCHAR(100) NOT NULL,
    loan_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    returned_date TIMESTAMP NULL,
    FOREIGN KEY (book_id) REFERENCES books(id)
);

-- Insert sample data - POSTGRESQL SPECIFIC BOOKS (using snake_case column names)
INSERT INTO books (title, author, isbn, published_date, genre, available_copies, price) VALUES
('PostgreSQL: Up and Running', 'Regina Obe', '978-1-4493-7321-9', '2017-01-06', 'PostgreSQL', 4, 34.99),
('Learning PostgreSQL 11', 'Salahaldin Juba', '978-1-7891-5392-1', '2019-01-31', 'PostgreSQL', 3, 44.99),
('PostgreSQL High Performance', 'Gregory Smith', '978-1-8496-1803-8', '2010-10-01', 'PostgreSQL', 2, 49.99),
('Mastering PostgreSQL 13', 'Hans-Jürgen Schönig', '978-1-8005-6748-4', '2020-11-13', 'PostgreSQL', 5, 54.99),
('PostgreSQL Administration Cookbook', 'Simon Riggs', '978-1-8496-8444-4', '2015-04-27', 'PostgreSQL', 6, 39.99),
('Clean Code', 'Robert C. Martin', '978-0-13-235088-4', '2008-08-01', 'Technology', 8, 42.99),
('Design Patterns', 'Gang of Four', '978-0-201-63361-0', '1994-10-21', 'Technology', 2, 54.99),
('The Pragmatic Programmer', 'Andrew Hunt', '978-0-201-61622-4', '1999-10-20', 'Technology', 5, 39.99);

-- Insert some sample loans
INSERT INTO book_loans (book_id, borrower_name, loan_date, returned_date) VALUES
(1, 'Mario Rossi', '2024-10-01', '2024-10-15'),
(2, 'Luigi Bianchi', '2024-10-05', NULL),
(3, 'Anna Verdi', '2024-10-10', NULL);

-- Create indexes for better performance (using snake_case)
CREATE INDEX idx_books_author ON books(author);
CREATE INDEX idx_books_title ON books(title);
CREATE INDEX idx_books_genre ON books(genre);
CREATE INDEX idx_book_loans_book_id ON book_loans(book_id);
CREATE INDEX idx_book_loans_returned_date ON book_loans(returned_date);