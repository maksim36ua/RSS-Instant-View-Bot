#load "../Models/UserModel.cs"
#load "../Models/FeedModel.cs"
#load "../Models/LastFeedPostModel.cs"

using System;
using System.Web;
using System.Net.Http;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Globalization;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

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

	public async Task PostArticles(TraceWriter log)
	{

		var storageAccount = CloudStorageAccount.Parse(System.Configuration.ConfigurationManager.AppSettings["AzureWebJobsStorage"]);
		var tableClient = storageAccount.CreateCloudTableClient();

		var usersTable = tableClient.GetTableReference("users");
		var users = usersTable.ExecuteQuery(new TableQuery<UserModel>()).ToList();

		var feedsTable = tableClient.GetTableReference("feeds");
		var feeds = feedsTable.ExecuteQuery(new TableQuery<FeedModel>()).ToList();

		var lastFeedPostTable = tableClient.GetTableReference("lastFeedPost");
		var lastFeedPosts = lastFeedPostTable.ExecuteQuery(new TableQuery<LastFeedPostModel>()).ToList();

		foreach (var feed in feeds)
		{
			foreach (var user in users)
			{
				using (HttpClient client = new HttpClient())
				{
					try
					{
						var contentStream = await (await client.GetAsync(feed.RssUrl)).Content.ReadAsStreamAsync();
						XmlReader reader = XmlReader.Create(contentStream);
						var response = XDocument.Load(reader);

						var lastFeedPost = lastFeedPosts.FirstOrDefault(q => q.TelegramUserId == user.TelegramUserId && q.FeedName == feed.Name);

						if (lastFeedPost == null)
						{
							lastFeedPost = new LastFeedPostModel
							{
								FeedName = feed.Name,
								TelegramUserId = user.TelegramUserId,
								LastPostTime = new DateTime(1602, 1, 1),
								PartitionKey = Guid.NewGuid().ToString(),
								RowKey = Guid.NewGuid().ToString()
							};

							lastFeedPostTable.Execute(TableOperation.Insert(lastFeedPost));
						}

						var items = response.Descendants("item")
							.Where(q => DateTime.Parse(q.Descendants("pubDate").First().Value).ToUniversalTime() > lastFeedPost.LastPostTime)
							.ToList();

						if (items.Count() != 0)
						{
							lastFeedPost.LastPostTime = DateTime.Parse(items.First().Descendants("pubDate").First().Value);
							lastFeedPostTable.Execute(TableOperation.Replace(lastFeedPost));
						}

						foreach (var item in items)
						{
							var requestBody = new
							{
								chat_id = user.TelegramUserId,
								text = item.Descendants("link").First().Value
							};

							var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
							var result = await client.PostAsync(System.Configuration.ConfigurationManager.AppSettings["TelegramApiUrl"], content);
							log.Info(result.ToString());
						}
					}
					catch (Exception ex)
					{
						log.Info(ex.Message);
					}
				}
			}
		}

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