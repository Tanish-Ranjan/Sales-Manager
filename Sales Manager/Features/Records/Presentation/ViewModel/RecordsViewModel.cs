using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sales_Manager.Commons.Data.Repo;
using Sales_Manager.Data.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Sales_Manager.Features.Records.Presentation.ViewModel
{
	public class RecordsViewModel : ObservableObject, IRecordsViewModel
	{

		private readonly IDataService _dataService;

		private ObservableCollection<ComboBoxState> _itemsList;
		public ObservableCollection<ComboBoxState> ItemsList
		{
			get { return _itemsList; }
			set { SetProperty(ref _itemsList, value); }
		}

		private ObservableCollection<SalesRecordState> _dataList;
		public ObservableCollection<SalesRecordState> DataList
		{
			get { return _dataList; }
			set
			{
				SetProperty(ref _dataList, value);
				OnPropertyChanged(nameof(ListVisibility));
				OnPropertyChanged(nameof(PlaceholderVisibility));
			}
		}

		private int _selectedIndex;
		public int SelectedIndex
		{
			get { return _selectedIndex; }
			set { SetProperty(ref _selectedIndex, value); }
		}
		private DateTimeOffset _startDate;
		public DateTimeOffset StartDate
		{
			get { return _startDate; }
			set { SetProperty(ref _startDate, value); }
		}
		private DateTimeOffset _endDate;
		public DateTimeOffset EndDate
		{
			get { return _endDate; }
			set { SetProperty(ref _endDate, value); }
		}

		private double _quantityAggregate;
		public string QuantityAggregate
		{
			get { return _quantityAggregate.ToString(); }
			set
			{
				if (double.TryParse(value, out double parsedValue))
				{
					SetProperty(ref _quantityAggregate, parsedValue);
				}
				else
				{
					SetProperty(ref _quantityAggregate, 0.0);
				}
			}
		}

		private double _totalAggregate;
		public string TotalAggregate
		{
			get { return _totalAggregate.ToString(); }
			set
			{
				if (double.TryParse(value, out double parsedValue))
				{
					SetProperty(ref _totalAggregate, parsedValue);
				}
				else
				{
					SetProperty(ref _totalAggregate, 0.0);
				}
			}
		}

		public ICommand ClearCommand { get; }
		public ICommand SearchCommand { get; }

		public Visibility ListVisibility { get => DataList.Count() > 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility PlaceholderVisibility { get => DataList.Count() <= 0 ? Visibility.Visible : Visibility.Collapsed; }

		private readonly int PageSize = 50;
		private int PageCount = 0;

		private string LastSearch_Name;
		private DateTimeOffset LastSearch_StartDate;
		private DateTimeOffset LastSearch_EndDate;

		private bool isQuerying;

		public RecordsViewModel(IDataService dataService)
		{

			_dataService = dataService;

			_itemsList = new ObservableCollection<ComboBoxState>();
			_dataList = new ObservableCollection<SalesRecordState>();

			SelectedIndex = -1;
			StartDate = DateTime.Now.Date;
			EndDate = DateTime.Now.Date;

			LastSearch_Name = "";
			LastSearch_StartDate = StartDate;
			LastSearch_EndDate = EndDate;

			_quantityAggregate = 0.0;
			_totalAggregate = 0.0;

			ClearCommand = new RelayCommand(Clear);
			SearchCommand = new RelayCommand(Search);

			isQuerying = false;

			DataList.CollectionChanged += (s, e) =>
			{
				OnPropertyChanged(nameof(ListVisibility));
				OnPropertyChanged(nameof(PlaceholderVisibility));
			};

			Search();
			_ = GetItemList();

		}

		public async void LoadNextSetOfRecords()
		{

			if (isQuerying)
			{
				return;
			}

			isQuerying = true;

			var name = "";
			if (SelectedIndex > -1)
			{
				name = ItemsList[SelectedIndex].Data;
			}
			var start = StartDate.Date;
			var end = EndDate.Date;

			if (LastSearch_Name == name && LastSearch_StartDate == start && LastSearch_EndDate == end)
			{
				PageCount++;
				await Query(name, start, end);
			}

			isQuerying = false;

		}

		private void Clear()
		{
			SelectedIndex = 0;
			StartDate = DateTime.Now;
			EndDate = DateTime.Now;
			Search();
		}

		private void Search()
		{
			var name = "";
			if (SelectedIndex > -1)
			{
				name = ItemsList[SelectedIndex].Data;
			}
			var start = StartDate.Date;
			var end = EndDate.Date;

			LastSearch_Name = name;
			LastSearch_StartDate = start;
			LastSearch_EndDate = end;

			PageCount = 0;
			_ = Query(name, start, end);

		}

		private async Task Query(string name, DateTime start, DateTime end)
		{

			var list = await _dataService.QueryPaginatedSalesRecords(name, start, end, PageSize, PageCount);
			if (PageCount == 0)
			{
				QuantityAggregate = "0.0".ToString();
				TotalAggregate = "0.0".ToString();
				DataList.Clear();
			}

			var aggQty = 0.0;
			var aggTotal = 0.0;

			list.ForEach(item =>
			{
				DataList.Add(
					new SalesRecordState
					{
						Id = item.Id,
						ItemName = item.ItemName,
						ItemQty = item.ItemQty.ToString(),
						ItemRate = item.ItemRate.ToString(),
						Total = item.Total.ToString(),
						Date = item.Date.ToString("dd-MM-yyyy")
					}
				);
				aggQty += item.ItemQty;
				aggTotal += item.Total;
			});

			QuantityAggregate = (_quantityAggregate + aggQty).ToString();
			TotalAggregate = (_totalAggregate + aggTotal).ToString();

		}

		private async Task GetItemList()
		{
			var list = await _dataService.GetSalesItemList();
			ItemsList.Clear();
			ItemsList.Add(new ComboBoxState
			{
				Name = "All Items",
				Data = ""
			});
			list.ForEach(item =>
			{
				ItemsList.Add(new ComboBoxState
				{
					Name = item.ItemName,
					Data = item.ItemName
				});
			});
			SelectedIndex = 0;
		}

	}
}
