using Sales_Manager.Commons.Data.Model;
using Sales_Manager.Features.NotesDialogs.DeleteDialog.Presentation.ViewModel;
using Windows.UI.Xaml.Controls;

namespace Sales_Manager.Features.NotesDialogs.DeleteDialog.Presentation.Screen
{
	public sealed partial class DeleteNotesDialog : ContentDialog
	{
		public DeleteNotesDialog(NotesRecordState state)
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
