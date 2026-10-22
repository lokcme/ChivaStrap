using System.Collections.Generic;

namespace Chivas.UI.Elements.Settings.Pages;

public static class ExtensionCatalog
{
	public const string FleasionIcon = "https://avatars.githubusercontent.com/u/186699266";

	public const string RiShadeIcon = "https://raw.githubusercontent.com/lokcme/RiShade/main/Images/RiShde.png";

	public const string ApiDumpIcon = "https://raw.githubusercontent.com/MaximumADHD/Roblox-API-Dump-Tool/master/Resources/AppLogo.png";

	public const string RojoIcon = "https://github.com/rojo-rbx.png";

	public const string ChivasIcon = "pack://application:,,,/Chivas.png";

	public static readonly string[] Categories = { ExtensionViewModel.TypeExtensions, ExtensionViewModel.TypeStudio };

	public static readonly string[] Targets = { "Roblox Player", "Roblox Studio", "Classic clients" };

	public static List<ExtensionEntry> Build(ExtensionViewModel vm)
	{
		return new List<ExtensionEntry>
		{
			new ExtensionEntry("rishade", () => vm.rishadeenabler, value => vm.rishadeenabler = value)
			{
				Name = "RiShade",
				Author = "Chivas",
				AuthorIcon = RiShadeIcon,
				Type = ExtensionViewModel.TypeExtensions,
				WorksWith = ["Roblox Player"],
				Tags = ["Shaders", "Visuals", "Screenshots"],
				Summary = "Devuelve los shaders a Roblox para que se vea mejor. F8 abre su ventana. Mejor para fotos que para jugar diario.",
				Description = [
					"RiShade mete post-procesado a Roblox con bloom, tonos, color y nitidez que Roblox ya no trae.",
					"Todo se ajusta en su ventana. Ábrelo con F8 o desde aquí, mueve sliders y mira en vivo.",
					"Los shaders cuestan cuadros, mejor para fotos y videos que para competir."
				],
				Icon = RiShadeIcon,
				CanOpen = true,
				SearchText = "RiShade shaders visuals effects bloom tonemap panel F8"
			},
			new ExtensionEntry("apidump", () => vm.apidumpenabler, value => vm.apidumpenabler = value)
			{
				Name = "Roblox API Dump Tool",
				Author = "MaximumADHD",
				AuthorIcon = ApiDumpIcon,
				Type = ExtensionViewModel.TypeExtensions,
				WorksWith = ["Roblox Player", "Roblox Studio"],
				Tags = ["API", "Developer", "Diff"],
				Summary = "Explora clases y enums de la API y mira qué cambió entre versiones.",
				Description = [
					"Lee el dump que trae cada Roblox y muestra clases, propiedades, funciones, eventos y enums en un árbol con buscador.",
					"Compara versiones para ver qué agregó, quitó o cambió Roblox, útil si un script se rompe tras update.",
					"Chivas baja una copia verificada y la guarda junto a tu Roblox instalado."
				],
				Icon = ApiDumpIcon,
				Source = "https://github.com/MaximumADHD/Roblox-API-Dump-Tool",
				CanOpen = true,
				SearchText = "Roblox API Dump Tool MaximumADHD api dump diff classes members enums"
			},
			new ExtensionEntry("rojo", () => vm.rojoenabler, value => vm.rojoenabler = value)
			{
				Name = "Rojo",
				Author = "rojo-rbx",
				AuthorIcon = RojoIcon,
				Type = ExtensionViewModel.TypeStudio,
				WorksWith = ["Roblox Studio"],
				Tags = ["Sync", "Git", "Build"],
				Summary = "Sincroniza tu código a Roblox Studio en vivo.",
				Description = [
					"Rojo te deja escribir Luau en tu editor y mantiene Studio sincronizado al guardar. Tu proyecto vive en disco, así usas Git y todo lo de tu editor.",
					"Chivas instala Rojo por ti y agrega herramientas: crear proyecto, prender o parar sync, compilar y poner el plugin, todo desde aquí."
				],
				Icon = RojoIcon,
				Source = "https://github.com/rojo-rbx/rojo",
				CanOpen = false,
				HasTools = true,
				SearchText = "Rojo filesystem sync studio project git version control serve build init plugin"
			},
		};
	}
}
