using System.Text.Json.Serialization;

namespace Chivas.Models.APIs.Config;

public class SupporterData
{
	[JsonPropertyName("monthly")]
	public SupporterGroup Monthly { get; set; } = new SupporterGroup();

	[JsonPropertyName("oneoff")]
	public SupporterGroup OneOff { get; set; } = new SupporterGroup();
}
