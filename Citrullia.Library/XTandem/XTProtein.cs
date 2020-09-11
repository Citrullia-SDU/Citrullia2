using System.Collections.Generic;

namespace Citrullia
{
    /// <summary>Data class for representing X!Tandem protein.</summary>
    // TODO: Maybe initialise by constructor
    // TODO: Remove unnecessary properties 
    internal class XTProtein
    {
        /// <summary>The e-value for the protein.</summary>
        internal double ProtEVal { get; set; }
        /// <summary>The protein ID.</summary>
        internal double ProtID { get; set; }
        /// <summary>The start position of the protein.</summary>
        internal int ProtStartPos { get; set; }
        /// <summary>The stop position of the protein.</summary>
        internal int ProtEndPos { get; set; }
        /// <summary>The sum of fragment ions that identify the protein.</summary>
        internal double ProtLogFragIonInt { get; set; }
        /// <summary>The sequence of the protein.</summary>
        internal string ProtSeq { get; set; }
        /// <summary>The description line from the FASTA file.</summary>
        internal string ProtLabel { get; set; }
        /// <summary>The URL for the FASTA file.</summary>
        internal string ProtFasta { get; set; }
        /// <summary>The start position of the domain.</summary>
        internal int DomainStartPos { get; set; }
        /// <summary>The end position of the domain.</summary>
        internal int DomainEndPos { get; set; }
        /// <summary>The e-value for the domain.</summary>
        internal double DomainExpect { get; set; }
        /// <summary>The mass of the protein.</summary>
        internal double DomainMZ { get; set; }
        /// <summary>The difference between the spectrum and calculated mass</summary>
        internal double Delta { get; set; }
        /// <summary>X!Tandem identification score.</summary>
        internal double Hyperscore { get; set; }
        /// <summary>The next score for the protein. TODO: Understand.</summary>
        internal double NextScore { get; set; }
        /// <summary>The y ion score. TODO: Understand.</summary>
        internal double YScore { get; set; }
        /// <summary>The number of y ions.</summary>
        internal int YIons { get; set; }
        /// <summary>The b ion score. TODO: Understand.</summary>
        internal double BScore { get; set; }
        /// <summary>The number of b ions.</summary>
        internal int BIons { get; set; }
        /// <summary>The 4 amino acids before the domain.</summary>
        internal string PreSeq { get; set; }
        /// <summary>The 4 amino acids after the domain.</summary>
        internal string PostSeq { get; set; }
        /// <summary>The sequence of the domain.</summary>
        internal string DoaminSeq { get; set; }
        /// <summary>The number of potential missed clevage sites.</summary>
        internal int MissedCleavages { get; set; }

        /// <summary>The list of protein modifications.</summary>
        internal List<XTModification> Modifications { get; set; }
    }
}
