using System.Linq;

namespace Citrullia
{
    /// <summary>
    /// Data class for finding the MS1.
    /// </summary>
    static class FindMS1
    {
        /// <summary>
        /// Find the original MS1 information such as retention time, orgininal scan number and the parent MS1 scan.
        /// </summary>
        /// <param name="spectrumMS2">The MS2 spectrum.</param>
        /// <returns>The MS2 spectrum with the MS1 data.</returns>
        internal static XTSpectrum FindOriginalMS1Info (XTSpectrum spectrumMS2)
        {
            // Get the scan number
            string description = spectrumMS2.SpectrumDescription;
            int scanNumb = int.Parse(description.Split(';').First().Split(' ').Last());

            // Loop through each of the input files
            foreach (Input input in FileReader.inputFiles)
            {
                // Get the input file name
                string inputFileName = input.FilePath.Split('\\').Last();
                // Check if the original filename matches the name of the input file
                if (spectrumMS2.OriginalFileName == inputFileName)
                {
                    // If the filename matches the input filename
                    // Loop through all of the MS2 scans in the input
                    foreach (RawScan scan in input.MS2Scans)
                    {
                        // Check if the scan number matches the scan number of the MS2 spectrum
                        if (scanNumb == scan.ScanNumber)
                        {
                            // If so.
                            // Set the retention time
                            spectrumMS2.RetentionTime = scan.RetentionTime;
                            // Set the parent scan number
                            spectrumMS2.ParentScanNumber = scan.ParentScanNumber;
                            // Set the precursor MZ-value
                            spectrumMS2.PreCursorMz = scan.PreCursorMz;
                        }
                    }

                    // Loop through each of the MS1 scans
                    foreach (RawScan s in input.MS1Scans)
                    {
                        // Check if the MS2 spectrum parent scan number matches the MS1 scan
                        if (spectrumMS2.ParentScanNumber == s.ScanNumber)
                        {
                            // If so
                            // Set the parent scan
                            spectrumMS2.ParentScan = s;
                            // Set it to be an non-orphan
                            spectrumMS2.Orphan = false;
                            // Break and continue with the next scan
                            break;
                        }
                        else
                        {
                            // If not
                            // Set the scan to be an orphan
                            spectrumMS2.Orphan = true;
                        }
                    }
                }
            }
            // Return the spectrum with the new information
            return spectrumMS2;
        }

        /// <summary>
        /// Find potential citrullination parent in MS1.
        /// </summary>
        /// <param name="scanMS2">The MS2 scan.</param>
        /// <returns>The MS2 spectrum with the new information.</returns>
        internal static RawScan FindPotCitParentMS1Scan(RawScan scanMS2)
        {
            // Loop through each of the input files
            foreach (Input input in FileReader.inputFiles)
            {
                // Get the filename of the input file
                string inputFileName = input.FilePath.Split('\\').Last();
                // Check if input filename matches the original filename of the MS2 scan
                if (scanMS2.OriginalFileName == inputFileName)
                {
                    // Loop through each of the MS1 scans
                    foreach (RawScan s in input.MS1Scans)
                    {
                        // Check if the MS2 parent scan number matches the MS1 scan number
                        if (scanMS2.ParentScanNumber == s.ScanNumber)
                        {
                            // If so: Set the MS1 scan as the parent scan of MS2
                            scanMS2.ParentScan = s;
                            // Set MS2 to be an non-orphan
                            scanMS2.Orphan = false;
                            // Break and continue with the next scan
                            break;
                        }
                        else
                        {
                            // If not: Set the scan to be an orphan
                            scanMS2.Orphan = true;
                        }
                    }
                }
            }
            // Return the scan with the new information
            return scanMS2;
        }

        /// <summary>
        /// Handle orphan spectra.
        /// </summary>
        /// <param name="spectrumMS2">The MS2 spectrum.</param>
        /// <returns>The MS2 spectrum with a "false" parent.</returns>
        internal static XTSpectrum HandleOrphanSpecs(XTSpectrum spectrumMS2)
        {
            // Create an array of MZ-values
            double[] valMZ = { 100, 300, 500, 700, 900, 1100, 1300, spectrumMS2.PreCursorMz };
            // Create an array of intensities
            double[] valInt = { 30, 50, 100, 70, 40, 10, 20, 60 };
            // Create a parent scan
            var scanParent = new RawScan { MzValues = valMZ, RetentionTime = 0, Intencities = valInt, TotalIonCount = 0 };
            
            // If the spectrum is an orphan set the newly created scan to be the parent
            if (spectrumMS2.Orphan == true)
            {
                spectrumMS2.ParentScan = scanParent;
            }

            // The spectrum with a parent
            return spectrumMS2;
        }

        /// <summary>
        /// Handle orphan scan.
        /// </summary>
        /// <param name="scanMS2">The MS2 scan.</param>
        /// <returns>The MS2 scan with a "false" parent.</returns>
        internal static RawScan HandleOrphanScans(RawScan scanMS2)
        {
            // Create an array of MZ-values
            double[] valMZ = { 100, 300, 500, 700, 900, 1100, 1300, scanMS2.PreCursorMz };
            // Create an array of intensities
            double[] valInt = { 30, 50, 100, 70, 40, 10, 20, 60 };
            // Create a parent scan
            var scanParent = new RawScan { MzValues = valMZ, RetentionTime = 0, Intencities = valInt, TotalIonCount = 0 };

            // If the scan is an orphan set the newly created scan to be the parent
            if (scanMS2.Orphan == true)
            {
                scanMS2.ParentScan = scanParent;
            }

            // The spectrum with a parent
            return scanMS2;
        }
    }
}
