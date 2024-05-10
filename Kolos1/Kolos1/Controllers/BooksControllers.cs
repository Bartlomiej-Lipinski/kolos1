using Kolos1.DTOs;
using Kolos1.Services;
using Microsoft.AspNetCore.Mvc;

namespace Kolos1.Controllers;
[ApiController]
[Route("api/[controller]")]
public class books(IDbService dbService) : ControllerBase
{
    
    [HttpGet("{id:int}/genres")]
    public async Task<IActionResult> GetBook(int id)
    {
        GetBookWithGenresResponse? book = await dbService.GetBook(id);
        if (book is null)
        {
            return NotFound($"Book with id: {id} not found");
        }
        return Ok(book);
    }
    [HttpPost]
    public async Task<IActionResult> AddBook(AddBookRequest book)
    {
        foreach (var genre in book.Genres)
        {
            await dbService.GenreValidation(genre);
        }
        var result = await dbService.AddBook(book);
        if (!result)
        {
            return BadRequest("Book not added");
        }

        return Created();
    }
}