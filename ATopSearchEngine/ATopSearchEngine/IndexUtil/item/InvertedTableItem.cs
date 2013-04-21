using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATopSearchEngine.IndexUtil.item
{
    public class InvertedTableItem
    {
        private int fileIndex;
        private int occurCounts;

        public int FileIndex
        {
            get {
                return this.fileIndex;
            }
            set {
                this.fileIndex = value;
            }
        }

        public int OccurCounts
        {
            get {
                return this.occurCounts;
            }
            set {
                this.occurCounts = value;
            }
        }

        public InvertedTableItem(int _index, int _counts)
        {
            this.FileIndex = _index;
            this.OccurCounts = _counts;
        }
    }
}
