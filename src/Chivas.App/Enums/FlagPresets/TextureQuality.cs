using Chivas.Models.Attributes;

namespace Chivas.Enums.FlagPresets;

public enum TextureQuality
{
	[EnumName(FromTranslation = "Default")]
	Default,
	[EnumName(StaticName = "Lowest")]
	Lowest,
	[EnumName(StaticName = "Low")]
	Low,
	[EnumName(StaticName = "Medium")]
	Medium,
	[EnumName(StaticName = "High")]
	High
}
