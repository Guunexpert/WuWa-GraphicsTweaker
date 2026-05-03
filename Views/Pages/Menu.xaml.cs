using PhoebeEditor.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace PhoebeEditor.Views.Pages
{
    public partial class Menu : Page
    {
        public Menu()
        {
            InitializeComponent();
        }

        public Menu(MainViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
        private void VideoBg_MediaEnded(object sender, RoutedEventArgs e)
        {

            VideoBg.Position = TimeSpan.FromMilliseconds(1);

            VideoBg.Play();
        }
        private void Element_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show(e.ErrorException.Message);
        }
    }

}
