namespace ProductCatalogAPI.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException() : base("Recurso no encontrado.") { }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}
