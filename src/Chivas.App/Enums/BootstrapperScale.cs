using Chivas.Models.Attributes;

namespace Chivas.Enums;

public enum BootstrapperScale
{
	[EnumName(FromTranslation = "Menu.Appearance.Page.ScaleCompact")]
	Compact,
	[EnumName(FromTranslation = "Menu.Appearance.Page.ScaleNormal")]
	Normal,
	[EnumName(FromTranslation = "Menu.Appearance.Page.ScaleLarge")]
	Large
}
