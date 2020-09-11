using System;
using System.Collections.Generic;
using System.Linq;

namespace Citrullia
{
    /// <summary>
    /// Utility class for calculating the ions.
    /// </summary>
    // Transfer. Maybe refactor
    internal static class IonCalculation
    {
        /// <summary>The dictionary of amino acids and their mass.</summary>
        internal static Dictionary<string, double> massDictAminoAcids = new Dictionary<string, double>();

        /// <summary>
        /// Find the MS2 precursor peak the MS1 scans.
        /// </summary>
        /// <param name="precursorMass">The precursor mass.</param>
        /// <param name="scanMS1">The MS1 scan.</param>
        /// <returns>The intensity for the precursor peak.</returns>
        internal static double FindMS2PrecursorPeakIntInMS1(double precursorMass, RawScan scanMS1)
        {
            // Set a temporary intensity value
            double intVal = 0;
            // Loop through of the M/Z values in the MS1 scan
            for (int i = 0; i <= scanMS1.MzValues.Count() - 1; i++)
            {
                // If the M/Z-value matches the precursor mass, set the intensity.
                if (scanMS1.MzValues[i] == precursorMass)
                {
                    intVal = scanMS1.Intencities[i];
                }
            }
            // Return the intensity for the precursor peak
            return intVal;
        }

        /// <summary>
        /// Make the dictionary of AA masses.
        /// </summary>
        internal static void MakeAaMassDictionary()
        {
            // Create an array of amino acid one letter codes
            string[] aa = new string[] { "G", "A", "S", "P", "V", "T", "C", "I", "L", "N", "D", "Q", "K", "E", "M", "H", "F", "R", "Y", "W" };
            // Create an array of amino acid masses
            double[] aaMass = new double[] { 57.021464, 71.037114, 87.032028, 97.052764, 99.068414, 101.047679, 103.009185, 113.084064, 113.084064,
                114.042927, 115.026943, 128.058578, 128.094963, 129.042593, 131.040485, 137.058912, 147.068414, 156.101111, 163.063329, 186.079313 };

            // Create and dictionary
            var aaMassDict = new Dictionary<string, double>();
            // Loop through all of the amino acids and add the aa one-letter code and the mass to the dictionary
            for (int i = 0; i <= aa.Length - 1; i++)
            {
                aaMassDict.Add(aa[i], aaMass[i]);
            }
            // Set the AA mass dictionary
            ReturnAAMassDict(aaMassDict);
        }

        private static Dictionary<string, double> ReturnAAMassDict(Dictionary<string, double> aaMassDict)
        {
            massDictAminoAcids = aaMassDict;
            return massDictAminoAcids;
        }

        /// <summary>
        /// Calculate all of the ions in the spectrum.
        /// </summary>
        /// <param name="scanMS2">The MS2 spectrum.</param>
        /// <returns>The MS2 spectrum with the ion information.</returns>
        internal static XTSpectrum CalculateAllIons(XTSpectrum scanMS2)
        {
            // Calculate the spectrum ions
            scanMS2 = CalcSpecIons(scanMS2);
            // Get the domain range
            scanMS2 = GetDomainRange(scanMS2);
            // Get the correct modification index for the b-ions
            scanMS2 = GetCorrectBIonModificationIndex(scanMS2);
            // Get the correct modification index for the y-ions
            scanMS2 = GetCorrectYIonModificationIndex(scanMS2);
            // Find the possible b-ions
            scanMS2 = FindPossibleBIons(scanMS2);
            // Find the possible ytions
            scanMS2 = FindPossibleYIons(scanMS2);
            // Find the correct existing ions
            scanMS2 = FindExistingIons(scanMS2);
            return scanMS2;
        }

