using Citrullia.Library.MassSpectra;
using Citrullia.Library.XTandem;
using System.Collections.Generic;
using System.Linq;

namespace Citrullia.Library.Quantification
{
    /// <summary>
    /// Utility class for the Qauantification.
    /// </summary>
    internal static class Quantification
    {
        /// <summary>The dictionary of citrullinated spectra marked for quantification.</summary>
        internal static Dictionary<XTSpectrum, XTSpectrum> citSpecSpecDict = new Dictionary<XTSpectrum, XTSpectrum>();
        /// <summary>The dictionary of arginine spectra marked for quantification.</summary>
        internal static Dictionary<XTSpectrum, MgxScan> argSpecScanDict = new Dictionary<XTSpectrum, MgxScan>();
        /// <summary>The list of lone citrullinated spectra marked for quantification.</summary>
        internal static List<XTSpectrum> loneCitSpecList = new List<XTSpectrum>();
        /// <summary>The parent mass tolerance in PPM. Used to calculate deviation.</summary>
        internal static int ppm = 10;
        /// <summary>The retention time interval in minutes. Used to calculate the extracted ion current.</summary>
        internal static int rTInterval = 5;
        /// <summary>The list of quantification results.</summary>
        internal static List<QuantificationResult> QuantificationResults = new List<QuantificationResult>();

        #region Add and remove citrullinated spectra
        /// <summary>
        /// Add the citrullinated spectra to the quantififcation dictionary.
        /// </summary>
        /// <param name="citSpec">The citrullinated spectra.</param>
        /// <param name="argSpec">The arginine spectra.</param>
        /// <returns>The dictionary containing the new spectra.</returns>
        internal static Dictionary<XTSpectrum, XTSpectrum> AddTokvpCitSpecSpecDict(XTSpectrum citSpec, XTSpectrum argSpec)
        {
            if(citSpecSpecDict.ContainsKey(citSpec) == false)
            {
                citSpecSpecDict.Add(citSpec, argSpec);
            }
            
            return citSpecSpecDict;
        }

        /// <summary>
        /// Remove the spectra from the quantification spectra.
        /// </summary>
        /// <param name="citSpec">The spectra to be removed.</param>
        /// <returns>The new dictionary without the removed spectra.</returns>
        internal static Dictionary<XTSpectrum, XTSpectrum> RemoveFromkvpCitSpecSpecList(XTSpectrum citSpec)
        {
            citSpecSpecDict.Remove(citSpec);
            return citSpecSpecDict;
        }
        #endregion

        #region Add and remove arginine spectra
        /// <summary>
        /// Add the arginine spectra to the quantififcation dictionary.
        /// </summary>
        /// <param name="argSpec">The arginine spectra.</param>
        /// <param name="citScan">The citrullinated scan.</param>
        /// <returns>The dictionary containing the new spectra.</returns>
        internal static Dictionary<XTSpectrum, MgxScan> AddTokvpArgSpecScanDict(XTSpectrum argSpec, MgxScan citScan)
        {
            if(argSpecScanDict.ContainsKey(argSpec) == false)
            {
                argSpecScanDict.Add(argSpec, citScan);
            }
            
            return argSpecScanDict;
        }

        /// <summary>
        /// Remove the spectra from the quantification spectra.
        /// </summary>
        /// <param name="argSpec">The spectra to be removed.</param>
        /// <returns>The new dictionary without the removed spectra.</returns>
        internal static Dictionary<XTSpectrum, MgxScan> RemoveFromkvpArgSpecScanDict(XTSpectrum argSpec)
        {
            argSpecScanDict.Remove(argSpec);
            return argSpecScanDict;
        }
        #endregion

        #region Add and remove lonely citrullinated spectra
        /// <summary>
        /// Add the lonely citrullinated spectra to the quantification dictionary.
        /// </summary>
        /// <param name="citSpec"></param>
        /// <returns>The dictionary containing the new spectra.</returns>
        internal static List<XTSpectrum> AddToLoneCitSpecList(XTSpectrum citSpec)
        {
            loneCitSpecList.Add(citSpec);
            return loneCitSpecList;
        }

