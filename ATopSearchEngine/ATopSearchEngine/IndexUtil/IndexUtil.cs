/*
 *建立倒排索引过程：
 *数据表
 *  DOCID             KeyWord
 *  doc1              k1, k2, k5, k2
 *  doc2              k3, k4, k6, k3
 *  doc3              k1, k3, k5
 *  doc4              k20, k2, k20, k20
 * 索引表
 *  KeyWord           DOCID
 *  k1                doc1, doc3
 *  k2                doc1, doc1, doc4
 *  k3                doc2, doc2, doc3
 *  k4                doc2
 *  k5                doc1
 *  k6                doc2
 *  k20               doc4, doc4, doc4
 * 右项归并后的索引表
 *  KeyWord           DOCID
 *  k1                doc1, doc3
 *  k2                doc1(2), doc4
 *  k3                doc2(2), doc3
 *  k4                doc2
 *  k5                doc1
 *  k6                doc2
 *  k20               doc4(3)
 *  
 * 此类主要功能是处理数据表，按照上面的步骤，最终生成右项归并后的索引表
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ATopSearchEngine.IndexUtil.item;

namespace ATopSearchEngine.IndexUtil
{
    public class IndexUtilClass
    {
        private List<List<string>> dataTable;
        private List<String> tokenList;
        private List<List<int>> tempIndexTable;//第二步中的索引表
        private List<List<InvertedTableItem>> invertedTable;//第san步中的索引表

        public IndexUtilClass(List<List<string>> _dataTable)
        {
            this.dataTable = _dataTable;
            this.tokenList = new List<string>();
            tempIndexTable = new List<List<int>>();
            invertedTable = new List<List<InvertedTableItem>>();
            StartIndex();
        }

        public List<List<InvertedTableItem>> GetInvertedTable()
        {
            return this.invertedTable;
        }

        public List<String> TokenList
        {
            get {
                return this.tokenList;
            }
        }

        public void StartIndex()
        {
            for (int fileIndex = 0; fileIndex < this.dataTable.Count(); ++fileIndex)
            {
                List<String> tokenListOfFile = this.dataTable[fileIndex];
                foreach (String token in tokenListOfFile)
                {
                    IndexToken(token, fileIndex);
                }
            }

            //
            MergeDupIndex();
        }

        public void MergeDupIndex()
        {
            foreach (List<int> docListOfEachToken in this.tempIndexTable)
            {
                List<InvertedTableItem> rowOfInvertedTable = new List<InvertedTableItem>();
                int preId = -1;
                
                int counts = 0;
                foreach (int docId in docListOfEachToken)
                {
                    if (docId != preId)
                    {
                        if (preId != -1)
                        {
                            rowOfInvertedTable.Add(new InvertedTableItem(preId, counts));
                        }

                        preId = docId;
                        counts = 1;
                    }
                    else
                    {
                        ++counts;
                    }
                }
                if (counts != 0)
                {
                    rowOfInvertedTable.Add(new InvertedTableItem(preId, counts));
                }
                this.invertedTable.Add(rowOfInvertedTable);
            }
        }

        private void IndexToken(String token, int fileIndex)
        { 
            int tokenId = -1;
            if ((tokenId = GetTokenId(token)) == -1)
            {
                this.tokenList.Add(token);
                List<int> tokenListOfFile = new List<int>();
                tokenListOfFile.Add(fileIndex);
                tempIndexTable.Add(tokenListOfFile);
            }
            else 
            {
                List<int> tokenListOfFile = tempIndexTable[tokenId];

                tokenListOfFile.Add(fileIndex);
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
    }
}
