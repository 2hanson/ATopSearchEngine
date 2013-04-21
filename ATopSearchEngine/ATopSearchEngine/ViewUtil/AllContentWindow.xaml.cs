using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ATopSearchEngine.ViewUtil
{
    /// <summary>
    /// Interaction logic for AllContentWindow.xaml
    /// </summary>
    public partial class AllContentWindow : Window
    {
        private string fileContent;

        public AllContentWindow(string content)
        {
            InitializeComponent();

            fileContent = content;

            ShowAllContent();
        }

        private void ShowAllContent()
        {
            this.ShowControl.Text = this.fileContent;
        }
    }
}