        /// <summary>
        /// Remove the spectra from the quantification spectra.
        /// </summary>
        /// <param name="citSpec">The spectra to be removed.</param>
        /// <returns>The new dictionary without the removed spectra.</returns>
        internal static List<XTSpectrum> RemoveFromLoneCitSpecList(XTSpectrum citSpec)
        {
            loneCitSpecList.Remove(citSpec);
            return loneCitSpecList;
        }
        #endregion


        /// <summary>
        /// Perform Quantification.
        /// </summary>
        internal static void PerformQuantification()
        {
            // Create a list of temporary list of quantification result
            List<QuantificationResult> resultsQuant = new List<QuantificationResult>();
            CitrullinationQuantification(resultsQuant);
            ArginineQuantification(resultsQuant);
            LoneCitrullinationQunatification(resultsQuant);
            // Set the quantification results
            ReturnQuantResults(resultsQuant);
        }

        /// <summary>
        /// Quantification of lonely citrullinations.
        /// </summary>
        /// <param name="resultsQuant">The list of quantification results.</param>
        private static void LoneCitrullinationQunatification(List<QuantificationResult> resultsQuant)
        {
            // Loop through all of the lonely citrullination spectra
            foreach (XTSpectrum spectrum in loneCitSpecList)
            {
                // Create an empty instance of quantification result
                QuantificationResult result = new QuantificationResult();
                // Calculate the extracted ion count for the spectrum
                // Create a placeholder variable for the extracted ion count
                double citXIC = 0;
                // Loop through the input files and find the one that have the same input filename
                foreach (InputData input in FileReader.inputFiles)
                {
                    string inputFileName = input.FilePath.Split('\\').Last();
                    if (inputFileName == spectrum.OriginalFileName)
                    {
                        citXIC = CalculateExtractedIonCount(spectrum, citXIC, input);
                    }
                }
                // Add the data to the quantification result
                result.Protein = RemoveProteinDbPrefix(spectrum.Proteins[0].ProtLabel);
                result.Sequence = spectrum.Proteins[0].DoaminSeq;
                result.CitrullinatedExtractedIonCount = citXIC;
                result.ArginineExtractedIonCount = 0;
                result.CitPercent = 100;
                result.ValidationBy = "Lone";
                result.CitFileName = spectrum.OriginalFileName;
                result.CitRetentionTime = spectrum.RetentionTime;
                result.ArgFileName = "NaN";
                result.ArgRetentionTime = 0;
                result.CitCharge = spectrum.Charge;
                result.ArgCharge = 0;
                result.ArgSpectrumID = 0;
                result.CitSpectrumID = spectrum.ID;
                result.Score = spectrum.CitScore;
                result.MatchScore = 0;
                // Add the result to the result list
                resultsQuant.Add(result);
            }
        }

        /// <summary>
        /// Quantification of arginine.
        /// </summary>
        /// <param name="resultsQuant">The list of quantification results.</param>
        private static void ArginineQuantification(List<QuantificationResult> resultsQuant)
        {
            foreach (KeyValuePair<XTSpectrum, MgxScan> kvp in argSpecScanDict)
            {
                // Create an empty instance of quantification result
                QuantificationResult result = new QuantificationResult();
                // Create two temporary variables for holding the extracted ion count for both citrulline and arginine
                double argXIC = 0;
                double citXIC = 0;
                // Loop through the input files and find the one that have the same input filename
                foreach (InputData input in FileReader.inputFiles)
                {
                    string inputFileName = input.FilePath.Split('\\').Last();
                    if (inputFileName == kvp.Key.OriginalFileName)
                    {
                        argXIC = CalculateExtractedIonCount(kvp.Key, argXIC, input);
                    }
                    if (inputFileName == kvp.Value.OriginalFileName)
                    {
                        citXIC = CalculateExtractedIonCount(kvp.Value, citXIC, input);                        
                    }
                }
                // Add the data to the quantification result
                result.Protein = RemoveProteinDbPrefix(kvp.Key.Proteins[0].ProtLabel);
                result.Sequence = kvp.Key.Proteins[0].DoaminSeq;
                result.CitrullinatedExtractedIonCount = citXIC;
                result.ArginineExtractedIonCount = argXIC;
                result.CitPercent = citXIC / (citXIC + argXIC) * 100;
                result.ValidationBy = "Arg-paired";
                result.ArgFileName = kvp.Key.OriginalFileName;
                result.CitFileName = kvp.Value.OriginalFileName;
                result.ArgRetentionTime = kvp.Key.RetentionTime;
                result.CitRetentionTime = kvp.Value.RetentionTime;
                result.CitCharge = kvp.Value.Charge;
                result.ArgCharge = kvp.Key.Charge;
                result.ArgSpectrumID = kvp.Key.ID;
                result.CitSpectrumID = kvp.Value.ScanNumber;
                result.Score = kvp.Value.CitScore;
                result.MatchScore = kvp.Value.MatchScore;
                // Add the result to the result list
                resultsQuant.Add(result);

            }
        }
        
