using LLRPConsole.Repositories;
using LLRPConsole.Services;
using LLRPConsole.State;
using LLRPConsole.ViewModels;
using Microsoft.Extensions.Logging;

namespace LLRPConsole;

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

		// Register singletons
		builder.Services.AddSingleton<AppState>();
		builder.Services.AddSingleton<LlrpSimulator>();
		builder.Services.AddSingleton<ILlrpReaderRepository, LlrpReaderRepository>();
		builder.Services.AddSingleton<IAppLogService, AppLogService>();
		builder.Services.AddSingleton<EndpointHistoryService>();
		builder.Services.AddSingleton<ReaderManagementService>();
		builder.Services.AddSingleton<InventoryService>();
		builder.Services.AddSingleton<AccessOperationService>();

		// Register viewmodels
		builder.Services.AddTransient<ReadersViewModel>();
		builder.Services.AddTransient<ConfigViewModel>();
		builder.Services.AddTransient<RospecViewModel>();
		builder.Services.AddTransient<InventoryConfigViewModel>();
		builder.Services.AddTransient<InventoryViewModel>();
		builder.Services.AddTransient<AccessViewModel>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
