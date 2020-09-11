using Citrullia.Library.MassSpectra;
using System;
using System.Collections.Generic;
using System.Text;

namespace Citrullia.Library
{
    /// <summary>Data class that represents the Mascot Generic Extended-file input (.MGX-files).</summary>
    // TODO: Mabye use constructor initalisation.
    internal class InputData
    {
        /// <summary>The filepath of the input.</summary>
        internal string FilePath { get; set; }
        /// <summary>The list of MS1 scans.</summary>
        internal List<MgxScan> MS1Scans { get; set; }
        /// <summary>The list of MS2 scans.</summary>
        internal List<MgxScan> MS2Scans { get; set; }
    }
}
