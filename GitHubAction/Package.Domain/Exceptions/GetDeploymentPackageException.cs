namespace Package.Domain.Exceptions;
    
public class GetDeploymentPackageException : Exception
{
    public GetDeploymentPackageException(string message) : base(message)
    {
    }

    public GetDeploymentPackageException(string message, Exception e) : base(message, e)
    {
    }
}
