using Chivas.Models.Attributes;

namespace Chivas.Enums.FlagPresets;

public enum LightingMode
{
	Default,
	Voxel,
	ShadowMap,
	Future,
	[EnumName(StaticName = "Unified (Phase 4)")]
	Unified
}
