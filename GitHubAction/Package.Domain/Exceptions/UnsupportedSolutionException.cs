namespace Package.Builder.Exceptions
{
    public class UnsupportedSolutionException : Exception
    {
        public UnsupportedSolutionException()
        {

        }

        public UnsupportedSolutionException(string message) : base(message)
        {

        }

        public UnsupportedSolutionException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
