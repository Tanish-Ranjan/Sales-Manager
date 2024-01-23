using Sales_Manager.Features.Notes.Presentation.Screen;
using Sales_Manager.Features.Records.Presentation.Screen;
using Sales_Manager.UI.Screens.Analytics;
using Sales_Manager.UI.Screens.Dashboard;
using System;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Sales_Manager
{
	public sealed partial class MainPage : Page
	{
		public MainPage()
		{
			InitializeComponent();
			var item = MainNav.MenuItems[0];
			MainNav.SelectedItem = item;
			MainNavFrame.Navigate(typeof(DashboardPage), null, new EntranceNavigationTransitionInfo());
		}

		private void MainNav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{

			string selectedTag = args.InvokedItemContainer.Tag.ToString();
			switch (selectedTag)
			{
				case "Dashboard":
					if (MainNavFrame.CurrentSourcePageType == typeof(DashboardPage)) return;
					MainNavFrame.Navigate(typeof(DashboardPage), null, args.RecommendedNavigationTransitionInfo);
					MainNavFrame.BackStack.Clear();
					break;
				case "Analytics":
					if (MainNavFrame.CurrentSourcePageType == typeof(AnalyticsPage)) return;
					MainNavFrame.Navigate(typeof(AnalyticsPage), null, args.RecommendedNavigationTransitionInfo);
					MainNavFrame.BackStack.Clear();
					break;
				case "Records":
					if (MainNavFrame.CurrentSourcePageType == typeof(RecordsPage)) return;
					MainNavFrame.Navigate(typeof(RecordsPage), null, args.RecommendedNavigationTransitionInfo);
					MainNavFrame.BackStack.Clear();
					break;
				case "Notes":
					if (MainNavFrame.CurrentSourcePageType == typeof(NotesPage)) return;
					MainNavFrame.Navigate(typeof(NotesPage), null, args.RecommendedNavigationTransitionInfo);
					MainNavFrame.BackStack.Clear();
					break;
			}

		}

		private void MainNav_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
		{
			if (e.NewSize.Width < 1200)
			{
				if (MainNav.PaneDisplayMode != NavigationViewPaneDisplayMode.LeftCompact)
					MainNav.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact;
			}
			else
			{
				if (MainNav.PaneDisplayMode != NavigationViewPaneDisplayMode.Left)
					MainNav.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
			}
		}

		private async void NavigationViewItem_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			string path = ApplicationData.Current.LocalFolder.Path;
			StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
			_ = await Launcher.LaunchFolderAsync(folder);
		}
	}
}
