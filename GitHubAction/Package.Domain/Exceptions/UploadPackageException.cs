namespace Package.Domain.Exceptions;

public class UploadPackageException : Exception
{
    public UploadPackageException(string message) : base(message)
    {

    }

    public UploadPackageException(string message, Exception e) : base(message, e)
    {

    }
}