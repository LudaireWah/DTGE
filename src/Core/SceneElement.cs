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

    protected void CopyFrom(SceneElement other)
    {
        this.Id = other.Id;
    }

	public static bool operator ==(SceneElement left, SceneElement right)
	{
		return left.Id == right.Id;
	}

	public static bool operator !=(SceneElement left, SceneElement right)
	{
		return !(left == right);
	}

	public override bool Equals(object other)
	{
		bool isEqual = false;
		SceneElement sceneElement = other as SceneElement;

		if (sceneElement != null)
		{
			isEqual = this == (SceneElement)other;
		}

		return isEqual;
	}

	public override int GetHashCode()
	{
		return this.Id.GetHashCode();
	}
}