        /// <summary>
        /// Quantification of citrullinations.
        /// </summary>
        /// <param name="resultsQuant">The list of quantification results.</param>
        private static void CitrullinationQuantification(List<QuantificationResult> resultsQuant)
        {
            // Loop through each of the citrullinted spectra
            foreach (KeyValuePair<XTSpectrum, XTSpectrum> kvp in citSpecSpecDict)
            {
                // Create an empty instance of quantification result
                QuantificationResult result = new QuantificationResult();
                // Create two temporary variables for holding the extracted ion count for both citrulline and arginine
                double citXIC = 0;
                double argXIC = 0;
                // Loop through the input files and find the one that have the same input filename
                foreach (InputData input in FileReader.inputFiles)
                {
                    string inputFileName = input.FilePath.Split('\\').Last();
                    if (inputFileName == kvp.Key.OriginalFileName)
                    {
                        citXIC = CalculateExtractedIonCount(kvp.Key, citXIC, input);
                    }

                    if (inputFileName == kvp.Value.OriginalFileName)
                    {
                        argXIC = CalculateExtractedIonCount(kvp.Value, argXIC, input);
                    }
                }
                // Add the data to the quantification result
                result.Protein = RemoveProteinDbPrefix(kvp.Key.Proteins[0].ProtLabel);
                result.Sequence = kvp.Key.Proteins[0].DoaminSeq;
                result.CitrullinatedExtractedIonCount = citXIC;
                result.ArginineExtractedIonCount = argXIC;
                result.CitPercent = citXIC / (citXIC + argXIC) * 100;
                result.ValidationBy = "Cit-paired";
                result.CitFileName = kvp.Key.OriginalFileName;
                result.ArgFileName = kvp.Value.OriginalFileName;
                result.CitRetentionTime = kvp.Key.RetentionTime;
                result.ArgRetentionTime = kvp.Value.RetentionTime;
                result.CitCharge = kvp.Key.Charge;
                result.ArgCharge = kvp.Value.Charge;
                result.CitSpectrumID = kvp.Key.ID;
                result.ArgSpectrumID = kvp.Value.ID;
                result.Score = kvp.Key.CitScore;
                result.MatchScore = kvp.Value.MatchScore;
                // Add the result to the result list
                resultsQuant.Add(result);
            }
        }

