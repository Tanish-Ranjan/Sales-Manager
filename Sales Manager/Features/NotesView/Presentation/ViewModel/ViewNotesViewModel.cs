using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sales_Manager.Commons.Data.Model;
using Sales_Manager.Commons.Data.Repo;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Sales_Manager.Features.NotesView.Presentation.ViewModel
{
	public class ViewNotesViewModel : ObservableObject, IViewNotesViewModel
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

		private DateTimeOffset LastSearch_StartDate;
		private DateTimeOffset LastSearch_EndDate;

		public ICommand SearchCommand { get; }

		public Visibility ListVisibility { get => DataList.Count() > 0 ? Visibility.Visible : Visibility.Collapsed; }
		public Visibility PlaceholderVisibility { get => DataList.Count() <= 0 ? Visibility.Visible : Visibility.Collapsed; }

		private bool isQuerying;

		private readonly int pageSize = 50;
		private int currentPage = 0;

		public ViewNotesViewModel(IDataService dataService)
		{

			DataList = new ObservableCollection<NotesRecordState>();
			StartDate = DateTime.Now;
			EndDate = DateTime.Now;

			LastSearch_StartDate = StartDate;
			LastSearch_EndDate = EndDate;

			SearchCommand = new RelayCommand(LoadData);

			DataList.CollectionChanged += (s, e) =>
			{
				OnPropertyChanged(nameof(ListVisibility));
				OnPropertyChanged(nameof(PlaceholderVisibility));
			};

			_dataService = dataService;
			LoadData();
		}

		public async Task LoadNextSetOfRecords()
		{
			if (isQuerying)
			{
				return;
			}

			isQuerying = true;

			if (LastSearch_StartDate == StartDate.Date && LastSearch_EndDate == EndDate.Date)
			{
				currentPage++;
				await LoadDataList();
			}

			isQuerying = false;
		}

		public void LoadData()
		{
			currentPage = 0;
			_ = LoadDataList();
		}

		private async Task LoadDataList()
		{

			var start = StartDate.Date;
			var end = EndDate.Date;
			LastSearch_StartDate = start;
			LastSearch_EndDate = end;
			var list = await _dataService.QueryNotesPaginated(start, end, pageSize, currentPage);

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

	}
}
