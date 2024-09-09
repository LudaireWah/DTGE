using System.Collections.Generic;

namespace DtgeCore;

/**
 * The SimpleEntityManager is the first implementation of the entity system, which only
 * covers simple boolean values with string keys. Entity values can be stored and retrieved
 * using the key. It also allows you to determine whether or not an entity value has been set,
 * allowing errors to be raised to authors if they attempt to access an entity value that
 * doesn't exist.
 */
public class SimpleEntityManager
{
	private static SimpleEntityManager instance;

	private Dictionary<string, bool> entityValues;

	private SimpleEntityManager()
	{
		this.entityValues = new Dictionary<string, bool>();
	}

	public static SimpleEntityManager GetSimpleEntityManager()
	{
		if (instance == null)
		{
			instance = new SimpleEntityManager();
		}

		return instance;
	}

	public void Clear()
	{
		this.entityValues.Clear();
	}

	public void SetEntityValue(string key, bool value)
	{
		this.entityValues[key] = value;
	}

	public bool HasEntityValue(string key)
	{
		return this.entityValues.ContainsKey(key);
	}

	public bool GetEntityValue(string key)
	{
		return this.entityValues[key];
	}

	public Dictionary<string, bool>.KeyCollection GetEntityKeys()
	{
		return this.entityValues.Keys;
	}
}
