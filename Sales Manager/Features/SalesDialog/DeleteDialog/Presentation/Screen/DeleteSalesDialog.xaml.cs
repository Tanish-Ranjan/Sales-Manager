using Sales_Manager.Data.Model;
using Sales_Manager.Features.DeleteDialog.Presentation.ViewModel;
using Windows.UI.Xaml.Controls;

namespace Sales_Manager.Features.DeleteDialog.Presentation.Screen
{
	public sealed partial class DeleteSalesDialog : ContentDialog
	{
		public DeleteSalesDialog(SalesRecordState state)
		{
			InitializeComponent();
			DataContext = new DeleteDialogViewModel(state);
		}

		private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			Hide();
		}

		private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			await ((DeleteDialogViewModel)DataContext).DeleteRecord();
			Hide();
		}
	}
}
