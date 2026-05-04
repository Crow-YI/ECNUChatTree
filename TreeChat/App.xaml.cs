using System.Windows;
using TreeChat.Services;

namespace TreeChat
{
    public partial class App : Application
    {
        /// <summary>
        /// 启动时传入的文件路径
        /// </summary>
        public static string? StartupFilePath { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length > 0)
            {
                StartupFilePath = e.Args[0];
            }
            base.OnStartup(e);
            // 应用启动时读取保存的配置
            ApiConfig.LoadFromFile();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 应用关闭时保存当前配置
            ApiConfig.SaveToFile();
            base.OnExit(e);
        }
    }
}