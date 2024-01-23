using Microsoft.Extensions.DependencyInjection;
using Sales_Manager.Commons.Data.Model;
using Sales_Manager.Features.NotesDialogs.DeleteDialog.Presentation.Screen;
using Sales_Manager.Features.NotesView.Presentation.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Sales_Manager.Features.NotesView.Presentation.Screen
{

	public sealed partial class ViewNotesPage : Page
	{

		public ViewNotesPage()
		{
			InitializeComponent();
			DataContext = ((App)Application.Current).Container.GetService<IViewNotesViewModel>();
		}

		private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
		{
			var tappedItem = (UIElement)e.OriginalSource;
			var attachedFlyout = (MenuFlyout)FlyoutBase.GetAttachedFlyout(sender as FrameworkElement);

			attachedFlyout.ShowAt(tappedItem, e.GetPosition(tappedItem));
		}

		private void Edit_Click(object sender, RoutedEventArgs e)
		{
			var item = ((FrameworkElement)sender).DataContext;
			EditNotesDialog dialog = new EditNotesDialog(item as NotesRecordState);

			var viewModel = (IViewNotesViewModel)DataContext;

			dialog.Closed += (s, args) =>
			{
				viewModel?.LoadData();
			};

			_ = dialog.ShowAsync();

		}

		private void Delete_Click(object sender, RoutedEventArgs e)
		{
			var item = ((FrameworkElement)sender).DataContext;
			DeleteNotesDialog dialog = new DeleteNotesDialog(item as NotesRecordState);

			var viewModel = (IViewNotesViewModel)DataContext;

			dialog.Closed += (s, args) =>
			{
				viewModel?.LoadData();
			};

			_ = dialog.ShowAsync();
		}

		private void ListView_Loaded(object sender, RoutedEventArgs e)
		{
			var listView = sender as ListView;
			var scrollViewer = GetScrollViewer(listView);
			if (scrollViewer != null)
			{
				scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
			}
		}

		public static ScrollViewer GetScrollViewer(DependencyObject depObj)
		{
			if (depObj is ScrollViewer) return depObj as ScrollViewer;

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
			{
				var child = VisualTreeHelper.GetChild(depObj, i);

				var result = GetScrollViewer(child);
				if (result != null) return result;
			}
			return null;
		}

		private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			var scrollViewer = sender as ScrollViewer;
			if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
			{
				((IViewNotesViewModel)DataContext).LoadNextSetOfRecords();
			}
		}

	}

}
