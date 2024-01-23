using Microsoft.Extensions.DependencyInjection;
using Sales_Manager.Features.AnalyticsForecast.Presentation.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Sales_Manager.UI.Screens.Analytics.Pages
{
	public sealed partial class ForecastPage : Page
	{
		public ForecastPage()
		{
			InitializeComponent();
			DataContext = ((App)Application.Current).Container.GetService<IForecastViewModel>();
		}
	}
}
