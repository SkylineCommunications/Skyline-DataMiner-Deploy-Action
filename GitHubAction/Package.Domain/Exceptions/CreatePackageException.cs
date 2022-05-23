namespace Package.Builder
{
    public class CreatePackageException : Exception
    {
        public CreatePackageException()
        {

        }

        public CreatePackageException(string message) : base(message)
        {

        }

        public CreatePackageException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
