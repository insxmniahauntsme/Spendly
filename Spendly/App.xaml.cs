using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Spendly.Infrastructure.DependencyInjection;
using Spendly.Infrastructure.Interfaces;
using Spendly.Infrastructure.Services;
using Spendly.Sqlite.Configurations;
using Spendly.Sqlite.DependencyInjection;
using Spendly.ViewModels;

namespace Spendly;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	private IHost Host { get; }

	public App()
	{
		Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
			.ConfigureServices((services) =>
			{
				services.AddInfrastructure();
				services.AddSqlite();
				//services.AddPostgresql();
				
				//TODO: Remove vms registrations
				services.AddSingleton<MainLayoutViewModel>();
				services.AddSingleton<DashboardViewModel>();
				services.AddSingleton<TransactionsViewModel>();
			})
			.Build();
	}

	protected override async void OnStartup(StartupEventArgs e)
	{
		await Host.StartAsync();

		using var scope = Host.Services.CreateScope();

		var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();

		try
		{
			await dbInitializer.InitializeAsync();
		}
		catch (Exception)
		{
			//TODO: Add logging
			Shutdown();
			throw;
		}

		var mainWindow = new MainWindow
		{
			DataContext = Host.Services.GetRequiredService<MainLayoutViewModel>()
		};

		mainWindow.Show();

		base.OnStartup(e);
	}

	protected override async void OnExit(ExitEventArgs e)
	{
		await Host.StopAsync();
		Host.Dispose();

		base.OnExit(e);
	}
}