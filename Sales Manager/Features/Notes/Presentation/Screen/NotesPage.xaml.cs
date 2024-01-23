using Sales_Manager.Features.NotesAdd.Presentation.Screen;
using Sales_Manager.Features.NotesView.Presentation.Screen;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Sales_Manager.Features.Notes.Presentation.Screen
{
	public sealed partial class NotesPage : Page
	{
		public NotesPage()
		{
			InitializeComponent();
			var item = NotesNav.MenuItems[0];
			NotesNav.SelectedItem = item;
			NotesFrame.Navigate(typeof(AddNotesPage), null, new EntranceNavigationTransitionInfo());
		}

		private void NotesNav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{
			if (args.InvokedItemContainer.Tag == null)
			{
				return;
			}

			string selectedTag = args.InvokedItemContainer.Tag.ToString();
			switch (selectedTag)
			{
				case "Add_Notes":
					if (NotesFrame.CurrentSourcePageType == typeof(AddNotesPage)) return;
					NotesFrame.Navigate(typeof(AddNotesPage), null, args.RecommendedNavigationTransitionInfo);
					NotesFrame.BackStack.Clear();
					break;
				case "View_Notes":
					if (NotesFrame.CurrentSourcePageType == typeof(ViewNotesPage)) return;
					NotesFrame.Navigate(typeof(ViewNotesPage), null, args.RecommendedNavigationTransitionInfo);
					NotesFrame.BackStack.Clear();
					break;
			}
		}
	}
}
