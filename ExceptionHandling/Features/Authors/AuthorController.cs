using ExceptionHandling.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExceptionHandling.Mapping;
using ExceptionHandling.Database.Entities;
using Microsoft.AspNetCore.Http;
using ExceptionHandling.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExceptionHandling.Features.Authors;

[Route("api/[controller]")]
[ApiController]
public class AuthorController(
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
        var authors = await context.Authors.ToListAsync();
        var authorDtos = authors.Select(a => a.MapToAuthorDto()).ToList();

        return Ok(authorDtos);
    }

    // GET api/<AuthorController>/5
    /// <summary>
    /// Gets an Author by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>The author by Id</returns>
    [HttpGet("{id}", Name = "GetAuthorById")]
    [ProducesResponseType(typeof(AuthorDto),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Get(Guid id)
    {
        var author = await context.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.Id == id);

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
    /// <param name="author"></param>
    /// <returns>A newly created Author</returns>
    /// /// <remarks>
    /// Sample request:
    ///
    ///     POST /Todo
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
    [HttpPut("{id}", Name = "UpdateAuthorById")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Put(Guid id, [FromBody] Author updatedAuthor)
    {
        var author = await context.Authors.FindAsync(id);
        if (author == null)
            return NotFound(new { Message = $"Author with ID {id} not found." });


        context.Entry(author).State = EntityState.Modified;
        await context.SaveChangesAsync();

        return NoContent(); 
    }

    // DELETE api/<AuthorController>/5
    [HttpDelete("{id}", Name = "DeleteAuthorById")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Delete(Guid id)
    {
        var author = await context.Authors.FindAsync(id);
        if (author == null)
            return NotFound(new { Message = $"Author with ID {id} not found." });

        context.Authors.Remove(author);
        
        await context.SaveChangesAsync();
        return NoContent(); 
    }
}
