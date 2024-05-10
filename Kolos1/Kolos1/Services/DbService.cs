using System.Data;
using System.Data.SqlClient;
using Kolos1.DTOs;
using Kolos1.Models;

namespace Kolos1.Services;

public class DbService(IConfiguration configuration) : IDbService
{
    private async Task<SqlConnection> GetConnection()
    {
        var connection = new SqlConnection(configuration.GetConnectionString("Default"));
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        return connection;
    }

    public async Task<GetBookWithGenresResponse?> GetBook(int id)
    {
        var connection = await GetConnection();
        var command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = """
                              SELECT b.PK, b.title, g.PK
                              FROM books b left join books_genres bg
                              on b.PK = bg.FK_book
                              left join genres g
                              on g.PK = bg.FK_genre
                              WHERE b.PK = @id                       
                              """;
        command.Parameters.AddWithValue("@id", id);
        var reader = await command.ExecuteReaderAsync();

        if (!reader.HasRows)
        {
            return null;   
        }
        await reader.ReadAsync();
        var result = new GetBookWithGenresResponse(
            reader.GetInt32(0), 
            reader.GetString(1), 
            !await reader.IsDBNullAsync(2) ? [Enum.GetName(typeof(Genres), reader.GetInt32(2))] : []
        );
        
        while (await reader.ReadAsync())
        {
            result.Genres.Add(Enum.GetName(typeof(Genres), reader.GetInt32(2)));
        }

        return result;
    }
    
    public async Task<bool> AddBook(AddBookRequest book)
    {
        int affectedRows = 0;
        await using var connection = await GetConnection();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var command1 = new SqlCommand();
            command1.Connection = connection;
            command1.CommandText = """
                                   INSERT INTO books VALUES (@title) select convert(int,SCOPE_IDENTITY());                        
                                   """;
            command1.Transaction = (SqlTransaction)transaction;
            command1.Parameters.AddWithValue("@title", book.Title);
            var id = (int)(await command1.ExecuteScalarAsync())!;
            foreach (var genre in book.Genres)
            {
                var command2 = new SqlCommand();
                command2.Connection = connection;
                command2.CommandText = """
                                       INSERT INTO books_genres values (@id,@genre) select convert(INT,SCOPED_IDENTITY()); 
                                       """;
                command2.Transaction = (SqlTransaction)transaction;
                command2.Parameters.AddWithValue("@id", id);
                command2.Parameters.AddWithValue("@genre", genre);
                 affectedRows = await command2.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
            return affectedRows != 0;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<string?> GenreValidation(int genreId)
    {
        await using var connection = await GetConnection();
        await connection.OpenAsync();
        await using var command = new SqlCommand();
        command.CommandText = $"SELECT name FROM genres WHERE PK=@1";
        command.Parameters.AddWithValue("@1", genreId);
        command.Connection = connection;
        var genre = (string?)await command.ExecuteScalarAsync();
        return genre;
    }
}