namespace Citrullia
{

    /// <summary>Data class for representing X!Tandem modification.</summary>
    // TODO: Maybe initialise by constructor
    internal class XTModification
    {
        /// <summary>The modified amino acid as one-letter code.</summary>
        internal string AminoAcid { get; set; }
        /// <summary>The position of the modified amino acid.</summary>
        internal int AtPosition { get; set; }
        /// <summary>The change in mass caused by the modification.</summary>
        internal double MassChange { get; set; }
    }
}
