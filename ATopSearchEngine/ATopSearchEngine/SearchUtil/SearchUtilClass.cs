using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ATopSearchEngine.IndexUtil.item;
using Lucene.Net.Analysis;
using System.IO;
using ATopSearchEngine.SearchUtil.info;
using System.Text.RegularExpressions;

namespace ATopSearchEngine.SearchUtil
{
    public class SearchUtilClass
    {
        private List<List<InvertedTableItem>> invertedTable;
        List<string> queryTerms;
        private List<String> tokenList;
        int queryType;//0 是布尔查询，1是关键词查询，2是通配符查询
        int docCounts;
        private List<FileInfoClass> relativedDocs;
        private List<FileInfoClass> rankedDocsIndex;

        public SearchUtilClass(int _counts, List<List<InvertedTableItem>> _invertedTable, List<String> _tokenList)
        {
            this.invertedTable = _invertedTable;

            this.queryTerms = new List<string>();
            this.relativedDocs = new List<FileInfoClass>();
            this.rankedDocsIndex = new List<FileInfoClass>();
            this.docCounts = _counts;
            this.tokenList = _tokenList;
        }

        public List<FileInfoClass> GetRankedDocsIndex()
        {
            return this.rankedDocsIndex;
        }

        private List<FileInfoClass> RetriveRelativeDocFromQuery(String query)
        {
            if (query == null || query.CompareTo("") == 0)
                return null;
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

            List<FileInfoClass> results = new List<FileInfoClass>();

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

                List<FileInfoClass> subDocs = RetriveSubDocs(ANDTerms);
                
                foreach (FileInfoClass fic in subDocs)
                {
                    bool flag = false;
                    foreach (FileInfoClass doc in results)
                    {
                        if (doc.FileIndex == fic.FileIndex)
                        {
                            flag = true;
                            doc.DFITFValue += fic.DFITFValue;
                        }
                    }

                    if (false == flag)
                    {
                        results.Add(fic);
                    }
                }
            }

            return results;
        }

        private List<FileInfoClass> RetriveSubDocs(List<string> ANDTerms)
        {
            List<FileInfoClass> subDocs = new List<FileInfoClass>();

            List<List<InvertedTableItem>> allItemLists = new List<List<InvertedTableItem>>();
            //foreach (string term in ANDTerms)
            for (int index = 0; index < ANDTerms.Count; ++index )
            {
                string term = ANDTerms[index];
                int tokenId = GetTokenId(term);
                if (tokenId != -1)
                {
                    List<InvertedTableItem> itemList = this.invertedTable[tokenId];
                    allItemLists.Add(itemList);
                }
            }

            List<InvertedTableItem> subItemLists = allItemLists[0];
            //计算交集
            for (int index = 1; index < allItemLists.Count; ++index)
            {
                List<InvertedTableItem> tempList = new List<InvertedTableItem>();
                foreach (InvertedTableItem item in allItemLists[index])
                {
                    foreach (InvertedTableItem item2 in subItemLists)
                    {
                        if (item2.FileIndex == item.FileIndex)
                        {
                            tempList.Add(item);
                            break;
                        }
                    }
                }

                subItemLists.Clear();
                foreach (InvertedTableItem item in tempList)
                {
                    subItemLists.Add(item);
                }
            }

            foreach (InvertedTableItem item in subItemLists)
            {
                //计算df-itf
                double df = item.OccurCounts;
                double itf = Math.Log10(((double)this.docCounts / (double)subItemLists.Count()));
                FileInfoClass fic = new FileInfoClass(item.FileIndex, df * itf);

                bool flag = false;
                foreach (FileInfoClass doc in subDocs)
                {
                    if (doc.FileIndex == fic.FileIndex)
                    {
                        flag = true;
                        doc.DFITFValue += fic.DFITFValue;
                    }
                }

                if (false == flag)
                {
                    subDocs.Add(fic);
                }
            }

            return subDocs;
        }

        public void ProcessQuery(string queryText)
        {
            this.queryType = DetectQueryType(queryText);
            
            //布尔查询
            if (this.queryType == 0)
            {
                String includeTerm="";
                String excludeTerm="";
                int NOTSplit = queryText.IndexOf("NOT");
                if (NOTSplit != -1)
                {
                    includeTerm  = queryText.Substring(0, NOTSplit).Trim();
                    excludeTerm = queryText.Substring(NOTSplit + 3).Trim();
                }
                else
                {
                    includeTerm = queryText;
                }
                List<FileInfoClass> includeDoc = RetriveRelativeDocFromQuery(includeTerm);
                List<FileInfoClass> excludeDoc = RetriveRelativeDocFromQuery(excludeTerm);
                if (includeDoc != null)
                {
                    foreach (FileInfoClass fic in includeDoc)
                    {
                        AddIntoRelativedDocs(fic);
                    }
                }
                if (excludeDoc != null)
                {
                    foreach (FileInfoClass exfic in excludeDoc)
                    {
                       // AddIntoRelativedDocs(exfic);
                        RemoveFileFromRelativedDocs(exfic);
                    }
                }
            }
            else
            {
                if (this.queryType == 1)//关键词查询
                {
                    RetriveTokenFromQuery(queryText);//主要是生成this.queryTerms供下面使用 
                }
                else if (this.queryType == 2)//通配符查询
                {
                    RetriveTokenThroughMatchPatten(queryText);
                }

                //计算tf-idf
                GetRelativeDoc();
            }

            //根据tf-idf，rank 各个doc
            RankRelativeDocs();
        }

