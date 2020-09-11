using System;
using System.Collections.Generic;
using System.Text;

namespace Citrullia.Library.MassSpectra
{
    /// <summary>
    /// Information about an MGX-scan.
    /// </summary>
    public class MgxScan : MsSpectrumBase
    {
        /// <summary>The scan number.</summary>
        internal int ScanNumber { get; set; }
        /// <summary>The MS Level.</summary>
        internal int MSLevel { get; set; }
        /// <summary>The title.</summary>
        internal string Title { get; set; }
        /// <summary>The intensity of the precursor.</summary>
        internal double PreCursorIntencity { get; set; }
        /// <summary>The MZ-value for the base peak.</summary>
        internal double BasePeak { get; set; }
        /// <summary>The intensisty for the base peak.</summary>
        internal double BaseIntensity { get; set; }
        /// <summary>The total ion count.</summary>
        internal int TotalIonCount { get; set; }
    }
}
