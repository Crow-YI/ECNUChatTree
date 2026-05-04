namespace TreeChat.Services
{
    public class ApiConfigData
    {
        public string ApiKey { get; set; } = "";
        public string ApiEndpoint { get; set; } = "https://chat.ecnu.edu.cn/open/api/v1/chat/completions";
        public string ModelName { get; set; } = "ecnu-plus";
        public double Temperature { get; set; } = 0.7;
        public double TopP { get; set; } = 0.8;
        public int TopK { get; set; } = 20;
    }
}