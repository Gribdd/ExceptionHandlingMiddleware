using ExceptionHandling.Database;
using ExceptionHandling.Database.Entities;
using ExceptionHandling.Features.Authors;
using ExceptionHandling.Mapping;
using ExceptionHandling.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExceptionHandling.Features.Users;
[Route("api/users")]
[ApiController]
public class UsersController(
    ApplicationDbContext context,
    CancellationToken cancellationToken = default) : ControllerBase
{
    // GET: api/<UsersController>
    [HttpGet(Name = "GetUsers")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Get()
    {
        var users = await context.Users
            .AsNoTracking()
            .Select(u => u.MapToUserDto())
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    // GET api/<UsersController>/5
    [HttpGet("{id}", Name = "GetUserById")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            return NotFound(new { Message = $"User with ID {id} not found." });
        }

        var userDto = user.MapToUserDto();
        return Ok(userDto);
    }

    // POST api/<UsersController>
    [HttpPost(Name = "CreateUser")]
    [ProducesDefaultResponseType]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Post([FromBody] CreateUserRequest request)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = user.Id }, user.MapToUserDto());
    }

    // PUT api/<UsersController>/5
    [HttpPut("{id}", Name = "UpdateUserById")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await context.Users.FindAsync(id, cancellationToken);
        if (user == null)
            return NotFound(new { Message = $"User with ID {id} not found." });

        user.Email = request.Email;
        context.Entry(user).State = EntityState.Modified;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // DELETE api/<UsersController>/5
    [HttpDelete("{id}", Name = "DeleteUserById")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var author = await context.Users.FindAsync(id, cancellationToken);
        if (author == null)
            return NotFound(new { Message = $"User with ID {id} not found." });

        context.Users.Remove(author);

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // GET api/users/entities
    /// <summary>
    /// Gets all users as entity objects (not DTOs).
    /// </summary>
    /// <returns>All author entities</returns>
    [HttpGet("entities", Name = "GetUsersEntities")]
    [ProducesResponseType(typeof(List<User>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEntities(CancellationToken cancellationToken)
    {
        var users = await context.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(users);
    }
}
