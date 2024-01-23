using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sales_Manager.Commons.Data.Repo;
using Sales_Manager.Data.Model;
using Sales_Manager.Features.Dashboard.Presentation.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace Sales_Manager.Data.ViewModel
{

	public class DashboardViewModel : ObservableObject, IDashboardViewModel
	{

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

		private ObservableCollection<ComboBoxState> _itemsList;
		public ObservableCollection<ComboBoxState> ItemsList
		{
			get { return _itemsList; }
			set { SetProperty(ref _itemsList, value); }
		}

		private string _itemName;
		public string ItemName
		{
			get { return _itemName; }
			set { SetProperty(ref _itemName, value); }
		}

		private double _rate;
		public string Rate
		{
			get { return _rate.ToString(); }
			set
			{
				if (double.TryParse(value, out double parsedValue))
				{
					SetProperty(ref _rate, parsedValue);
				}
				else
				{
					SetProperty(ref _rate, 0.0);
				}
				UpdateTotal();
			}
		}

		private int _quantity;
		public string Quantity
		{
			get { return _quantity.ToString(); }
			set
			{
				if (int.TryParse(value, out int parsedValue))
				{
					SetProperty(ref _quantity, parsedValue);
				}
				else
				{
					SetProperty(ref _quantity, 0);
				}
				UpdateTotal();
			}
		}

		private double _total;
		public string Total
		{
			get { return _total.ToString(); }
			private set
			{
				if (double.TryParse(value, out double parsedValue))
				{
					SetProperty(ref _total, parsedValue);
				}
				else
				{
					SetProperty(ref _total, 0.0);
				}
			}
		}

		private DateTimeOffset _itemDate;
		public DateTimeOffset ItemDate
		{
			get { return _itemDate; }
			set { SetProperty(ref _itemDate, value); }
		}

		public ICommand AddCommand { get; }

		public Visibility ListVisibility { get => DataList.Count() > 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility PlaceholderVisibility { get => DataList.Count() <= 0 ? Visibility.Visible : Visibility.Collapsed; }

		private readonly int pageSize = 50;
		private int currentPage = 0;

		private bool isLoading = false;

		private readonly IDataService _dataService;

		public DashboardViewModel(IDataService dataService)
		{
			DataList = new ObservableCollection<SalesRecordState>();
			ItemsList = new ObservableCollection<ComboBoxState>();
			ItemName = "";
			Rate = "";
			Quantity = "";
			Total = "";
			ItemDate = DateTime.Now;
			AddCommand = new RelayCommand(AddRecord, () => true);

			DataList.CollectionChanged += (s, e) =>
			{
				OnPropertyChanged(nameof(ListVisibility));
				OnPropertyChanged(nameof(PlaceholderVisibility));
			};

			_dataService = dataService;
			LoadData();
		}

		public void LoadData()
		{
			currentPage = 0;
			_ = LoadItems();
			_ = LoadRecords();
		}

		private async Task LoadItems()
		{
			ItemsList.Clear();
			List<SalesItemCounterRecord> items = await _dataService.GetSalesItemList();
			foreach (SalesItemCounterRecord item in items)
			{
				ItemsList.Add(
					new ComboBoxState
					{
						Name = item.ItemName,
						Data = item.ItemName
					}
				);
			}
		}

		private async Task LoadRecords()
		{
			List<SalesRecord> newData = await _dataService.ReadPaginatedSalesRecords(pageSize, currentPage);
			if (currentPage == 0)
			{
				DataList.Clear();
			}
			foreach (SalesRecord data in newData)
			{
				DataList.Add(
					new SalesRecordState
					{
						Id = data.Id,
						ItemName = data.ItemName,
						ItemQty = data.ItemQty.ToString(),
						ItemRate = data.ItemRate.ToString(),
						Total = data.Total.ToString(),
						Date = data.Date.ToString("dd-MM-yyyy")
					}
				);
			}
		}

		public async void LoadNextSetOfRecords()
		{

			if (isLoading)
			{
				return;
			}

			isLoading = true;

			currentPage++;
			await LoadRecords();

			isLoading = false;

		}

		private void AddRecord()
		{

			if (ItemName == null || ItemName.Length == 0)
			{
				MessageDialog dialog = new MessageDialog("Item Name cannot be empty.", "Missing Field");
				_ = dialog.ShowAsync();
				return;
			}

			if (_rate <= 0)
			{
				MessageDialog dialog = new MessageDialog("Rate must be greater than 0", "Invalid Field");
				_ = dialog.ShowAsync();
				return;
			}

			if (_quantity <= 0)
			{
				MessageDialog dialog = new MessageDialog("Quantity must be greater than 0", "Invalid Field");
				_ = dialog.ShowAsync();
				return;
			}

			var name = ItemName;
			var rate = _rate;
			var quantity = _quantity;
			var total = _total;
			var date = ItemDate.Date;

			_ = InsertData(new SalesRecord
			{
				ItemName = name,
				ItemRate = rate,
				ItemQty = quantity,
				Total = total,
				Date = date
			});

		}

		public async Task InsertData(SalesRecord item)
		{
			await _dataService.InsertData(item);
			ClearFields();
			currentPage = 0;
			DataList.Clear();
			LoadData();
		}

		private void ClearFields()
		{
			ItemName = "";
			Rate = "";
			Quantity = "";
			Total = "";
		}

		private void UpdateTotal()
		{

			double total = _quantity * _rate;

			Total = total.ToString();

		}

	}

}
