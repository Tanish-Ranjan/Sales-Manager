using Sales_Manager.Commons.Data.Model;
using Sales_Manager.Data.Repo;
using System.Threading.Tasks;

namespace Sales_Manager.Features.NotesDialogs.DeleteDialog.Presentation.ViewModel
{
	public class DeleteDialogViewModel
	{

		private readonly NotesRecordState originalRecord;
		private readonly DataService dataService;

		public DeleteDialogViewModel(NotesRecordState state)
		{
			originalRecord = state;
			dataService = new DataService();
		}

		public async Task DeleteRecord()
		{
			int id = originalRecord.Id;

			await dataService.DeleteNote(
				new NotesRecord
				{
					Id = id
				}
			);
		}

	}
}
