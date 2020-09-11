using System;
using System.Collections.Generic;
using System.Text;

namespace Citrullia.Library
{
    /// <summary>
    /// Information about the file paths etc. used by Citrullia
    /// </summary>
    // TODO: Consider moving the file to another location
    // TODO: Find another way of storing the files
    public class CitrulliaFileSettings
    {
        /// <summary>The folder path to the Citrullia application.</summary>
        internal string CitrulliaFolderPath { get; set; }

        #region Modification filepath
        /// <summary>The file that contains the variable modifications (Monoisotopic mass).</summary>
        internal string VariableModificationMonoMassFile { get; }
        /// <summary>The file that contains the variable modifications (Average mass).</summary>
        internal string VariableModificatioAvgMassFile { get; }
        /// <summary>The file that contains the fixed modifications.</summary>
        internal string FixedModificationFile { get; }
        #endregion

        #region Enzymes filepaths
        /// <summary>The file that contains the digestion enzymes.</summary>
        internal string DigestionEnzymeFile { get; }
        #endregion

        #region X!Tandem filepaths
        /// <summary>The folder for X!Tandem.</summary>
        internal string XTandemFolder { get; }
        ///<summary>The folder for the input MGF-files.</summary>
        internal string InputMgfFilesFolder { get; }
        /// <summary>The folder for the X!Tandem outputs.</summary>
        internal string OutputXTandemFilesFolder { get; }
        /// <summary>The XML file for the X!Tandem user input.</summary>
        internal string XTandemUserInputFile { get; }
        /// <summary>The XML file for the X!Tandem default input.</summary>
        internal string XTandemDefaultInputFile { get; }
        /// <summary>The XML taxonomy file the X!Tandem.</summary>
        internal string XTandemTaxonomyFile { get; }
        #endregion

        /// <summary>
        /// Create a new instance of <see cref="CitrulliaFileSettings"/>.
        /// </summary>
        internal CitrulliaFileSettings()
        {
            // Get the current directory
            string currentDirectory = Environment.CurrentDirectory.ToString();
            CitrulliaFolderPath = currentDirectory;
            // Set the filepaths for the modifications
            VariableModificationMonoMassFile = currentDirectory + @"\Externals\VModMono.txt";
            VariableModificatioAvgMassFile = currentDirectory + @"\Externals\VModAvg.txt";
            FixedModificationFile = currentDirectory + @"\Externals\FMod.txt";

            // Set the filepaths for the digestion enzyme
            DigestionEnzymeFile = currentDirectory + @"\Externals\Enzymes.txt";

            // Get the X!Tandem folders
            XTandemFolder = currentDirectory + @"\Externals\XTandem";
            InputMgfFilesFolder = currentDirectory + @"\Externals\XTandem\InputMGFs";
            OutputXTandemFilesFolder = currentDirectory + @"\Externals\XTandem\Outputs";
            XTandemUserInputFile = currentDirectory + @"\Externals\XTandem\input2.xml";
            XTandemDefaultInputFile = currentDirectory + @"\Externals\XTandem\default_input.xml";
            XTandemTaxonomyFile = currentDirectory + @"\Externals\XTandem\taxonomy2.xml";
        }
    }
}
