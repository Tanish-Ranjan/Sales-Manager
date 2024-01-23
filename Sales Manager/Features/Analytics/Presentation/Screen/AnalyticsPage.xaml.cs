using Sales_Manager.UI.Screens.Analytics.Pages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Sales_Manager.UI.Screens.Analytics
{
	public sealed partial class AnalyticsPage : Page
	{
		public AnalyticsPage()
		{
			InitializeComponent();
			var item = AnalyticsNav.MenuItems[0];
			AnalyticsNav.SelectedItem = item;
			AnalyticsFrame.Navigate(typeof(OverviewPage), null, new EntranceNavigationTransitionInfo());
		}

		private void AnalyticsNav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{
			if (args.InvokedItemContainer.Tag == null)
			{
				return;
			}

			string selectedTag = args.InvokedItemContainer.Tag.ToString();
			switch (selectedTag)
			{
				case "Overview":
					if (AnalyticsFrame.CurrentSourcePageType == typeof(OverviewPage)) return;
					AnalyticsFrame.Navigate(typeof(OverviewPage), null, args.RecommendedNavigationTransitionInfo);
					AnalyticsFrame.BackStack.Clear();
					break;
				case "Forecast":
					if (AnalyticsFrame.CurrentSourcePageType == typeof(ForecastPage)) return;
					AnalyticsFrame.Navigate(typeof(ForecastPage), null, args.RecommendedNavigationTransitionInfo);
					AnalyticsFrame.BackStack.Clear();
					break;
				case "Comparison":
					if (AnalyticsFrame.CurrentSourcePageType == typeof(ComparisonPage)) return;
					AnalyticsFrame.Navigate(typeof(ComparisonPage), null, args.RecommendedNavigationTransitionInfo);
					AnalyticsFrame.BackStack.Clear();
					break;
				case "ProfitAnalysis":
					if (AnalyticsFrame.CurrentSourcePageType == typeof(ProfitPage)) return;
					AnalyticsFrame.Navigate(typeof(ProfitPage), null, args.RecommendedNavigationTransitionInfo);
					AnalyticsFrame.BackStack.Clear();
					break;
			}
		}
	}
}
