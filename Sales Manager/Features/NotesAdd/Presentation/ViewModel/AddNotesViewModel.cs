using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sales_Manager.Commons.Data.Model;
using Sales_Manager.Commons.Data.Repo;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace Sales_Manager.Features.NotesAdd.Presentation.ViewModel
{
	public class AddNotesViewModel : ObservableObject, IAddNotesViewModel
	{

		private readonly IDataService _dataService;

		private ObservableCollection<NotesRecordState> _dataList;
		public ObservableCollection<NotesRecordState> DataList
		{
			get { return _dataList; }
			set
			{
				SetProperty(ref _dataList, value);
				OnPropertyChanged(nameof(ListVisibility));
				OnPropertyChanged(nameof(PlaceholderVisibility));
			}
		}
		private string _noteText;
		public string NoteText
		{
			get { return _noteText; }
			set { SetProperty(ref _noteText, value); }
		}
		private DateTimeOffset _noteDate;
		public DateTimeOffset NoteDate
		{
			get { return _noteDate; }
			set { SetProperty(ref _noteDate, value); }
		}
		public ICommand SaveCommand { get; }

		public Visibility ListVisibility { get => DataList.Count() > 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility PlaceholderVisibility { get => DataList.Count() <= 0 ? Visibility.Visible : Visibility.Collapsed; }

		private readonly int pageSize = 50;
		private int currentPage = 0;

		private bool isQuerying = false;

		public AddNotesViewModel(IDataService dataService)
		{
			DataList = new ObservableCollection<NotesRecordState>();
			NoteText = "";
			NoteDate = DateTime.Now;
			SaveCommand = new RelayCommand(SaveNote);

			DataList.CollectionChanged += (s, e) =>
			{
				OnPropertyChanged(nameof(ListVisibility));
				OnPropertyChanged(nameof(PlaceholderVisibility));
			};

			_dataService = dataService;
			currentPage = 0;
			_ = LoadDataList();
		}

		public async Task LoadNextSetOfRecords()
		{
			if (isQuerying)
			{
				return;
			}

			isQuerying = true;

			currentPage++;
			await LoadDataList();

			isQuerying = false;
		}

		public void LoadData()
		{
			currentPage = 0;
			_ = LoadDataList();
		}

		private async Task LoadDataList()
		{

			var list = await _dataService.ReadNotesPaginated(pageSize, currentPage);

			if (currentPage == 0)
			{
				DataList.Clear();
			}

			list.ForEach(item =>
			{
				DataList.Add(
					new NotesRecordState
					{
						Id = item.Id,
						Note = item.Note,
						Date = item.Date.ToString("dd-MM-yyyy")
					}
				);
			});

		}

		private void SaveNote()
		{

			var note = NoteText;
			var date = NoteDate.Date;

			if (note == null || note.Length <= 0)
			{
				MessageDialog dialog = new MessageDialog("Note content cannot be empty.", "Missing Field");
				_ = dialog.ShowAsync();
				return;
			}

			_ = SaveToDatabase(note, date);

		}

		private async Task SaveToDatabase(string note, DateTime date)
		{
			await _dataService.InsertNote(
				new NotesRecord
				{
					Note = note,
					Date = date
				}
			);

			Clear();
			currentPage = 0;
			await LoadDataList();
		}

		private void Clear()
		{
			NoteText = "";
		}

	}
}
