using Sales_Manager.Data.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace Sales_Manager.Features.Dashboard.Presentation.ViewModel
{
	public interface IDashboardViewModel
	{

		ObservableCollection<SalesRecordState> DataList { get; }
		ObservableCollection<ComboBoxState> ItemsList { get; }

		string ItemName { get; set; }
		string Rate { get; set; }
		string Quantity { get; set; }
		string Total { get; }
		DateTimeOffset ItemDate { get; set; }

		ICommand AddCommand { get; }

		Visibility ListVisibility { get; }
		Visibility PlaceholderVisibility { get; }

		void LoadNextSetOfRecords();
		void LoadData();

	}
}
