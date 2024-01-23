using CommunityToolkit.Mvvm.ComponentModel;
using Sales_Manager.Commons.Data.Model;
using Sales_Manager.Data.Repo;
using System;
using System.Threading.Tasks;

namespace Sales_Manager.Features.NotesDialogs.EditDialog.Presentation.ViewModel
{
	public class EditDialogViewModel : ObservableObject
	{

		private readonly NotesRecordState originalRecord;
		private readonly DataService dataService;

		private DateTimeOffset _noteDate;
		public DateTimeOffset NoteDate
		{
			get { return _noteDate; }
			set { SetProperty(ref _noteDate, value); }
		}

		private string _noteText;
		public string NoteText
		{
			get { return _noteText; }
			set { SetProperty(ref _noteText, value); }
		}

		public EditDialogViewModel(NotesRecordState state)
		{
			originalRecord = state;
			dataService = new DataService();

			NoteText = state.Note;
			NoteDate = DateTimeOffset.ParseExact(state.Date, "dd-MM-yyyy", null);
		}

		public async Task<bool> UpdateNote()
		{
			int id = originalRecord.Id;

			string note = NoteText;
			DateTime date = NoteDate.Date;

			if (note == null || note.Length <= 0)
			{
				return false;
			}

			await dataService.UpdateNote(
				new NotesRecord
				{
					Id = id
				},
				new NotesRecord
				{
					Note = note,
					Date = date
				}
			);

			return true;

		}

	}
}
