namespace ExceptionHandling.Features.Books;

public record CreateBookRequest(string Title, int Year, Guid AuthorId);
