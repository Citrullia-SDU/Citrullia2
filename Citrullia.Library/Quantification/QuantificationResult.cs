namespace Citrullia
{
    /// <summary>
    /// Data class representing the Quantification result.
    /// </summary>
    // TODO: Maybe initialise by constructor
    internal class QuantificationResult
    {
        /// <summary>The protein label.</summary>
        internal string Protein { get; set; }
        /// <summary>The protein sequence.</summary>
        internal string Sequence { get; set; }
        /// <summary>The extracted ion count for the citrullinated spectra.</summary>
        internal double CitrullinatedExtractedIonCount { get; set; }
        /// <summary>The extracted ion count for the arginine spectra.</summary>
        internal double ArginineExtractedIonCount { get; set; }
        /// <summary>The citrullination percentage for the result.</summary>
        internal double CitPercent { get; set; }
        /// <summary>The type of validation. Cit-paired=Citrulline-pair. Arg-paired=Arginine-pair. Lone=Lonely.</summary>
        internal string ValidationBy { get; set; }
        /// <summary>The filename of the file that contains the citrulinated spectra.</summary>
        internal string CitFileName { get; set; }
        /// <summary>The filename of the file that contains the arginine spectra.</summary>
        internal string ArgFileName { get; set; }
        /// <summary>The retention time for the citrullination spectra.</summary>
        internal double CitRetentionTime { get; set; }
        /// <summary>The rention time for the arginine spectra.</summary>
        internal double ArgRetentionTime { get; set; }
        /// <summary>The charge of the citrullinated spectra.</summary>
        internal double CitCharge { get; set; }
        /// <summary>The charge of the arginine spectra.</summary>
        internal double ArgCharge { get; set; }
        /// <summary>The ID for the citrullinated spectra.</summary>
        internal int CitSpectrumID { get; set; }
        /// <summary>The ID for the arginine spectra.</summary>
        internal int ArgSpectrumID { get; set; }
        /// <summary>The spectrum score.</summary>
        internal double Score { get; set; }
        /// <summary>The score of the match.</summary>
        internal double MatchScore { get; set; }
    }
}
