using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using LLRPSdk;
using LLRPReaderUI_WPF.Logging;
using LLRPReaderUI_WPF.Models;
using LLRPReaderUI_WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace LLRPReaderUI_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 从配置文件加载 Serilog 配置
            Log.Logger = LoggingConfigurationManager.BuildLogger();

            var services = new ServiceCollection();
            ConfigureServices(services);
            Ioc.Default.ConfigureServices(services.BuildServiceProvider());

            _ = Ioc.Default.GetRequiredService<LlrpLoggingBridge>();

            var mainWindow = Ioc.Default.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.ClearProviders();
                builder.AddSerilog(Log.Logger, dispose: false);
            });

            services.AddSingleton<IAppLogService, AppLogService>();
            services.AddSingleton<LlrpReader>();
            services.AddSingleton<ReaderSettingsStore>();
            services.AddSingleton<ReaderStatusStore>();
            services.AddSingleton<LlrpLoggingBridge>();
            services.AddSingleton<MainWindowViewModel>();

            services.AddTransient<DeviceConnectionViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<GpioViewModel>();
            services.AddTransient<InventoryConfigViewModel>();
            services.AddTransient<InventoryViewModel>();
            services.AddTransient<ReadWriteViewModel>();
            services.AddTransient<AdvancedTagOpsViewModel>();
            services.AddTransient<LogViewModel>();

            services.AddSingleton<MainWindow>();
        }
    }

}
