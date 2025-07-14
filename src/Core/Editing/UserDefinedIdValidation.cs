using System;
using System.Text.RegularExpressions;

namespace DtgeCore.Editing;

/**
 * This class is used for validating identifiers that are strings written by authors, usually
 * called names. These string names are used in place of more robust ids so that authors can
 * easily reference them while writing. The most common is the Scene Id. These names also support
 * nesting using '.' such as when a user is identifying a subscene within a scene (it would read
 * "sceneName.subsceneName").
 * 
 * This validator ensures that only certain characters are allowed. While this can be expanded
 * over time (which is one reason for consolidating this in a single class), we should be careful
 * what we add, as keeping things limited can guide authors towards using better, more readable
 * names.
 */
public class UserDefinedNameValidator
{
	private static string validNameRegex = "^[\\w]*$";
	private static string charactersToRemoveFromNameRegex = "[^\\w]+";
	private static string validNestableNameRegex = "^[\\w\\.]*$";
	private static string charactersToRemoveFromNestableNameRegex = "[^\\w\\.]+";

	public static bool IsValidName(string name)
	{
		bool isValidName = false;

		if (name == null)
		{
			EditingErrorHandler.InvokeIllegalOperationError("UserDefinedNameValidator received a null string");
		}
		else
		{
			isValidName = IsValidString(name, validNameRegex);
		}

		return isValidName;
	}

	public static string CorrectName(string name)
	{
		string correctedName = name;

		if (name == null)
		{
			EditingErrorHandler.InvokeIllegalOperationError("UserDefinedNameValidator received a null string");
		}
		else
		{
			correctedName = CorrectString(correctedName, charactersToRemoveFromNameRegex);
		}

		return correctedName;
	}

	public static bool IsValidNestableName(string nestableName)
	{
		bool isValidNestableName = false;

		if (nestableName == null)
		{
			EditingErrorHandler.InvokeIllegalOperationError("UserDefinedNameValidator received a null string");
		}
		else
		{
			isValidNestableName = IsValidString(nestableName, validNestableNameRegex);
		}

		return isValidNestableName;
	}

	public static string CorrectNestableName(string nestableName)
	{
		string correctedNestableName = nestableName;

		if (nestableName == null)
		{
			EditingErrorHandler.InvokeIllegalOperationError("UserDefinedNameValidator received a null string");
		}
		else
		{
			correctedNestableName = CorrectString(nestableName, charactersToRemoveFromNestableNameRegex);
		}

		return correctedNestableName;
	}

	private static bool IsValidString(string stringToCheck, string regex)
	{
		bool isValidString = false;

		try
		{
			Match match = Regex.Match(
				stringToCheck,
				regex,
				RegexOptions.None,
				TimeSpan.FromSeconds(1));

			isValidString = match.Success;
		}
		catch (RegexMatchTimeoutException timeoutException)
		{
			EditingErrorHandler.InvokeIllegalOperationError("UserDefinedNameValidator timed out while checking a string. Exception message: " + timeoutException.Message);
		}

		return isValidString;
	}

	private static string CorrectString(string stringToCorrect, string regexToRemove)
	{
		string correctedString = stringToCorrect;

		try
		{
			correctedString = Regex.Replace(
				stringToCorrect,
				regexToRemove,
				"",
				RegexOptions.None,
				TimeSpan.FromSeconds(1));
		}
		catch (RegexMatchTimeoutException timeoutException)
		{
			EditingErrorHandler.InvokeIllegalOperationError("UserDefinedNameValidator timed out while correcting a string. Exception message: " + timeoutException.Message);
		}

		return correctedString;
	}
}
