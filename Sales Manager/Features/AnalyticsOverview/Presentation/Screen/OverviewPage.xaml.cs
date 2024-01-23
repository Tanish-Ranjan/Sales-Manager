using Microsoft.Extensions.DependencyInjection;
using Sales_Manager.Features.AnalyticsOverview.Presentation.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Sales_Manager.UI.Screens.Analytics.Pages
{
	public sealed partial class OverviewPage : Page
	{

		public OverviewPage()
		{
			InitializeComponent();
			DataContext = ((App)Application.Current).Container.GetService<IOverviewViewModel>();
		}

	}
}
