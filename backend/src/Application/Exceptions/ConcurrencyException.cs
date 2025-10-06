namespace ProductCatalogAPI.Application.Exceptions;

public class ConcurrencyException : Exception
{
    public ConcurrencyException() : base("Insufficient stock available")
    {
    }

    public ConcurrencyException(string message) : base(message)
    {
    }

    public ConcurrencyException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}