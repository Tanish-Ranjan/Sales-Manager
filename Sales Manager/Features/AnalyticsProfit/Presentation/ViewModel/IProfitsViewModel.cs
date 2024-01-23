using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Sales_Manager.Data.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Sales_Manager.Features.AnalyticsProfit.Presentation.ViewModel
{
	public interface IProfitsViewModel
	{

		ISeries[] WeeklySeries { get; }
		ISeries[] MonthlySeries { get; }
		ISeries[] YearlySeries { get; }

		Axis[] WeeklyXAxis { get; }
		Axis[] MonthlyXAxis { get; }
		Axis[] YearlyXAxis { get; }
		Axis[] CommonYAxis { get; }

		ICommand SetCommand { get; }
		ICommand LoadPrevWeeklyCommand { get; }
		ICommand LoadNextWeeklyCommand { get; }
		ICommand LoadPrevMonthlyCommand { get; }
		ICommand LoadNextMonthlyCommand { get; }
		ICommand LoadPrevYearlyCommand { get; }
		ICommand LoadNextYearlyCommand { get; }

		Visibility WeeklyChartVisibility { get; }
		Visibility WeeklyPlaceholderVisibility { get; }
		Visibility MonthlyChartVisibility { get; }
		Visibility MonthlyPlaceholderVisibility { get; }
		Visibility YearlyChartVisibility { get; }
		Visibility YearlyPlaceholderVisibility { get; }

		ObservableCollection<ComboBoxState> ItemsList { get; }

		int SelectedIndex { get; }
		string RateText { get; }

		bool HasWeeklyPrev { get; }
		bool HasMonthlyPrev { get; }
		bool HasYearlyPrev { get; }
		bool HasWeeklyNext { get; }
		bool HasMonthlyNext { get; }
		bool HasYearlyNext { get; }

	}
}