        /// <summary>
        /// Calculate the spectrum ions.
        /// </summary>
        /// <param name="scanMS2">The spectrum.</param>
        /// <returns></returns>
        internal static XTSpectrum CalcSpecIons(XTSpectrum scanMS2)
        {
            // Get all of the amino acids as a char array
            char[] aaSeq = scanMS2.Proteins[0].DoaminSeq.ToCharArray();
            // Convert the char array to a string array
            string[] aaSeqArray = aaSeq.Select(a => a.ToString()).ToArray();
            // Set the string array to the spectrum's amino acid sequence
            scanMS2.SpectrumAminoAcidSequence = aaSeqArray;
            // Create a temporary list of amino acid masses
            var aaMasses = new List<double>();
            // Loop through all of the amino acid sequence
            foreach (string aa in aaSeqArray)
            {
                // Try to get the amino acid mass
                if (massDictAminoAcids.TryGetValue(aa, out double aaMass))
                {
                    // If possible, add the mass to the list of amino acid masses
                    aaMasses.Add(aaMass);
                }
            }
            // Set the mass array to the array of spectrum's amino acid sequence masses
            scanMS2.SpectrumAaMasses = aaMasses.ToArray();
            return scanMS2;
        }

        /// <summary>
        /// Get the domain range.
        /// </summary>
        /// <param name="scanMS2">The spectrum.</param>
        /// <returns>The spectrum with domain ranges.</returns>
        internal static XTSpectrum GetDomainRange(XTSpectrum scanMS2)
        {
            // Get the domain start position
            int dStart = scanMS2.Proteins[0].DomainStartPos;
            // Get the domain end position
            int dEnd = scanMS2.Proteins[0].DomainEndPos;
            // Get the range length
            int rangeLength = dEnd - dStart + 1;
            // Get the domain range
            int[] domainRange = Enumerable.Range(scanMS2.Proteins[0].DomainStartPos, rangeLength).ToArray();
            // Get the reverse domain range
            int[] domainRangeReverse = domainRange.Reverse().ToArray();
            // Set the normal and reverse domain range to the spectrum
            scanMS2.SpectrumDomainRange = domainRange;
            scanMS2.SpectrumDomainRangeReverse = domainRangeReverse;
            // Return the scan.
            return scanMS2;
        }

        /// <summary>
        /// Get the correct modification index for the b-ions.
        /// </summary>
        /// <param name="scanMS2">The spectrum.</param>
        /// <returns>The spectrum with the new information</returns>
        internal static XTSpectrum GetCorrectBIonModificationIndex(XTSpectrum scanMS2)
        {
            // Create a new temporary list of modification indexes and their mass change
            Dictionary<int, double> modDictB = new Dictionary<int, double>();
            // Loop through all of the modifications
            foreach (XTModification mod in scanMS2.Proteins[0].Modifications)
            {
                // Get the modification position and mass
                int pos = mod.AtPosition;
                double massChange = mod.MassChange;
                // Get the index of the position in the domain range
                int index = Array.IndexOf(scanMS2.SpectrumDomainRange, pos);
                // Check if the modification index already contains the index
                if (modDictB.ContainsKey(index))
                {
                    // If so.
                    // Get the old modification mass
                    modDictB.TryGetValue(index, out double oldMod);
                    // Add the old modification mass and the mass change
                    massChange = oldMod + massChange;
                    // Remove the old modification
                    modDictB.Remove(index);
                    // And add the new index with the new mass change
                    modDictB.Add(index, massChange);
                }
                else
                {
                    // If no index already, add the mass charge.
                    modDictB.Add(index, massChange);
                }
            }
            // Set the dictionary to the spectrum.
            scanMS2.SpectrumBIonModDict = modDictB;
            // Return the spectrum
            return scanMS2;
        }

        /// <summary>
        /// Get the correct modification index for the y-ions.
        /// </summary>
        /// <param name="scanMS2">The spectrum.</param>
        /// <returns>The spectrum with the new information</returns>
        internal static XTSpectrum GetCorrectYIonModificationIndex(XTSpectrum scanMS2)
        {
            // Create a new temporary list of modification indexes and their mass change
            Dictionary<int, double> modDictY = new Dictionary<int, double>();
            // Loop through all of the modifications
            foreach (XTModification mod in scanMS2.Proteins[0].Modifications)
            {
                // Get the modification position and mass
                int pos = mod.AtPosition;
                double massChange = mod.MassChange;
                // Get the index of the position in the reverse domain range
                int index = Array.IndexOf(scanMS2.SpectrumDomainRangeReverse, pos);
                // Check if the modification index already contains the index
                if (modDictY.ContainsKey(index))
                {
                    // If so.
                    // Get the old modification mass
                    modDictY.TryGetValue(index, out double oldMod);
                    // Add the old modification mass and the mass change
                    massChange = oldMod + massChange;
                    // Remove the old modification
                    modDictY.Remove(index);
                    // And add the new index with the new mass change
                    modDictY.Add(index, massChange);
                }
                else
                {
                    // If no index already, add the mass charge.
                    modDictY.Add(index, massChange);
                }
            }
            // Set the dictionary to the spectrum.
            scanMS2.SpectrumYIonModDict = modDictY;
            // Return the spectrum
            return scanMS2;
        }

