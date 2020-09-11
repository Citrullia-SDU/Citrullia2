using Citrullia.Library.XTandem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Citrullia.Library.MassSpectra
{
    /// <summary>
    /// The abstract class containing general information about <see cref="MgxScan"/> and <see cref="XTSpectrum"/>.
    /// </summary>
    public abstract class MsSpectrumBase
    {
        #region General information
        /// <summary>The filename of the orginal input file.</summary>
        internal string OriginalFileName { get; set; }
        /// <summary>The retention time of the spectra.</summary>
        internal int RetentionTime { get; set; }
        /// <summary>The scan number of the parent.</summary>
        internal int ParentScanNumber { get; set; }
        /// <summary>The parent scan.</summary>
        internal MgxScan ParentScan { get; set; }
        /// <summary>Indicates that the spectrum is an orphan (No parents).</summary>
        internal bool Orphan { get; set; }
        #endregion

        #region Spectra information
        /// <summary>The MZ-values in the spectrum.</summary>
        internal double[] MzValues { get; set; }
        /// <summary>The intensities in the spectrum.</summary>
        internal double[] Intencities { get; set; }
        /// <summary>The MZ-value of the precursor.</summary>
        internal double PreCursorMz { get; set; }
        /// <summary>The charge of the scan.</summary>
        internal double Charge { get; set; }
        #endregion

        #region Ion index array
        /// <summary>The index of the B ions in the sequence.</summary>
        internal int[] BIonIndex { get; set; }
        /// <summary>The index of the B-17 ions in the sequence.</summary>
        internal int[] B17IonIndex { get; set; }
        /// <summary>The index of the B-18 ions in the sequence.</summary>
        internal int[] B18IonIndex { get; set; }
        /// <summary>The index of the Y ions in the sequence.</summary>
        internal int[] YIonIndex { get; set; }
        /// <summary>The index of the Y-17 ions in the sequence.</summary>
        internal int[] Y17IonIndex { get; set; }
        /// <summary>The index of the Y-18 ions in the sequence.</summary>
        internal int[] Y18IonIndex { get; set; }
        /// <summary>The index of the A ions in the sequence.</summary>
        internal int[] AIonIndex { get; set; }
        #endregion

        #region Spectra ions
        /// <summary>The posible B ion for every aa in the sequence.</summary>
        internal double[] SpectrumPossibleBIons { get; set; }
        /// <summary>The posible B-18 ion for every aa in the sequence.</summary>
        internal double[] SpectrumPossibleBm18Ions { get; set; }
        /// <summary>The posible B-17 ion for every aa in the sequence.</summary>
        internal double[] SpectrumPossibleBm17Ions { get; set; }

        /// <summary>The B-18 ions in the spectrum.</summary>
        internal double[] SpectrumBm18Ions { get; set; }
        /// <summary>The B-17 ions in the spectrum.</summary>
        internal double[] SpectrumBm17Ions { get; set; }
        /// <summary>The B ions in the spectrum.</summary>
        internal double[] SpectrumExistingBIons { get; set; }

        /// <summary>The posible Y ion for every aa in the sequence.</summary>
        internal double[] SpectrumPossibleYIons { get; set; }
        /// <summary>The posible Y-18 ion for every aa in the sequence.</summary>
        internal double[] SpectrumPossibleYm18Ions { get; set; }
        /// <summary>The posible Y-17 ion for every aa in the sequence.</summary>
        internal double[] SpectrumPossibleYm17Ions { get; set; }

        /// <summary>The Y-18 ions in the spectrum.</summary>
        internal double[] SpectrumYm18Ions { get; set; }
        /// <summary>The Y-17 ions in the spectrum.</summary>
        internal double[] SpectrumYm17Ions { get; set; }
        /// <summary>The Y ions in the spectrum.</summary>
        internal double[] SpectrumExistingYIons { get; set; }

        /// <summary>The A ions in the spectrum.</summary>
        internal double[] SpectrumAIons { get; set; }
        /// <summary>The posible A ion for every aa in the sequence.</summary>
        internal double[] SpectrumPossibleAIons { get; set; }
        /// <summary>The M/Z value of the isocyanic ion on MS1 level.</summary>
        internal double IsoCyanicMzMs1 { get; set; }
        /// <summary>The M/Z value of the isocyanic ion on MS2 level.</summary>
        internal double[] IsoCyanicLossMzMs2 { get; set; }
        #endregion

        #region Citrullia score
        /// <summary>The CitScore for the main spectrum. Is 0 if spectra is complementary.</summary>
        internal double CitScore { get; set; }
        /// <summary>The score for the complementary spectra.</summary>
        internal double MatchScore { get; set; }
        #endregion
    }
}
