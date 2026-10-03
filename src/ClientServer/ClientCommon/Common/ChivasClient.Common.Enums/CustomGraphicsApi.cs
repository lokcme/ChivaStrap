using System.ComponentModel;

namespace ChivasClient.Common.Enums;

public enum CustomGraphicsApi
{
	None,
	[Description("DXVK (Vulkan)")]
	DXVK,
	[Description("dgVoodoo (DX11/12)")]
	DgVoodoo
}
