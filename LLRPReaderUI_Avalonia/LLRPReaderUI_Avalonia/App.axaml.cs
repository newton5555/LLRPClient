using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.DependencyInjection;
using LLRPReaderUI_Avalonia.Logging;
using LLRPReaderUI_Avalonia.Models;
using LLRPReaderUI_Avalonia.ViewModels;
using LLRPReaderUI_Avalonia.Views;
using LLRPSdk;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Linq;
using System.Runtime.InteropServices;

namespace LLRPReaderUI_Avalonia
{
    public partial class App : Application
    {
        private ServiceProvider? serviceProvider;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OverrideFontWeight(Type controlType)
        {
            if (this.FindResource(controlType) is not ControlTheme baseTheme)
                return;

            var androidTheme = new ControlTheme(controlType)
            {
                BasedOn = baseTheme
            };

            androidTheme.Setters.Add(new Setter(
                TextBlock.FontWeightProperty,
                FontWeight.Normal
            ));

            this.Resources[controlType] = androidTheme;
        }


        public override void OnFrameworkInitializationCompleted()
        {
            if (RuntimeInformation.OSDescription.Contains("Android", StringComparison.OrdinalIgnoreCase))
            {              // 安卓：覆盖 Button 默认样式，强制 Normal 字重
                           // 安卓：覆盖控件默认样式，强制 Normal 字重

                // Button
                OverrideFontWeight(typeof(Button));

                // CheckBox
                OverrideFontWeight(typeof(CheckBox));

                // RadioButton
                OverrideFontWeight(typeof(RadioButton));

                // ToggleButton
                OverrideFontWeight(typeof(ToggleButton));

                // RepeatButton
                OverrideFontWeight(typeof(RepeatButton));

                // DropDownButton / SplitButton（如果 Semi 有）
                OverrideFontWeight(typeof(DropDownButton));
                OverrideFontWeight(typeof(SplitButton));

                // HyperlinkButton
                OverrideFontWeight(typeof(HyperlinkButton));



                Styles.Add(new Style(x => x.OfType<TextBlock>())
                {
                    Setters =
            {
                new Setter(TextBlock.FontWeightProperty, FontWeight.Normal)
            }
                });
            }






            Log.Logger = LoggingConfigurationManager.BuildLogger();

            var services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
            Ioc.Default.ConfigureServices(serviceProvider);
            _ = serviceProvider.GetRequiredService<LlrpLoggingBridge>();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                

                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow(serviceProvider.GetRequiredService<MainViewModel>());
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = serviceProvider.GetRequiredService<MainViewModel>()
                };
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
            services.AddSingleton<LlrpLoggingBridge>();
            services.AddSingleton<MainViewModel>();

            services.AddTransient<DeviceConnectionViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<GpioViewModel>();
            services.AddTransient<InventoryConfigViewModel>();
            services.AddTransient<InventoryViewModel>();
            services.AddTransient<ReadWriteViewModel>();
            services.AddTransient<LogViewModel>();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}
