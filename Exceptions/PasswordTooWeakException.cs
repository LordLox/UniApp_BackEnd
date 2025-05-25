public class PasswordTooWeakException : Exception
{
    // Constructor with default message
    public PasswordTooWeakException(string message = "")
        : base(message + "\n" +
            "at least 1 lowercase alphabetical character\n" +
            "at least 1 uppercase alphabetical character\n" +
            "at least 1 numeric character\n" +
            "at least one special character\n" +
            "must be eight characters or longer")
    {
    }

    // Constructor with custom message and inner exception
    public PasswordTooWeakException(string message, Exception inner)
        : base(message + "\n" +
            "at least 1 lowercase alphabetical character\n" +
            "at least 1 uppercase alphabetical character\n" +
            "at least 1 numeric character\n" +
            "at least one special character\n" +
            "must be eight characters or longer", inner)
    {
    }
}