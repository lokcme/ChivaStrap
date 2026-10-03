using Chivas.Models.Attributes;

namespace Chivas.Enums;

public enum BootstrapperStyle
{
	VistaDialog,
	LegacyDialog2008,
	LegacyDialog2011,
	ProgressDialog,
	ClassicFluentDialog,
	ByfronDialog,
	[EnumName(StaticName = "Chivas")]
	FluentDialog,
	FluentAeroDialog,
	CustomDialog
}
