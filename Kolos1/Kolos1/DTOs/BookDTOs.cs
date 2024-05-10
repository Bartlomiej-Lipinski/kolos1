namespace Kolos1.DTOs;

public record GetBookWithGenresResponse(int Id, string Title, List<String>? Genres);
public record AddBookRequest(string Title, List<int> Genres);