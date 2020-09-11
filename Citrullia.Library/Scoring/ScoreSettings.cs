using System.Globalization;

namespace Citrullia.Library.Scoring
{
    /// <summary>
    /// Class containing information about the score weights.
    /// </summary>
    internal static class ScoreSettings
    {
        /// <summary>The score at which the spectra should be added to quantification automatically.</summary>
        internal static double CutOffScore { get; set; }
        /// <summary>Indicates if the spectra above the cut-off should be automatically added.</summary>
        internal static bool IncludeCutOff { get; set; }
        /// <summary>The tolerance for determining if ion is a cyanic loss.</summary>
        internal const double CyanicLossTolerance = 0.07;
        /// <summary>The score per A-ion match.</summary>
        internal static int AIonMatchScore { get; set; }
        /// <summary>The score per Y-ion match.</summary>
        internal static int YIonMatchScore { get; set; }
        /// <summary>The score per B-ion match.</summary>
        internal static int BIonMatchScore { get; set; }
        /// <summary>The number the ion score should be divied for when ion is loss.</summary>
        internal static int LossMatchScoreDivider { get; set; }
        /// <summary>The score for a isocyanic loss on MS1.</summary>
        internal static int MS1IsoCyanicLossScore { get; set; }
        /// <summary>The score for the isocyanic loss for an a-ion on MS2.</summary>
        internal static int MS2IsoCyanLossAScore { get; set; }
        /// <summary>The score for the isocyanic loss for an b-ion on MS2.</summary>
        internal static int MS2IsoCyanLossBScore { get; set; }
        /// <summary>The score for the isocyanic loss for an y-ion on MS2.</summary>
        internal static int MS2IsoCyanLossYScore { get; set; }
        /// <summary>The divider for when the isocyanic loss is from an -17 or -18 ion.</summary>
        internal static int MS2IsoCyanOnLossScoreDivider { get; set; }
    }
}
