using Sales_Manager.Commons.Data.Model;
using Sales_Manager.Features.NotesDialogs.EditDialog.Presentation.ViewModel;
using Windows.UI.Xaml.Controls;

namespace Sales_Manager.Features.NotesDialogs.DeleteDialog.Presentation.Screen
{
	public sealed partial class EditNotesDialog : ContentDialog
	{
		public EditNotesDialog(NotesRecordState state)
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
			var success = await (DataContext as EditDialogViewModel).UpdateNote();
			if (success)
			{
				Hide();
			}
		}
	}
}
