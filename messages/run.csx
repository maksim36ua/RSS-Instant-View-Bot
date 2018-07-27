#r "Newtonsoft.Json"
#load "ProcessMessage.csx"

using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

// Bot Storage: Register the optional private state storage for your bot. 

// (Default) For Azure Table Storage, set the following environment variables in your bot app:
// -UseTableStorageForConversationState set to 'true'
// -AzureWebJobsStorage set to your table connection string

// For CosmosDb, set the following environment variables in your bot app:
// -UseCosmosDbForConversationState set to 'true'
// -CosmosDbEndpoint set to your cosmos db endpoint
// -CosmosDbKey set to your cosmos db key

[FunctionName("Message")]
public static async Task<object> Run([HttpTrigger] HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Webhook was triggered!");

    using (BotService.Initialize())
    {
        string jsonContent = await req.Content.ReadAsStringAsync();
        var activity = JsonConvert.DeserializeObject<Activity>(jsonContent);
        
        if (!await BotService.Authenticator.TryAuthenticateAsync(req, new [] {activity}, CancellationToken.None))
        {
            return BotAuthenticator.GenerateUnauthorizedResponse(req);
        }
        
        if (activity != null)
        {
			Conversation.SendAsync(activity, () => new ProcessMessage());
        }

        return req.CreateResponse(HttpStatusCode.Accepted);
    }    
}


[FunctionName("Timer")]
public static async Task<object> Run([TimerTrigger("* * * * *")]TimerInfo myTimer, TraceWriter log)
{
	log.Info($"Time trigger was triggered!");

	//using (BotService.Initialize())
	//{
	//	string jsonContent = await req.Content.ReadAsStringAsync();
	//	var activity = JsonConvert.DeserializeObject<Activity>(jsonContent);

	//	if (!await BotService.Authenticator.TryAuthenticateAsync(req, new[] { activity }, CancellationToken.None))
	//	{
	//		return BotAuthenticator.GenerateUnauthorizedResponse(req);
	//	}

	//	if (activity != null)
	//	{
	//		Conversation.SendAsync(activity, () => new ProcessMessage());
	//	}

	//	return req.CreateResponse(HttpStatusCode.Accepted);
	//}
}
