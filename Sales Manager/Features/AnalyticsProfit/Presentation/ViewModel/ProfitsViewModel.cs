using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Sales_Manager.Commons.Data.Repo;
using Sales_Manager.Data.Model;
using Sales_Manager.Features.AnalyticsProfit.Presentation.ViewModel;
using SkiaSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Sales_Manager.Data.ViewModel
{
	public class ProfitsViewModel : ObservableObject, IProfitsViewModel
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
		public Axis[] WeeklyXAxis
		{
			get { return _weeklyXAxis; }
			set
			{
				_weeklyXAxis = value;
				OnPropertyChanged(nameof(WeeklyXAxis));
			}
		}
		public Axis[] MonthlyXAxis
		{
			get { return _monthlyXAxis; }
			set
			{
				_monthlyXAxis = value;
				OnPropertyChanged(nameof(MonthlyXAxis));
			}
		}
		public Axis[] YearlyXAxis
		{
			get { return _yearlyXAxis; }
			set
			{
				_yearlyXAxis = value;
				OnPropertyChanged(nameof(YearlyXAxis));
			}
		}
		public Axis[] CommonYAxis { get; private set; }

		public Visibility WeeklyChartVisibility { get => WeeklySeries.Length > 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility WeeklyPlaceholderVisibility { get => WeeklySeries.Length <= 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility MonthlyChartVisibility { get => MonthlySeries.Length > 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility MonthlyPlaceholderVisibility { get => MonthlySeries.Length <= 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility YearlyChartVisibility { get => YearlySeries.Length > 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility YearlyPlaceholderVisibility { get => YearlySeries.Length <= 0 ? Visibility.Visible : Visibility.Collapsed; }

		private ObservableCollection<ComboBoxState> _itemsList = new ObservableCollection<ComboBoxState>();
		public ObservableCollection<ComboBoxState> ItemsList
		{
			get { return _itemsList; }
			set { SetProperty(ref _itemsList, value); }
		}

		private int _selectedIndex;
		public int SelectedIndex
		{
			get { return _selectedIndex; }
			set { SetProperty(ref _selectedIndex, value); }
		}
		private double _rateText;
		public string RateText
		{
			get { return _rateText.ToString(); }
			set
			{
				if (double.TryParse(value, out double parsedValue))
				{
					SetProperty(ref _rateText, parsedValue);
				}
				else
				{
					SetProperty(ref _rateText, 0.0);
				}
			}
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

		public ICommand SetCommand { get; }
		public ICommand LoadPrevWeeklyCommand { get; }
		public ICommand LoadNextWeeklyCommand { get; }
		public ICommand LoadPrevMonthlyCommand { get; }
		public ICommand LoadNextMonthlyCommand { get; }
		public ICommand LoadPrevYearlyCommand { get; }
		public ICommand LoadNextYearlyCommand { get; }

		public ProfitsViewModel(IDataService dataService)
		{
			defaultTextColor = ((SolidColorBrush)Application.Current.Resources["SystemControlForegroundBaseHighBrush"]).Color;
			labelSkColor = new SKColor(defaultTextColor.R, defaultTextColor.G, defaultTextColor.B).WithAlpha(150);

			WeeklyCurrentPage = 0;
			MonthlyCurrentPage = 0;
			YearlyCurrentPage = 0;

			WeeklyPageCount = 0;
			MonthlyPageCount = 0;
			YearlyPageCount = 0;

			SetCommand = new RelayCommand(SetParameters, () => true);
			LoadPrevWeeklyCommand = new RelayCommand(LoadPrevWeekly, () => HasWeeklyPrev);
			LoadNextWeeklyCommand = new RelayCommand(LoadNextWeekly, () => HasWeeklyNext);
			LoadPrevMonthlyCommand = new RelayCommand(LoadPrevMonthly, () => HasMonthlyPrev);
			LoadNextMonthlyCommand = new RelayCommand(LoadNextMonthly, () => HasMonthlyNext);
			LoadPrevYearlyCommand = new RelayCommand(LoadPrevYearly, () => HasYearlyPrev);
			LoadNextYearlyCommand = new RelayCommand(LoadNextYearly, () => HasYearlyNext);

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

			_dataService = dataService;
			_ = LoadItemsList();

		}

		private async Task LoadWeeklyPageCount(string name)
		{
			var item = await _dataService.GetWeeklyItemAggregationRecordsCount(name);
			var count = item != null ? item.Count : 0;
			WeeklyPageCount = count;
			(LoadPrevWeeklyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			(LoadNextWeeklyCommand as RelayCommand)?.NotifyCanExecuteChanged();
		}

		private async Task LoadMonthlyPageCount(string name)
		{
			var item = await _dataService.GetMonthlyItemAggregationRecordsCount(name);
			var count = item != null ? item.Count : 0;
			MonthlyPageCount = count;
			(LoadPrevMonthlyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			(LoadNextMonthlyCommand as RelayCommand)?.NotifyCanExecuteChanged();
		}

		private async Task LoadYearlyPageCount(string name)
		{
			var item = await _dataService.GetYearlyItemAggregationRecordsCount(name);
			var count = item != null ? item.Count : 0;
			YearlyPageCount = count;
			(LoadPrevYearlyCommand as RelayCommand)?.NotifyCanExecuteChanged();
			(LoadNextYearlyCommand as RelayCommand)?.NotifyCanExecuteChanged();
		}

		private void SetParameters()
		{
			WeeklyCurrentPage = 0;
			MonthlyCurrentPage = 0;
			YearlyCurrentPage = 0;

			if (SelectedIndex > -1)
			{
				var selectedItem = ItemsList[SelectedIndex];
				_ = LoadWeekly(selectedItem.Name);
				_ = LoadMonthly(selectedItem.Name);
				_ = LoadYearly(selectedItem.Name);
				LoadPrevNext(selectedItem.Name);
			}
		}

		private void LoadPrevNext(string name)
		{
			_ = LoadWeeklyPageCount(name);
			_ = LoadMonthlyPageCount(name);
			_ = LoadYearlyPageCount(name);
		}

		private void LoadPrevWeekly()
		{
			WeeklyCurrentPage--;
			if (SelectedIndex > -1)
			{
				var selectedItem = ItemsList[SelectedIndex];
				_ = LoadWeekly(selectedItem.Name);
				_ = LoadWeeklyPageCount(selectedItem.Name);
			}
		}

		private void LoadNextWeekly()
		{
			WeeklyCurrentPage++;
			if (SelectedIndex > -1)
			{
				var selectedItem = ItemsList[SelectedIndex];
				_ = LoadWeekly(selectedItem.Name);
				_ = LoadWeeklyPageCount(selectedItem.Name);
			}
		}

		private void LoadPrevMonthly()
		{
			MonthlyCurrentPage--;
			if (SelectedIndex > -1)
			{
				var selectedItem = ItemsList[SelectedIndex];
				_ = LoadMonthly(selectedItem.Name);
				_ = LoadMonthlyPageCount(selectedItem.Name);
			}
		}

		private void LoadNextMonthly()
		{
			MonthlyCurrentPage++;
			if (SelectedIndex > -1)
			{
				var selectedItem = ItemsList[SelectedIndex];
				_ = LoadMonthly(selectedItem.Name);
				_ = LoadMonthlyPageCount(selectedItem.Name);
			}
		}

		private void LoadPrevYearly()
		{
			YearlyCurrentPage--;
			if (SelectedIndex > -1)
			{
				var selectedItem = ItemsList[SelectedIndex];
				_ = LoadYearly(selectedItem.Name);
				_ = LoadYearlyPageCount(selectedItem.Name);
			}
		}

		private void LoadNextYearly()
		{
			YearlyCurrentPage++;
			if (SelectedIndex > -1)
			{
				var selectedItem = ItemsList[SelectedIndex];
				_ = LoadYearly(selectedItem.Name);
				_ = LoadYearlyPageCount(selectedItem.Name);
			}
		}

		private async Task LoadItemsList()
		{

			List<ComboBoxState> list = new List<ComboBoxState>();
			(await _dataService.GetSalesItemList()).ForEach(item =>
			{
				list.Add(new ComboBoxState
				{
					Name = item.ItemName,
					Data = item.ItemName
				});
			});
			ItemsList.Clear();
			ItemsList = new ObservableCollection<ComboBoxState>(list);

		}

		private async Task LoadWeekly(string name)
		{

			var selectedItem = ItemsList[SelectedIndex];
			var list = await _dataService.ReadPaginatedWeeklyItemAggregation(name, pageSize, WeeklyCurrentPage);
			var rate = _rateText;

			var weeklyData = new List<double>();
			var labels = new List<string>();
			list.ForEach(item =>
			{
				var profit = item.Total - (rate * item.Qty);
				weeklyData.Add(profit);
				labels.Add(item.StartDate.ToString("dd/MM/yy"));
			});

			WeeklySeries = new ISeries[]
			{
				new LineSeries<double>
				{
					Values = weeklyData.ToArray(),
					Fill = null,
					XToolTipLabelFormatter = (value) =>
					{
						int index = value.Index;
						var startDate = list[index].StartDate;
						var endDate = list[index].EndDate;
						return $"{startDate:dd/MM/yy} - {endDate:dd/MM/yy}";
					}
				}
			};

			WeeklyXAxis = new Axis[]
			{
				new Axis
				{
					Labels = labels.ToArray(),
					LabelsPaint = new SolidColorPaint(labelSkColor)
				}
			};
		}

		private async Task LoadMonthly(string name)
		{

			var list = await _dataService.ReadPaginatedMonthlyItemAggregation(name, pageSize, MonthlyCurrentPage);
			var rate = _rateText;

			var monthlyData = new List<double>();
			var labels = new List<string>();
			list.ForEach(item =>
			{
				var profit = item.Total - (rate * item.Qty);
				monthlyData.Add(profit);
				labels.Add(item.StartDate.ToString("MMM/yy"));
			});

			MonthlySeries = new ISeries[]
			{
				new LineSeries<double>
				{
					Values = monthlyData.ToArray(),
					Fill = null
				}
			};

			MonthlyXAxis = new Axis[]
			{
				new Axis
				{
					Labels = labels.ToArray(),
					LabelsPaint = new SolidColorPaint(labelSkColor)
				}
			};
		}

		private async Task LoadYearly(string name)
		{

			var list = await _dataService.ReadPaginatedYearlyItemAggregation(name, pageSize, YearlyCurrentPage);
			var rate = _rateText;

			var yearlyData = new List<double>();
			var labels = new List<string>();
			list.ForEach(item =>
			{
				var profit = item.Total - (rate * item.Qty);
				yearlyData.Add(profit);
				labels.Add(item.StartDate.ToString("yyyy"));
			});

			YearlySeries = new ISeries[]
			{
				new LineSeries<double>
				{
					Values = yearlyData.ToArray(),
					Fill = null
				}
			};

			YearlyXAxis = new Axis[]
			{
				new Axis
				{
					Labels = labels.ToArray(),
					LabelsPaint = new SolidColorPaint(labelSkColor)
				}
			};

		}

	}

}