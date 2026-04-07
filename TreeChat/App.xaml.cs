using System.Windows;

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
        }
    }
}