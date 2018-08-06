using System;
using System.Web;
using System.Xml;
using System.Xml.Linq;
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
		using (HttpClient client = new HttpClient())
		{
			var contentStream = await (await client.GetAsync("https://nplus1.ru/rss")).Content.ReadAsStreamAsync();
			XmlReader reader = XmlReader.Create(contentStream);
			var response = XDocument.Load(reader);
			foreach (var link in response.Descendants("link").Skip(2).Take(15).ToList())
			{
				await context.PostAsync($"{link.Value}");
			}
		}
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