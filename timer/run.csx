#load "../Services/ProcessMessage.csx"

using System;
using System.Threading;
using Newtonsoft.Json;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

public static async Task<object> Run(TimerInfo timer, TraceWriter log)
{
	log.Info($"Timer was triggered!");

	using (BotService.Initialize())
	{
		var processMessage = new ProcessMessage();
		await processMessage.PostArticles(log);
	}

	return new HttpRequestMessage();
}