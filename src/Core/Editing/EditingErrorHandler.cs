using System;

namespace DtgeCore.Editing;

/**
 * A class for handling errors within DtgeCore.Editing, meant to handle errors that are exclusvive
 * to editing. Code within DtgeCore.Editing should use the Invoke functions to initiate errors
 * that cannot be recovered from exactlyk how CoreErrorHandler is used.
 * 
 * The main difference is that this provides a different set of errors that are intended to be
 * hooked into by Editor shells instead of Game shells. These erors are:
 *   - Load errors - These are thrown mainly when constructing elemetns from Serializables coming
 *     in from Json. They indicate that there's a serious error in the editor or saved file such
 *     that the editor cannot load the file correctly.
 *   - Save errors - These are thrown when creating Serializables or when serializing them to
 *     Json. They indicate that the saving failed in some way due to a bug in the code.
 *   - Illegal operation errors - These are thrown when the editor attempts to perform an illegal
 *     operation. Editor shells should be designed to prevent users from making these kinds of
 *     serious errors, such as sanitizing user input when needed, disabling UI elements when they
 *     would perform an invalid operation, etc. These are thrown directly as a result of a call
 *     to the editables that is an illegal operation.
 *   - Editing error - These are more general errors for when the editor has gotten into a bad
 *     state. While illegal operation errors are generally an indication of an issue in shell
 *     code, these are likely due to a bug in DtgeCore.Editing code.
 */
public class EditingErrorHandler
{
	private static Action<string> loadErrorCallback;
	private static Action<string> saveErrorCallback;
	private static Action<string> illegalOperationErrorCallback;
	private static Action<string> editingErrorCallback;

	public static void InvokeLoadError(string message)
	{
		if (System.Diagnostics.Debugger.IsAttached)
		{
			System.Diagnostics.Debugger.Break();
		}

		if (loadErrorCallback != null)
		{
			loadErrorCallback(message);
		}
	}

	public static bool RegisterLoadErrorCallback(Action<string> callback)
	{
		bool successfullyRegisteredCallback = false;

		if (loadErrorCallback == null)
		{
			loadErrorCallback = callback;
			successfullyRegisteredCallback = true;
		}

		return successfullyRegisteredCallback;
	}

	public static void InvokeSaveError(string message)
	{
		if (System.Diagnostics.Debugger.IsAttached)
		{
			System.Diagnostics.Debugger.Break();
		}

		if (saveErrorCallback != null)
		{
			saveErrorCallback(message);
		}
	}

	public static bool RegisterSaveErrorCallback(Action<string> callback)
	{
		bool successfullyRegisteredCallback = false;

		if (saveErrorCallback == null)
		{
			saveErrorCallback = callback;
			successfullyRegisteredCallback = true;
		}

		return successfullyRegisteredCallback;
	}

	public static void InvokeIllegalOperationError(string message)
	{
		if (System.Diagnostics.Debugger.IsAttached)
		{
			System.Diagnostics.Debugger.Break();
		}

		if (illegalOperationErrorCallback != null)
		{
			illegalOperationErrorCallback(message);
		}
	}

	public static bool RegisterIllegalOperationErrorCallback(Action<string> callback)
	{
		bool successfullyRegisteredCallback = false;

		if (illegalOperationErrorCallback == null)
		{
			illegalOperationErrorCallback = callback;
			successfullyRegisteredCallback = true;
		}

		return successfullyRegisteredCallback;
	}

	public static void InvokeEditingError(string message)
	{
		if (System.Diagnostics.Debugger.IsAttached)
		{
			System.Diagnostics.Debugger.Break();
		}

		if (editingErrorCallback != null)
		{
			editingErrorCallback(message);
		}
	}

	public static bool RegisterEditingErrorCallback(Action<string> callback)
	{
		bool successfullyRegisteredCallback = false;

		if (editingErrorCallback == null)
		{
			editingErrorCallback = callback;
			successfullyRegisteredCallback = true;
		}

		return successfullyRegisteredCallback;
	}
}
