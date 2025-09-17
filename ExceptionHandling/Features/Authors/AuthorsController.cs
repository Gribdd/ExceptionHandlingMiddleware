using ExceptionHandling.Database;
using ExceptionHandling.Database.Entities;
using ExceptionHandling.Mapping;
using ExceptionHandling.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExceptionHandling.Features.Authors;

[Authorize]
[Route("api/authors")]
[ApiController]
public class AuthorsController(
    ApplicationDbContext context) 
    : ControllerBase
{
    /// <summary>
    /// Retrieves all authors along with their books.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of authors as DTOs.</returns>
    [HttpGet(Name = "GetAuthors")]
    [ProducesResponseType(typeof(List<AuthorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var authorDtos = await context.Authors
            .Include(a => a.Books)
            .AsNoTracking()
            .Select(a => a.MapToAuthorDto())
            .ToListAsync(cancellationToken);

        return Ok(authorDtos);
    }

    // GET api/<AuthorController>/5
    /// <summary>
    /// Retrieves a specific author by ID, including their books.
    /// </summary>
    /// <param name="id">The unique identifier of the author.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The author DTO if found, otherwise 404 Not Found.</returns>
    [HttpGet("{id}", Name = "GetAuthorById")]
    [ProducesResponseType(typeof(AuthorDto),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var author = await context.Authors
            .Include(a => a.Books)
            .AsNoTracking() 
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (author == null)
        {
            return NotFound(new { Message = $"Author with ID {id} not found." });
        }

        var authorDto = author.MapToAuthorDto();
        return Ok(authorDto);
    }

    // POST api/<AuthorController>
    /// <summary>
    /// Creates a new author.
    /// </summary>
    /// <param name="request">The request object containing the author's details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The newly created author as a DTO.</returns>
    /// /// <remarks>
    /// Sample request:
    ///
    ///     POST /Authors
    ///     {
    ///        "name": "Michael Jackstone",
    ///     }
    ///
    /// </remarks>
    [HttpPost(Name = "CreateAuthor")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(typeof(AuthorDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Post([FromBody] CreateAuthorRequest request, CancellationToken cancellationToken)
    {
        var author = new Author
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
        };

        context.Authors.Add(author);
        await context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = author.Id }, author.MapToAuthorDto());
    }

    // PUT api/<AuthorController>/5
    /// <summary>
    /// Updates the name of an existing author by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the author.</param>
    /// <param name="request">The request object containing the updated details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>No content if successful, otherwise 404 if not found.</returns>
    /// /// <remarks>
    /// Sample request:
    ///
    ///     PUT /Authors/5
    ///     {
    ///        "name": "Michael Jackson",
    ///     }
    ///
    /// </remarks>
    [HttpPut("{id}", Name = "UpdateAuthorById")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] UpdateAuthorRequest request, CancellationToken cancellationToken)
    {
        var author = await context.Authors.FindAsync(id, cancellationToken);
        if (author == null)
            return NotFound(new { Message = $"Author with ID {id} not found." });

        author.Name = request.Name;
        context.Entry(author).State = EntityState.Modified;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // DELETE api/<AuthorController>/5
    /// <summary>
    /// Permanently deletes an author by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the author.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>No content if successful, otherwise 404 if not found.</returns>

    [HttpDelete("{id}", Name = "DeleteAuthorById")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var author = await context.Authors.FindAsync(id, cancellationToken);
        if (author == null)
            return NotFound(new { Message = $"Author with ID {id} not found." });

        context.Authors.Remove(author);
        
        await context.SaveChangesAsync(cancellationToken);
        return NoContent(); 
    }

    // GET api/authors/entities
    /// <summary>
    /// Retrieves all authors as entity objects (with their books).
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of author entities.</returns>
    [HttpGet("entities", Name = "GetAuthorsEntities")]
    [ProducesResponseType(typeof(List<Author>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEntities(CancellationToken cancellationToken)
    {
        var authors = await context.Authors
            .Include(a => a.Books)    // eager load books if you want full entity graph
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(authors);
    }
}
