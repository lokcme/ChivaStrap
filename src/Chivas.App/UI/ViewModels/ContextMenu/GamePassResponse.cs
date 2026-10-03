using System.Collections.Generic;

namespace Chivas.UI.ViewModels.ContextMenu;

internal class GamePassResponse
{
	public List<GamePassData> GamePasses { get; set; } = new List<GamePassData>();
}
