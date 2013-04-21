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
 * 此类主要功能是对原文件进行分词，生成数据表
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.China;
using Lucene.Net;
using Lucene.Net.Analysis;
using Lucene.Net.Search;
using System.IO;

namespace ATopSearchEngine.TokenUtil
{
    public class TokenAnalyzer
    {
        private const int MAXFILESIZE = 8000;
        private List<List<string>> dataTable;
       
        private int totalFileCount;

        public TokenAnalyzer()
        {
            this.dataTable = new List<List<string>>();
        }

        public List<List<string>> GetDataTable()
        {
            return this.dataTable;
        }

        public int GetDocCounts()
        {
            return this.totalFileCount;
        }

        public void ProcessSourceFile()
        {
            totalFileCount = 0;
            for (int fileIndex = 0; fileIndex < MAXFILESIZE; ++fileIndex)
            {
                string filePath = "./sourcefile/" + fileIndex.ToString() + ".txt";
                String sourceText = null;

                //各文件按数字从小到大命名，如果某数字命名的文件不存在，后面的也不会有了。
                if ((sourceText = ReadTextFromFile(filePath)) == null)
                    break;

                totalFileCount++;
                RetriveTokenFromText(sourceText, fileIndex);
            }
        }

        private void RetriveTokenFromText(string sourceText, int fileIndex)
        {
            List<string> dataTableItem = new List<string>();
            string curNewToken = "";
            Analyzer analyzer = new Lucene.China.ChineseAnalyzer();
            StringReader sr = new StringReader(sourceText);
            TokenStream stream = analyzer.TokenStream(null, sr);
            Token nextToken = stream.Next();

            while (nextToken != null)
            {
                curNewToken = nextToken.ToString();   //显示格式： (关键词,0,2) ，需要处理
                curNewToken = curNewToken.Replace("(", "");
                char[] separator = { ',' };
                curNewToken = curNewToken.Split(separator)[0];
                //StoreTokenIntoList(curNewToken, fileIndex);
                dataTableItem.Add(curNewToken);
                nextToken = stream.Next();
            }

            this.dataTable.Add(dataTableItem);
        }

        /// <summary>
        /// 这个函数把文件的每一行读入list
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadTextFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                // 打开文件时 一定要注意编码 也许你的那个文件并不是GBK编码的
                using (StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding("GBK")))
                {
                    if (!sr.EndOfStream) //读到结尾退出
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            return null;
        }
    }
}
