namespace sp_back.models.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class ValidationException : Exception
{
    public ValidationException()
    {
    }

    public ValidationException(string message)
        : base(message)
    {
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class AuctionException : Exception
{
    public AuctionException()
    {
    }

    public AuctionException(string message)
        : base(message)
    {
    }

    public AuctionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}