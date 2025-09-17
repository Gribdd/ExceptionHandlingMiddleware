using ExceptionHandling.Database;
using ExceptionHandling.Database.Entities;
using ExceptionHandling.Features.Authors;
using ExceptionHandling.Mapping;
using ExceptionHandling.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExceptionHandling.Features.Users;

[Authorize]
[Route("api/users")]
[ApiController]
public class UsersController(ApplicationDbContext context) : ControllerBase
{
    // GET: api/users
    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of users as DTOs.</returns>
    [HttpGet(Name = "GetUsers")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var users = await context.Users
            .AsNoTracking()
            .Select(u => u.MapToUserDto())
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    // GET api/users/{id}
    /// <summary>
    /// Retrieves a specific user by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The user DTO if found, otherwise 404 Not Found.</returns>
    [HttpGet("{id}", Name = "GetUserById")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] Guid id, CancellationToken cancellationToken)
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

    // POST api/users
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="request">The request containing the new user's details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The newly created user as a DTO.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /users
    ///     {
    ///        "email": "diddy@gmail.com"
    ///     }
    /// </remarks>
    [AllowAnonymous]
    [HttpPost(Name = "CreateUser")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Post([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
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

    // PUT api/users/{id}
    /// <summary>
    /// Updates an existing user by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="request">The request containing the updated details.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>No content if successful, otherwise 404 if not found.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /users/5
    ///     {
    ///        "email": "daddy@gmail.com"
    ///     }
    /// </remarks>
    [HttpPut("{id}", Name = "UpdateUserById")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync(id, cancellationToken);
        if (user == null)
            return NotFound(new { Message = $"User with ID {id} not found." });

        user.Email = request.Email;
        context.Entry(user).State = EntityState.Modified;

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // DELETE api/users/{id}
    /// <summary>
    /// Permanently deletes a user by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>No content if successful, otherwise 404 if not found.</returns>
    [HttpDelete("{id}", Name = "DeleteUserById")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync(id, cancellationToken);
        if (user == null)
            return NotFound(new { Message = $"User with ID {id} not found." });

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    // GET api/users/entities
    /// <summary>
    /// Retrieves all users as entity objects (not DTOs).
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of user entities.</returns>
    [HttpGet("entities", Name = "GetUsersEntities")]
    [ProducesResponseType(typeof(List<User>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntities(CancellationToken cancellationToken)
    {
        var users = await context.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(users);
    }
}