        /// <summary>
        /// Find the possible B-ions.
        /// </summary>
        /// <param name="scanMS2">The spectrum</param>
        /// <returns>The spectrum with the possible B-ions.</returns>
        internal static XTSpectrum FindPossibleBIons(XTSpectrum scanMS2)
        {
            // Set the proton masses
            double pMass = 1.007276470;
            // Create a temporary list of B-ions
            List<double> bIons = new List<double>();
            // The temporary variable for the b-ion
            double bIon = 0;
            // Loop through all of the amino acid. TODO: Reformat so same code for much as possible
            for (int i = 0; i <= scanMS2.SpectrumAaMasses.Length - 1; i++)
            {

                // Try to get the modification mass and add it to the b-ion.
                if (scanMS2.SpectrumBIonModDict.TryGetValue(i, out double modMass))
                {
                    bIon += modMass;
                }
                // Add the amino acid mass and the mass qnd the mass of the proton to the b-ion
                double extraMass = (i == 0) ? pMass : 0;
                bIon += scanMS2.SpectrumAaMasses[i] + extraMass;
                // Add the b-ion value to the list
                bIons.Add(bIon);
            }
            // Set the array of b-ions to spectrum's possible b-ions
            scanMS2.SpectrumPossibleBIons = bIons.ToArray();
            // Return the spectrum
            return scanMS2;
        }

        /// <summary>
        /// Find the possible Y-ions.
        /// </summary>
        /// <param name="scanMS2">The spectrum</param>
        /// <returns>The spectrum with the possible Y-ions.</returns>
        internal static XTSpectrum FindPossibleYIons(XTSpectrum scanMS2)
        {
            // Set the proton, hydrogen and oxygen mass and calculate the mass for the hydroxyl-group
            double pMass = 1.007276470;
            double hMass = 1.007825035;
            double oMass = 15.994914630;
            double ohMass = hMass + oMass;
            // Reverse the amino acid masses
            var aaMasses = scanMS2.SpectrumAaMasses.Reverse().ToArray();
            // Create a temporary list of B-ions
            List<double> yIons = new List<double>();
            // The temporary variable for the y-ion
            double yIon = 0;
            // Loop through all of the amino acid. TODO: Reformat so same code for much as possible
            for (int i = 0; i <= aaMasses.Length - 1; i++)
            {
                // Try to get the modification mass and add it to the y-ion.
                if (scanMS2.SpectrumYIonModDict.TryGetValue(i, out double modMass))
                {
                    yIon += modMass;
                }
                // Add the amino acid mass and the mass qnd the mass of the proton, hydrogen and hydroxyl-group to the y-ion+
                double extraMass = (i == 0) ? (ohMass + hMass + pMass) : 0;
                yIon += aaMasses[i] + extraMass;
                // Add the y-ion value to the list
                yIons.Add(yIon);

            }
            // Set the array of y-ions to spectrum's possible y-ions
            scanMS2.SpectrumPossibleYIons = yIons.ToArray();
            // Return the spectrum
            return scanMS2;
        }

