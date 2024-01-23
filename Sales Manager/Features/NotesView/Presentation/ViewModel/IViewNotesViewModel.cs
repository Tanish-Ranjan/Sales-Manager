using Sales_Manager.Commons.Data.Model;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Sales_Manager.Features.NotesView.Presentation.ViewModel
{
	public interface IViewNotesViewModel
	{

		ObservableCollection<NotesRecordState> DataList { get; }
		Task LoadNextSetOfRecords();
		void LoadData();
		DateTimeOffset StartDate { get; set; }
		DateTimeOffset EndDate { get; set; }
		ICommand SearchCommand { get; }
		Visibility ListVisibility { get; }
		Visibility PlaceholderVisibility { get; }

	}
}
