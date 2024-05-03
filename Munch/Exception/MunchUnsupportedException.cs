namespace ProcessMunch.Exception;

public class MunchUnsupportedException : MunchException
{
	public MunchUnsupportedException(string message) : base(message)
	{
	}
}