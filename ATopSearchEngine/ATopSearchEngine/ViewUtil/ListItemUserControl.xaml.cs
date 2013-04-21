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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ATopSearchEngine.TokenUtil;

namespace ATopSearchEngine.ViewUtil
{
    /// <summary>
    /// Interaction logic for ListItemUserControl.xaml
    /// </summary>
    public partial class ListItemUserControl : UserControl
    {
        private int fileIndex;
        private string fileContent;
        public ListItemUserControl(int index)
        {
            InitializeComponent();

            this.summaryContent.Click += new RoutedEventHandler(summaryContent_Click);

            //this.SubContent
            this.fileIndex = index;
            GetFileContent();

            SetCurItem();
        }

        void summaryContent_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("hello");
            AllContentWindow acw = new AllContentWindow(this.fileContent);
            acw.Show();
        }

        private void SetCurItem()
        {
            if (this.fileContent != null)
            {
                this.summaryContent.Content = this.fileContent;
            }
        }

        private void GetFileContent()
        {
            string filePath = "./sourcefile/" + fileIndex.ToString() + ".txt";

            fileContent = TokenAnalyzer.ReadTextFromFile(filePath);
        }
    }
}
