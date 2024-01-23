using Sales_Manager.Data.Model;
using Sales_Manager.Data.Repo;
using System;
using System.Threading.Tasks;

namespace Sales_Manager.Features.DeleteDialog.Presentation.ViewModel
{
	public class DeleteDialogViewModel
	{

		private readonly SalesRecordState originalRecord;
		private readonly DataService dataService;

		public DeleteDialogViewModel(SalesRecordState record)
		{
			originalRecord = record;
			dataService = new DataService();
		}

		public async Task DeleteRecord()
		{

			int id = originalRecord.Id;
			string name = originalRecord.ItemName;
			double rate = double.Parse(originalRecord.ItemRate);
			int qty = int.Parse(originalRecord.ItemQty);
			double total = double.Parse(originalRecord.Total);
			DateTime date = DateTimeOffset.ParseExact(originalRecord.Date, "dd-MM-yyyy", null).Date;

			await dataService.DeleteData(
				new SalesRecord
				{
					Id = id,
					ItemName = name,
					ItemRate = rate,
					ItemQty = qty,
					Total = total,
					Date = date
				}
			);

		}

	}
}
