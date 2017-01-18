using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Common.Integration
{
    public enum EncoderStatus
    {
        /// <summary>
        /// Hasn't attempted to encode
        /// </summary>
        not_processed,
        /// <summary>
        /// Is Queued for encoding
        /// </summary>
        queued,
        /// <summary>
        /// Is currently processing
        /// </summary>
        processing,
        /// <summary>
        /// Failed to process
        /// </summary>
        fail,
        /// <summary>
        /// Raw data only, encoder aborted or skipped
        /// </summary>
        raw,
        /// <summary>
        /// Encoding complete
        /// </summary>
        complete
    }
}
