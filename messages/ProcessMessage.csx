using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

[Serializable]
public class ProcessMessage : IDialog<object>
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

	public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
	{
		var message = await argument;

		switch (message.Text)
		{
			case "reset":
				PromptDialog.Confirm(context,
								AfterResetAsync,
								"Are you sure you want to reset the count?",
								"Didn't get that!",
								promptStyle: PromptStyle.Auto);
				break;
			case "get":
				await GetArticles(context);
				break;
			default:
				await context.PostAsync($"You said {message.Text}. Your message body: {JsonConvert.SerializeObject(message)}");
				break;

		}

		context.Wait(MessageReceivedAsync);

	}

	public async Task GetArticles(IDialogContext context)
	{
		await context.PostAsync($"Article 1");
		await context.PostAsync($"Article 2");
		await context.PostAsync($"Article 3");
	}

	public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
	{
		var confirm = await argument;
		if (confirm)
		{
			await context.PostAsync("Reset count.");
		}
		else
		{
			await context.PostAsync("Did not reset count.");
		}
		context.Wait(MessageReceivedAsync);
	}
}