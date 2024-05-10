using Kolos1.DTOs;

namespace Kolos1.Services;

public interface IDbService
{
    public Task<GetBookWithGenresResponse?> GetBook(int id);
    public Task<bool> AddBook(AddBookRequest book);
    public Task<string?> GenreValidation(int genreId);
}
