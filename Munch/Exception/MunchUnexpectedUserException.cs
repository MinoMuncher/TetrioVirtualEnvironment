namespace ProcessMunch.Exception;

public class MunchUnexpectedUserException : MunchException
{
	public MunchUnexpectedUserException(string message) : base(message)
	{
	}
}