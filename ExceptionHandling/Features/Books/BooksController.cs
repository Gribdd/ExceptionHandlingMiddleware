using ExceptionHandling.Database;
using ExceptionHandling.Database.Entities;
using ExceptionHandling.Features.Authors;
using ExceptionHandling.Mapping;
using ExceptionHandling.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExceptionHandling.Features.Books;

[Authorize]
[Route("api/books")]
[ApiController]
public class BooksController(
    ApplicationDbContext context) 
    : ControllerBase
{
    // GET: api/<BooksController>
    [HttpGet(Name = "GetBooks")]
    [ProducesResponseType(typeof(List<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var bookDtos = await context.Books
            .Include(b => b.Author)
            .AsNoTracking()
            .Select(b => b.MapToBookDto())
            .ToListAsync(cancellationToken);

        return Ok(bookDtos);
    }

    // GET api/<BooksController>/5
    [HttpGet("{id}", Name = "GetBookById")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var book = await context.Books
            .Include(b => b.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (book == null)
        {
            return NotFound(new { Message = $"Book with ID {id} not found." });
        }

        var bookDto = book?.MapToBookDto();
        return Ok(bookDto);
    }

    // POST api/<BooksController>
    [HttpPost(Name = "CreateBook")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Post([FromBody] CreateBookRequest request, CancellationToken cancellationToken)
    {
        var author = await context.Authors.FindAsync(request.AuthorId, cancellationToken);
        
        if(author == null)
        {
            return NotFound(new { Message = $"Author with ID {request.AuthorId} not found." });
        }

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Year = request.Year,
            AuthorId = request.AuthorId
        };

        context.Books.Add(book);
        await context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = book.Id }, book.MapToBookDto());
    }

    // PUT api/<BooksController>/5
    [HttpPut("{id}", Name = "UpdateBookById")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] UpdateBookRequest request, CancellationToken cancellationToken)
    {
        var book = await context.Books.FindAsync(id, cancellationToken);
        if (book == null)
            return NotFound(new { Message = $"Book with ID {id} not found." });

        book.Title = request.Title;
        book.Year = request.Year;
        book.AuthorId = request.AuthorId;

        context.Entry(book).State = EntityState.Modified;
        await context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    // DELETE api/<BooksController>/5
    [HttpDelete("{id}", Name = "DeleteBookById")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var book = await context.Books.FindAsync(id, cancellationToken);
        if (book == null)
            return NotFound(new { Message = $"Book with ID {id} not found." });

        context.Books.Remove(book);

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // GET api/authors/entities
    /// <summary>
    /// Gets all authors as entity objects (not DTOs).
    /// </summary>
    /// <returns>All author entities</returns>
    [HttpGet("entities", Name = "GetBooksEntities")]
    [ProducesResponseType(typeof(List<Book>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEntities(CancellationToken cancellationToken)
    {
        var books = await context.Books
            .Include(a => a.Author)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(books);
    }
}
