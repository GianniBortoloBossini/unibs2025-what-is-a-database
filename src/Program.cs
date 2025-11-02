using LibraryAPI.Infrastructure.Database;
using LibraryAPI.Repositories.Interfaces;
using LibraryAPI.Repositories.Implementations;
using LibraryAPI.Services.Interfaces;
using LibraryAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ SOLUZIONE: Configurazione Dependency Injection per Repository Pattern
// Registrazione delle dipendenze in modo pulito e organizzato

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Library API", 
        Version = "v1",
        Description = "Demo API CON Repository Pattern + Application Service - Branch 2-mysecondlibrary" 
    });
});

// ✅ INFRASTRUCTURE LAYER: Database connection factory
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

// ✅ DATA ACCESS LAYER: Repository registration con selezione dinamica del database
builder.Services.AddScoped<IBookRepository>(serviceProvider =>
{
    var connectionFactory = serviceProvider.GetRequiredService<IDbConnectionFactory>();
    var logger = serviceProvider.GetRequiredService<ILogger<IBookRepository>>();
    
    return connectionFactory.DatabaseType switch
    {
        "SqlServer" => new SqlServerBookRepository(connectionFactory),
        "PostgreSQL" => new PostgresBookRepository(connectionFactory),
        "SQLite" => new SqliteBookRepository(connectionFactory),
        _ => throw new InvalidOperationException($"Unsupported database type: {connectionFactory.DatabaseType}")
    };
});

// ✅ APPLICATION LAYER: Business logic services
builder.Services.AddScoped<IBookService, BookService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API V1");
        c.RoutePrefix = string.Empty; // Swagger UI alla radice
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();