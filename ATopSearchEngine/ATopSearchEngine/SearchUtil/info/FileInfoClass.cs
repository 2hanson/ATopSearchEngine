using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ATopSearchEngine.SearchUtil.info
{
    public class FileInfoClass
    {
        private double dfitfValue;
        private int fileIndex;

        public double DFITFValue
        { 
            get {
                return this.dfitfValue;
            }
            set {
                this.dfitfValue = value;
            }
        }

        public int FileIndex
        {
            get {
                return this.fileIndex;
            }
            set {
                this.fileIndex = value;
            }
        }

        public FileInfoClass(int index, double value)
        {
            this.FileIndex = index;
            this.DFITFValue = value;
        }
    }
}
