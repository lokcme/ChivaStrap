using System.Windows.Media;
using Chivas.Enums;
using Chivas.Extensions;

namespace Chivas.Models;

public class BootstrapperIconEntry
{
	public BootstrapperIcon IconType { get; set; }

	public ImageSource ImageSource => IconEx.GetIconSource(IconType);
}
