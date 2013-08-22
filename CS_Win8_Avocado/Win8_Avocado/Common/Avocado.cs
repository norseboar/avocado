using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.Data.Json;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;


namespace Win8_Avocado.Common
{
    /// <summary>
    /// Contains several static methods for accessing the Avocado API. Handles all requests to and from Avocado
    /// </summary>
    public static class Avocado
    {
        // Developer id info
	// DEV ID AND KEY NOT AVAILABLE FOR PRIVACY REASONS
        private const String DEV_ID = "";
        private const String DEV_KEY = "";

        // Avocado API urls
        private const String BASE_URL = "https://avocado.io/api";
        private const String LOGIN_URL = BASE_URL + "/authentication/login";
        private const String LOGOUT_URL = BASE_URL + "/authentication/logout";
        private const String ACTIVITIES_URL = BASE_URL + "/activities";
        private const String MESSAGE_URL = BASE_URL + "/conversation";
        private const String USERS_URL = BASE_URL + "/user";

        // API constants
        private const String COOKIE_NAME = "user_email";
        public const String EMAIL = "email";
        public const String PASSWORD = "password";

        /// <summary>
        /// Logs user in to avocado, sets a global auth token to be used on future api calls until logout
        /// TODO: implement some kind of return
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public async static Task<string> Login(String email, String password)
        {
            // Necessary for HTTP calls because of AsTask
            // TODO: understand what AsTask is
            var cts = new CancellationTokenSource();
            var content = new Dictionary<String, String> { { "email", email }, { "password", password } };
            var encodedContent = new HttpFormUrlEncodedContent(content);
            var client = new HttpClient();
            var response = await client.PostAsync(new Uri(LOGIN_URL), encodedContent).AsTask(cts.Token);

            // Get cookie for auth token
            var filter = new HttpBaseProtocolFilter();
            var cookieCollection = filter.CookieManager.GetCookies(new Uri(LOGIN_URL));
            var cookieValue = "";
            foreach (var cookie in cookieCollection)
            {
                if (cookie.Name.Equals(COOKIE_NAME))
                {
                    cookieValue = cookie.Value;
                    break;
                }
            }

            if (String.IsNullOrEmpty(cookieValue))
            {
                // TODO: Make this meaningful and robust
                throw new Exception();
            }
            
            // Generate auth token
            var utf8Buf = CryptographicBuffer.ConvertStringToBinary(cookieValue + DEV_KEY, BinaryStringEncoding.Utf8);
            var provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            var hashBuf = provider.HashData(utf8Buf);
            var authToken = DEV_ID + ":" + CryptographicBuffer.EncodeToHexString(hashBuf);

            return authToken;
        }

        public async static void Logout(string authToken)
        {
            var cts = new CancellationTokenSource();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-AvoSig", authToken);
            var response = await client.GetAsync(new Uri(LOGOUT_URL)).AsTask(cts.Token);
        }

        public async static void SendMessage(string authToken, string message)
        {
            // Necessary for HTTP calls because of AsTask
            // TODO: understand what AsTask is
            var cts = new CancellationTokenSource();
            var content = new Dictionary<String, String> { { "message", message }};
            var encodedContent = new HttpFormUrlEncodedContent(content);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-AvoSig", authToken);
            var response = await client.PostAsync(new Uri(MESSAGE_URL), encodedContent).AsTask(cts.Token);
        }

        public async static Task<JsonArray> GetActivities(string authToken, long lastActivityTime)
        {
            var cts = new CancellationTokenSource();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-AvoSig", authToken);
            HttpResponseMessage response;
            if (lastActivityTime == 0)
            {
                response = await client.GetAsync(new Uri(ACTIVITIES_URL)).AsTask(cts.Token);            }
            else
            {
                response = await client.GetAsync(new Uri(ACTIVITIES_URL + "?after=" + lastActivityTime)).AsTask(cts.Token);
            }
            var content = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine(content);

            if (String.IsNullOrEmpty(content))
            {
                throw new Exception();
            }
            JsonArray activitiesArray;
            JsonArray.TryParse(content, out activitiesArray);
            if (activitiesArray == null)
            {
                throw new Exception();
            }

            return activitiesArray;
        }

        public async static Task<JsonArray> GetUsers(string authToken)
        {
            var cts = new CancellationTokenSource();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-AvoSig", authToken);
            var response = await client.GetAsync(new Uri(USERS_URL)).AsTask(cts.Token);
            var content = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine(content);

            if (String.IsNullOrEmpty(content))
            {
                throw new Exception();
            }

            JsonArray user;
            JsonArray.TryParse(content, out user);

            if (user == null)
            {
                throw new Exception();
            }

            return user;
        }
    }
}