        /// <summary>
        /// Calculate the extracted ion count from a <paramref name="spectrum"/>.
        /// </summary>
        /// <param name="spectrum">The spectrum to be calculated.</param>
        /// <param name="xic">The current extracted ion count.</param>
        /// <param name="input">The input file.</param>
        /// <returns>The extracted ion count for the spectrum.</returns>
        private static double CalculateExtractedIonCount(MsSpectrumBase spectrum, double xic, InputData input)
        {
            // Calculate the retention time interval
            int timeLow = spectrum.RetentionTime - (rTInterval * 60);
            int timeHigh = spectrum.RetentionTime + (rTInterval * 60);
            // Loop through the MS1 scans
            foreach (MgxScan ms1 in input.MS1Scans)
            {
                // Check if the retention time are within the interval
                if (ms1.RetentionTime >= timeLow & ms1.RetentionTime <= timeHigh)
                {
                    // If so, loop through the M/Z values
                    for (int i = 0; i <= ms1.MzValues.Length - 1; i++)
                    {
                        // Calculate the PPM deviation. TODO: Is this correct?
                        double delta = CalculatePPMDeviation(spectrum.PreCursorMz, ppm);
                        // If the difference between the precursor M/Z and current M/Z value is less than the deviation, then add the intensity to the extracted ion count
                        if (spectrum.PreCursorMz - ms1.MzValues[i] <= delta)
                        {
                            xic += ms1.Intencities[i];
                        }
                    }
                }
            }

            return xic;
        }

        /// <summary>
        /// Calculate the deviation.
        /// TODO: Understand why it is returning 0.
        /// </summary>
        /// <param name="mzVal"></param>
        /// <param name="ppm">The ppm.</param>
        /// <returns>The deviation in ppm.</returns>
        private static double CalculatePPMDeviation(double mzVal, int ppm)
        {
            double deviation = mzVal / 1000000 * ppm;
            return 0;
        }

        /// <summary>
        /// Remove the database prefix in the protein label.
        /// </summary>
        /// <param name="proteinLabel">The protein label with the DB prefix.</param>
        /// <returns>The protein label without the DB prefix.</returns>
        private static string RemoveProteinDbPrefix(string proteinLabel)
        {
            int index = proteinLabel.IndexOf('|');
            return proteinLabel.Remove(0, index + 1);
        }

        internal static int ReturnPPM(int tolerance)
        {
            ppm = tolerance;
            return ppm;
        }

        internal static int ReturnRTInterval(int interval)
        {
            rTInterval = interval;
            return rTInterval;
        }

        private static List<QuantificationResult> ReturnQuantResults(List<QuantificationResult> resultsQuant)
        {
            QuantificationResults = resultsQuant;
            return QuantificationResults;
        }

        /// <summary>
        /// Get the quantified complementary citrullinated spectrum index.
        /// </summary>
        /// <param name="argMainSpectrum">The quantified main arginine spectrum.</param>
        /// <param name="complCitSpectra">The list of complementary arginine scans.</param>
        /// <returns>The index of the quantified complementary spectra in <paramref name="complCitSpectra"/>.</returns>
        internal static int GetQuantifiedComplementaryCitSpectrumIndex(XTSpectrum argMainSpectrum, List<MgxScan> complCitSpectra)
        {
            // Get the ID of the complementary cit spectrum
            if(argSpecScanDict.TryGetValue(argMainSpectrum, out MgxScan complSpectrum))
            {
                // Get the compl spectrum ID
                int spectrumId = complSpectrum.ScanNumber;

                for (int i = 0; i < complCitSpectra.Count - 1; i++)
                {
                    if (complCitSpectra[i].ScanNumber == spectrumId) return i;
                }
            }

            // If no spectra is found, return 0
            return 0;
        }

        /// <summary>
        /// Get the quantified complementary arginine spectrum index.
        /// </summary>
        /// <param name="citMainSpectrum">The quantified main citrullination spectrum.</param>
        /// <param name="complArgSpectra">The list of complementary arginine spectra.</param>
        /// <returns>The index of the quantified complementary spectra in <paramref name="complArgSpectra"/>.</returns>
        internal static int GetQuantifiedComplementaryArgSpectrumId(XTSpectrum citMainSpectrum, List<XTSpectrum> complArgSpectra)
        {
            // Get the ID of the complementary cit spectrum
            if (citSpecSpecDict.TryGetValue(citMainSpectrum, out XTSpectrum complSpectrum))
            {
                // Get the compl spectrum ID
                int spectrumId = complSpectrum.ID;

                for (int i = 0; i < complArgSpectra.Count - 1; i++)
                {
                    if (complArgSpectra[i].ID == spectrumId) return i;
                }
            }
            // If no spectra is found, return 0
            return 0;
        }
    }
}
