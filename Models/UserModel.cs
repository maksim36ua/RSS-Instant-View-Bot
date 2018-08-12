using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

public class UserModel : TableEntity
{
	public string TelegramName { get; set; }
	public string TelegramNickname { get; set; }
	public int TelegramUserId { get; set; }
	public string TelegraphAccessToken { get; set; }
}