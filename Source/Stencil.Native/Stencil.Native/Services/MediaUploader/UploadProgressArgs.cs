using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.Native.Services.MediaUploader
{
    public class UploadProgressArgs : EventArgs
    {
        long _incrementTransferred;
        long _total;
        long _transferred;

        public UploadProgressArgs(long incrementTransferred, long transferred, long total)
        {
            this._incrementTransferred = incrementTransferred;
            this._transferred = transferred;
            this._total = total;
        }
        public int PercentDone
        {
            get { return (int)((_transferred * 100) / _total); }
        }

        internal long IncrementTransferred
        {
            get { return this._incrementTransferred; }
        }


        public long TransferredBytes
        {
            get { return _transferred; }
        }

        public long TotalBytes
        {
            get { return _total; }
        }

        public override string ToString()
        {
            return String.Concat(
                "Transfer Statistics. Percentage completed: ",
                PercentDone,
                ", Bytes transferred: ",
                _transferred,
                ", Total bytes to transfer: ",
                _total
                );
        }    
    }
}
