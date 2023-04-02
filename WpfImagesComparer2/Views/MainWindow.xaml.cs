using System.Windows;

namespace WpfImagesComparer2.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            KeyDown += (s, e) =>
            {
                // Prevent infinite loop
                if (e.Source != this)
                    return;
                // Pass event to Page
                if (PagesContainer.Content is UIElement ui)
                    ui.RaiseEvent(e);
            };
        }
    }
}
