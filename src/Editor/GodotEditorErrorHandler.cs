using System;

namespace DtgeEditor;

public class GodotEditorErrorHandler
{
	private static Action<string, string> errorCallback;

	public static void ConnectToDtgeErrors()
	{
		DtgeCore.Editing.EditingErrorHandler.RegisterLoadErrorCallback(loadErrorCallback);
		DtgeCore.Editing.EditingErrorHandler.RegisterSaveErrorCallback(saveErrorCallback);
		DtgeCore.Editing.EditingErrorHandler.RegisterIllegalOperationErrorCallback(illegalOperationErrorCallback);
		DtgeCore.Editing.EditingErrorHandler.RegisterEditingErrorCallback(editingErrorCallback);
		DtgeCore.CoreErrorHandler.RegisterInitializationErrorCallback(initializationErrorCallback);
		DtgeCore.CoreErrorHandler.RegisterPlayErrorCallback(playErrorCallback);
	}

	public static void RegisterErrorCallback(Action<string, string> callback)
	{
		if (errorCallback == null)
		{
			errorCallback = callback;
		}
	}

	public static void InvokeError(string message)
	{
		if (System.Diagnostics.Debugger.IsAttached)
		{
			System.Diagnostics.Debugger.Break();
		}

		if (errorCallback != null)
		{
			errorCallback("Editor Error (General)", message);
		}
	}

	private static void loadErrorCallback(string message)
	{
		if (errorCallback != null)
		{
			errorCallback("DTGE Load Error", message);
		}
	}

	private static void saveErrorCallback(string message)
	{
		if (errorCallback != null)
		{
			errorCallback("DTGE Save Error", message);
		}
	}

	private static void illegalOperationErrorCallback(string message)
	{
		if (errorCallback != null)
		{
			errorCallback("Editor Error (Illegal Operation)", message);
		}
	}

	private static void editingErrorCallback(string message)
	{
		if (errorCallback != null)
		{
			errorCallback("DTGE Editing Error", message);
		}
	}

	private static void initializationErrorCallback(string message)
	{
		if (errorCallback != null)
		{
			errorCallback("DTGE Initialization Error", message);
		}
	}

	private static void playErrorCallback(string message)
	{
		if (errorCallback != null)
		{
			errorCallback("DTGE Play Error", message);
		}
	}
}
