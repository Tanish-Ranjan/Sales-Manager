using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Sales_Manager.Commons.Data.Repo;
using Sales_Manager.Data.Model;
using Sales_Manager.Features.AnalyticsForecast.Presentation.ViewModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Sales_Manager.Data.ViewModel
{
	public class ForecastViewModel : ObservableObject, IForecastViewModel
	{

		private readonly IDataService _dataService;
		private readonly SKColor labelSkColor;
		private Windows.UI.Color defaultTextColor;

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

		public ForecastViewModel(IDataService dataService)
		{

			_dataService = dataService;
			defaultTextColor = ((SolidColorBrush)Application.Current.Resources["SystemControlForegroundBaseHighBrush"]).Color;
			labelSkColor = new SKColor(defaultTextColor.R, defaultTextColor.G, defaultTextColor.B).WithAlpha(150);

			WeeklySeries = new ISeries[] { };
			MonthlySeries = new ISeries[] { };
			YearlySeries = new ISeries[] { };

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

			_ = GetData();

		}

		private async Task GetData()
		{

			var lastDate = await _dataService.ReadLastEntryDate();

			if (lastDate != null)
			{
				var date = lastDate.StartDate;
				_ = LoadWeekly(date.AddDays(-3 * 7));
				_ = LoadMonthly(date.AddMonths(-3));
				_ = LoadYearly(date.AddYears(-3));
			}

		}

		private async Task LoadWeekly(DateTime currentDate)
		{

			var list = await _dataService.ReadWeeklyForecast(currentDate);
			var actualData = list[0];
			var trainingData = list[1];

			if (trainingData.Count == 4)
			{

				var actualLineData = new List<double>();
				var forecastLineData = new List<double>();
				var labels = new List<string>();
				var tooltips = new List<AggregatedRecord>();

				var modelData = new List<double>();
				trainingData.ForEach(item => { modelData.Add(item.TotalAmount); });

				for (int i = 0; i < 6; i++)
				{
					var date = currentDate.AddDays(i * 7);
					var expectedTotal = Math.Round(modelData.Sum() / 4, 2);
					modelData.RemoveAt(0);
					modelData.Add(expectedTotal);
					forecastLineData.Add(expectedTotal);
					labels.Add(date.ToString("dd/MM/yy"));
					tooltips.Add(new AggregatedRecord
					{
						StartDate = date,
						EndDate = date.AddDays(7).AddSeconds(-1)
					});
				}

				for (int i = 0; i < 4; i++)
				{
					var date = currentDate.AddDays(i * 7);
					var recordItem = actualData.FirstOrDefault(item =>
					{
						var itemDate = item.StartDate.Date;
						var nextDate = date.Date;
						return itemDate == nextDate.AddDays(-(int)nextDate.DayOfWeek);
					});
					var total = recordItem != null ? recordItem.TotalAmount : 0;
					actualLineData.Add(total);
				}

				WeeklySeries = new ISeries[]
				{
					new LineSeries<double>
					{
						Values = actualLineData.ToArray(),
						Fill = null,
						Name = "Current",
						XToolTipLabelFormatter = (value) =>
						{
							int index = value.Index;
							var startDate = tooltips[index].StartDate;
							var endDate = tooltips[index].EndDate;
							return $"{startDate:dd/MM/yy} - {endDate:dd/MM/yy}";
						}
					},
					new LineSeries<double>
					{
						Values = forecastLineData.ToArray(),
						Fill = null,
						Name = "Forecast",
						XToolTipLabelFormatter = (value) =>
						{
							int index = value.Index;
							var startDate = tooltips[index].StartDate;
							var endDate = tooltips[index].EndDate;
							return $"{startDate:dd/MM/yy} - {endDate:dd/MM/yy}";
						}
					}
				};

				WeeklyXAxis = new Axis[]
				{
					new Axis {
						Labels = labels,
						LabelsPaint = new SolidColorPaint(labelSkColor)
					}
				};

			}

		}

		private async Task LoadMonthly(DateTime currentDate)
		{

			var list = await _dataService.ReadMonthlyForecast(currentDate);
			var actualData = list[0];
			var trainingData = list[1];

			if (trainingData.Count == 4)
			{

				var actualLineData = new List<double>();
				var forecastLineData = new List<double>();
				var labels = new List<string>();

				var modelData = new List<double>();
				trainingData.ForEach(item => { modelData.Add(item.TotalAmount); });

				for (int i = 0; i < 6; i++)
				{
					var date = currentDate.AddMonths(i);
					var expectedTotal = Math.Round(modelData.Sum() / 4, 2);
					modelData.RemoveAt(0);
					modelData.Add(expectedTotal);
					forecastLineData.Add(expectedTotal);
					labels.Add(date.ToString("MMM, yy"));
				}

				for (int i = 0; i < 4; i++)
				{
					var date = currentDate.AddMonths(i);
					var recordItem = actualData.FirstOrDefault(item =>
					{
						var itemDate = item.StartDate.Date;
						var nextDate = date.Date;
						return new DateTime(itemDate.Year, itemDate.Month, 1) == new DateTime(nextDate.Year, nextDate.Month, 1);
					});
					var total = recordItem != null ? recordItem.TotalAmount : 0;
					actualLineData.Add(total);
				}

				MonthlySeries = new ISeries[]
				{
					new LineSeries<double>
					{
						Values = actualLineData.ToArray(),
						Fill = null,
						Name = "Current"
					},
					new LineSeries<double>
					{
						Values = forecastLineData.ToArray(),
						Fill = null,
						Name = "Forecast"
					}
				};

				MonthlyXAxis = new Axis[]
				{
					new Axis {
						Labels = labels,
						LabelsPaint = new SolidColorPaint(labelSkColor)
					}
				};

			}

		}

		private async Task LoadYearly(DateTime currentDate)
		{

			var list = await _dataService.ReadYearlyForecast(currentDate);
			var actualData = list[0];
			var trainingData = list[1];

			if (trainingData.Count == 4)
			{

				var actualLineData = new List<double>();
				var forecastLineData = new List<double>();
				var labels = new List<string>();

				var modelData = new List<double>();
				trainingData.ForEach(item => { modelData.Add(item.TotalAmount); });

				for (int i = 0; i < 6; i++)
				{
					var date = currentDate.AddYears(i);
					var expectedTotal = Math.Round(modelData.Sum() / 4, 2);
					modelData.RemoveAt(0);
					modelData.Add(expectedTotal);
					forecastLineData.Add(expectedTotal);
					labels.Add(date.ToString("yyyy"));
				}

				for (int i = 0; i < 4; i++)
				{
					var date = currentDate.AddYears(i);
					var recordItem = actualData.FirstOrDefault(item =>
					{
						var itemDate = item.StartDate.Date;
						var nextDate = date.Date;
						return new DateTime(itemDate.Year, 1, 1) == new DateTime(nextDate.Year, 1, 1);
					});
					var total = recordItem != null ? recordItem.TotalAmount : 0;
					actualLineData.Add(total);
				}

				YearlySeries = new ISeries[]
				{
					new LineSeries<double>
					{
						Values = actualLineData.ToArray(),
						Fill = null,
						Name = "Current"
					},
					new LineSeries<double>
					{
						Values = forecastLineData.ToArray(),
						Fill = null,
						Name = "Forecast"
					}
				};

				YearlyXAxis = new Axis[]
				{
					new Axis {
						Labels = labels,
						LabelsPaint = new SolidColorPaint(labelSkColor)
					}
				};

			}

		}

	}
}