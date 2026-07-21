using System.Text.Json;
using System.Text;
using System.Reflection;

namespace FF.Architecture.Providers;

public class JsonProvider
{
	/// <typeparam name="T">
	/// <param name="bytes">
	public static async Task<T> DeserializeAsync<T>(byte[] bytes) where T : class
	{
		using MemoryStream stream = new(bytes);
		T? deserialized = await JsonSerializer.DeserializeAsync<T>(stream);
		return deserialized!;
	}

	/// <typeparam name="T">
	/// <param name="content">

	public static T? DeserializeSimple<T>(string content) where T : class
	{
		return JsonSerializer.Deserialize<T>(content, GetJsonSerializerOptions());
	}

	/// <typeparam name="T">
	/// <param name="content">
	public static async Task<T> DeserializeAsync<T>(string content) where T : class
	{
		byte[] bytes = Encoding.UTF8.GetBytes(content);
		return await DeserializeAsync<T>(bytes);
	}

	/// <param name="content">

	public static string Serialize(object content)
	{
		var serialized = JsonSerializer.Serialize(content);
		return serialized;
	}

	private static JsonSerializerOptions GetJsonSerializerOptions()
	{
		return new JsonSerializerOptions
		{
			AllowTrailingCommas = true,
			PropertyNameCaseInsensitive = true,
		};
	}
}
