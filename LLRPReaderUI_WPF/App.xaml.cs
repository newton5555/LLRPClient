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

            // 根据配置初始化 SQLite 原始帧持久化（可选）
            var loggingConfig = LoggingConfigurationManager.LoadConfiguration();
            if (loggingConfig.RawFrameLogging?.Enabled == true)
            {
                // 将数据库放在本地应用数据目录，便于权限与清理
                var dbPath = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                    "LLRPReaderUI_WPF",
                    "llrp_rawframes.db");

                RawFrameRepository.Init(dbPath);
            }

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
