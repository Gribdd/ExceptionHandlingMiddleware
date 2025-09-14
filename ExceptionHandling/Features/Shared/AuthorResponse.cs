namespace ExceptionHandling.Features.Shared;

public sealed record AuthorResponse(Guid Id, string Name, List<BookResponse> Books);
