namespace ProcessMunch.Exception;

public class MunchException : System.Exception
{
	public override string Message { get; }

	public MunchException(string message)
	{
		Message = message;
	}
}