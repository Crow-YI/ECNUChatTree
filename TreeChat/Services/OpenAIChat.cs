using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TreeChat.Models;

namespace TreeChat.Services
{
    public class OpenAIChat
    {
        private readonly HttpClient _httpClient;

        public OpenAIChat()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ApiConfig.ApiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// 调用AI接口（传入上下文）
        /// </summary>
        /// <param name="context">完整上下文</param>
        public async Task<string> CallAiApi(List<ChatMessage> context)
        {
            List<OpenAIMessage> tempList = new List<OpenAIMessage>();
            foreach (ChatMessage message in context)
            {
                OpenAIMessage item = new OpenAIMessage { role = message.Role, content = message.Content };
                tempList.Add(item);
            }

            try
            {
                // 构造强类型请求体
                var request = new ChatCompletionRequest
                {
                    model = ApiConfig.ModelName,
                    temperature = ApiConfig.Temperature,
                    top_p = ApiConfig.TopP,
                    top_k = ApiConfig.TopK,
                    messages = tempList
                };

                // 序列化强类型对象
                var jsonContent = new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json");

                // 请求与解析返回内容
                var response = await _httpClient.PostAsync(ApiConfig.ApiEndpoint, jsonContent);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                dynamic responseData = JsonConvert.DeserializeObject(responseJson);
                return responseData.choices[0].message.content.ToString().Trim();
            }
            catch (Exception ex)
            {
                return $"API调用失败：{ex.Message}";
            }
        }
    }
}
