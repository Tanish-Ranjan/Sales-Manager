using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Sales_Manager.Commons.Data.Repo;
using Sales_Manager.Data.Model;
using Sales_Manager.Features.AnalyticsComparison.Presentation.ViewModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Sales_Manager.Data.ViewModel
{

	public class ComparisonViewModel : ObservableObject, IComparisonViewModel
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

		private ISeries[] _weeklySeries;
		private ISeries[] _monthlySeries;
		private ISeries[] _yearlySeries;
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

		public ComparisonViewModel(IDataService dataService)
		{

			_dataService = dataService;
			defaultTextColor = ((SolidColorBrush)Application.Current.Resources["SystemControlForegroundBaseHighBrush"]).Color;
			labelSkColor = new SKColor(defaultTextColor.R, defaultTextColor.G, defaultTextColor.B).WithAlpha(150);
			LegendPaint = new SolidColorPaint(labelSkColor);

			WeeklySeries = new ISeries[] { };
			MonthlySeries = new ISeries[] { };
			YearlySeries = new ISeries[] { };

			var commonAxis = new Axis[]
			{
				new Axis
				{
					Labels = new string[] { },
					LabelsPaint = new SolidColorPaint(labelSkColor),
					TicksAtCenter = true,
					SeparatorsAtCenter = false
				}
			};

			WeeklyXAxis = commonAxis;
			MonthlyXAxis = commonAxis;
			YearlyXAxis = commonAxis;
			CommonYAxis = new Axis[]
			{
				new Axis
				{
					LabelsPaint = new SolidColorPaint(labelSkColor)
				}
			};

			_ = GetData();

		}

		private async Task GetData()
		{

			var date = await _dataService.ReadLastEntryDate();
			if (date != null)
			{
				var list = await _dataService.GetSalesItemList();
				if (list.Count > 0)
				{
					_ = GetWeekly(date.StartDate, list);
					_ = GetMonthly(date.StartDate, list);
					_ = GetYearly(date.StartDate, list);
				}
			}

		}

		private async Task GetWeekly(DateTime date, List<SalesItemCounterRecord> list)
		{

			List<string> labels = new List<string>();
			List<double> currentItems = new List<double>();
			foreach (SalesItemCounterRecord item in list)
			{
				var salesItem = await _dataService.ReadWeeklyItemSale(item.ItemName, date);
				var total = salesItem != null ? salesItem.Total : 0;
				currentItems.Add(total);
				labels.Add(item.ItemName);
			}

			List<double> previousItems = new List<double>();
			foreach (SalesItemCounterRecord item in list)
			{
				var salesItem = await _dataService.ReadWeeklyItemSale(item.ItemName, date.AddDays(-7));
				var total = salesItem != null ? salesItem.Total : 0;
				previousItems.Add(total);
			}

			WeeklySeries = new ISeries[]
			{
				new ColumnSeries<double>
				{
					Values = previousItems.ToArray(),
					Stroke = null,
					Name = "Previous"
				},
				new ColumnSeries<double>
				{
					Values = currentItems.ToArray(),
					Stroke = null,
					Name = "Current"
				}
			};

			WeeklyXAxis = new Axis[]
			{
				new Axis
				{
					Labels = labels.ToArray(),
					LabelsPaint = new SolidColorPaint(labelSkColor),
					TicksAtCenter = true,
					SeparatorsAtCenter = false
				}
			};

		}

		private async Task GetMonthly(DateTime date, List<SalesItemCounterRecord> list)
		{

			List<string> labels = new List<string>();
			List<double> currentItems = new List<double>();
			foreach (SalesItemCounterRecord item in list)
			{
				var salesItem = await _dataService.ReadMonthlyItemSale(item.ItemName, date);
				var total = salesItem != null ? salesItem.Total : 0;
				currentItems.Add(total);
				labels.Add(item.ItemName);
			}

			List<double> previousItems = new List<double>();
			foreach (SalesItemCounterRecord item in list)
			{
				var salesItem = await _dataService.ReadMonthlyItemSale(item.ItemName, date.AddMonths(-1));
				var total = salesItem != null ? salesItem.Total : 0;
				previousItems.Add(total);
			}

			MonthlySeries = new ISeries[]
			{
				new ColumnSeries<double>
				{
					Values = previousItems.ToArray(),
					Stroke = null,
					Name = "Previous"
				},
				new ColumnSeries<double>
				{
					Values = currentItems.ToArray(),
					Stroke = null,
					Name = "Current"
				}
			};

			MonthlyXAxis = new Axis[]
			{
				new Axis
				{
					Labels = labels.ToArray(),
					LabelsPaint = new SolidColorPaint(labelSkColor),
					TicksAtCenter = true,
					SeparatorsAtCenter = false
				}
			};

		}

		private async Task GetYearly(DateTime date, List<SalesItemCounterRecord> list)
		{

			List<string> labels = new List<string>();
			List<double> currentItems = new List<double>();
			foreach (SalesItemCounterRecord item in list)
			{
				var salesItem = await _dataService.ReadYearlyItemSale(item.ItemName, date);
				var total = salesItem != null ? salesItem.Total : 0;
				currentItems.Add(total);
				labels.Add(item.ItemName);
			}

			List<double> previousItems = new List<double>();
			foreach (SalesItemCounterRecord item in list)
			{
				var salesItem = await _dataService.ReadYearlyItemSale(item.ItemName, date.AddYears(-1));
				var total = salesItem != null ? salesItem.Total : 0;
				previousItems.Add(total);
			}

			YearlySeries = new ISeries[]
			{
				new ColumnSeries<double>
				{
					Values = previousItems.ToArray(),
					Stroke = null,
					Name = "Previous"
				},
				new ColumnSeries<double>
				{
					Values = currentItems.ToArray(),
					Stroke = null,
					Name = "Current"
				}
			};

			YearlyXAxis = new Axis[]
			{
				new Axis
				{
					Labels = labels.ToArray(),
					LabelsPaint = new SolidColorPaint(labelSkColor),
					TicksAtCenter = true,
					SeparatorsAtCenter = false
				}
			};

		}

	}

}
