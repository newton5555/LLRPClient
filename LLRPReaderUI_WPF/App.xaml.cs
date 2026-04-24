using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using LLRPSdk;
using LLRPReaderUI_WPF.Logging;
using LLRPReaderUI_WPF.Models;
using LLRPReaderUI_WPF.ViewModels;
using LLRPReaderUI_WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using LLRPReaderUI_WPF.Data;
using Microsoft.EntityFrameworkCore;

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

            // 读取日志配置用于 DbContext 连接字符串
            var loggingConfig = LoggingConfigurationManager.LoadConfiguration();
            if (loggingConfig.RawFrameLogging?.Enabled == true)
            {
                var dbPath = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                    "LLRPReaderUI_WPF",
                    "llrp_rawframes.db");

                // 确保数据库目录存在
                var dbDir = System.IO.Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(dbDir) && !System.IO.Directory.Exists(dbDir))
                {
                    System.IO.Directory.CreateDirectory(dbDir);
                }

                // 注册 DbContext 和 EF 实现的仓库
                services.AddDbContext<RawFrameDbContext>(options =>
                    options.UseSqlite($"Data Source={dbPath}")
                           .UseLoggerFactory(Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance));
                services.AddScoped<IRawFrameRepository, RawFrameRepository>();
            }

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
            
            // Theme and Language services
            services.AddSingleton<ThemeService>();
            services.AddSingleton<LanguageService>();
            services.AddSingleton<ThemeLanguageViewModel>();
            
            services.AddSingleton<MainWindowViewModel>();

            services.AddTransient<DeviceConnectionViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<InventoryConfigViewModel>();
            services.AddTransient<InventoryViewModel>();
            services.AddTransient<ReadWriteViewModel>();
            services.AddTransient<AdvancedTagOpsViewModel>();
            services.AddTransient<LogViewModel>();
            services.AddTransient<LLRPMessageViewModel>();

            services.AddSingleton<MainWindow>();
        }
    }

}
