using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Windows.UI.Xaml;

namespace Sales_Manager.Features.AnalyticsComparison.Presentation.ViewModel
{
	public interface IComparisonViewModel
	{

		SolidColorPaint LegendPaint { get; }

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
