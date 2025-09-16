namespace ExceptionHandling.Features.Users;

public record UpdateUserRequest(
    Guid Id,
    string Email);