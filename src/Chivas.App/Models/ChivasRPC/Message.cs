using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chivas.Models.ChivasRPC;

public class Message
{
	[JsonPropertyName("command")]
	public string Command { get; set; } = null!;

	[JsonPropertyName("data")]
	public JsonElement Data { get; set; }
}
