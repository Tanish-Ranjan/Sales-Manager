using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Sales_Manager.Features.AnalyticsOverview.Presentation.ViewModel
{
	public interface IOverviewViewModel
	{

		SolidColorPaint LegendPaint { get; }

		bool HasWeeklyNext { get; }
		bool HasWeeklyPrev { get; }
		bool HasMonthlyNext { get; }
		bool HasMonthlyPrev { get; }
		bool HasYearlyNext { get; }
		bool HasYearlyPrev { get; }

		ICommand LoadPrevWeeklyCommand { get; }
		ICommand LoadNextWeeklyCommand { get; }
		ICommand LoadPrevMonthlyCommand { get; }
		ICommand LoadNextMonthlyCommand { get; }
		ICommand LoadPrevYearlyCommand { get; }
		ICommand LoadNextYearlyCommand { get; }

		ISeries[] WeeklySeries { get; }
		ISeries[] MonthlySeries { get; }
		ISeries[] YearlySeries { get; }
		ISeries[] LifetimeSeries { get; }

		Visibility WeeklyChartVisibility { get; }
		Visibility WeeklyPlaceholderVisibility { get; }
		Visibility MonthlyChartVisibility { get; }
		Visibility MonthlyPlaceholderVisibility { get; }
		Visibility YearlyChartVisibility { get; }
		Visibility YearlyPlaceholderVisibility { get; }
		Visibility LifetimeChartVisibility { get; }
		Visibility LifetimePlaceholderVisibility { get; }

		Axis[] WeeklyXAxis { get; }
		Axis[] MonthlyXAxis { get; }
		Axis[] YearlyXAxis { get; }
		Axis[] CommonYAxis { get; }

	}
}
