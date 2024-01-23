using CommunityToolkit.Mvvm.ComponentModel;
using Sales_Manager.Data.Model;
using Sales_Manager.Data.Repo;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Sales_Manager.Features.EditDialog.Presentation.ViewModel
{
	public class EditDialogViewModel : ObservableObject
	{

		private readonly SalesRecordState originalRecord;
		private readonly DataService dataService;

		private string _name;
		public string Name
		{
			get { return _name; }
			set { SetProperty(ref _name, value); }
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

		private int _qty;
		public string Qty
		{
			get { return _qty.ToString(); }
			set
			{
				if (int.TryParse(value, out int parsedValue))
				{
					SetProperty(ref _qty, parsedValue);
				}
				else
				{
					SetProperty(ref _qty, 0);
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

		private DateTimeOffset _date;
		public DateTimeOffset Date
		{
			get { return _date; }
			set { SetProperty(ref _date, value); }
		}

		private ObservableCollection<ComboBoxState> _itemsList;
		public ObservableCollection<ComboBoxState> ItemsList
		{
			get { return _itemsList; }
			set { SetProperty(ref _itemsList, value); }
		}

		public EditDialogViewModel(SalesRecordState record)
		{
			dataService = new DataService();
			originalRecord = record;
			Name = record.ItemName;
			Rate = record.ItemRate;
			Qty = record.ItemQty;
			Date = DateTimeOffset.ParseExact(record.Date, "dd-MM-yyyy", null);
			ItemsList = new ObservableCollection<ComboBoxState>();

			_ = GetItemsList();
		}

		private async Task GetItemsList()
		{
			var list = await dataService.GetSalesItemList();
			list.ForEach(item =>
			{
				ItemsList.Add(
					new ComboBoxState
					{
						Name = item.ItemName,
						Data = item.ItemName
					}
				);
			});
		}

		private void UpdateTotal()
		{

			double total = _qty * _rate;
			Total = total.ToString();

		}

		public async Task<bool> UpdateSalesRecord()
		{

			int id = originalRecord.Id;
			string name = originalRecord.ItemName;
			double rate = double.Parse(originalRecord.ItemRate);
			int qty = int.Parse(originalRecord.ItemQty);
			double total = double.Parse(originalRecord.Total);
			DateTime date = DateTimeOffset.ParseExact(originalRecord.Date, "dd-MM-yyyy", null).Date;

			string new_name = originalRecord.ItemName;
			double new_rate = _rate;
			int new_qty = _qty;
			double new_total = _total;
			DateTime new_date = Date.Date;

			if (new_name == null || new_name.Length == 0)
			{
				return false;
			}

			if (new_rate <= 0)
			{
				return false;
			}

			if (new_qty <= 0)
			{
				return false;
			}

			await dataService.UpdateData(
				new SalesRecord
				{
					Id = id,
					ItemName = name,
					ItemRate = rate,
					ItemQty = qty,
					Total = total,
					Date = date
				},
				new SalesRecord
				{
					Id = id,
					ItemName = new_name,
					ItemRate = new_rate,
					ItemQty = new_qty,
					Total = new_total,
					Date = new_date
				}
			);

			return true;

		}

	}
}
