#load "ProcessMessage.csx"

public static async Task<object> Run(TimerInfo timer, TraceWriter log)
{
	log.Info($"Timer was triggered!");

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

	//	return req.CreateResponse("200");
	//}
	return new HttpRequestMessage("200");
}