        //this.queryTerms
        private void RetriveTokenThroughMatchPatten(string queryText)
        {
            Regex queryPattern = new Regex(queryText);
            for (int tokenIndex = 0; tokenIndex < this.tokenList.Count(); ++tokenIndex)
            {
                string existItem = this.tokenList[tokenIndex];
               
                if ( queryPattern.IsMatch(existItem) == true)
                {
                    this.queryTerms.Add(existItem);
                }
            }
        }

        private int DetectQueryType(string queryText)
        {
            if (queryText.Contains("||") || queryText.Contains("&&") || queryText.Contains("NOT") || queryText.Contains("not"))
            {
                return 0;
            }
            else if (queryText.Contains("*") || queryText.Contains("+") || queryText.Contains("?") || queryText.Contains("/d") || queryText.Contains("]"))
            {
                return 2;
            }

            return 1;
        }

        private Boolean CheckIsContainedInRankedDocs(FileInfoClass newFic)
        {
            foreach (FileInfoClass item in this.rankedDocsIndex)
            {
                if (item.FileIndex == newFic.FileIndex)
                {
                    return true;
                }
            }

            return false;
        }

        private void InsertSort()
        {
            for (int curIndex = 0; curIndex < this.relativedDocs.Count(); ++curIndex)
            {
                FileInfoClass newFic = this.relativedDocs[curIndex];

                if (CheckIsContainedInRankedDocs(newFic) == false)
                {
                    this.rankedDocsIndex.Add(newFic);

                    for (int index = curIndex; index > 0; --index)
                    {
                        FileInfoClass curFic = this.relativedDocs[index];
                        FileInfoClass preFic = this.rankedDocsIndex[index - 1];
                        if (preFic.DFITFValue < curFic.DFITFValue)
                        {
                            Swap(curFic, preFic);
                        }
                    }
                }
 
            }
        }

        private void Swap(FileInfoClass curFic, FileInfoClass preFic)
        {
            int tempIndex = curFic.FileIndex;
            double tempValue = curFic.DFITFValue;

            curFic.FileIndex = preFic.FileIndex;
            curFic.DFITFValue = preFic.DFITFValue;

            preFic.FileIndex = tempIndex;
            preFic.DFITFValue = tempValue;

        }

        private void RankRelativeDocs()
        {
            InsertSort();
        }

        private void AddIntoRelativedDocs(FileInfoClass fic)
        {
            bool flag = false;
            foreach (FileInfoClass doc in this.relativedDocs)
            {
                if (doc.FileIndex == fic.FileIndex)
                {
                    flag = true;
                    doc.DFITFValue += fic.DFITFValue;
                }
            }

            if (false == flag)
            {
                this.relativedDocs.Add(fic);
            }
        }

        private void RemoveFileFromRelativedDocs(FileInfoClass fic)
        {
            foreach (FileInfoClass doc in this.relativedDocs)
            {
                if (doc.FileIndex == fic.FileIndex)
                {
                    this.relativedDocs.Remove(doc);
                    break;
                }
            }
        }

        private void GetRelativeDoc()
        {
            foreach (string term in this.queryTerms)
            {
                int tokenId = GetTokenId(term);
                if (tokenId != -1)
                {
                    List<InvertedTableItem> itemList = this.invertedTable[tokenId];

                    foreach (InvertedTableItem item in itemList)
                    { 
                        //计算df-itf
                        double df = item.OccurCounts;
                        double itf = Math.Log10(((double)this.docCounts / (double)itemList.Count()));
                        FileInfoClass fic = new FileInfoClass(item.FileIndex, df*itf);

                        AddIntoRelativedDocs(fic);
                    }
                }
            }
        }

        private int GetTokenId(String newToken)
        {
            for (int tokenIndex = 0; tokenIndex < this.tokenList.Count(); ++tokenIndex)
            {
                string existItem = this.tokenList[tokenIndex];
                if (existItem.CompareTo(newToken) == 0)
                {
                    return tokenIndex;
                }
            }

            return -1;
        }

        //关键词查询
        private void RetriveTokenFromQuery(string queryText)
        {
            
            string curNewToken = "";
            Analyzer analyzer = new Lucene.China.ChineseAnalyzer();
            StringReader sr = new StringReader(queryText);
            TokenStream stream = analyzer.TokenStream(null, sr);
            Token nextToken = stream.Next();

            while (nextToken != null)
            {
                curNewToken = nextToken.ToString();   //显示格式： (关键词,0,2) ，需要处理
                curNewToken = curNewToken.Replace("(", "");
                char[] separator = { ',' };
                curNewToken = curNewToken.Split(separator)[0];
                //StoreTokenIntoList(curNewToken, fileIndex);
                this.queryTerms.Add(curNewToken);
                nextToken = stream.Next();
            }
        }

    }
}
