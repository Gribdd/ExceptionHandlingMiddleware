namespace ExceptionHandling.Features.Shared;

public sealed record BookResponse(Guid Id, string Title, int Year, Guid AuthorId);
