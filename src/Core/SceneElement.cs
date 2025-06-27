using DtgeCore.Serialization;
using System;

namespace DtgeCore;

public abstract class SceneElement
{
    public SUID Id { get; private set; }

    protected Scene ParentScene { get; private set; }

    protected SceneElement(Scene parentScene)
    {
        this.Id = parentScene.GetSUID();
        this.ParentScene = parentScene;
    }

	protected SceneElement(Scene parentScene, SceneElementSerializable sceneElementSerializable)
	{
		this.Id = sceneElementSerializable.Id;
		this.ParentScene = parentScene;
	}

	protected T CreateSerializable<T>() where T : SceneElementSerializable, new()
	{
		T serializable = new T();
		serializable.Id = this.Id;
		return serializable;
	}

    protected void CopyFrom(SceneElement other)
    {
        this.Id = other.Id;
    }

	public static bool operator ==(SceneElement left, SceneElement right)
	{
		bool isEqual = false;
		if (Object.Equals(left, null) && Object.Equals(right, null))
		{
			isEqual = true;
		}
		else if (!Object.Equals(left, null) && !Object.Equals(right, null))
		{
			isEqual = left.Id == right.Id;
		}

		return isEqual;
	}

	public static bool operator !=(SceneElement left, SceneElement right)
	{
		return !(left == right);
	}

	public override bool Equals(object other)
	{
		bool isEqual = false;
		SceneElement otherSceneElement = other as SceneElement;

		if (otherSceneElement != null)
		{
			isEqual = this == otherSceneElement;
		}

		return isEqual;
	}

	public override int GetHashCode()
	{
		return this.Id.GetHashCode();
	}
}
