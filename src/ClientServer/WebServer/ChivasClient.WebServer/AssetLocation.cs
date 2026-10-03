using System.Text.Json.Serialization;

namespace ChivasClient.WebServer;

internal class AssetLocation
{
	[JsonPropertyName("location")]
	public string? Location { get; set; }
}
