using System.Threading;
using ExceptionHandling.Database;
using ExceptionHandling.Database.Entities;
using ExceptionHandling.Mapping;
using ExceptionHandling.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExceptionHandling.Features.Authors;

[Route("api/[controller]")]
[ApiController]
public class AuthorsController(
    ApplicationDbContext context) 
    : ControllerBase
{
    // GET: api/<AuthorController>
    /// <summary>
    /// Gets all authors
    /// </summary>
    /// <returns>All authors</returns>
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
    /// Gets an Author by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The author by Id</returns>
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
    /// Creates an Author
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A newly created Author</returns>
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
    /// Updates an Author by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Not Found and No content when successful </returns>
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
    /// Soft deletes an author by id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>No content when deleting successfully</returns>

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
    /// Gets all authors as entity objects (not DTOs).
    /// </summary>
    /// <returns>All author entities</returns>
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
