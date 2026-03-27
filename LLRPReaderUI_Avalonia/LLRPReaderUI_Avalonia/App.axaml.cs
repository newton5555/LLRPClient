using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.DependencyInjection;
using LLRPReaderUI_Avalonia.Data;
using LLRPReaderUI_Avalonia.Logging;
using LLRPReaderUI_Avalonia.Models;
using LLRPReaderUI_Avalonia.Services;
using LLRPReaderUI_Avalonia.ViewModels;
using LLRPReaderUI_Avalonia.Views;
using LLRPSdk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace LLRPReaderUI_Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Android 平台移除 FontWeight，避免中文字体加粗乱码
        if (OperatingSystem.IsAndroid())
        {
            // 覆盖所有 TextBlock
            Styles.Add(new Style(x => x.OfType<TextBlock>())
            {
                Setters = { new Setter(TextBlock.FontWeightProperty, FontWeight.Normal) }
            });
            // 覆盖 .semiBold 和 .bold 样式类
            Styles.Add(new Style(x => x.OfType<TextBlock>().Class("semiBold"))
            {
                Setters = { new Setter(TextBlock.FontWeightProperty, FontWeight.Normal) }
            });
            Styles.Add(new Style(x => x.OfType<TextBlock>().Class("bold"))
            {
                Setters = { new Setter(TextBlock.FontWeightProperty, FontWeight.Normal) }
            });
            // 覆盖 DataGridColumnHeader
            Styles.Add(new Style(x => x.OfType<DataGridColumnHeader>())
            {
                Setters = { new Setter(DataGridColumnHeader.FontWeightProperty, FontWeight.Normal) }
            });
        }

        // Configure Serilog
        Log.Logger = LoggingConfigurationManager.BuildLogger();

        var services = new ServiceCollection();
        ConfigureServices(services);

        // Configure EF Core if enabled
        var loggingConfig = LoggingConfigurationManager.LoadConfiguration();
        if (loggingConfig.RawFrameLogging?.Enabled == true)
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LLRPReaderUI_Avalonia",
                "llrp_rawframes.db");

            var dbDir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(dbDir) && !Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }

            services.AddDbContext<RawFrameDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}")
                       .UseLoggerFactory(Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance));
            services.AddScoped<IRawFrameRepository, RawFrameRepository>();
        }

        Ioc.Default.ConfigureServices(services.BuildServiceProvider());

        _ = Ioc.Default.GetRequiredService<LlrpLoggingBridge>();

        // 初始化主题和语言服务
        var themeService = Ioc.Default.GetRequiredService<ThemeService>();
        themeService.Initialize();

        var languageService = Ioc.Default.GetRequiredService<LanguageService>();
        languageService.Initialize();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = Ioc.Default.GetRequiredService<MainWindow>();
            var vm = Ioc.Default.GetRequiredService<MainWindowViewModel>();
            mainWindow.DataContext = vm;
            desktop.MainWindow = mainWindow;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            var mainView = Ioc.Default.GetRequiredService<MainView>();
            var vm = Ioc.Default.GetRequiredService<MainWindowViewModel>();
            mainView.DataContext = vm;
            singleView.MainView = mainView;
        }

        base.OnFrameworkInitializationCompleted();
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
        services.AddTransient<GpioViewModel>();
        services.AddTransient<InventoryConfigViewModel>();
        services.AddTransient<InventoryViewModel>();
        services.AddTransient<ReadWriteViewModel>();
        services.AddTransient<AdvancedTagOpsViewModel>();
        services.AddTransient<LogViewModel>();
        services.AddTransient<LLRPMessageViewModel>();

        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainView>();
    }
}
