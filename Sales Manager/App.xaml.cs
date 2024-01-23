using Microsoft.Extensions.DependencyInjection;
using Sales_Manager.Commons.Data.Repo;
using Sales_Manager.Commons.Features.Backup;
using Sales_Manager.Data.Repo;
using Sales_Manager.Data.ViewModel;
using Sales_Manager.Features.AnalyticsComparison.Presentation.ViewModel;
using Sales_Manager.Features.AnalyticsForecast.Presentation.ViewModel;
using Sales_Manager.Features.AnalyticsOverview.Presentation.ViewModel;
using Sales_Manager.Features.AnalyticsProfit.Presentation.ViewModel;
using Sales_Manager.Features.Dashboard.Presentation.ViewModel;
using Sales_Manager.Features.NotesAdd.Presentation.ViewModel;
using Sales_Manager.Features.NotesView.Presentation.ViewModel;
using Sales_Manager.Features.Records.Presentation.ViewModel;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Sales_Manager
{
	sealed partial class App : Application
	{

		public IServiceProvider Container { get; }

		public App()
		{
			InitializeComponent();
			Suspending += OnSuspending;
			Container = ConfigureDependencyInjection();
		}

		private IServiceProvider ConfigureDependencyInjection()
		{
			var serviceCollection = new ServiceCollection();

			serviceCollection.AddTransient<IDataService, DataService>();
			serviceCollection.AddTransient<IDashboardViewModel, DashboardViewModel>();
			serviceCollection.AddTransient<IOverviewViewModel, OverviewViewModel>();
			serviceCollection.AddTransient<IForecastViewModel, ForecastViewModel>();
			serviceCollection.AddTransient<IComparisonViewModel, ComparisonViewModel>();
			serviceCollection.AddTransient<IProfitsViewModel, ProfitsViewModel>();
			serviceCollection.AddTransient<IRecordsViewModel, RecordsViewModel>();
			serviceCollection.AddTransient<IAddNotesViewModel, AddNotesViewModel>();
			serviceCollection.AddTransient<IViewNotesViewModel, ViewNotesViewModel>();

			return serviceCollection.BuildServiceProvider();
		}

		protected override async void OnLaunched(LaunchActivatedEventArgs e)
		{
			// Backup before loading pages to prevent database file access errors
			BackupService backupService = new BackupService();
			backupService.BackupDatabaseIfChanged();

			Frame rootFrame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (rootFrame == null)
			{
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();

				rootFrame.NavigationFailed += OnNavigationFailed;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					//TODO: Load state from previously suspended application
				}

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
			}

			if (e.PrelaunchActivated == false)
			{
				if (rootFrame.Content == null)
				{
					// When the navigation stack isn't restored navigate to the first page,
					// configuring the new page by passing required information as a navigation
					// parameter
					rootFrame.Navigate(typeof(MainPage), e.Arguments);
				}
				// Ensure the current window is active
				Window.Current.Activate();

				DataService dataService = new DataService();
				await dataService.InitializeDatabase();
			}
		}

		void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			//TODO: Save application state and stop any background activity
			deferral.Complete();
		}
	}
}
