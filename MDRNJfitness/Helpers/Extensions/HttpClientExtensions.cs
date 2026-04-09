using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace MDRNJfitness.Helpers.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<T?> GetAsync<T>(this HttpClient http, string url)
        {
            var response = await http.GetAsync(url);
            await EnsureSuccessAsync(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task<T?> PostAsync<T>(this HttpClient http, string url, object body)
        {
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var response = await http.PostAsync(url, content);
            await EnsureSuccessAsync(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task PostAsync(this HttpClient http, string url, object body)
        {
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var response = await http.PostAsync(url, content);
            await EnsureSuccessAsync(response);
        }

        public static async Task<T?> PutAsync<T>(this HttpClient http, string url, object body)
        {
            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
            var response = await http.PutAsync(url, content);
            await EnsureSuccessAsync(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task DeleteAsync(this HttpClient http, string url)
        {
            var response = await http.DeleteAsync(url);
            await EnsureSuccessAsync(response);
        }

        // Reads the API error body so the alert shows the real message instead of
        // a generic "Response status code does not indicate success: 500" string.
        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            var body = await response.Content.ReadAsStringAsync();
            string message;
            try
            {
                var obj = JObject.Parse(body);
                message = obj["error"]?.ToString()
                       ?? obj["message"]?.ToString()
                       ?? obj["title"]?.ToString()
                       ?? body;
            }
            catch
            {
                message = string.IsNullOrWhiteSpace(body)
                    ? $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}"
                    : body;
            }

            throw new HttpRequestException(message, null, response.StatusCode);
        }
    }
}
