using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Citrullia.Library.XTandem
{
    /// <summary>
    /// The runner of X!Tandem.
    /// </summary>
    public static class XTandemRunner
    {
        /// <summary>The Citrullia settings.</summary>
        internal static CitrulliaSettings CitrulliaSettings = new CitrulliaSettings();

        /// <summary>The X!Tandem input data.</summary>
        internal static XTInput XTandemInput = new XTInput();

        /// <summary>
        /// Launch X!Tandem.
        /// </summary>
        internal static void LaunchXTandem()
        {
            // Get the current directory
            string currentDir = Environment.CurrentDirectory.ToString();

            // Create a new process
            var process = new Process();
            // Create some start informations
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = string.Format("/C {0}\\tandem {1}", CitrulliaSettings.XTandemFolder, CitrulliaSettings.XTandemUserInputFile)
            };
            // Set the start info
            process.StartInfo = startInfo;
            // Start the process
            process.Start();
            // Halt the program until the process is finished.
            process.WaitForExit();
        }

        /// <summary>
        /// Create the input XML-file for an MGF-file.
        /// </summary>
        /// <param name="mgfPath">The MGF-file.</param>
        internal static void MakeTandemInputXml(string mgfPath)
        {
            string inputXmlPath = CitrulliaSettings.XTandemUserInputFile;
            string defaultInput = CitrulliaSettings.XTandemDefaultInputFile;
            string taxonomy = CitrulliaSettings.XTandemTaxonomyFile;
            string outputName = Path.GetFileNameWithoutExtension(mgfPath).Split('.')[0];
            string outputFile = Path.Combine(CitrulliaSettings.OutputXTandemFilesFolder, outputName);

            // Create an stream writer and write data to the file
            using (var xIW = new StreamWriter(inputXmlPath, false))
            {
                //BEGIN
                xIW.WriteLine("<?xml version=\"1.0\"?>");
                xIW.WriteLine("<bioml>");

                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, path\">{0}</note>", mgfPath));

                xIW.WriteLine(string.Format("<note type=\"input\" label=\"list path, default parameters\">{0}</note>", defaultInput));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"list path, taxonomy information\">{0}</note>", taxonomy));

                xIW.WriteLine("<note type=\"input\" label=\"protein, taxon\">yeast</note>");

                //Spectrum Parameters
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, parent monoisotopic mass error minus\">{0}</note>", XTandemInput.PMEMinus));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, parent monoisotopic mass error plus\">{0}</note>", XTandemInput.PMEPlus));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, parent monoisotopic mass error units\">{0}</note>", XTandemInput.PMEType));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, fragment monoisotopic mass error\">{0}</note>", XTandemInput.FMError));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, fragment monoisotopic mass error units\">{0}</note>", XTandemInput.FMEType));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, fragment mass type\">{0}</note>", XTandemInput.MassFragType));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, parent monoisotopic mass isotope error\">{0}</note>", XTandemInput.MIError));

                //Spectrum Conditioning Parameters
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, total peaks\">{0}</note>", XTandemInput.TPUsed));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, maximum parent charge\">{0}</note>", XTandemInput.MPCharge));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, use noise suppression\">{0}</note>", XTandemInput.NSuppression));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, minimum parent m+h\">{0}</note>", XTandemInput.MPmz));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, minimum fragment mz\">{0}</note>", XTandemInput.MFmz));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, minimum peaks\">{0}</note> ", XTandemInput.MPUsed));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"spectrum, threads\">{0}</note>", XTandemInput.Threads));

                //Residue Modification Parameters
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"residue, modification mass\">{0}</note>", XTandemInput.ModMass));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"residue, potential modification mass\">{0}</note>", XTandemInput.PotModMass));

                //Protein Parameters. The file with the modified residue masses is not included.
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"protein, cleavage site\">{0}</note>", XTandemInput.Enzyme));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"protein, cleavage C-terminal mass change\">{0}</note>", XTandemInput.MCCterm));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"protein, cleavage N-terminal mass change\">{0}</note>", XTandemInput.MCNterm));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"protein, N-terminal residue modification mass\">{0}</note>", XTandemInput.MMNterm));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"protein, C-terminal residue modification mass\">{0}</note>", XTandemInput.MMCterm));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"protein, quick acetyl\">{0}</note>", XTandemInput.NTAcetylation));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"protein, quick pyrolidone\">{0}</note>", XTandemInput.NTPyrolidone));

                //Model Refinement Parameters. The Max E-value, N-term- and C-term Modifications is not included
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"refine\">{0}</note>", XTandemInput.Refinement));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"refine, modification mass\">{0}</note>", XTandemInput.RModMass));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"refine, spectrum synthesis\">{0}</note>", XTandemInput.SSynthesis));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"refine, unanticipated cleavage\">{0}</note>", XTandemInput.UCleavage));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"refine, potential modification mass\">{0}</note>", XTandemInput.RefinmentPotModMass));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"refine, point mutations\">{0}</note>", XTandemInput.PMutations));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"refine, use potential modifications for full refinement\">no</note>"));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"refine, cleavage semi\">{0}</note>", XTandemInput.SCleavage));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"refine, potential N-terminus modifications\"></note>"));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"refine, potential C-terminus modifications\"></note>"));

                //Scoring Parameters
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"scoring, minimum ion count\">{0}</note>", XTandemInput.MICount));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"scoring, maximum missed cleavage sites\">{0}</note>", XTandemInput.MMCleavages));
                //xIW.WriteLine(String.Format("<note type=\"input\" label=\"spectrum, minimum parent m + h\">{0}</note>", tandemInput.MPmz));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"scoring, x ions\">{0}</note>", XTandemInput.Xions));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"scoring, y ions\">{0}</note>", XTandemInput.Yions));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"scoring, z ions\">{0}</note>", XTandemInput.Zions));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"scoring, a ions\">{0}</note>", XTandemInput.Aions));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"scoring, b ions\">{0}</note>", XTandemInput.Bions));
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"scoring, c ions\">{0}</note>", XTandemInput.Cions));

                //Output Parameters
                xIW.WriteLine("<note type=\"input\" label=\"output, path hashing\">no</note>"); //path Hashing can be yes or no, tells if date/time + .t should be showed on the output
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"output, maximum valid expectation value\">{0}</note>", XTandemInput.MEValue));
                // Get the output file name.
                xIW.WriteLine(string.Format("<note type=\"input\" label=\"output, path\">{0}.xml</note>", outputFile));
                //END
                xIW.WriteLine("</bioml>");
            }
        }

        /// <summary>
        /// Create the taxonomy XML file.
        /// </summary>
        internal static void MakeTaxonomyXml()
        {
            // Create the stream writer and write the data to the file
            using (var xTW = new StreamWriter(CitrulliaSettings.XTandemTaxonomyFile, false))
            {
                xTW.WriteLine("<?xml version=\"1.0\"?>");
                xTW.WriteLine("<bioml label=\"x! taxon-to-file matching list\">");
                xTW.WriteLine("<taxon label=\"yeast\">");
                // Write the sequence FASTA filepath
                xTW.WriteLine(string.Format("<file format=\"peptide\" URL=\"{0}\" />", XTandemInput.SequenceFile));
                // Write data about crap list if X!Tandem should search the crap list.
                if (XTandemInput.SearchCrapList == true)
                {
                    xTW.WriteLine(string.Format("<file format=\"peptide\" URL=\"{0}\" />", $"{ CitrulliaSettings.XTandemFolder }\\crap.fasta.pro"));
                }

                xTW.WriteLine("</taxon>");
                xTW.WriteLine("</bioml>");
            }
        }

        /// <summary>
        /// Add the modification string to X!Tandem.
        /// </summary>
        // TODO: Change modification string source. Maybe take in a list of modifications and create the modification list
        private static void AddModificationsToTandem()
        {
            // Create an empty modification string for the variable modifications.
            // Get the modification string.
            string modificationStringVariable = UserControls.AnalModUC.ModificationsOutput(UserControls.AnalModUC.variableModifications);
            // Set the variable modifications
            XTandemInput.PotModMass = modificationStringVariable;
            Console.WriteLine(XTandemInput.PotModMass);

            // Get the modification string.
            string fixedModificationString = UserControls.AnalModUC.ModificationsOutput(UserControls.AnalModUC.fixedModifications);
            // Set the variable modifications
            XTandemInput.ModMass = fixedModificationString;
            XTandemInput.RModMass = fixedModificationString;
            Console.WriteLine(XTandemInput.ModMass);

            // Get the modification string.
            string variableRefinmentModificationString = UserControls.AnalModUC.ModificationsOutput(UserControls.AnalModUC.variableRefinmentModifications);
            // Set the variable modifications
            XTandemInput.RefinmentPotModMass = variableRefinmentModificationString;
            Console.WriteLine(XTandemInput.RefinmentPotModMass);
        }

        /// <summary>
        /// Delete all the existing output files.
        /// </summary>
        internal static void DeleteExistingOutputs()
        {
            // Get all the files in the output folder
            string[] filepaths = Directory.GetFiles(CitrulliaSettings.OutputXTandemFilesFolder);
            // Loop through all of the files and delete them
            foreach (string file in filepaths)
            {
                File.Delete(file);
            }
        }

        /// <summary>
        /// Perform X!Tandem search for each of the input MGF-files.
        /// </summary>
        internal static void XTandemSearchForEachMGF()
        {
            // Get all of the files in the the input MGF-file folder and loop though them.
            foreach (string file in Directory.GetFiles(CitrulliaSettings.InputMgfFilesFolder))
            {
                // Add the chosen modifications to the input file. TODO: Maybe only do one time??
                AddModificationsToTandem();
                // Create the input XML-file for the file.
                MakeTandemInputXml(file);
                // Run X!Tandem
                LaunchXTandem();
            }
        }
    }
}
