using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Windows.UI.Xaml;

namespace Sales_Manager.Features.AnalyticsForecast.Presentation.ViewModel
{
	public interface IForecastViewModel
	{

		ISeries[] WeeklySeries { get; }
		ISeries[] MonthlySeries { get; }
		ISeries[] YearlySeries { get; }

		Axis[] WeeklyXAxis { get; }
		Axis[] MonthlyXAxis { get; }
		Axis[] YearlyXAxis { get; }
		Axis[] CommonYAxis { get; }

		Visibility WeeklyChartVisibility { get; }
		Visibility WeeklyPlaceholderVisibility { get; }
		Visibility MonthlyChartVisibility { get; }
		Visibility MonthlyPlaceholderVisibility { get; }
		Visibility YearlyChartVisibility { get; }
		Visibility YearlyPlaceholderVisibility { get; }

	}
}
