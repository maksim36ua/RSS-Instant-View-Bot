using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;

public class LastFeedPostModel : TableEntity
{
	public string FeedName { get; set; }
	public int TelegramUserId { get; set; }
	public DateTime LastPostTime { get; set; }
}