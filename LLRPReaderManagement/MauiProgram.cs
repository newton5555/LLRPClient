using LLRPReaderManagement.Repositories;
using LLRPReaderManagement.Services;
using LLRPReaderManagement.State;
using LLRPReaderManagement.ViewModels;
using Microsoft.Extensions.Logging;

namespace LLRPReaderManagement
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<AppState>();
            builder.Services.AddSingleton<LlrpSimulator>();
            builder.Services.AddSingleton<ILlrpReaderRepository, LlrpReaderRepository>();
            builder.Services.AddSingleton<IAppLogService, AppLogService>();
            builder.Services.AddSingleton<EndpointHistoryService>();
            builder.Services.AddSingleton<ReaderManagementService>();
            builder.Services.AddSingleton<InventoryService>();
            builder.Services.AddSingleton<AccessOperationService>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<ReadersViewModel>();
            builder.Services.AddTransient<InventoryViewModel>();
            builder.Services.AddTransient<InventoryConfigViewModel>();
            builder.Services.AddTransient<AccessViewModel>();
            builder.Services.AddTransient<ConfigViewModel>();
            builder.Services.AddTransient<RospecViewModel>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
