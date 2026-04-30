using System.Windows;
using TreeChat.Services;

namespace TreeChat.Views
{
    /// <summary>
    /// ConfigDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigDialog : Window
    {
        public string ApiKey { get; set; }
        public string ApiEndpoint { get; set; }
        public string ModelName { get; set; }
        public double Temperature { get; set; }
        public double TopP { get; set; }
        public int TopK { get; set; }

        public string TemperatureDisplay => $"（默认{ApiConfig.Temperature:F1}）";
        public string TopPDisplay => $"（默认{ApiConfig.TopP:F1}）";
        public string TopKDisplay => $"（默认{ApiConfig.TopK}）";

        public ConfigDialog()
        {
            InitializeComponent();
            
            // 设置默认值，API Key为空字符串以保护隐私
            ApiKey = "";
            ApiEndpoint = ApiConfig.ApiEndpoint;
            ModelName = ApiConfig.ModelName;
            Temperature = ApiConfig.Temperature;
            TopP = ApiConfig.TopP;
            TopK = ApiConfig.TopK;
            
            DataContext = this;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}