        /// <summary>
        /// Find the existing ions.
        /// </summary>
        /// <param name="scanMS2">The spectrum.</param>
        /// <returns>The spectrum with all of its ions.</returns>
        internal static XTSpectrum FindExistingIons(XTSpectrum scanMS2)
        {
            // Create temporary lists for all of the variables
            List<double> bIons = new List<double>();
            List<double> b17Ions = new List<double>();
            List<double> b18Ions = new List<double>();
            List<double> yIons = new List<double>();
            List<double> y17Ions = new List<double>();
            List<double> y18Ions = new List<double>();
            List<double> aIons = new List<double>();

            List<double> b17IonsPos = new List<double>();
            List<double> b18IonsPos = new List<double>();
            List<double> y17IonsPos = new List<double>();
            List<double> y18IonsPos = new List<double>();
            List<double> aIonsPos = new List<double>();

            List<int> bIonIndex = new List<int>();
            List<int> b17IonIndex = new List<int>();
            List<int> b18IonIndex = new List<int>();
            List<int> yIonIndex = new List<int>();
            List<int> y17IonIndex = new List<int>();
            List<int> y18IonIndex = new List<int>();
            List<int> aIonIndex = new List<int>();

            // Set tha mass of hydrogen, carbon and oxygen. Calculate masses for OH- and CO-groups and water
            double hMass = 1.007825035;
            double cMAss = 12.000000000;
            double oMass = 15.994914630;
            double ohMass = hMass + oMass;
            double coMass = cMAss + oMass;
            double h2oMass = (2 * hMass) + oMass;

            // Get the mass fragment error
            double error = double.Parse(External.XTandemInput.FMError, FileReader.NumberFormat);

            // Loop through all of the M/Z-values in the spectrum
            foreach (double mz in scanMS2.MzValues)
            {
                // Loop through all of the possible B-ions to find the B-ions.
                for (int i = 0; i <= scanMS2.SpectrumPossibleBIons.Length - 1; i++)
                {
                    // Calculate the the fragment difference
                    double delta = Math.Abs(mz - scanMS2.SpectrumPossibleBIons[i]);
                    // If the fragment difference is less than the max error, add it to the list
                    if (delta <= error)
                    {
                        bIons.Add(mz);
                        bIonIndex.Add(i);
                    }
                }
                // Loop through all of the possible B-ions to find the B-17 ions.
                for (int i = 0; i <= scanMS2.SpectrumPossibleBIons.Length - 1; i++)
                {
                    // Add the possible ion
                    b17IonsPos.Add(Math.Round(scanMS2.SpectrumPossibleBIons[i] - ohMass, 3));
                    // Calculate the the fragment difference
                    double delta = Math.Abs(mz - (scanMS2.SpectrumPossibleBIons[i] - ohMass));
                    // If the fragment difference is less than the max error, add it to the list
                    if (delta <= error)
                    {
                        b17Ions.Add(mz);
                        b17IonIndex.Add(i);
                    }
                }

                // Loop through all of the possible B-ions to find the B-18 ions.
                for (int i = 0; i <= scanMS2.SpectrumPossibleBIons.Length - 1; i++)
                {
                    // Add the possible ion
                    b18IonsPos.Add(Math.Round(scanMS2.SpectrumPossibleBIons[i] - h2oMass, 3));
                    // Calculate the the fragment difference
                    double delta = Math.Abs(mz - (scanMS2.SpectrumPossibleBIons[i] - h2oMass));
                    // If the fragment difference is less than the max error, add it to the list
                    if (delta <= error)
                    {
                        b18Ions.Add(mz);
                        b18IonIndex.Add(i);
                    }
                }
                // Loop through all of the possible B-ions to find the A-ions.
                for (int i = 0; i <= scanMS2.SpectrumPossibleBIons.Length - 1; i++)
                {
                    // Add the possible ion
                    aIonsPos.Add(Math.Round(scanMS2.SpectrumPossibleBIons[i] - coMass, 3));
                    // Calculate the the fragment difference
                    double delta = Math.Abs(mz - (scanMS2.SpectrumPossibleBIons[i] - coMass));
                    // If the fragment difference is less than the max error, add it to the list
                    if (delta <= error)
                    {
                        aIons.Add(mz);
                        aIonIndex.Add(i);
                    }
                }

                // Loop through all of the possible Y-ions
                for (int i = 0; i <= scanMS2.SpectrumPossibleYIons.Length - 1; i++)
                {
                    // Calculate the the fragment difference
                    double delta = Math.Abs(mz - scanMS2.SpectrumPossibleYIons[i]);
                    // If the fragment difference is less than the max error, add it to the list
                    if (delta <= error)
                    {
                        yIons.Add(mz);
                        yIonIndex.Add(i);
                    }
                }
                // Loop through all of the possible Y-ions to find the A-17 -ions.
                for (int i = 0; i <= scanMS2.SpectrumPossibleYIons.Length - 1; i++)
                {
                    // Add the possible ion
                    y17IonsPos.Add(Math.Round(scanMS2.SpectrumPossibleYIons[i] - ohMass, 3));
                    // Calculate the the fragment difference
                    double delta = Math.Abs(mz - (scanMS2.SpectrumPossibleYIons[i] - ohMass));
                    // If the fragment difference is less than the max error, add it to the list
                    if (delta <= error)
                    {
                        y17Ions.Add(mz);
                        y17IonIndex.Add(i);
                    }
                }

                // Loop through all of the possible Y-ions to find the Y-18 ions.
                for (int i = 0; i <= scanMS2.SpectrumPossibleYIons.Length - 1; i++)
                {
                    // Add the possible ion
                    y18IonsPos.Add(Math.Round(scanMS2.SpectrumPossibleYIons[i] - h2oMass, 3));
                    // Calculate the the fragment difference
                    double delta = Math.Abs(mz - (scanMS2.SpectrumPossibleYIons[i] - h2oMass));
                    // If the fragment difference is less than the max error, add it to the list
                    if (delta <= error)
                    {
                        y18Ions.Add(mz);
                        y18IonIndex.Add(i);
                    }
                }
            }
            // Set the lists to the spectrum
            scanMS2.SpectrumPossibleAIons = aIonsPos.ToArray();
            scanMS2.SpectrumPossibleBm18Ions = b18IonsPos.ToArray();
            scanMS2.SpectrumPossibleBm17Ions = b17IonsPos.ToArray();
            scanMS2.SpectrumPossibleYm18Ions = y18IonsPos.ToArray();
            scanMS2.SpectrumPossibleYm17Ions = y17IonsPos.ToArray();

            scanMS2.SpectrumExistingBIons = bIons.ToArray();
            scanMS2.SpectrumBm17Ions = b17Ions.ToArray();
            scanMS2.SpectrumBm18Ions = b18Ions.ToArray();

            scanMS2.SpectrumExistingYIons = yIons.ToArray();
            scanMS2.SpectrumYm17Ions = y17Ions.ToArray();
            scanMS2.SpectrumYm18Ions = y18Ions.ToArray();
            scanMS2.SpectrumAIons = aIons.ToArray();

            scanMS2.BIonIndex = bIonIndex.ToArray();
            scanMS2.B17IonIndex = b17IonIndex.ToArray();
            scanMS2.B18IonIndex = b18IonIndex.ToArray();
            scanMS2.YIonIndex = yIonIndex.ToArray();
            scanMS2.Y17IonIndex = y17IonIndex.ToArray();
            scanMS2.Y18IonIndex = y18IonIndex.ToArray();
            scanMS2.AIonIndex = aIonIndex.ToArray();
            // Return the spectrum with the new information
            return scanMS2;
        }

