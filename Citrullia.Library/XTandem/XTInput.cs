namespace Citrullia.Library.XTandem
{
    /// <summary>The class containg the X!Tandem input data.</summary>
    // TODO: Maybe initialise by constructor
    // TODO: Check types. Convert strings to their correct type (Yes/no: bool. Double, int etc.)
    internal class XTInput
    {
        #region General information  
        /// <summary>The digestion enzyme.</summary>
        internal string Enzyme = "[KR]|{P}";
        /// <summary>Should search crap list-</summary>
        internal bool SearchCrapList = false;
        /// <summary>Should perform refinments.</summary>
        internal string Refinement = "no";
        /// <summary>The number of processor threads to be used.</summary>
        internal string Threads = "1";
        /// <summary>The sequence FASTA file.</summary>
        internal string SequenceFile = "";
        #endregion

        #region Parameters
        /// <summary>The parent mass error minus.</summary>
        internal string PMEMinus = "10";
        /// <summary>The parent mass error plus.</summary>
        internal string PMEPlus = "10";
        /// <summary>The parent mass error unit.</summary>
        internal string PMEType = "ppm";
        /// <summary>The fragment mass error.</summary>
        internal string FMError = "0.10";
        /// <summary>The fragment mass error unit.</summary>
        internal string FMEType = "Dalton";
        /// <summary>Should search for A-ions.</summary>
        internal string Aions = "yes";
        /// <summary>Should search for B-ions.</summary>
        internal string Bions = "yes";
        /// <summary>Should search for C-ions.</summary>
        internal string Cions = "no";
        /// <summary>Should search for X-ions.</summary>
        internal string Xions = "no";
        /// <summary>Should search for Y-ions.</summary>
        internal string Yions = "yes";
        /// <summary>Should search for Z-ions.</summary>
        internal string Zions = "no";
        /// <summary>The total number of peaks used.</summary>
        internal string TPUsed = "50";
        /// <summary>The minimum number of peaks used.</summary>
        internal string MPUsed = "13";
        /// <summary>The minimum parent MZ-value.</summary>
        internal string MPmz = "300";
        /// <summary>The minimum fragment MZ-value.</summary>
        internal string MFmz = "146";
        /// <summary>The maximum parent charge.</summary>
        internal string MPCharge = "4";
        /// <summary>The maximum e-value.</summary>
        internal string MEValue = "0.010";
        /// <summary>The minimum ion count.</summary>
        internal string MICount = "6";
        /// <summary>The maximum missed cleavages.</summary>
        internal string MMCleavages = "2";
        /// <summary>The maximum e-value for refinment.</summary>
        internal string RMEVaue = "0.100";
        /// <summary>The modification mass at the N-terminal.</summary>
        internal string MCNterm = "1.0107825";
        /// <summary>The modification mass at the C-terminal.</summary>
        internal string MCCterm = "17.0027350";
        /// <summary>The mass modification at the N-terminal.</summary>
        internal string MMNterm = "0.00000";
        /// <summary>The mass modification at the C-terminal.</summary>
        internal string MMCterm = "0.00000";
        #endregion

        #region Modifications
        /// <summary>The mass fragment type. Monoisotopic or Average.</summary>
        internal string MassFragType = "monoisotopic";
        /// <summary>The fixed modifications.</summary>
        internal string ModMass = "";
        /// <summary>The variable modifications.</summary>
        internal string PotModMass = "";
        /// <summary>The refinment fixed mofications. Same as <see cref="ModMass"/>.</summary>
        internal string RModMass = "";
        /// <summary>The refinement potential modifications.</summary>
        internal string RefinmentPotModMass = "";
        #endregion

        #region Options
        /// <summary>Perform noise supression.</summary>
        internal string NSuppression = "no";
        /// <summary>The N-terminal is acetylated.</summary>
        internal string NTAcetylation = "yes";
        /// <summary>The N-terminal is pyrolidonated.</summary>
        internal string NTPyrolidone = "yes";
        /// <summary>TODO: Mass isotope error?</summary>
        internal string MIError = "no";
        #endregion

        #region Refinement options
        /// <summary>Refinment for semi cleavages.</summary>
        internal string SCleavage = "no";
        /// <summary>Refinment for unspecified cleavages.</summary>
        internal string UCleavage = "no";
        /// <summary>Refinment for point mutations.</summary>
        internal string PMutations = "no";
        /// <summary>Refinment for spectrum syntesis.</summary>
        internal string SSynthesis = "yes";
        /// <summary>Refinment for N-terminal acetylations.</summary>
        internal string RNTAcetylation = "no";
        #endregion
    }
}
