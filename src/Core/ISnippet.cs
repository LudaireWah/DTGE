using System;
using System.Net.NetworkInformation;

namespace DtgeCore;

public static class Snippet
{
	public enum Mode
	{
		Simple,
		Subscene,
		Random
	}
}

public interface ISnippet
{
	public Snippet.Mode Mode { get; }

	public abstract string CalculateText();
}
