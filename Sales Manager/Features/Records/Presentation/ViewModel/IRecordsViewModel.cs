using Sales_Manager.Data.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Sales_Manager.Features.Records.Presentation.ViewModel
{
	public interface IRecordsViewModel
	{

		ObservableCollection<ComboBoxState> ItemsList { get; }
		ObservableCollection<SalesRecordState> DataList { get; }

		int SelectedIndex { get; set; }
		DateTimeOffset StartDate { get; set; }
		DateTimeOffset EndDate { get; set; }

		ICommand ClearCommand { get; }
		ICommand SearchCommand { get; }

		Visibility ListVisibility { get; }
		Visibility PlaceholderVisibility { get; }

		string QuantityAggregate { get; }
		string TotalAggregate { get; }

		void LoadNextSetOfRecords();

	}
}
