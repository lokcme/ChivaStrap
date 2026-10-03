using Chivas.Models.Attributes;

namespace Chivas.Enums;

public enum Theme
{
	[EnumName(FromTranslation = "Common.SystemDefault")]
	Default,
	Dark,
	Light,
	Chivas,
	UltraGray,
	Berry,
	Blue,
	Cyan,
	Green,
	Orange,
	Pink,
	Purple,
	Red,
	Yellow,
	Custom
}
