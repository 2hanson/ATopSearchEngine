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
using ATopSearchEngine.IndexUtil;
using ATopSearchEngine.IndexUtil.item;
using ATopSearchEngine.SearchUtil;
using ATopSearchEngine.SearchUtil.info;
using ATopSearchEngine.ViewUtil;

namespace ATopSearchEngine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<List<string>> dataTable;
        private List<List<InvertedTableItem>> invertedTable;
        private List<String> tokenList;
        private List<FileInfoClass> rankedDocsIndex;
        private int docCounts;

        public MainWindow()
        {
            InitializeComponent();

            testIndexof();

            TokenProcess();
            IndexProcess();
        }

        private void testIndexof()
        {
            String query = "123 || 2334||中国 && 美国 || 日本 || 4563445&& 朝鲜||0980&&yudjfn ||po";

            List<string> ORTerms = new List<string>();
            
            int startPoint = 0;
            int endPoint = query.IndexOf("||");
            for (; ; endPoint = query.IndexOf("||", startPoint))
            {
                if (endPoint == -1)
                {
                    ORTerms.Add(query.Substring(startPoint, query.Length - startPoint).Trim());
                    break;
                }

                ORTerms.Add(query.Substring(startPoint, endPoint - startPoint).Trim());
                startPoint = endPoint + 2;
            }
            for (int index = 0; index < ORTerms.Count; ++index)
            {
                List<string> ANDTerms = new List<string>();
                int startPoint2 = 0;
                String query2 = ORTerms[index];
                int endPoint2 = query2.IndexOf("&&");
                for (; ; endPoint2 = query2.IndexOf("&&", startPoint2))
                {
                    if (endPoint2 == -1)
                    {
                        ANDTerms.Add(query2.Substring(startPoint2, query2.Length - startPoint2).Trim());
                        break;
                    }

                    ANDTerms.Add(query2.Substring(startPoint2, endPoint2 - startPoint2).Trim());
                    startPoint2 = endPoint2 + 2;
                }
            }
          /*  string includeTerm = query.Substring(0,k).Trim();
            string excludeTerm = query.Substring(k+3).Trim();
            int a = query.IndexOf("$%");
            int b = 0;
            while ((b = query.IndexOf("||", b)) == -2)
            {
                string lrftTerm1 = includeTerm.Substring(0, b).Trim();
                string rightTerm1 = includeTerm.Substring(b + 2).Trim();
            }*/
            

           // string lrftTerm = lrftTerm1.Substring(0, a).Trim();
           // string rightTerm = lrftTerm1.Substring(a + 2).Trim();


        }

        private void TokenProcess()
        {
            TokenAnalyzer tA = new TokenAnalyzer();
            tA.ProcessSourceFile();
            this.dataTable = tA.GetDataTable();
            this.docCounts = tA.GetDocCounts();

        }

        private void IndexProcess()
        {
            IndexUtilClass iU = new IndexUtilClass(this.dataTable);
            iU.StartIndex();
            this.invertedTable = iU.GetInvertedTable();
            this.tokenList = iU.TokenList;
           // iU.
        }

        private void SearchProcess()
        {
            SearchUtilClass suc = new SearchUtilClass(this.docCounts, this.invertedTable, this.tokenList);
            suc.ProcessQuery(queryTextBox.Text);
            this.rankedDocsIndex = suc.GetRankedDocsIndex();
            SearchView();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SearchProcess();
        }


        private void SearchView()
        {
            if (this.rankedDocsIndex == null)
                return;
            this.docListViewer.Items.Clear();
            for (int k = 0; k < this.rankedDocsIndex.Count; ++k)
            {
                ListItemUserControl bt = new ListItemUserControl(this.rankedDocsIndex[k].FileIndex);
                this.docListViewer.Items.Add(bt);
            }
        }
    }
}
