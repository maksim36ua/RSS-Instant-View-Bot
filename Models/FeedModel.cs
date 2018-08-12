using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

public class FeedModel : TableEntity
{
	public string Name { get; set; }
	public string RssUrl { get; set; }
	public string TopHtmlNode { get; set; }
}