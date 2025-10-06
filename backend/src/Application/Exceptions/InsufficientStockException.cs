namespace ProductCatalogAPI.Application.Exceptions;

public class InsufficientStockException : Exception
{
    public InsufficientStockException() : base("Insufficient stock available")
    {
    }

    public InsufficientStockException(string message) : base(message)
    {
    }

    public InsufficientStockException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}