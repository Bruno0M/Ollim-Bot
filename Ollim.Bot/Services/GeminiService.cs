using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Text;
using System.Text.Json;

namespace Ollim.Bot.Services
{
    public class GeminiService
    {
        private string _apiKey = "AIzaSyCxAppyEo0m6eofnO5y8ecAc8pCK0_0kO0";
        private string _baseUrl = "https://generativelanguage.googleapis.com";

        public GeminiService()
        {
        }
        public GeminiService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<string> GenerateContent(string prompt, string fileUri = null, string filePath = null)
        {
            HttpClient client = new HttpClient();
            var apiUrl = $"{_baseUrl}/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

            var parts = new List<object>
            {
                new { text = $"Answer this: {prompt}, But under no circumstances should you exceed 1000 characters, always keep to this limit or below in your answers" }
            };

            if (!string.IsNullOrEmpty(fileUri))
            {
                parts.Add(new
                {
                    file_data = new
                    {
                        mime_type = "application/pdf",
                        file_uri = fileUri
                    }
                });
            }

            var data = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = parts.ToArray()
                        //Posso abstrair isso transformando em uma library ou utilizando as abstrações desse serviço em outra classe.
                    }
                }
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            Console.WriteLine(jsonContent);


            HttpResponseMessage response = await client.PostAsync(apiUrl, jsonContent);


            string responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine(responseBody);
            dynamic? jsonObj = JsonConvert.DeserializeObject<ExpandoObject>(responseBody, new ExpandoObjectConverter());

            string text = jsonObj.candidates[0].content.parts[0].text;

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            Console.WriteLine(text);
            return text;
        }

        public async Task<string> UploadFile(string path, string filePath, string mediaType)
        {
            HttpClient client = new HttpClient();

            var responsePath = await client.GetAsync(path);
            using (var stream = await responsePath.Content.ReadAsStreamAsync())
            using (var fileStream = File.Create(filePath))
            {
                await stream.CopyToAsync(fileStream);
            }

            string tmpHeaderFile = "upload-header.tmp";
            long numBytes = new FileInfo(filePath).Length;

            var apiUrl = $"{_baseUrl}/upload/v1beta/files?key={_apiKey}";

            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

            request.Headers.Add("X-Goog-Upload-Protocol", "resumable");
            request.Headers.Add("X-Goog-Upload-Command", "start");
            request.Headers.Add("X-Goog-Upload-Header-Content-Length", numBytes.ToString());
            request.Headers.Add("X-Goog-Upload-Header-Content-Type", "application/pdf");
            request.Content = new StringContent(
                JsonConvert.SerializeObject(new { file = new { display_name = "TEXT" } }),
                Encoding.UTF8,
                "application/json");


            HttpResponseMessage response = await client.SendAsync(request);


            var uploadUrl = response.Headers.GetValues("X-Goog-Upload-URL").FirstOrDefault();

            var uploadRequest = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            uploadRequest.Headers.Add("X-Goog-Upload-Command", "upload, finalize");
            uploadRequest.Headers.Add("X-Goog-Upload-Offset", "0");
            uploadRequest.Content = new ByteArrayContent(File.ReadAllBytes(filePath));
            uploadRequest.Content.Headers.ContentLength = numBytes;

            var uploadResponse = await client.SendAsync(uploadRequest);
            var fileInfo = await uploadResponse.Content.ReadAsStringAsync();
            var fileInfoJson = JsonDocument.Parse(fileInfo);
            var fileUri = fileInfoJson.RootElement.GetProperty("file").GetProperty("uri").GetString();

            Console.WriteLine($"file_uri={fileUri}");
            return fileUri;
        }
    }
}