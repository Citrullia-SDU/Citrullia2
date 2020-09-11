using Citrullia.Library.MassSpectra;
using System.Collections.Generic;

namespace Citrullia.Library.XTandem
{
    /// <summary>Data class for representing X!Tandem spectrum.</summary>
    // TODO: Maybe initialise by constructor
    // TODO: Remove unnecessary properties 
    internal class XTSpectrum : MsSpectrumBase
    {
        /// <summary>The spectrum ID.</summary>
        internal int ID { get; set; }
        /// <summary>The parent mass.</summary>
        internal double ParentMass { get; set; }
        /// <summary>The parent ion charge.</summary>
        internal int ParentIonCharge { get; set; }
        /// <summary>The e-value for the spectrum.</summary>
        internal double SpectrumEVal { get; set; }
        /// <summary>The spectrum label. FASTA file description line for the highest ranked identified protein.</summary>
        internal string SpectrumLabel { get; set; }
        /// <summary>The log10 value of the intensity sum.</summary>
        internal double LogSumFragIons { get; set; }
        /// <summary>The max fragment intensity.</summary>
        internal double MaxFragIonInt { get; set; }
        /// <summary>Factor to convert normalized spectrum values to orginal values.</summary>
        internal double NormMulitplier { get; set; }
        /// <summary>The list of proteins in the spectrum.</summary>
        internal List<XTProtein> Proteins { get; set; }
        /// <summary>The hyperscore A0 of the spectrum.</summary>
        internal double A0 { get; set; }
        /// <summary>The hyperscore's A1 of the spectrum.</summary>
        internal double A1 { get; set; }
        /// <summary>The number of hyperscores.??</summary>
        internal int[] HyperscoreNumbs { get; set; }
        /// <summary>The count of hyperscores for each score??.</summary>
        internal int[] HyperscoreCounts { get; set; }
        /// <summary>The number of convolution survival function number.??</summary>
        internal int[] ConSurvivalNumbs { get; set; }
        /// <summary>The count of convolution survival function for each score.??</summary>
        internal int[] ConSurvivalCounts { get; set; }
        /// <summary>The number of b ions.</summary>
        internal int[] BIonNumbs { get; set; }
        /// <summary>The count of b ions.</summary>
        internal int[] BIonCounts { get; set; }
        /// <summary>The number of y ions.</summary>
        internal int[] YIonNumbs { get; set; }
        /// <summary>The number of y ions</summary>
        internal int[] YIonCounts { get; set; }
        /// <summary>The M/Z-value of the spectrum (M+H).</summary>
        internal double SpectrumMZ { get; set; }
        /// <summary>The scan title in the input file.</summary>
        internal string SpectrumDescription { get; set; }

        #region Display spectra data
        /// <summary>The masses of the amino acids in the seqence.</summary>
        internal double[] SpectrumAaMasses { get; set; }
        /// <summary>The domain range of the spectrum.</summary>
        internal int[] SpectrumDomainRange { get; set; }
        /// <summary>The reverse domain range of the spectrum.</summary>
        internal int[] SpectrumDomainRangeReverse { get; set; }
        /// <summary>The amino acids in the seqence.</summary>
        internal string[] SpectrumAminoAcidSequence { get; set; }
        /// <summary>The dictionary of modification indexes and their mass changes for the b-ion.</summary>
        internal Dictionary<int, double> SpectrumBIonModDict { get; set; }
        /// <summary>The dictionary of modification indexes and their mass changes for the y-ion.</summary>
        internal Dictionary<int, double> SpectrumYIonModDict { get; set; }
        #endregion
    }
}
