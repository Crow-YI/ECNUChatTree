using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeChat.Services
{
    public class ApiConfig
    {
        // 基础配置
        public static string ApiKey = "";
        public static string ApiEndpoint = "https://chat.ecnu.edu.cn/open/api/v1/chat/completions";
        public static string ModelName = "ecnu-plus";

        // 模型生成参数
        public static double Temperature = 0.7;   // 随机性
        public static double TopP = 0.8;          // 核采样
        public static int TopK = 20;              // 候选词数量
    }
}
