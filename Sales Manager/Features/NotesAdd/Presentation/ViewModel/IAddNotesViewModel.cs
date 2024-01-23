using Sales_Manager.Commons.Data.Model;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Sales_Manager.Features.NotesAdd.Presentation.ViewModel
{
	public interface IAddNotesViewModel
	{

		ObservableCollection<NotesRecordState> DataList { get; }

		string NoteText { get; set; }
		DateTimeOffset NoteDate { get; set; }
		ICommand SaveCommand { get; }
		Visibility ListVisibility { get; }
		Visibility PlaceholderVisibility { get; }

		void LoadData();
		Task LoadNextSetOfRecords();

	}
}
