using System;

namespace DtgeCore;

/**
 * A class for handling errors within DTGE in a more central place. This can get around
 * limitations of specific technologies around DTGE Core (such  as Godot's tendency to quietly eat
 * exceptions).
 * 
 * This will also eventually allow errors to be split cleanly into two types of errors.
 * Engineering errors will be errors that are likely the fault of the engine in some way and need
 * to be resolved with a workaround and/or logged as a bug to be eventually fixed. Authoring
 * errors will be errors that are most likely the fault of the author, not the engine, and should
 * be sent to the author rather than the DTGE team.
 * 
 * This is also set up so that a sinngle callback point can be registered to handle an error more
 * gracefully based on the technology around DTGE Core. This is also the entry point where
 * relevent information will eventually be obtained about the state of the game, which can be
 * submitted alongside the bug report.
 * 
 * At the moment, this will also be used in DtgeGame and DtgeEditor since those have similar
 * issues, but they should likely do something different.
 * 
 * This will be handled in DTGE-73 and existing code should be refactored to better leverage it in
 * DTGE-107.
 */
public class GlobalErrorHandler
{
	private static Action<string> errorCallback;

	public static void InvokeError(string message)
	{
		if (System.Diagnostics.Debugger.IsAttached)
		{
			System.Diagnostics.Debugger.Break();
		}

		if (errorCallback != null)
		{
			errorCallback(message);
		}
	}

	public static void InvokeErrorIf(bool isAnError,  string message)
	{
		if (isAnError)
		{
			InvokeError(message);
		}
	}

	public static bool RegisterErrorCallback(Action<string> callback)
	{
		bool successfullyRegisteredCallback = false;

		if (errorCallback == null)
		{
			errorCallback = callback;
			successfullyRegisteredCallback = true;
		}

		return successfullyRegisteredCallback;
	}
}
