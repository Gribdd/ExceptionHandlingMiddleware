using ExceptionHandling.Database;
using ExceptionHandling.Database.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExceptionHandling.Features;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get()
    {
        var authors = await context.Authors.ToListAsync();
        return Ok(authors);
    }

    // GET api/<AuthorController>/5
    /// <summary>
    /// Gets an Author by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>The author by Id</returns>
    [HttpGet("{id}", Name = "GetAuthorById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
        return Ok(author);
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Post([FromBody] Author author)
    {
        if (author == null)
            return BadRequest(new { Message = "Invalid author data." });

        author.Id = Guid.NewGuid(); // ensure ID is set
        context.Authors.Add(author);
        await context.SaveChangesAsync();

        // Return 201 with location header
        return CreatedAtRoute("GetAuthorById", new { id = author.Id }, author);
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
