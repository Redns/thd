namespace WaveViewer
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += ContentPage_Loaded;
        }

        private async void ContentPage_Loaded(object? sender, EventArgs? e)
        {
#if WINDOWS
        // 获取 webView 句柄
        var webView = blazorWebView.Handler?.PlatformView as Microsoft.UI.Xaml.Controls.WebView2;
        if(webView == null)
        {
            return;
        }
        await webView.EnsureCoreWebView2Async();

        // 禁用鼠标缩放
        var webViewSettings = webView.CoreWebView2.Settings;
        webViewSettings.IsZoomControlEnabled = false;
        webViewSettings.AreBrowserAcceleratorKeysEnabled = false;
#else
        await Task.Delay(10);
#endif
        }
    }
}
