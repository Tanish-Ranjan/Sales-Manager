using Microsoft.Extensions.DependencyInjection;
using Sales_Manager.Features.AnalyticsComparison.Presentation.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Sales_Manager.UI.Screens.Analytics.Pages
{
	public sealed partial class ComparisonPage : Page
	{
		public ComparisonPage()
		{
			InitializeComponent();
			DataContext = ((App)Application.Current).Container.GetService<IComparisonViewModel>();
		}
	}
}
