using System.Collections.Generic;

namespace Citrullia.Library.XTandem
{
    /// <summary>Data class for representing X!Tandem result.</summary>
    // TODO: Maybe initialise by constructor
    internal class XTResult
    {
        /// <summary>The filepath of the result file.</summary>
        internal string FilePath { get; set; }
        /// <summary>The filename of the result file.</summary>
        internal string FileName { get; set; }
        /// <summary>The list of spectra in the result file.</summary>
        internal List<XTSpectrum> XSpectrums { get; set; }
        /// <summary>The filename of the original input file.</summary>
        internal string OriginalInputFile { get; set; }
    }
}
