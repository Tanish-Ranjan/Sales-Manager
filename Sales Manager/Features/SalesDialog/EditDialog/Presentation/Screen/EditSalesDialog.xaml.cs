using Sales_Manager.Data.Model;
using Sales_Manager.Features.EditDialog.Presentation.ViewModel;
using Windows.UI.Xaml.Controls;

namespace Sales_Manager.Features.EditDialog.Presentation.Screen
{
	public sealed partial class EditSalesDialog : ContentDialog
	{
		public EditSalesDialog(SalesRecordState state)
		{
			InitializeComponent();
			DataContext = new EditDialogViewModel(state);
		}

		private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			Hide();
		}

		private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			var success = await ((EditDialogViewModel)DataContext).UpdateSalesRecord();
			if (success)
			{
				Hide();
			}
		}
	}
}
