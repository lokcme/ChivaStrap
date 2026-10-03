using System;

namespace Chivas.Models.Attributes;

[AttributeUsage(AttributeTargets.Field)]
internal class EnumSortAttribute : Attribute
{
	public int Order { get; set; }
}
