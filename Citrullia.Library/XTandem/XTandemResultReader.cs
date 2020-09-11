using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Citrullia.Library.XTandem
{
    /// <summary>
    /// The reader for X!Tandem results.
    /// </summary>
    // TODO: Remove unnecessary reading of properties 
    public static class XTandemResultReader
    {
        /// <summary>The X!Tandem search results.</summary>
        internal static List<XTResult> XTandemSearchResults = new List<XTResult>();

        /// <summary>
        /// Read the result XML from X!Tandem.
        /// </summary>
        internal static void ReadResultXMLs()
        {
            // Get the current directory
            string currentDir = Environment.CurrentDirectory.ToString();
            // Set the folder path for the output
            string folderpath = currentDir + "\\Externals\\XTandem\\Outputs";
            // Create a temporary list of X!Tandem results
            var xTandemResults = new List<XTResult>();
            // Get the files in the directory and loop though them.
            foreach (string file in Directory.GetFiles(External.CitrulliaSettings.OutputXTandemFilesFolder))
            {
                // Create an empty result
                var result = new XTResult();
                // Read the result to the empty result
                result = ReadXML(file);
                // Add the result to the temporary list
                xTandemResults.Add(result);
                // Set the new X!Tandem results
                ReturnXResult(xTandemResults);

            }
        }

        /// <summary>
        /// Set the X!Tandem results.
        /// </summary>
        /// <param name="xTandemResults">The new X!Tandem results.</param>
        /// <returns>The new X!Tandem results.</returns>
        private static List<XTResult> ReturnXResult(List<XTResult> xTandemResults)
        {
            XTandemSearchResults = xTandemResults;
            return XTandemSearchResults;
        }

        /// <summary>
        /// Read the X!Tandem result from a file.
        /// </summary>
        /// <param name="filename">The filename of the file.</param>
        /// <returns>The <see cref="XTResult"/>.</returns>
        internal static XTResult ReadXML(string filename)
        {
            // Load the file into an XML Document
            XDocument xml = XDocument.Load(filename);
            // Set the df?? TODO: Rename
            string df = "{http://www.bioml.com/gaml/}";
            // Create an temporary list of X!Tandem spectra
            var spectrums = new List<XTSpectrum>();
            // Get the orginal name of the input file
            string originalInputFileName = xml.Root.Attribute("label").ToString().Split(' ').Last().Replace("'", "").Split('\\').Last().Replace("\"", "").Replace(".mgf", "");
            // Loop through each of the element in the root
            foreach (var el in xml.Root.Elements())
            {
                Console.WriteLine(el.FirstAttribute.ToString());
                // Create an empty instance of spectrum
                var spectrum = new XTSpectrum();
                // Check if the first attribute in the element is equal to id
                if (el.FirstAttribute == el.Attribute("id"))
                {
                    // If so, parse the element
                    // Get the group data
                    spectrum = ExtractGroupData(el, spectrum);
                    //Console.WriteLine("Underelements of spectrum: {0}", el.Elements().Count());
                    // Create a temporary list of proteins
                    var proteins = new List<XTProtein>();
                    // Loop through each of the subelements
                    foreach (var el2 in el.Elements())
                    {

                        //Console.WriteLine("Name of spectrum underelement: {0}",el2.Name);
                        // Check if the name of the subelement is protein
                        if (el2.Name == "protein")
                        {
                            //Protein data:
                            var protein = new XTProtein();
                            protein = ExtractProteinData(el2, protein);
                            //Console.WriteLine("Protein: {0}", protein.ProtID);
                            proteins.Add(protein);

                        }
                        else
                        {
                            // If not protein data
                            // Check that if the first attribute is "supporting data"

                            if (el2.FirstAttribute.Value == "supporting data")
                            {
                                // If so, extract supporting data
                                //Supporting data:
                                spectrum = ExtractSupportingData(el2, df, spectrum);
                            }
                            else
                            {
                                // If not supporting data extract spectrum data
                                //Spectrum data:
                                spectrum = ExtractSpectrumData(el2, df, spectrum);
                            }
                        }
                    }
                    // Add the proteins to the spectrum
                    spectrum.Proteins = proteins;
                    // Add the spectrum to the temporary list of spectra
                    spectrums.Add(spectrum);
                    //Console.WriteLine(spectrum.ID.ToString());
                }
            }
            // Create a new instance of X!Tandem result
            var result = new XTResult
            {
                // Set the filepath
                FilePath = filename,
                // Set the filename
                FileName = filename.Split('\\').Last(),
                // Set the list of spectra
                XSpectrums = spectrums,
                // Set the filename of the input file
                OriginalInputFile = originalInputFileName
            };
            // Return the result
            return result;
        }

        /// <summary>
        /// Extract the group data from the result file.
        /// </summary>
        /// <param name="group">The group data.</param>
        /// <param name="spectrum">The spectrum.</param>
        /// <returns>The spectrum with the group data.</returns>
        internal static XTSpectrum ExtractGroupData(XElement group, XTSpectrum spectrum)
        {
            // Get the spectrum ID
            spectrum.ID = int.Parse(group.Attribute("id").Value);
            // Get the parent mass
            spectrum.ParentMass = double.Parse(group.Attribute("mh").Value, FileUtilities.NumberFormat);
            // Get the parent ion mass
            spectrum.ParentIonCharge = int.Parse(group.Attribute("z").Value);
            // Get the e-value for the spectrum
            spectrum.SpectrumEVal = double.Parse(group.Attribute("expect").Value, FileUtilities.NumberFormat);
            // Get the spectrum
            spectrum.SpectrumLabel = group.Attribute("label").Value;
            // Get the log10(intensity sum)
            spectrum.LogSumFragIons = double.Parse(group.Attribute("sumI").Value, FileUtilities.NumberFormat);
            // Get the max fragment intensity.
            spectrum.MaxFragIonInt = double.Parse(group.Attribute("maxI").Value, FileUtilities.NumberFormat);
            // Get the norm multiplier.
            spectrum.NormMulitplier = double.Parse(group.Attribute("fI").Value, FileUtilities.NumberFormat);
            // Return the spectrum
            return spectrum;
        }

        /// <summary>
        /// Extract protein data from the result file.
        /// </summary>
        /// <param name="pData">The protein data.</param>
        /// <param name="protein">The protein.</param>
        /// <returns>The protein with the protein data.</returns>
        internal static XTProtein ExtractProteinData(XElement pData, XTProtein protein) // Contians: note, file, peptide, domain and aa
        {
            /* Protein information */
            // Get the protein ID
            protein.ProtID = double.Parse(pData.Attribute("id").Value.Trim(), FileUtilities.NumberFormat);
            // Get the E-value for the protein
            protein.ProtEVal = double.Parse(pData.Attribute("expect").Value, FileUtilities.NumberFormat);
            // Get the sum of fragment ions identified
            protein.ProtLogFragIonInt = double.Parse(pData.Attribute("sumI").Value, FileUtilities.NumberFormat);

            /* Note information */
            // Get and set the protein label
            var note = pData.Element("note");
            protein.ProtLabel = note.Value;

            /* File information */
            // Get the URL for the FASTA file
            protein.ProtFasta = pData.Element("file").Attribute("URL").Value;

            /* Peptide information */
            // Get the peptide XML element
            var peptide = pData.Element("peptide");
            // Get the start position of the peptide
            protein.ProtStartPos = int.Parse(peptide.Attribute("start").Value);
            // Get the end position of the peptide
            protein.ProtEndPos = int.Parse(peptide.Attribute("end").Value);
            // Get the sequence of the peptide
            protein.ProtSeq = peptide.Value;

            /* Domain information*/
            // Get the start position of the domain
            protein.DomainStartPos = int.Parse(peptide.Element("domain").Attribute("start").Value);
            // Get the end position of the domain
            protein.DomainEndPos = int.Parse(peptide.Element("domain").Attribute("end").Value);
            // Get the e-value for the domain
            protein.DomainExpect = double.Parse(peptide.Element("domain").Attribute("expect").Value, FileUtilities.NumberFormat);
            // Get the protein mass
            protein.DomainMZ = double.Parse(peptide.Element("domain").Attribute("mh").Value, FileUtilities.NumberFormat);
            // Get the difference between the spectrum and calculated mass
            protein.Delta = double.Parse(peptide.Element("domain").Attribute("delta").Value, FileUtilities.NumberFormat);
            // Get X!Tandem identification score
            protein.Hyperscore = double.Parse(peptide.Element("domain").Attribute("hyperscore").Value, FileUtilities.NumberFormat);
            protein.NextScore = double.Parse(peptide.Element("domain").Attribute("nextscore").Value, FileUtilities.NumberFormat);
            protein.YScore = double.Parse(peptide.Element("domain").Attribute("y_score").Value, FileUtilities.NumberFormat);
            protein.YIons = int.Parse(peptide.Element("domain").Attribute("y_ions").Value);
            protein.BScore = double.Parse(peptide.Element("domain").Attribute("b_score").Value, FileUtilities.NumberFormat);
            protein.BIons = int.Parse(peptide.Element("domain").Attribute("b_ions").Value);
            // Get the 4 amino acids before the domain
            protein.PreSeq = peptide.Element("domain").Attribute("pre").Value;
            // Get the 4 amino acids after the domain
            protein.PostSeq = peptide.Element("domain").Attribute("post").Value;
            // Get the sequence of the domain
            protein.DoaminSeq = peptide.Element("domain").Attribute("seq").Value;
            // Get the number of potential missed cleavage sites
            protein.MissedCleavages = int.Parse(peptide.Element("domain").Attribute("missed_cleavages").Value);

            /* Modifications */
            // Create an temporary list of modifications
            var modifications = new List<XTModification>();
            // Loop through all of the modifications
            foreach (var mod in pData.Descendants("aa"))
            {
                // Create an empty instance of modification
                var modi = new XTModification
                {
                    AminoAcid = mod.Attribute("type").Value,
                    AtPosition = int.Parse(mod.Attribute("at").Value),
                    MassChange = double.Parse(mod.Attribute("modified").Value, FileUtilities.NumberFormat)
                };
                // Add the modification to the temporary list of modifications
                modifications.Add(modi);
            }
            // Set the modification list to the protein
            protein.Modifications = modifications;
            // Return the protein
            return protein;
        }

        /// <summary>
        /// Extract the supporting data from the result file.
        /// </summary>
        /// <param name="supData">The suplementary data.</param>
        /// <param name="df"></param>
        /// <param name="spectrum">The spectrum.</param>
        /// <returns>The specturm with the supporting data.</returns>
        internal static XTSpectrum ExtractSupportingData(XElement supData, string df, XTSpectrum spectrum) //Contains: trace, attribute, Xdata, Ydata and values
        {
            // Get the hyperscore expectation function
            var traceHyperscore = supData.Descendants(df + "trace").FirstOrDefault(el => el.Attribute("type").Value == "hyperscore expectation function");
            // From hyperscore get and set A0 and A1
            spectrum.A0 = double.Parse(traceHyperscore.Descendants(df + "attribute").FirstOrDefault(el => el.Attribute("type").Value == "a0").Value, FileUtilities.NumberFormat);
            spectrum.A1 = double.Parse(traceHyperscore.Descendants(df + "attribute").FirstOrDefault(el => el.Attribute("type").Value == "a1").Value, FileUtilities.NumberFormat);
            // Get the number of hyperscores
            var hScoreNumbs = traceHyperscore.Element(df + "Xdata").Element(df + "values").Value.Replace("\n", "").Split(' ').ToArray();
            // Set the number of hyperscores
            spectrum.HyperscoreNumbs = Array.ConvertAll(hScoreNumbs.Where(x => string.IsNullOrEmpty(x) == false).ToArray(), int.Parse);
            // Get the count of hyperscores
            var hScoreCounts = traceHyperscore.Element(df + "Ydata").Element(df + "values").Value.Replace("\n", "").Split(' ').ToArray();
            // Set the number of hyperscores
            spectrum.HyperscoreCounts = Array.ConvertAll(hScoreCounts.Where(x => string.IsNullOrEmpty(x) == false).ToArray(), int.Parse);

            // Get the convolution survival function
            var traceSurvival = supData.Descendants(df + "trace").FirstOrDefault(el => el.Attribute("type").Value == "convolution survival function");
            // Get the number of convolution survival function
            var cSurvivalNumbs = traceSurvival.Element(df + "Xdata").Element(df + "values").Value.Replace("\n", "").Split(' ').ToArray();
            // Set the number of convolution survival function
            spectrum.ConSurvivalNumbs = Array.ConvertAll(cSurvivalNumbs.Where(x => string.IsNullOrEmpty(x) == false).ToArray(), int.Parse);
            // Get the count of convolution survival function
            var cSurvivalCounts = traceSurvival.Element(df + "Ydata").Element(df + "values").Value.Replace("\n", "").Split(' ').ToArray();
            // Set the count of convolution survival function
            spectrum.ConSurvivalCounts = Array.ConvertAll(cSurvivalCounts.Where(x => string.IsNullOrEmpty(x) == false).ToArray(), int.Parse);

            // Get the b ion trace
            var traceB = supData.Descendants(df + "trace").FirstOrDefault(el => el.Attribute("type").Value == "b ion histogram");
            // Get the number of b ions
            var bIonNumbs = traceB.Element(df + "Xdata").Element(df + "values").Value.Replace("\n", "").Split(' ').ToArray();
            // Set the number of b ions
            spectrum.BIonNumbs = Array.ConvertAll(bIonNumbs.Where(x => string.IsNullOrEmpty(x) == false).ToArray(), int.Parse);
            // Get the count of b ions
            var bIonCounts = traceB.Element(df + "Ydata").Element(df + "values").Value.Replace("\n", "").Split(' ').ToArray();
            // Set the count of b ions
            spectrum.BIonCounts = Array.ConvertAll(bIonCounts.Where(x => string.IsNullOrEmpty(x) == false).ToArray(), int.Parse);

            var traceY = supData.Descendants(df + "trace").FirstOrDefault(el => el.Attribute("type").Value == "y ion histogram");
            // Get the number of y ions
            var yIonNumbs = traceY.Element(df + "Xdata").Element(df + "values").Value.Replace("\n", "").Split(' ').ToArray();
            // Set the number of y ions
            spectrum.YIonNumbs = Array.ConvertAll(yIonNumbs.Where(x => string.IsNullOrEmpty(x) == false).ToArray(), int.Parse);
            // Get the count of y ions
            var yIonCounts = traceY.Element(df + "Ydata").Element(df + "values").Value.Replace("\n", "").Split(' ').ToArray();
            // Set the count of y ions
            spectrum.YIonCounts = Array.ConvertAll(bIonCounts.Where(x => string.IsNullOrEmpty(x) == false).ToArray(), int.Parse);

            // Return the spectrum
            return spectrum;
        }

        /// <summary>
        /// Extract the spectrum data from the result file.
        /// </summary>
        /// <param name="specData">The spectrum data.</param>
        /// <param name="df"></param>
        /// <param name="spectrum">The spectrum.</param>
        /// <returns>The specturm with the spectrum data.</returns>
        internal static XTSpectrum ExtractSpectrumData(XElement specData, string df, XTSpectrum spectrum) //Contians: note, trace, attribute, Xdata, Ydata and values
        {
            // Get and set the spectrum description
            var note = specData.Element("note");
            spectrum.SpectrumDescription = note.Value;

            var trace = specData.Element(df + "trace");
            // Get the M/Z-value of the spectrum (M+H)
            spectrum.SpectrumMZ = double.Parse(trace.Descendants(df + "attribute").FirstOrDefault(el => el.Attribute("type").Value == "M+H").Value, FileUtilities.NumberFormat);
            // Get the charge of the spectrum
            spectrum.Charge = double.Parse(trace.Descendants(df + "attribute").FirstOrDefault(el => el.Attribute("type").Value == "charge").Value, FileUtilities.NumberFormat);

            // Get the MZ-values
            var mzVals = trace.Element(df + "Xdata").Element(df + "values").Value.Replace("\n", " ").Split(' ').ToArray();
            // Convert the found non-empty M/Z-values to doubles and set them to the spectra
            spectrum.MzValues = mzVals.Where(x => string.IsNullOrEmpty(x) == false).Select(val => double.Parse(val, FileUtilities.NumberFormat)).ToArray();

            // Get the intensities
            var ints = trace.Element(df + "Ydata").Element(df + "values").Value.Replace("\n", " ").Split(' ').ToArray();
            // Convert the found non-empty intensities to doubles and set them to the spectra
            spectrum.Intencities = ints.Where(x => string.IsNullOrEmpty(x) == false).Select(val => double.Parse(val, FileUtilities.NumberFormat)).ToArray();
            // Return the spectrum
            return spectrum;
        }
    }
}
