using Chivas.Models.Attributes;

namespace Chivas.Enums.FlagPresets;

public enum ProfileMode
{
	[EnumName(FromTranslation = "Common.Automatic")]
	Default,
	[EnumName(StaticName = "Chivass Official")]
	Chivas,
	[EnumName(StaticName = "Stoofs")]
	Stoof
}
