using System;

namespace DtgeCore;

/**
 * A class for handling errors within DTGE Core. Code within Core should use the Invoke functions
 * to initiate errors that cannot be recovered from during runtime similar to Exceptions.
 * Exceptions aren't used to make this error handling a bit more agnostic to specific technologies
 * (for example, Godot's mix of managed and native code can cause exceptions to fail mostly
 * quietly). It also gives shell code a clear way to hook into errors without needing many try/
 * catch blocks everywhere.
 * 
 * Code within DTGE core should utilize these error functions to verify the validity of actions
 * that might throw exceptions so that such errors can travel through this function instead of
 * the usual C# Exception flow. If exceptions are hit while developing Core code, these errors
 * should be used to prevent that issue. In addition to invoking the correct kind of error with
 * a descriptive message, DTGE Core code should ensure that no modifications are made to Scenes,
 * Entities, or any other state if an error has been reached.
 * 
 * This class supports several kinds of errors to be used in different situations:
 *   - Initialization errors - These are thrown mainly when constructing elements from
 *     Serializables coming in from Json. They indicate that there's a serious error in the game
 *     and that it cannot be run at all (or at least that said scene, entity definition, etc.
 *     cannot be used).
 *   - Play errors - These are thrown during the course of playing the game. This is most common
 *     when attempting to change to a new scene, and the game should be fine to continue running
 *     from the previous scene if the player so desires.
 *     
 *     
 * The class also provides a few useful shorthands for common error checking, such as checking to
 * see if an index is valid given the count of a collection.
 */
public class CoreErrorHandler
{
	private static Action<string> initializationErrorCallback;
	private static Action<string> runtimeErrorCallback;

	public static void InvokeInitializationError(string message)
	{
		if (System.Diagnostics.Debugger.IsAttached)
		{
			System.Diagnostics.Debugger.Break();
		}

		if (initializationErrorCallback != null)
		{
			initializationErrorCallback(message);
		}
	}

	public static bool RegisterInitializationErrorCallback(Action<string> callback)
	{
		bool successfullyRegisteredCallback = false;

		if (initializationErrorCallback == null)
		{
			initializationErrorCallback = callback;
			successfullyRegisteredCallback = true;
		}

		return successfullyRegisteredCallback;
	}

	public static void InvokePlayError(string message)
	{
		if (System.Diagnostics.Debugger.IsAttached)
		{
			System.Diagnostics.Debugger.Break();
		}

		if (runtimeErrorCallback != null)
		{
			runtimeErrorCallback(message);
		}
	}

	public static bool RegisterPlayErrorCallback(Action<string> callback)
	{
		bool successfullyRegisteredCallback = false;

		if (runtimeErrorCallback == null)
		{
			runtimeErrorCallback = callback;
			successfullyRegisteredCallback = true;
		}

		return successfullyRegisteredCallback;
	}

	public static bool IsValidIndex(int index, int collectionCount)
	{
		return 0 <= index && index < collectionCount;
	}
}
