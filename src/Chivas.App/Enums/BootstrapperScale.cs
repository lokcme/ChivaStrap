using Chivas.Models.Attributes;

namespace Chivas.Enums;

public enum BootstrapperScale
{
	[EnumName(StaticName = "Compact")]
	Compact,
	[EnumName(StaticName = "Normal")]
	Normal,
	[EnumName(StaticName = "Large")]
	Large
}
