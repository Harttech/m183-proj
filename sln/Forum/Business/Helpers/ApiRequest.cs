using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Forum.Business.Helpers
{
	public enum RequestKind
	{
		Get,
		Post
	}

	public static class ApiRequest
	{
		/// <summary>
		/// Sends an API request to the specified URL and returns the response as the specified value if possible.
		/// </summary>
		/// <typeparam name="T">Type of the expected return value.</typeparam>
		/// <param name="url">The URL which the request should be sent to.</param>
		/// <param name="parameters">Parameters to be sent with the request.</param>
		/// <param name="kind">How the body of the request should be transmitted.</param>
		/// <exception cref="UriFormatException">When <paramref name="url"/> can't be parsed to a URL.</exception>
		/// <exception cref="InvalidCastException">The response can't be parsed to the type <see cref="T"/>.</exception>
		/// <exception cref="ArgumentNullException">When The url is null.</exception>
		/// <returns>The response parsed to the specified type, if possible.</returns>
		public static async Task<T> SendRequestAsync<T>(string url, Dictionary<string, object> header, Dictionary<string, object> parameters, RequestKind kind)
		{
			if (url.NullOrWhiteSpace())
				throw new ArgumentNullException(nameof(url));

			var client = new HttpClient();

			if (header.ContainsKey("Authorization"))
			{
				client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(header["Authorization"].ToString());
				header.Remove("Authorization");
			}
			var contentType = "application/json";
			if (header.ContainsKey("Content-Type"))
			{
				contentType = header["Content-Type"].ToString();
				header.Remove("Content-Type");
			}

			foreach (var headerPair in header)
			{
				client.DefaultRequestHeaders.Add(headerPair.Key, headerPair.Value.ToString());
			}

			if (kind == RequestKind.Get)
			{
				var param = new List<string>();
				foreach (var parameter in parameters)
				{
					param.Add($"{parameter.Key}={parameter.Value}");
				}

				var path = url.Trim('/', '\\').ToLowerInvariant().Slugify();

				if (param.Count > 0)
				{
					path += "?" + string.Join('&', param);
					path = path.Slugify().ToLowerInvariant();
				}

				if (Uri.TryCreate(path, UriKind.Absolute, out var requestUri))
				{
					var response = await client.GetAsync(requestUri);
					if (response.IsSuccessStatusCode)
					{
						var responseString = await response.Content.ReadAsStringAsync();
						var parsed = (T)Convert.ChangeType(responseString, typeof(T));
						return parsed;
					}

					throw new HttpRequestException($"The server responded with the statuscode {response.StatusCode}: {response.ReasonPhrase}.");
				}

				throw new UriFormatException($"The string \"{url}\" is not a valid URL.");
			}
			else
			{
				if (Uri.TryCreate(url, UriKind.Absolute, out var requestUri))
				{
					var jsonBody = JsonSerializer.Serialize(parameters);
					var response = await client.PostAsync(requestUri, new StringContent(jsonBody, Encoding.UTF8, contentType));
					if (response.IsSuccessStatusCode)
					{
						var responseString = await response.Content.ReadAsStringAsync();
						var parsed = (T)Convert.ChangeType(responseString, typeof(T));
						return parsed;
					}

					throw new HttpRequestException($"The server responded with the statuscode {response.StatusCode}: {response.ReasonPhrase}.");
				}

				throw new UriFormatException($"The string \"{url}\" is not a valid URL.");
			}
		}
	}
}
