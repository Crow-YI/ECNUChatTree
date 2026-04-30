using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using TreeChat.Models;

namespace TreeChat.Services
{
    /// <summary>
    /// 提供与大模型服务器交互的服务类
    /// </summary>
    public class OpenAIChat
    {
        public static OpenAIChat Instance { get; private set; } = new OpenAIChat();

        public OpenAIChat()
        {
        }

        /// <summary>
        /// 调用AI接口（传入上下文和配置）
        /// </summary>
        /// <param name="context">完整上下文</param>
        /// <param name="chatTree">对话树，包含配置信息</param>
        public async Task<string> CallAiApi(List<ChatMessage> context, ChatTree chatTree)
        {
            List<OpenAIMessage> tempList = new List<OpenAIMessage>();
            foreach (ChatMessage message in context)
            {
                OpenAIMessage item = new OpenAIMessage { role = message.Role, content = message.Content };
                tempList.Add(item);
            }

            try
            {
                // 使用传入的配置
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", chatTree.ApiKey);
                    httpClient.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    // 构造强类型请求体
                    var request = new ChatCompletionRequest
                    {
                        model = chatTree.ModelName,
                        temperature = chatTree.Temperature,
                        top_p = chatTree.TopP,
                        top_k = chatTree.TopK,
                        messages = tempList
                    };

                    // 序列化强类型对象
                    var jsonContent = new StringContent(
                        JsonConvert.SerializeObject(request),
                        Encoding.UTF8,
                        "application/json");

                    // 请求与解析返回内容
                    var response = await httpClient.PostAsync(chatTree.ApiEndpoint, jsonContent);
                    response.EnsureSuccessStatusCode();

                    var responseJson = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JsonConvert.DeserializeObject(responseJson);
                    return responseData.choices[0].message.content.ToString().Trim();
                }
            }
            catch (Exception ex)
            {
                return $"API调用失败：{ex.Message}";
            }
        }
    }
}
