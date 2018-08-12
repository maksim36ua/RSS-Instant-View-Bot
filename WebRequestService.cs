using System.Web;
using System.Net.Http;

[Serializable]
public class WebRequestService
{
	public Task StartAsync(IDialogContext context)
	{
		try
		{
			context.Wait(MessageReceivedAsync);
		}
		catch (OperationCanceledException error)
		{
			return Task.FromCanceled(error.CancellationToken);
		}
		catch (Exception error)
		{
			return Task.FromException(error);
		}

		return Task.CompletedTask;
	}
}