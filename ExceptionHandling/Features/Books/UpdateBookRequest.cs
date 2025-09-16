namespace ExceptionHandling.Features.Books;

public record UpdateBookRequest(Guid Id, string Title, int Year, Guid AuthorId);