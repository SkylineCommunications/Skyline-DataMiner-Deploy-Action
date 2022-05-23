namespace Package.Domain.Exceptions;

public class DeployPackageException : Exception
{
    public DeployPackageException(string message) : base(message)
    {

    }
    public DeployPackageException(string message, Exception e) : base(message, e)
    {
        
    }
}