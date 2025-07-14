using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DtgeCore;

/**
 * A SUID, short for Scene Unique Identifier, is used to  identify elements within a scene. It's
 * assigned at edit time, and its main purpose is to provide a consistent way to identify elements
 * instead of more fluid alternatives like indices or author visible names. SUIDs should only be
 * obtained by the Scene's GetNewSUID() function and never created manually.
 */
[JsonConverter(typeof(SUIDConverter))]
public class SUID
{
	private readonly int suid;
	public static SUID None = new SUID(0);
	public const int FIRST_VALID_SUID = 1;

	public SUID(int suid)
	{
		this.suid = suid;
	}

	public static bool operator ==(SUID left, SUID right)
	{
		bool equals = false;
		bool leftIsNull = Object.ReferenceEquals(left, null);
		bool rightIsNull = object.ReferenceEquals(right, null);
		
		if (leftIsNull && rightIsNull)
		{
			equals = true;
		}
		else if (leftIsNull || rightIsNull)
		{
			equals = false;
		}
		else
		{
			equals = left.suid == right.suid;
		}

		return equals;
	}

	public static bool operator !=(SUID left, SUID right)
	{
		return !(left == right);
	}

	public override bool Equals(object other)
	{
		bool isEqual = false;

		if (other.GetType() == typeof(SUID))
		{
			isEqual = this == (SUID)other;
		}

		return isEqual;
	}

	public override int GetHashCode()
	{
		return this.suid;
	}

	public int ToInt()
	{
		return this.suid;
	}
}

public class SUIDConverter : JsonConverter<SUID>
{
	public override SUID Read(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options)
	{
		return new SUID(reader.GetInt32());
	}

	public override void Write(
		Utf8JsonWriter writer,
		SUID value,
		JsonSerializerOptions options)
	{
		if (value == null)
		{
			JsonSerializer.Serialize(writer, null, options);
		}
		else
		{
			JsonSerializer.Serialize(writer, value.ToInt(), options);
		}
	}

	public override SUID ReadAsPropertyName(
		ref Utf8JsonReader reader,
		Type typeToConvert,
		JsonSerializerOptions options)
	{
		return new SUID(Int32.Parse(reader.GetString()));
	}

	public override void WriteAsPropertyName(
		Utf8JsonWriter writer,
		[DisallowNull] SUID value,
		JsonSerializerOptions options)
	{
		if (value == null)
		{
			writer.WritePropertyName("null");
		}
		else
		{
			writer.WritePropertyName(value.ToInt().ToString());
		}
	}
}
