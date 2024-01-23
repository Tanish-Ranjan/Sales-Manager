using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Sales_Manager.Commons.Data.Repo;
using Sales_Manager.Data.Model;
using Sales_Manager.Features.AnalyticsOverview.Presentation.ViewModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Sales_Manager.Data.ViewModel
{

	public class OverviewViewModel : ObservableObject, IOverviewViewModel
	{

		private readonly IDataService _dataService;

		private readonly SKColor labelSkColor;
		private Windows.UI.Color defaultTextColor;
		private SolidColorPaint _legendPaint;
		public SolidColorPaint LegendPaint
		{
			get { return _legendPaint; }
			set { SetProperty(ref _legendPaint, value); }
		}

		private readonly int pageSize = 6;
		private int _weeklyCurrentPage;
		public int WeeklyCurrentPage
		{
			get { return _weeklyCurrentPage; }
			set
			{
				if (_weeklyCurrentPage != value)
				{
					SetProperty(ref _weeklyCurrentPage, value);
					OnPropertyChanged(nameof(HasWeeklyPrev));
				}
			}
		}
		private int _monthlyCurrentPage;
		public int MonthlyCurrentPage
		{
			get { return _monthlyCurrentPage; }
			set
			{
				if (_monthlyCurrentPage != value)
				{
					SetProperty(ref _monthlyCurrentPage, value);
					OnPropertyChanged(nameof(HasMonthlyPrev));
				}
			}
		}
		private int _yearlyCurrentPage;
		public int YearlyCurrentPage
		{
			get { return _yearlyCurrentPage; }
			set
			{
				if (_yearlyCurrentPage != value)
				{
					SetProperty(ref _yearlyCurrentPage, value);
					OnPropertyChanged(nameof(HasYearlyPrev));
				}
			}
		}

		private int _weeklyPageCount;
		public int WeeklyPageCount
		{
			get { return _weeklyPageCount; }
			set
			{
				if (_weeklyPageCount != value)
				{
					SetProperty(ref _weeklyPageCount, value);
					OnPropertyChanged(nameof(HasWeeklyNext));
					OnPropertyChanged(nameof(HasWeeklyPrev));
				}
			}
		}
		private int _monthlyPageCount;
		public int MonthlyPageCount
		{
			get { return _monthlyPageCount; }
			set
			{
				if (_monthlyPageCount != value)
				{
					SetProperty(ref _monthlyPageCount, value);
					OnPropertyChanged(nameof(HasMonthlyNext));
					OnPropertyChanged(nameof(HasMonthlyPrev));
				}
			}
		}
		private int _yearlyPageCount;
		public int YearlyPageCount
		{
			get { return _yearlyPageCount; }
			set
			{
				if (_yearlyPageCount != value)
				{
					SetProperty(ref _yearlyPageCount, value);
					OnPropertyChanged(nameof(HasYearlyNext));
					OnPropertyChanged(nameof(HasYearlyPrev));
				}
			}
		}

		public bool HasWeeklyNext { get => (WeeklyCurrentPage + 1) * pageSize < WeeklyPageCount; }
		public bool HasWeeklyPrev { get => WeeklyCurrentPage > 0; }
		public bool HasMonthlyNext { get => (MonthlyCurrentPage + 1) * pageSize < MonthlyPageCount; }
		public bool HasMonthlyPrev { get => MonthlyCurrentPage > 0; }
		public bool HasYearlyNext { get => (YearlyCurrentPage + 1) * pageSize < YearlyPageCount; }
		public bool HasYearlyPrev { get => YearlyCurrentPage > 0; }

		public ICommand LoadPrevWeeklyCommand { get; }
		public ICommand LoadNextWeeklyCommand { get; }
		public ICommand LoadPrevMonthlyCommand { get; }
		public ICommand LoadNextMonthlyCommand { get; }
		public ICommand LoadPrevYearlyCommand { get; }
		public ICommand LoadNextYearlyCommand { get; }

		private ISeries[] _weeklySeries;
		private ISeries[] _monthlySeries;
		private ISeries[] _yearlySeries;
		private ISeries[] _lifetimeSeries;
		public ISeries[] WeeklySeries
		{
			get { return _weeklySeries; }
			set
			{
				SetProperty(ref _weeklySeries, value);
				OnPropertyChanged(nameof(WeeklyChartVisibility));
				OnPropertyChanged(nameof(WeeklyPlaceholderVisibility));
			}
		}
		public ISeries[] MonthlySeries
		{
			get { return _monthlySeries; }
			set
			{
				SetProperty(ref _monthlySeries, value);
				OnPropertyChanged(nameof(MonthlyChartVisibility));
				OnPropertyChanged(nameof(MonthlyPlaceholderVisibility));
			}
		}
		public ISeries[] YearlySeries
		{
			get { return _yearlySeries; }
			set
			{
				SetProperty(ref _yearlySeries, value);
				OnPropertyChanged(nameof(YearlyChartVisibility));
				OnPropertyChanged(nameof(YearlyPlaceholderVisibility));
			}
		}
		public ISeries[] LifetimeSeries
		{
			get { return _lifetimeSeries; }
			set
			{
				SetProperty(ref _lifetimeSeries, value);
				OnPropertyChanged(nameof(LifetimeChartVisibility));
				OnPropertyChanged(nameof(LifetimePlaceholderVisibility));
			}
		}

		private Axis[] _weeklyXAxis;
		private Axis[] _monthlyXAxis;
		private Axis[] _yearlyXAxis;
		private Axis[] _commonYAxis;
		public Axis[] WeeklyXAxis
		{
			get { return _weeklyXAxis; }
			set { SetProperty(ref _weeklyXAxis, value); }
		}
		public Axis[] MonthlyXAxis
		{
			get { return _monthlyXAxis; }
			set { SetProperty(ref _monthlyXAxis, value); }
		}
		public Axis[] YearlyXAxis
		{
			get { return _yearlyXAxis; }
			set { SetProperty(ref _yearlyXAxis, value); }
		}
		public Axis[] CommonYAxis
		{
			get { return _commonYAxis; }
			set { SetProperty(ref _commonYAxis, value); }
		}

		public Visibility WeeklyChartVisibility { get => WeeklySeries.Length > 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility WeeklyPlaceholderVisibility { get => WeeklySeries.Length <= 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility MonthlyChartVisibility { get => MonthlySeries.Length > 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility MonthlyPlaceholderVisibility { get => MonthlySeries.Length <= 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility YearlyChartVisibility { get => YearlySeries.Length > 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility YearlyPlaceholderVisibility { get => YearlySeries.Length <= 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility LifetimeChartVisibility { get => LifetimeSeries.Length > 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility LifetimePlaceholderVisibility { get => LifetimeSeries.Length <= 0 ? Visibility.Visible : Visibility.Collapsed; }

		public OverviewViewModel(IDataService dataService)
		{

			defaultTextColor = ((SolidColorBrush)Application.Current.Resources["SystemControlForegroundBaseHighBrush"]).Color;
			labelSkColor = new SKColor(defaultTextColor.R, defaultTextColor.G, defaultTextColor.B).WithAlpha(150);
			LegendPaint = new SolidColorPaint(labelSkColor);

			WeeklyCurrentPage = 0;
			MonthlyCurrentPage = 0;
			YearlyCurrentPage = 0;

			WeeklyPageCount = 0;
			MonthlyPageCount = 0;
			YearlyPageCount = 0;

			LoadPrevWeeklyCommand = new RelayCommand(LoadPrevWeekly, () => HasWeeklyPrev);
			LoadNextWeeklyCommand = new RelayCommand(LoadNextWeekly, () => HasWeeklyNext);
			LoadPrevMonthlyCommand = new RelayCommand(LoadPrevMonthly, () => HasMonthlyPrev);
			LoadNextMonthlyCommand = new RelayCommand(LoadNextMonthly, () => HasMonthlyNext);
			LoadPrevYearlyCommand = new RelayCommand(LoadPrevYearly, () => HasYearlyPrev);
			LoadNextYearlyCommand = new RelayCommand(LoadNextYearly, () => HasYearlyNext);

			WeeklySeries = new ISeries[] { };
			MonthlySeries = new ISeries[] { };
			YearlySeries = new ISeries[] { };
			LifetimeSeries = new ISeries[] { };

			WeeklyXAxis = new Axis[] { };
			MonthlyXAxis = new Axis[] { };
			YearlyXAxis = new Axis[] { };
			CommonYAxis = new Axis[]
			{
				new Axis
				{
					LabelsPaint = new SolidColorPaint(labelSkColor)
				}
			};

			_dataService = dataService;
			_ = LoadWeeklyPageCount();
			_ = LoadMonthlyPageCount();
			_ = LoadYearlyPageCount();
			_ = LoadWeekly();
			_ = LoadMonthly();
			_ = LoadYearly();
			_ = LoadLifetime();

		}

		private async Task LoadWeeklyPageCount()
		{
			var item = await _dataService.GetWeeklyAggregationRecordsCount();
			var count = item != null ? item.Count : 0;
			WeeklyPageCount = count;
		}

		private async Task LoadMonthlyPageCount()
		{
			var item = await _dataService.GetMonthlyAggregationRecordsCount();
			var count = item != null ? item.Count : 0;
			MonthlyPageCount = count;
		}

		private async Task LoadYearlyPageCount()
		{
			var item = await _dataService.GetYearlyAggregationRecordsCount();
			var count = item != null ? item.Count : 0;
			YearlyPageCount = count;
		}

		private void LoadPrevWeekly()
		{
			WeeklyCurrentPage--;
			(LoadPrevWeeklyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			(LoadNextWeeklyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			_ = LoadWeekly();
		}

		private void LoadNextWeekly()
		{
			WeeklyCurrentPage++;
			(LoadPrevWeeklyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			(LoadNextWeeklyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			_ = LoadWeekly();
		}

		private void LoadPrevMonthly()
		{
			MonthlyCurrentPage--;
			(LoadPrevMonthlyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			(LoadNextMonthlyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			_ = LoadMonthly();
		}

		private void LoadNextMonthly()
		{
			MonthlyCurrentPage++;
			(LoadPrevMonthlyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			(LoadNextMonthlyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			_ = LoadMonthly();
		}

		private void LoadPrevYearly()
		{
			YearlyCurrentPage--;
			(LoadPrevYearlyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			(LoadNextYearlyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			_ = LoadYearly();
		}

		private void LoadNextYearly()
		{
			YearlyCurrentPage++;
			(LoadPrevYearlyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			(LoadNextYearlyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			_ = LoadYearly();
		}

		private async Task LoadWeekly()
		{
			var temp = await _dataService.ReadPaginatedWeeklyAggregation(pageSize, WeeklyCurrentPage);
			if (temp.Count == 0)
			{
				if (WeeklyCurrentPage != 0)
				{
					WeeklyCurrentPage--;
				}
			}
			else
			{
				temp.Reverse();
				UpdateWeeklySeries(temp);
			}
		}

		private async Task LoadMonthly()
		{
			var temp = await _dataService.ReadPaginatedMonthlyAggregation(pageSize, MonthlyCurrentPage);
			if (temp.Count == 0)
			{
				if (MonthlyCurrentPage != 0)
				{
					MonthlyCurrentPage--;
				}
			}
			else
			{
				temp.Reverse();
				UpdateMonthlySeries(temp);
			}
		}

		private async Task LoadYearly()
		{
			var temp = await _dataService.ReadPaginatedYearlyAggregation(pageSize, YearlyCurrentPage);
			if (temp.Count == 0)
			{
				if (YearlyCurrentPage != 0)
				{
					YearlyCurrentPage--;
				}
			}
			else
			{
				temp.Reverse();
				UpdateYearlySeries(temp);
			}
		}

		private async Task LoadLifetime()
		{
			var aggr = await _dataService.ReadLifetimeAggregation();
			var temp = await _dataService.ReadLifetimeYearlyAggregation();
			temp.Reverse();
			UpdateLifetimeSeries(aggr, temp);
		}

		private void UpdateWeeklySeries(List<AggregatedRecord> weeklyItems)
		{

			if (weeklyItems.Count > 0)
			{

				var values = new List<double>();
				var labels = new List<string>();

				foreach (AggregatedRecord item in weeklyItems)
				{
					values.Add(item.TotalAmount);
					labels.Add(item.StartDate.ToString("dd/MM/yy"));
				}

				WeeklyXAxis = new Axis[] {
					new Axis
					{
						Labels = labels.ToArray(),
						LabelsPaint = new SolidColorPaint(labelSkColor)
					}
				};
				WeeklySeries = new ISeries[] {
					new LineSeries<double>
					{
						Name = "Weekly Sales",
						Values = values.ToArray(),
						Fill = null,
						XToolTipLabelFormatter = (value) =>
						{
							int index = value.Index;
							var startDate = weeklyItems[index].StartDate;
							var endDate = weeklyItems[index].EndDate;
							return $"{startDate:dd/MM/yy} - {endDate:dd/MM/yy}";
						}
					}
				};

			}

		}

		private void UpdateMonthlySeries(List<AggregatedRecord> monthlyItems)
		{

			if (monthlyItems.Count > 0)
			{

				var values = new List<double>();
				var labels = new List<string>();

				foreach (AggregatedRecord item in monthlyItems)
				{
					values.Add(item.TotalAmount);
					labels.Add(item.StartDate.ToString("MMM, yy"));
				}

				MonthlyXAxis = new Axis[] {
					new Axis
					{
						Labels = labels.ToArray(),
						LabelsPaint = new SolidColorPaint(labelSkColor)
					}
				};
				MonthlySeries = new ISeries[] {
					new LineSeries<double>
					{
						Name = "Monthly Sales",
						Values = values.ToArray(),
						Fill = null
					}
				};

			}

		}

		private void UpdateYearlySeries(List<AggregatedRecord> yearlyItems)
		{

			if (yearlyItems.Count > 0)
			{

				var values = new List<double>();
				var labels = new List<string>();

				foreach (AggregatedRecord item in yearlyItems)
				{
					values.Add(item.TotalAmount);
					labels.Add(item.StartDate.ToString("yyyy"));
				}

				YearlyXAxis = new Axis[] {
					new Axis
					{
						Labels = labels.ToArray(),
						LabelsPaint = new SolidColorPaint(labelSkColor)
					}
				};
				YearlySeries = new ISeries[] {
					new LineSeries<double>
					{
						Name = "Yearly Sales",
						Values = values.ToArray(),
						Fill = null
					}
				};

			}

		}

		private void UpdateLifetimeSeries(AggregatedRecord lifetimeAggregation, List<AggregatedRecord> yearlyItems)
		{

			if (yearlyItems.Count > 0)
			{

				var series = new List<ISeries>();
				var lifetimeAggr = lifetimeAggregation != null ? lifetimeAggregation.TotalAmount : 0;

				if (lifetimeAggr == 0)
				{
					return;
				}

				foreach (AggregatedRecord item in yearlyItems)
				{
					var percentage = item.TotalAmount / lifetimeAggr * 100;
					series.Add(new PieSeries<double>
					{
						Name = item.StartDate.ToString("yyyy") + $" - {Math.Round(percentage, 2)}%",
						Values = new double[] { item.TotalAmount },
						Stroke = null
					});
				}

				LifetimeSeries = series.ToArray();

			}

		}

	}

}