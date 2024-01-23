using Microsoft.Extensions.DependencyInjection;
using Sales_Manager.Data.Model;
using Sales_Manager.Features.DeleteDialog.Presentation.Screen;
using Sales_Manager.Features.EditDialog.Presentation.Screen;
using Sales_Manager.Features.Records.Presentation.ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Sales_Manager.Features.Records.Presentation.Screen
{
	public sealed partial class RecordsPage : Page
	{
		public RecordsPage()
		{
			InitializeComponent();
			DataContext = ((App)Application.Current).Container.GetService<IRecordsViewModel>();
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

		private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
		{
			var tappedItem = (UIElement)e.OriginalSource;
			var attachedFlyout = (MenuFlyout)FlyoutBase.GetAttachedFlyout(sender as FrameworkElement);

			attachedFlyout.ShowAt(tappedItem, e.GetPosition(tappedItem));
		}

		private void Edit_Click(object sender, RoutedEventArgs e)
		{
			var item = ((FrameworkElement)sender).DataContext;
			EditSalesDialog dialog = new EditSalesDialog(item as SalesRecordState);

			var viewModel = (IRecordsViewModel)DataContext;

			dialog.Closed += (s, args) =>
			{
				viewModel?.SearchCommand.Execute(null);
			};

			_ = dialog.ShowAsync();

		}

		private void Delete_Click(object sender, RoutedEventArgs e)
		{
			var item = ((FrameworkElement)sender).DataContext;
			DeleteSalesDialog dialog = new DeleteSalesDialog(item as SalesRecordState);

			var viewModel = (IRecordsViewModel)DataContext;

			dialog.Closed += (s, args) =>
			{
				viewModel?.SearchCommand.Execute(null);
			};

			_ = dialog.ShowAsync();
		}

		private ScrollViewer GetScrollViewer(DependencyObject depObj)
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
				((IRecordsViewModel)DataContext).LoadNextSetOfRecords();
			}
		}

	}
}
