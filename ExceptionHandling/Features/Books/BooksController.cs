using ExceptionHandling.Database;
using ExceptionHandling.Database.Entities;
using ExceptionHandling.Features.Authors;
using ExceptionHandling.Mapping;
using ExceptionHandling.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExceptionHandling.Features.Books;

/// <summary>
/// Manages CRUD operations for books, including retrieval, creation, updating, 
/// and deletion. Supports both DTO-based and entity-based responses.
/// </summary>
[Authorize]
[Route("api/books")]
[ApiController]
public class BooksController(ApplicationDbContext context) : ControllerBase
{
    // GET: api/books
    /// <summary>
    /// Retrieves all books.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of book DTOs.</returns>
    [HttpGet(Name = "GetBooks")]
    [ProducesResponseType(typeof(List<BookDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var bookDtos = await context.Books
            .Include(b => b.Author)
            .AsNoTracking()
            .Select(b => b.MapToBookDto())
            .ToListAsync(cancellationToken);

        return Ok(bookDtos);
    }

    // GET api/books/{id}
    /// <summary>
    /// Retrieves a specific book by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the book.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The book DTO if found, otherwise 404 Not Found.</returns>
    [HttpGet("{id}", Name = "GetBookById")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        return Ok(book.MapToBookDto());
    }

    // POST api/books
    /// <summary>
    /// Creates a new book.
    /// </summary>
    /// <param name="request">The request containing the new book's details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The newly created book as a DTO.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /books
    ///     {
    ///        "title": "Sample Book",
    ///        "year": 2024,
    ///        "authorId": "b6c7ef94-3ff2-4cdd-8a2f-1a01fcb62290"
    ///     }
    /// </remarks>
    [HttpPost(Name = "CreateBook")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post([FromBody] CreateBookRequest request, CancellationToken cancellationToken)
    {
        var author = await context.Authors.FindAsync(request.AuthorId, cancellationToken);

        if (author == null)
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

    // PUT api/books/{id}
    /// <summary>
    /// Updates an existing book by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the book.</param>
    /// <param name="request">The request containing the updated details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>No content if successful, otherwise 404 if not found.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /books/{id}
    ///     {
    ///        "title": "Updated Title",
    ///        "year": 2025,
    ///        "authorId": "b6c7ef94-3ff2-4cdd-8a2f-1a01fcb62290"
    ///     }
    /// </remarks>
    [HttpPut("{id}", Name = "UpdateBookById")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    // DELETE api/books/{id}
    /// <summary>
    /// Permanently deletes a book by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the book.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>No content if successful, otherwise 404 if not found.</returns>
    [HttpDelete("{id}", Name = "DeleteBookById")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var book = await context.Books.FindAsync(id, cancellationToken);
        if (book == null)
            return NotFound(new { Message = $"Book with ID {id} not found." });

        context.Books.Remove(book);
        await context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    // GET api/books/entities
    /// <summary>
    /// Retrieves all books as entity objects (not DTOs).
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of book entities.</returns>
    [HttpGet("entities", Name = "GetBooksEntities")]
    [ProducesResponseType(typeof(List<Book>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntities(CancellationToken cancellationToken)
    {
        var books = await context.Books
            .Include(b => b.Author)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(books);
    }
}