        /// <summary>
        /// Calculate all of the potential citrullination ions.
        /// </summary>
        /// <param name="argScanMS2">The arginine MS2 spectrum.</param>
        /// <param name="potCitMS2Scan">The potential citrulliated scan.</param>
        /// <returns>The scan with the potential citrullination ions.</returns>
        internal static RawScan CalculateAllPotentialCitrullinationIons(XTSpectrum argScanMS2, RawScan potCitMS2Scan)
        {
            // Get the amino acid sequence
            string[] aas = argScanMS2.SpectrumAminoAcidSequence;
            // Create a variable for holding the arginine B-index
            int aaModB = 0;
            // Loop through the sequence and try to find the index of the arginine.
            for (int i = 0; i <= aas.Length-1; i++)
            {
                if (aas[i] == "R")
                {
                    aaModB = i;
                    break;
                }
            }
            // Calculate the Y-index
            int aaModY = Math.Abs(aaModB - aas.Length)-1;
            // Create a temporary list for the possible y-ions
            List<double> posYIonList = new List<double>();
            // Loop through all of the possible y-ions
            for (int i = 0; i <= argScanMS2.SpectrumPossibleYIons.Length-1; i++)
            {
                // If the current ion is greater than the currenty y-ion index
                if (i >= aaModY)
                {
                    // Calculate the modification and add it to the list
                    double modYIon = argScanMS2.SpectrumPossibleYIons[i] + 0.9845;
                    posYIonList.Add(modYIon);
                }
                else
                {
                    // If it is less than the value, just add it 
                    posYIonList.Add(argScanMS2.SpectrumPossibleYIons[i]);
                }
            }
            // Set the possible ions
            double[] yIonsPos = posYIonList.ToArray();

            // Create a temporary list of B-ions
            List<double> posBIonList = new List<double>();
            // Loop through all of the possible y-ions
            for (int i = 0; i <= argScanMS2.SpectrumPossibleBIons.Length-1; i++)
            {
                // If the current ion is greater than the currenty b-ion index
                if (i >= aaModB)
                {
                    // Calculate the modification and add it to the list
                    double modBIon = argScanMS2.SpectrumPossibleBIons[i] + 0.9845;
                    posBIonList.Add(modBIon);
                }
                else
                {
                    // If it is less than the value, just add it 
                    posBIonList.Add(argScanMS2.SpectrumPossibleBIons[i]);
                }
            }
            // Set the possible ions
            double[] posBIons = posBIonList.ToArray();


            // Make all the necesarry list to calculate all the Ions:
            List<double> bIons = new List<double>();
            List<double> b17Ions = new List<double>();
            List<double> b18Ions = new List<double>();
            List<double> yIons = new List<double>();
            List<double> y17Ions = new List<double>();
            List<double> y18Ions = new List<double>();
            List<double> aIons = new List<double>();

            List<double> b17IonsPos = new List<double>();
            List<double> b18IonsPos = new List<double>();
            List<double> y17IonsPos = new List<double>();
            List<double> y18IonsPos = new List<double>();
            List<double> aIonsPos = new List<double>();

            List<int> bIonIndex = new List<int>();
            List<int> b17IonIndex = new List<int>();
            List<int> b18IonIndex = new List<int>();
            List<int> yIonIndex = new List<int>();
            List<int> y17IonIndex = new List<int>();
            List<int> y18IonIndex = new List<int>();
            List<int> aIonIndex = new List<int>();

            // Set tha mass of hydrogen, carbon and oxygen. Calculate masses for OH- and CO-groups and water
            double hMass = 1.007825035;
            double cMAss = 12.000000000;
            double oMass = 15.994914630;
            double ohMass = hMass + oMass;
            double coMass = cMAss + oMass;
            double h2oMass = (2 * hMass) + oMass;

            //Begin calculating all the existing Ions in the spectrum:
            double error = double.Parse(External.XTandemInput.FMError, FileReader.NumberFormat);

            // Loop through all of the M/Z-values
            foreach (double mz in potCitMS2Scan.MzValues)
            {

                // Loop through all of the possible B-ions to find the B-ions.
                for (int i = 0; i <= posBIons.Length - 1; i++)
                {
                    // Calculate the the fragment difference
                    double delta = Math.Abs(mz - posBIons[i]);
                    // If the fragment difference is less than the max error, add it to the list
                    if (delta <= error)
                    {
                        bIons.Add(mz);
                        bIonIndex.Add(i);
                    }
                }

                // Loop through all of the possible B-ions to find the B-17 ions.
                for (int i = 0; i <= posBIons.Length - 1; i++)
                {
                    // Add the possible ion
                    b17IonsPos.Add(Math.Round(posBIons[i] - ohMass, 3));
                    // Calculate the the fragment difference
                    double delta = Math.Abs(mz - (posBIons[i] - ohMass));
                    // If the fragment difference is less than the max error, add it to the list
                    if (delta <= error)
                    {
                        b17Ions.Add(mz);
                        b17IonIndex.Add(i);
                    }
                }

                // Loop through all of the possible B-ions to find the B-17 ions.
                for (int i = 0; i <= posBIons.Length - 1; i++)
                {
                    // Add the possible ion
                    b18IonsPos.Add(Math.Round(posBIons[i] - h2oMass, 3));
                    // Calculate the the fragment difference
                    double delta = Math.Abs(mz - (posBIons[i] - h2oMass));
                    // If the fragment difference is less than the max error, add it to the list
                    if (delta <= error)
                    {
                        b18Ions.Add(mz);
                        b18IonIndex.Add(i);
                    }
                }

                // Loop through all of the possible Y-ions to find the Y-ions.
                for (int i = 0; i <= yIonsPos.Length - 1; i++)
                {
                    // Calculate the the fragment difference
                    double delta = Math.Abs(mz - yIonsPos[i]);
                    // If the fragment difference is less than the max error, add it to the list
                    if (delta <= error)
                    {
                        yIons.Add(mz);
                        yIonIndex.Add(i);
                    }
                }

                // Loop through all of the possible Y-ions to find the Y-17 ions.
                for (int i = 0; i <= yIonsPos.Length - 1; i++)
                {
                    // Add the possible ion
                    y17IonsPos.Add(Math.Round(yIonsPos[i] - ohMass, 3));
                    // Calculate the the fragment difference
                    double delta = Math.Abs(mz - (yIonsPos[i] - ohMass));
                    // If the fragment difference is less than the max error, add it to the list
                    if (delta <= error)
                    {
                        y17Ions.Add(mz);
                        y17IonIndex.Add(i);
                    }
                }

                // Loop through all of the possible Y-ions to find the Y-18 ions.
                for (int i = 0; i <= yIonsPos.Length - 1; i++)
                {
                    // Add the possible ion
                    y18IonsPos.Add(Math.Round(yIonsPos[i] - h2oMass, 3));
                    // Calculate the the fragment difference
                    double delta = Math.Abs(mz - (yIonsPos[i] - h2oMass));
                    // If the fragment difference is less than the max error, add it to the list
                    if (delta <= error)
                    {
                        y18Ions.Add(mz);
                        y18IonIndex.Add(i);
                    }
                }

                // Loop through all of the possible Y-ions to find the A-ions.
                for (int i = 0; i <= posBIons.Length - 1; i++)
                {
                    // Add the possible ion
                    aIonsPos.Add(Math.Round(posBIons[i] - coMass, 3));
                    // Calculate the the fragment difference
                    double delta = Math.Abs(mz - (posBIons[i] - coMass));
                    // If the fragment difference is less than the max error, add it to the list
                    if (delta <= error)
                    {
                        aIons.Add(mz);
                        aIonIndex.Add(i);
                    }
                }
            }

            // Set the lists to the spectrum
            potCitMS2Scan.SpectrumPossibleBIons = posBIons;
            potCitMS2Scan.SpectrumPossibleYIons = yIonsPos;

            potCitMS2Scan.SpectrumPossibleAIons = aIonsPos.ToArray();
            potCitMS2Scan.SpectrumPossibleBm18Ions = b18IonsPos.ToArray();
            potCitMS2Scan.SpectrumPossibleBm17Ions = b17IonsPos.ToArray();
            potCitMS2Scan.SpectrumPossibleYm18Ions = y18IonsPos.ToArray();
            potCitMS2Scan.SpectrumPossibleYm17Ions = y17IonsPos.ToArray();

            potCitMS2Scan.SpectrumExistingBIons = bIons.ToArray();
            potCitMS2Scan.SpectrumBm17Ions = b17Ions.ToArray();
            potCitMS2Scan.SpectrumBm18Ions = b18Ions.ToArray();

            potCitMS2Scan.SpectrumExistingYIons = yIons.ToArray();
            potCitMS2Scan.SpectrumYm17Ions = y17Ions.ToArray();
            potCitMS2Scan.SpectrumYm18Ions = y18Ions.ToArray();
            potCitMS2Scan.SpectrumAIons = aIons.ToArray();

            potCitMS2Scan.BIonIndex = bIonIndex.ToArray();
            potCitMS2Scan.B17IonIndex = b17IonIndex.ToArray();
            potCitMS2Scan.B18IonIndex = b18IonIndex.ToArray();
            potCitMS2Scan.YIonIndex = yIonIndex.ToArray();
            potCitMS2Scan.Y17IonIndex = y17IonIndex.ToArray();
            potCitMS2Scan.Y18IonIndex = y18IonIndex.ToArray();
            potCitMS2Scan.AIonIndex = aIonIndex.ToArray();
            // Return the spectrum with the new information
            return potCitMS2Scan;
        }
    }
}
