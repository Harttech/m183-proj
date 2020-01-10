using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Forum.Business.Helpers
{
	public static class SmsAuthHelper
	{
		private const string Url = @"https://europe-west1-gibz-informatik.cloudfunctions.net/send_2fa_sms";
		
		public static async Task<string> RequestToken(string recipient)
		{
			var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes("19_20.m183.mharter183"));
			try
			{
				var header = new Dictionary<string, object>
				{
					{"Authorization", auth },
					{"Content-Type", "application/json" }
				};

				var body = new Dictionary<string, object>
				{
					{"recipient", recipient },
					{"length", 6 },
					{"flash", false }
				};

				var response = await ApiRequest.SendRequestAsync<string>(Url, header, body, RequestKind.Post);
				dynamic dynamicJson = JsonSerializer.Deserialize<ExpandoObject>(response);
				var token = dynamicJson.token.ToString();
				return token;
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}
	}
}
