using FF.Architecture.Helpers;

namespace FF.Architecture.Providers;

public interface IRestProvider
{

	/// <param name="endpoint">
	/// <param name="id">
	Task<string> DeleteAsync(string endpoint, string id);

	/// <param name="endpoint">
	/// <param name="id">
	Task<string> GetAsync(string endpoint, string? id);

	/// <param name="endpoint">
	/// <param name="content">
	Task<string> PostAsync(string endpoint, string content);

	/// <param name="endpoint">
	/// <param name="requestUri">
	/// <param name="content">
	Task<string> PutAsync(string endpoint, string id, string content);
}

public class RestProvider : IRestProvider
{

    /// <param name="endpoint">
    /// <param name="id">
    public async Task<string> GetAsync(string endpoint, string? id)
	{
		try
		{
			var response = await RestProviderHelpers.CreateHttpClient(endpoint)
				.GetAsync(id);
			return await RestProviderHelpers.GetResponse(response);
		}
		catch (Exception ex)
		{
			throw RestProviderHelpers.ThrowError(endpoint, ex);
		}
	}

	/// <param name="endpoint">
	/// <param name="content">
	public async Task<string> PostAsync(string endpoint, string content)
	{
		try
		{
			var response = await RestProviderHelpers.CreateHttpClient(endpoint)
				.PostAsync(endpoint, RestProviderHelpers.CreateContent(content));
			var result = await RestProviderHelpers.GetResponse(response);
			return result;
		}
		catch (Exception ex)
		{
			throw RestProviderHelpers.ThrowError(endpoint, ex);
		}
	}

	/// <param name="endpoint">
	/// <param name="id">
	/// <param name="content">
	public async Task<string> PutAsync(string endpoint, string id, string content)
	{
		try
		{
			var response = await RestProviderHelpers.CreateHttpClient(endpoint)
				.PutAsync(id, RestProviderHelpers.CreateContent(content));
			var result = await RestProviderHelpers.GetResponse(response);
			return result;
		}
		catch (Exception ex)
		{
			throw RestProviderHelpers.ThrowError(endpoint, ex);
		}
	}

	/// <param name="endpoint">
	/// <param name="id">
	public async Task<string> DeleteAsync(string endpoint, string id)
	{
		try
		{
			var response = await RestProviderHelpers.CreateHttpClient(endpoint)
				.DeleteAsync(id);
			var result = await RestProviderHelpers.GetResponse(response);
			return result;
		}
		catch (Exception ex)
		{
			throw RestProviderHelpers.ThrowError(endpoint, ex);
		}
	}
}
