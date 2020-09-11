using Citrullia.Library;
using Citrullia.Library.MassSpectra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Citrullia
{
    /// <summary>
    /// Utility class for the score function.
    /// </summary>
    internal static class ScoreFunction
    {
        #region Main scoring functions
        /// <summary>
        /// Score the Citrullinated spectra.
        /// </summary>
        /// <param name="inputSpectra">The dictionary with the citrullinated spectra to be scored and their complementary arginine spectra.</param>
        /// <returns>The dictionary with the scored citrullinated spectra an their complementary scored arginine spectra.</returns>
        internal static Dictionary<XTSpectrum, List<XTSpectrum>> ScoreCitrullinationSpectra(Dictionary<XTSpectrum, List<XTSpectrum>> inputSpectra)
        {
            // Create a temporary list of scored spectra.
            Dictionary<XTSpectrum, List<XTSpectrum>> scoredSpectra = new Dictionary<XTSpectrum, List<XTSpectrum>>();

            // Loop through all of the spectra keyvaluepairs.
            foreach (KeyValuePair<XTSpectrum, List<XTSpectrum>> spectrum in inputSpectra)
            {
                // Score the main spectrum
                XTSpectrum scoredSpectrum = ScoreCitMainSpectrum(spectrum.Key);

                // Score all of the complementary spectra
                // Create a temporary list of scored complementary spectra
                List<XTSpectrum> scoredComplSpectra = new List<XTSpectrum>();

                // Loop through all of the complementary spectra
                foreach (XTSpectrum complSpectrum in spectrum.Value)
                {
                    // Score the complementary spectrum
                    XTSpectrum scoredComplSpectrum = ScoreMatchMainCitComplArg(scoredSpectrum, complSpectrum);
                    scoredComplSpectrum.IsoCyanicLossMzMs2 = new double[0];
                    // Add the scored spectrum to the list
                    scoredComplSpectra.Add(scoredComplSpectrum);
                }


                // Sort the list
                var scoredCompSpectraSorted = from x in scoredComplSpectra
                                              orderby x.MatchScore descending
                                              select x;
                scoredComplSpectra = scoredCompSpectraSorted.ToList();

                // Add the main spectrum and the complementary spectra to the dictionary
                scoredSpectra.Add(scoredSpectrum, scoredComplSpectra);
            }

            var sortedScoredSpectra = (from x in scoredSpectra
                                       orderby x.Key.CitScore descending
                                       select x).ToDictionary(x => x.Key, x => x.Value);

            // Add spectra above cut-off if set to do so.
            if (ScoreSettings.IncludeCutOff)
            {
                // Add to quantification
                AddCitSpectraAboveCutoffToQuantification(sortedScoredSpectra);
            }

            // Return the sorted scored spectra
            return sortedScoredSpectra;
        }

        /// <summary>
        /// Score the arginine spectra.
        /// </summary>
        /// <param name="inputSpectra">The dictionary with the arginine spectra to be scored and their complementary citrullinated spectra.</param>
        /// <returns>he dictionary with the scored spectra an their complementary citrullinated spectra.</returns>
        internal static Dictionary<XTSpectrum, List<RawScan>> ScoreArginineSpectra(Dictionary<XTSpectrum, List<RawScan>> inputSpectra)
        {
            // Create a temporary dictionary of scored spectra
            Dictionary<XTSpectrum, List<RawScan>> scoredSpectra = new Dictionary<XTSpectrum, List<RawScan>>();

            // Loop throguh all of the spectra keyvaluepairs
            foreach (KeyValuePair<XTSpectrum, List<RawScan>> spectrum in inputSpectra)
            {
                // Get the arginine spectrum and set the array of isocyanic loss m/z in order to prevent error
                XTSpectrum argSpectrum = spectrum.Key;
                argSpectrum.IsoCyanicLossMzMs2 = new double[0];
                List<RawScan> scoredCompCitScans = new List<RawScan>();
                // Loop trough all of the complementary cit spectra
                foreach (RawScan citScan in spectrum.Value)
                {
                    // Calculate the score
                    RawScan scoredScan = ScoreCitComplSpectrum(argSpectrum, citScan);
                    // Add the score to the list of cit scans
                    scoredCompCitScans.Add(scoredScan);

                }

                // Sort the list
                var scoredCompSpectraSorted = from x in scoredCompCitScans
                                              orderby x.MatchScore descending
                                              select x;
                scoredCompCitScans = scoredCompSpectraSorted.ToList();


                scoredSpectra.Add(argSpectrum, scoredCompCitScans);
            }

            // Sort dictionary
            var sortedScoredSpectra = (from x in scoredSpectra
                                       orderby x.Key.CitScore descending
                                       select x).ToDictionary(x => x.Key, x => x.Value);

            // Add spectra above cut-off if set to do so.
            if (ScoreSettings.IncludeCutOff)
            {
                // Add to quantification
                AddArgSpectraAboveCutOffToQuantification(sortedScoredSpectra);
            }
            // Return the sorted scored spectra
            return sortedScoredSpectra;
        }

        /// <summary>
        /// Score the lone citrullinated spectra.
        /// </summary>
        /// <param name="inputSpectra">>The list with the citrullinated spectra to be scored.</param>
        /// <returns>The list of scored spectra.</returns>
        internal static List<XTSpectrum> ScoreLoneCitrullinationSpectra(List<XTSpectrum> inputSpectra)
        {
            // Create a temporary list of scored spectra
            List<XTSpectrum> scoredSpectra = new List<XTSpectrum>();
            // Loop through all of the spectra
            foreach (XTSpectrum spectrum in inputSpectra)
            {
                // Score the spectrum
                XTSpectrum scoredSpectrum = ScoreCitMainSpectrum(spectrum);
                // Add it to the list of scored spectra
                scoredSpectra.Add(scoredSpectrum);
            }

            // Sort the scored spectra after its score
            var scoredSpectraSorted = (from x in scoredSpectra
                                       orderby x.MatchScore descending
                                       select x).ToList();

            // Add spectra above cut-off if set to do so.
            if (ScoreSettings.IncludeCutOff)
            {
                // Add to quantification
                AddLoneCitSpectraAboveCutoffToQunatification(scoredSpectraSorted);
            }
            // Return the list of scored spectra
            return scoredSpectraSorted;
        }
        #endregion

        /// <summary>
        /// Score the main citrullinated spectrum.
        /// </summary>
        /// <param name="spectrum">The spectrum to be scored.</param>
        /// <returns></returns>
        private static XTSpectrum ScoreCitMainSpectrum(XTSpectrum spectrum)
        {
            // The main spectrum does not have a match score
            spectrum.MatchScore = 0;
            // Validate the fragmentation.
            if (ValidateSpectrumFragmentation(spectrum, spectrum.SpectrumAminoAcidSequence) == false)
            {
                // If the fragmentation is invalid, set CitScore to 0 and return spectrum
                spectrum.CitScore = 0;
                spectrum.IsoCyanicLossMzMs2 = new double[0];
                return spectrum;
            }

            // Calculate the score
            double citScore = IsIsoCyanicAcidLossPresentMs1(spectrum) ? ScoreSettings.MS1IsoCyanicLossScore : 0;
            citScore += CountIsoCyanicAcidLossPresentMs2(spectrum) * 1;
            citScore += -Math.Log10(spectrum.SpectrumEVal);

            // Set the score
            spectrum.CitScore = Math.Round(citScore, 1);

            // Return the scored spectrum
            return spectrum;
        }

        /// <summary>
        /// Calculate CitScore and MatchScore for the complementary cit scan.
        /// </summary>
        /// <param name="mainArgSpectrum">The main arginine spectrum.</param>
        /// <param name="complCitScan">The complementary cit scan.</param>
        /// <returns>The scored scan.</returns>
        private static MgxScan ScoreCitComplSpectrum(XTSpectrum mainArgSpectrum, MgxScan complCitScan)
        {
            double citScore;

            //Validate the fragmentation.
            if (ValidateSpectrumFragmentation(complCitScan, mainArgSpectrum.SpectrumAminoAcidSequence))
            {
                // Calculate CitScore
                citScore = IsIsoCyanicAcidLossPresentMs1(complCitScan) ? 10 : 0;
                //citScore += CountIsoCyanicAcidLossPresentMs2(mainArgSpectrum, complCitScan) * 1;
                citScore = -Math.Log10(0.05);
                citScore += CountIsoCyanicAcidLossPresentMs2(complCitScan, GetIndexOfCitrullinationFromArg(mainArgSpectrum), mainArgSpectrum.SpectrumAminoAcidSequence);
            }
            else
            {
                // If the fragmentation is invalid, set CitScore to 0
                citScore = 0;
                complCitScan.IsoCyanicLossMzMs2 = new double[0];
            }


            // Set the CitScore
            complCitScan.CitScore = Math.Round(citScore, 1);
            // Score match between spectra
            complCitScan = ScoreMatchMainArgComplCit(mainArgSpectrum, complCitScan);
            // Return spectra
            return complCitScan;
        }

        /// <summary>
        /// Validate spectrum fragmentation.
        /// </summary>
        /// <param name="spectrum">The spectrum with the fragmentation to be validated.</param>
        /// <param name="aaSequence">The amino acid sequence.</param>
        /// <returns>True, if valid fragmentation; Otherwise, false.</returns>
        private static bool ValidateSpectrumFragmentation(MsSpectrum spectrum, string[] aaSequence)
        {
            // Check that spectrum contains an arginine
            if(aaSequence.Contains("R") == false)
            {
                return false;
            }

            // Check spectrum whether the spectrum contains  and N/Q. 
            if ((aaSequence.Contains("N") | aaSequence.Contains("Q")) == false)
            {
                //If it does not contain, N / Q it is concidered valid
                return true;
            }

            // Get the closset N/Q and R amino acid indexes
            GetClossetNQRAaRange(aaSequence, out int[] aaIndexes);

            // Loop through all of the aa indexes
            for (int aaIndex = 0; aaIndex < aaIndexes.Length; aaIndex++)
            {
                // Get the amino acid index
                int aa = aaIndexes[aaIndex];

                if (spectrum.BIonIndex.Contains(aa)) return true;
                if (spectrum.YIonIndex.Contains(-aa + aaSequence.Length - 1)) return true;
            }

            return false;
        }

        #region Score complementary spectrum
        /// <summary>
        /// Score the match between the main citrullinated spectrum and the complementary arginine spectrum.
        /// </summary>
        /// <param name="mainCitSpectrum">The main citrullinated spectrum.</param>
        /// <param name="complArgSpectrum">The complementary arginine spectrum.</param>
        /// <returns>The complementary spectrum with the match score.</returns>
        internal static XTSpectrum ScoreMatchMainCitComplArg(XTSpectrum mainCitSpectrum, XTSpectrum complArgSpectrum)
        {
            // Create a temporary variables for the counts
            int aIonCount = 0;
            int bIonCount = 0;
            int b17IonCount = 0;
            int b18IonCount = 0;
            int yIonCount = 0;
            int y17IonCount = 0;
            int y18IonCount = 0;

            int citIndex = GetIndexOfCitrullination(mainCitSpectrum);
            // Get the mass fragment error
            double fragMassError = double.Parse(External.XTandemInput.FMError, FileReader.NumberFormat);
            // Loop through the all of amino acid sequence. However only the index is used.
            for (int i = 0; i < mainCitSpectrum.SpectrumAminoAcidSequence.Length; i++)
            {
                // Check that both spectra contains the A-ion at this specific amino acid
                if (mainCitSpectrum.AIonIndex.Contains(i) && complArgSpectrum.AIonIndex.Contains(i))
                {
                    /* Calculate A-ion */
                    // Get the M/Z value for A-ion on the main citrullinated spectrum
                    double citMz = mainCitSpectrum.SpectrumPossibleAIons[i];
                    // Get the M/Z value for b-ion on the complementary arginine spectrum
                    double argMz = complArgSpectrum.SpectrumPossibleAIons[i];
                    // If the m/z-values has the right shift. Increment the ion count by 1
                    if (HasCorrectCitMassShift(citMz, argMz, fragMassError, i >= citIndex)) aIonCount++;
                }

                // Check that both spectra contains the B-ion at this specific amino acid
                if (mainCitSpectrum.BIonIndex.Contains(i) && complArgSpectrum.BIonIndex.Contains(i))
                {
                    /* Calculate B-ion */
                    // Get the M/Z value for b-ion on the main citrullinated spectrum
                    double citMz = mainCitSpectrum.SpectrumPossibleBIons[i];
                    // Get the M/Z value for b-ion on the complementary arginine spectrum
                    double argMz = complArgSpectrum.SpectrumPossibleBIons[i];
                    // If the m/z-values has the right shift. Increment the ion count by 1
                    if (HasCorrectCitMassShift(citMz, argMz, fragMassError, i >= citIndex)) bIonCount++;
                }

                // Check that both spectra contains the B-17-ion at this specific amino acid
                if (mainCitSpectrum.B17IonIndex.Contains(i) && complArgSpectrum.B17IonIndex.Contains(i))
                {
                    /* Calculate B-17-ion */
                    // Get the M/Z value for b-17-ion on the main citrullinated spectrum
                    double citMz = mainCitSpectrum.SpectrumPossibleBm17Ions[i];
                    // Get the M/Z value for b-17-ion on the complementary arginine spectrum
                    double artMz = complArgSpectrum.SpectrumPossibleBm17Ions[i];
                    // If the m/z-values has the right shift. Increment the ion count by 1
                    if (HasCorrectCitMassShift(citMz, artMz, fragMassError, i >= citIndex)) b17IonCount++;
                }

                // Check that both spectra contains the B-18-ion at this specific amino acid
                if (mainCitSpectrum.B18IonIndex.Contains(i) && complArgSpectrum.B18IonIndex.Contains(i))
                {
                    /* Calculate B-18-ion */
                    // Get the M/Z value for b-ion on the main citrullinated spectrum
                    double citMz = mainCitSpectrum.SpectrumPossibleBm18Ions[i];
                    // Get the M/Z value for b-ion on the complementary arginine spectrum
                    double argMz = complArgSpectrum.SpectrumPossibleBm18Ions[i];
                    // If the m/z-values has the right shift. Increment the ion count by 1
                    if (HasCorrectCitMassShift(citMz, argMz, fragMassError, i >= citIndex)) b18IonCount++;
                }

                // Check that both spectra contains the Y-ion at this specific amino acid
                if (mainCitSpectrum.YIonIndex.Contains(i) && complArgSpectrum.YIonIndex.Contains(i))
                {
                    /* Calculate Y-ion */
                    // Get the M/Z value for y-ion on the main citrullinated spectrum
                    double citMz = mainCitSpectrum.SpectrumPossibleYIons[i];
                    // Get the M/Z value for y-ion on the complementary arginine spectrum
                    double argMz = complArgSpectrum.SpectrumPossibleYIons[i];
                    // If the m/z-values has the right shift. Increment the ion count by 1
                    if (HasCorrectCitMassShift(citMz, argMz, fragMassError, i <= citIndex)) yIonCount++;
                }
                // Check that both spectra contains the B-ion at this specific amino acid
                if (mainCitSpectrum.Y17IonIndex.Contains(i) && complArgSpectrum.Y17IonIndex.Contains(i))
                {
                    /* Calculate Y-ion */
                    // Get the M/Z value for y-18-ion on the main citrullinated spectrum
                    double citMz = mainCitSpectrum.SpectrumPossibleYm17Ions[i];
                    // Get the M/Z value for y-ion on the complementary arginine spectrum
                    double argMz = complArgSpectrum.SpectrumPossibleYm17Ions[i];
                    // If the m/z-values has the right shift. Increment the ion count by 1
                    if (HasCorrectCitMassShift(citMz, argMz, fragMassError, i <= citIndex)) y17IonCount++;
                }

                if (mainCitSpectrum.Y18IonIndex.Contains(i) && complArgSpectrum.Y18IonIndex.Contains(i))
                {
                    /* Calculate Y-18-ion */
                    // Get the M/Z value for y-18-ion on the main citrullinated spectrum
                    double citMz = mainCitSpectrum.SpectrumPossibleYm18Ions[i];
                    // Get the M/Z value for y-18-ion on the complementary arginine spectrum
                    double argMz = complArgSpectrum.SpectrumPossibleYm18Ions[i];
                    // If the m/z-values has the right shift. Increment the ion count by 1.
                    if (HasCorrectCitMassShift(citMz, argMz, fragMassError, i <= citIndex)) y18IonCount++;
                }
            }

            /* Calculate the score */
            double score = aIonCount * ScoreSettings.AIonMatchScore;
            score += bIonCount * ScoreSettings.BIonMatchScore;
            score += b17IonCount * (ScoreSettings.BIonMatchScore / ScoreSettings.LossMatchScoreDivider);
            score += b18IonCount * (ScoreSettings.BIonMatchScore / ScoreSettings.LossMatchScoreDivider);
            score += yIonCount * ScoreSettings.YIonMatchScore;
            score += y17IonCount * (ScoreSettings.YIonMatchScore / ScoreSettings.LossMatchScoreDivider);
            score += y18IonCount * (ScoreSettings.YIonMatchScore / ScoreSettings.LossMatchScoreDivider);
            // Round the score to x.
            score = Math.Round(score, 1);

            // Set the score
            complArgSpectrum.MatchScore = score;

            // Return the match score
            return complArgSpectrum;
        }
        /// <summary>
        /// Score the match between the main arginine spectrum and the complementary citrullinated spectrum.
        /// </summary>
        /// <param name="mainArgSpectrum">The main arginine spectrum.</param>
        /// <param name="complCitSpectrum">The complementary citrullinated spectrum.</param>
        /// <returns>The complementary spectrum with the match score.</returns>
        internal static MgxScan ScoreMatchMainArgComplCit(XTSpectrum mainArgSpectrum, MgxScan complCitSpectrum)
        {
            // Create a temporary variables for the counts
            int aIonCount = 0;
            int bIonCount = 0;
            int b17IonCount = 0;
            int b18IonCount = 0;
            int yIonCount = 0;
            int y17IonCount = 0;
            int y18IonCount = 0;

            int citIndex = GetIndexOfCitrullinationFromArg(mainArgSpectrum);
            // Get the mass fragment error
            double fragMassError = double.Parse(External.XTandemInput.FMError, FileUtilities.NumberFormat);
            // Loop through the all of amino acid sequence. However only the index is used.
            for (int i = 0; i < mainArgSpectrum.SpectrumAminoAcidSequence.Length; i++)
            {
                // Check that both spectra contains the A-ion at this specific amino acid
                if (complCitSpectrum.AIonIndex.Contains(i) && mainArgSpectrum.AIonIndex.Contains(i))
                {
                    /* Calculate A-ion */
                    // Get the M/Z value for A-ion on the complementary citrullinated spectrum
                    double citMz = complCitSpectrum.SpectrumPossibleAIons[i];
                    // Get the M/Z value for b-ion on the main arginine spectrum
                    double argMz = mainArgSpectrum.SpectrumPossibleAIons[i];
                    // If the m/z-values has the right shift. Increment the ion count by 1
                    if (HasCorrectCitMassShift(citMz, argMz, fragMassError, i >= citIndex)) aIonCount++;
                }

                // Check that both spectra contains the B-ion at this specific amino acid
                if (complCitSpectrum.BIonIndex.Contains(i) && mainArgSpectrum.BIonIndex.Contains(i))
                {
                    /* Calculate B-ion */
                    // Get the M/Z value for b-ion on the complementary citrullinated spectrum
                    double citMz = complCitSpectrum.SpectrumPossibleBIons[i];
                    // Get the M/Z value for b-ion on the main arginine spectrum
                    double argMz = mainArgSpectrum.SpectrumPossibleBIons[i];
                    // If the m/z-values has the right shift. Increment the ion count by 1
                    if (HasCorrectCitMassShift(citMz, argMz, fragMassError, i >= citIndex)) bIonCount++;
                }

                // Check that both spectra contains the B-17-ion at this specific amino acid
                if (complCitSpectrum.B17IonIndex.Contains(i) && mainArgSpectrum.B17IonIndex.Contains(i))
                {
                    /* Calculate B-17-ion */
                    // Get the M/Z value for b-17-ion on the complementary citrullinated spectrum
                    double citMz = complCitSpectrum.SpectrumPossibleBm17Ions[i];
                    // Get the M/Z value for b-17-ion on the main arginine spectrum
                    double artMz = mainArgSpectrum.SpectrumPossibleBm17Ions[i];
                    // If the m/z-values has the right shift. Increment the ion count by 1
                    if (HasCorrectCitMassShift(citMz, artMz, fragMassError, i >= citIndex)) b17IonCount++;
                }

                // Check that both spectra contains the B-18-ion at this specific amino acid
                if (complCitSpectrum.B18IonIndex.Contains(i) && mainArgSpectrum.B18IonIndex.Contains(i))
                {
                    /* Calculate B-18-ion */
                    // Get the M/Z value for b-ion on the complementary citrullinated spectrum
                    double citMz = complCitSpectrum.SpectrumPossibleBm18Ions[i];
                    // Get the M/Z value for b-ion on the main arginine spectrum
                    double argMz = mainArgSpectrum.SpectrumPossibleBm18Ions[i];
                    // If the m/z-values has the right shift. Increment the ion count by 1
                    if (HasCorrectCitMassShift(citMz, argMz, fragMassError, i >= citIndex)) b18IonCount++;
                }

                // Check that both spectra contains the Y-ion at this specific amino acid
                if (complCitSpectrum.YIonIndex.Contains(i) && mainArgSpectrum.YIonIndex.Contains(i))
                {
                    /* Calculate Y-ion */
                    // Get the M/Z value for y-ion on the complementary citrullinated spectrum
                    double citMz = complCitSpectrum.SpectrumPossibleYIons[i];
                    // Get the M/Z value for y-ion on the main arginine spectrum
                    double argMz = mainArgSpectrum.SpectrumPossibleYIons[i];
                    // If the m/z-values has the right shift. Increment the ion count by 1
                    if (HasCorrectCitMassShift(citMz, argMz, fragMassError, i <= citIndex)) yIonCount++;
                }
                // Check that both spectra contains the B-ion at this specific amino acid
                if (complCitSpectrum.Y17IonIndex.Contains(i) && mainArgSpectrum.Y17IonIndex.Contains(i))
                {
                    /* Calculate Y-ion */
                    // Get the M/Z value for y-18-ion on the complementary citrullinated spectrum
                    double citMz = complCitSpectrum.SpectrumPossibleYm17Ions[i];
                    // Get the M/Z value for y-ion on the main arginine spectrum
                    double argMz = mainArgSpectrum.SpectrumPossibleYm17Ions[i];
                    // If the m/z-values has the right shift. Increment the ion count by 1
                    if (HasCorrectCitMassShift(citMz, argMz, fragMassError, i <= citIndex)) y17IonCount++;
                }

                if (complCitSpectrum.Y18IonIndex.Contains(i) && mainArgSpectrum.Y18IonIndex.Contains(i))
                {
                    /* Calculate Y-18-ion */
                    // Get the M/Z value for y-18-ion on the complementary citrullinated spectrum
                    double citMz = complCitSpectrum.SpectrumPossibleYm18Ions[i];
                    // Get the M/Z value for y-18-ion on the main arginine spectrum
                    double argMz = mainArgSpectrum.SpectrumPossibleYm18Ions[i];
                    // If the m/z-values has the right shift. Increment the ion count by 1.
                    if (HasCorrectCitMassShift(citMz, argMz, fragMassError, i <= citIndex)) y18IonCount++;
                }
            }

            /* Calculate the score */
            double score = aIonCount * ScoreSettings.AIonMatchScore;
            score += bIonCount * ScoreSettings.BIonMatchScore;
            score += b17IonCount * (ScoreSettings.BIonMatchScore / ScoreSettings.LossMatchScoreDivider);
            score += b18IonCount * (ScoreSettings.BIonMatchScore / ScoreSettings.LossMatchScoreDivider);
            score += yIonCount * ScoreSettings.YIonMatchScore;
            score += y17IonCount * (ScoreSettings.YIonMatchScore / ScoreSettings.LossMatchScoreDivider);
            score += y18IonCount * (ScoreSettings.YIonMatchScore / ScoreSettings.LossMatchScoreDivider);
            // Round the score to x.
            score = Math.Round(score, 1);

            // Set the score
            complCitSpectrum.MatchScore = score;

            // Return the match score
            return complCitSpectrum;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Get the closset N/Q and R amino acid range.
        /// </summary>
        /// <param name="aaSeq">The aa sequences..</param>
        /// <param name="aaIndexes">The array of indexes of the amino acid between N/Q and R amino acid as out parameter.</param>
        private static void GetClossetNQRAaRange(string[] aaSeq, out int[] aaIndexes)
        {
            // Set the out parameters to default values
            int argIndex = -1;
            int nqIndex = -1;
            int distance = int.MaxValue;
            // Get the amino acid sequence as a string to better get the index of the amino acid
            string aaSequence = string.Join("", aaSeq);

            // Get the index of the arginine
            List<int> argIndexes = aaSequence.AllIndexesOf("R");
            // Get the indexes of N and Q and combine the arrays
            List<int> nqIndexes = aaSequence.AllIndexesOf("N");
            nqIndexes.AddRange(aaSequence.AllIndexesOf("Q"));

            // Loop through all of the arginine indexes
            foreach (int argIn in argIndexes)
            {
                // Loop through all of the N/Q indexes
                foreach (int nqIn in nqIndexes)
                {
                    // If the current distance is less than set min distance, set the new one
                    if (Math.Abs(argIn - nqIn) < distance)
                    {
                        argIndex = argIn;
                        nqIndex = nqIn;
                        distance = Math.Abs(argIn - nqIn);
                    }
                }
            }
            // Get the starting index
            int startIndex;

            if (argIndex < nqIndex)
            {
                startIndex = argIndex;
            }
            else
            {
                startIndex = nqIndex;
            }

            // Create an array of indexes
            aaIndexes = Enumerable.Range(startIndex, distance + 1).ToArray();
        }

        /// <summary>
        /// Get the index of the citrullination in a citrullinated spectrum.
        /// </summary>
        /// <param name="spectrum">The spectrum.</param>
        /// <returns>The index of the citrullination.</returns>
        private static int GetIndexOfCitrullination(XTSpectrum spectrum)
        {
            // Loop trough all of the modifications in the spectrum
            foreach (KeyValuePair<int, double> mod in spectrum.SpectrumBIonModDict)
            {
                // Check that the modification mass change equals the citrullination. If it does, return the key
                if(mod.Value == 0.984)
                {
                    return mod.Key;
                }
            }
            // If no citrullinations was found, return -1
            return -1;
        }
        /// <summary>
        /// Get the index of the citrullination in a arginine spectrum. Returns a qualified guess.
        /// </summary>
        /// <param name="spectrum">The spectrum.</param>
        /// <returns>The index of the citrullination.</returns>
        private static int GetIndexOfCitrullinationFromArg(XTSpectrum spectrum)
        {
            string aaSeq = string.Join("", spectrum.SpectrumAminoAcidSequence);
            return aaSeq.IndexOf('R');
        }
        /// <summary>
        /// Check weather the spectrum has the correct mass shift.
        /// </summary>
        /// <param name="citMz">The m/z value for the citrullinated spectrum.</param>
        /// <param name="argMz">The m/z value for the arginine spectrum.</param>
        /// <param name="fragMassError">The fragment mass error. Used to calculate the tolerance.</param>
        /// <param name="containsCit">Indicates that the ion contains the citrullination.</param>
        /// <returns>True, if has correct mass shift; Otherwise, false.</returns>
        private static bool HasCorrectCitMassShift(double citMz, double argMz, double fragMassError, bool containsCit)
        {
            // Determine whether the ion contains the citrullination
            if (containsCit)
            {
                // Citrullination. The cit M/Z should be 0.984 heavier than Arg M/Z
                return HelperUtilities.IsApproximately(citMz - 0.984, argMz, fragMassError / 5);
            }
            else
            {
                // No citrullination. The cit M/Z and Arg M/Z should be equal
                return HelperUtilities.IsApproximately(citMz, argMz, fragMassError / 5);
            }
        }

        /// <summary>
        /// Check if iso cyanic acid loss is present in the spectrum's MS1 scan.
        /// </summary>
        /// <param name="spectrum">The spectrum to be checked.</param>
        /// <returns>True, if isocyanic acid is present; Otherwise, false.</returns>
        private static bool IsIsoCyanicAcidLossPresentMs1(MsSpectrum spectrum)
        {
            // Get the precursor m/z value
            double precursorMz = spectrum.PreCursorMz;
            // Loop through all of the M/Z-values
            foreach (double mzVal in spectrum.ParentScan.MzValues)
            {
                // Check that the m/z is not greater than the precursor, because the code is looking for an loss i.e. a value less than the precursor
                // TODO: Maybe insert to increase speed. 
                //if(mzVal > precursorMz) { break; }

                // Check if the isocyanic neutral loss is present.
                if (HelperUtilities.IsApproximately(mzVal, precursorMz - 43, ScoreSettings.CyanicLossTolerance))
                {
                    // The isocyanic netural loss ion is found
                    // If that is the case set the M/Z value of the ion
                    spectrum.IsoCyanicMzMs1 = mzVal;
                    // Return true, because the ion was found
                    return true;
                }

            }

            // No loss was found. Set the m/z value for the loss ion to an impossible value
            spectrum.IsoCyanicMzMs1 = -1;
            // Return false, because no ion was found
            return false;


        }

        /// <summary>
        /// Count the presence of isocyanic acid loss on MS2.
        /// </summary>
        /// <param name="citSpectrum">The citrullinated spectrum.</param>
        /// <returns>The number of isocyanic acid losses.</returns>
        private static double CountIsoCyanicAcidLossPresentMs2(XTSpectrum citSpectrum)
        {
            return CountIsoCyanicAcidLossPresentMs2(citSpectrum, GetIndexOfCitrullination(citSpectrum), citSpectrum.SpectrumAminoAcidSequence);
        }

        /// <summary>
        /// Count the presence of isocyanic acid loss on MS2.
        /// </summary>
        /// <param name="citSpectrum">The citrullinated spectrum.</param>
        /// <param name="citIndex">The index of the citrullinated arginine.</param>
        /// <param name="aaSeq">The amino acid sequence.</param>
        /// <returns>The number of isocyanic acid losses.</returns>
        private static double CountIsoCyanicAcidLossPresentMs2(MsSpectrum citSpectrum, int citIndex, string[] aaSeq)
        {
            // Create temporay list of isocyanic loss M/Z
            List<double> cyanlossMz = new List<double>();

            // Create scores
            int aIonCount = 0;
            int bIonCount = 0;
            int b17IonCount = 0;
            int b18IonCount = 0;
            int yIonCount = 0;
            int y17IonCount = 0;
            int y18IonCount = 0;

            List<double> mzValues = citSpectrum.MzValues.ToList();
            mzValues.Reverse();
            foreach (double mzVal in mzValues)
            {
                // Loop through the all of amino acid sequence.However only the index is used.
                for (int i = 0; i < aaSeq.Length; i++)
                {
                    if(IsoListContains(cyanlossMz, mzVal)) continue;
                    // Check that the this is the citrullinated a- or b-ions. The aa index is greater than or equal to the index of the citrullination.
                    if (i >= citIndex)
                    {
                        // Check that both spectra contains the A-ion at this specific amino acid
                        if (citSpectrum.AIonIndex.Contains(i))
                        {
                        /* Calculate A-ion */
                        // Get the M/Z value for A-ion on the complementary citrullinated spectrum
                        double citMz = citSpectrum.SpectrumPossibleAIons[i];
                        // If the spectrum has an ion that is 43 Da less than the cit spectrum, add the spectrum m/z to the list
                            if (HelperUtilities.IsApproximately(mzVal, citMz - 43, ScoreSettings.CyanicLossTolerance))
                            {
                                cyanlossMz.Add(mzVal);
                                aIonCount++;
                                continue;
                            }
                        }

                        // Check that both spectra contains the B-ion at this specific amino acid
                        if (citSpectrum.BIonIndex.Contains(i))
                        {
                            /* Calculate B-ion */
                            // Get the M/Z value for B-ion on the complementary citrullinated spectrum
                            double citMz = citSpectrum.SpectrumPossibleBIons[i];
                            // If the spectrum has an ion that is 43 Da less than the cit spectrum, add the spectrum m/z to the list
                            if (HelperUtilities.IsApproximately(mzVal, citMz - 43, ScoreSettings.CyanicLossTolerance))
                            {
                                cyanlossMz.Add(mzVal);
                                bIonCount++;
                                continue;
                            }
                        }

                        // Check that both spectra contains the B-17 ion at this specific amino acid
                        if (citSpectrum.B17IonIndex.Contains(i))
                        {
                            /* Calculate B-17 ion */
                            // Get the M/Z value for B-17 ion on the complementary citrullinated spectrum
                            double citMz = citSpectrum.SpectrumPossibleBm17Ions[i];
                            // If the spectrum has an ion that is 43 Da less than the cit spectrum, add the spectrum m/z to the list
                            if (HelperUtilities.IsApproximately(mzVal, citMz - 43, ScoreSettings.CyanicLossTolerance))
                            {
                                cyanlossMz.Add(mzVal);
                                b17IonCount++;
                                continue;
                            }
                        }

                        // Check that both spectra contains the B-18 ion at this specific amino acid
                        if (citSpectrum.B18IonIndex.Contains(i))
                        {
                            /* Calculate B-18 ion */
                            // Get the M/Z value for B-18 ion on the complementary citrullinated spectrum
                            double citMz = citSpectrum.SpectrumPossibleBm18Ions[i];
                            // If the spectrum has an ion that is 43 Da less than the cit spectrum, add the spectrum m/z to the list
                            if (HelperUtilities.IsApproximately(mzVal, citMz - 43, ScoreSettings.CyanicLossTolerance))
                            {
                                cyanlossMz.Add(mzVal);
                                b18IonCount++;
                                continue;
                            }
                        }
                    }
                    // Check that the this is the citrullinated y-ion. The aa index is less than or equal to the index of the citrullination.
                    if (i <= citIndex)
                    {
                        // Check that both spectra contains the Y-ion at this specific amino acid
                        if (citSpectrum.YIonIndex.Contains(i))
                        {
                            /* Calculate Y-ion */
                            // Get the M/Z value for Y-ion on the complementary citrullinated spectrum
                            double citMz = citSpectrum.SpectrumPossibleYIons[i];
                            // If the spectrum has an ion that is 43 Da less than the cit spectrum, add the spectrum m/z to the list
                            if (HelperUtilities.IsApproximately(mzVal, citMz - 43, ScoreSettings.CyanicLossTolerance))
                            {
                                cyanlossMz.Add(mzVal);
                                yIonCount++;
                                continue;
                            }
                        }   

                        // Check that both spectra contains the Y-17 ion at this specific amino acid
                        if (citSpectrum.Y17IonIndex.Contains(i))
                        {
                            /* Calculate Y-17 ion */
                            // Get the M/Z value for Y-17 ion on the complementary citrullinated spectrum
                            double citMz = citSpectrum.SpectrumPossibleYm17Ions[i];
                            // If the spectrum has an ion that is 43 Da less than the cit spectrum, add the spectrum m/z to the list
                            if (HelperUtilities.IsApproximately(mzVal, citMz - 43, ScoreSettings.CyanicLossTolerance))
                            {
                                cyanlossMz.Add(mzVal);
                                y17IonCount++;
                                continue;
                            }
                        }

                        // Check that both spectra contains the Y-18 ion at this specific amino acid
                        if (citSpectrum.Y18IonIndex.Contains(i))
                        {
                            /* Calculate Y-18 ion */
                            // Get the M/Z value for Y-18 ion on the complementary citrullinated spectrum
                            double citMz = citSpectrum.SpectrumPossibleYm18Ions[i];
                            // If the spectrum has an ion that is 43 Da less than the cit spectrum, add the spectrum m/z to the list
                            if (HelperUtilities.IsApproximately(mzVal, citMz - 43, ScoreSettings.CyanicLossTolerance))
                            {
                                cyanlossMz.Add(mzVal);
                                y18IonCount++;
                                continue;
                            }
                        }
                    }
                }
            }

            // Set collection
            citSpectrum.IsoCyanicLossMzMs2 = cyanlossMz.ToArray();
            // Calculate score
            double score = aIonCount * ScoreSettings.MS2IsoCyanLossAScore;
            score += bIonCount * ScoreSettings.MS2IsoCyanLossBScore;
            score += b17IonCount * (ScoreSettings.MS2IsoCyanLossBScore / ScoreSettings.MS2IsoCyanOnLossScoreDivider);
            score += b18IonCount * (ScoreSettings.MS2IsoCyanLossBScore / ScoreSettings.MS2IsoCyanOnLossScoreDivider);
            score += yIonCount * ScoreSettings.MS2IsoCyanLossYScore;
            score += y17IonCount * (ScoreSettings.MS2IsoCyanLossYScore / ScoreSettings.MS2IsoCyanOnLossScoreDivider);
            score += y18IonCount * (ScoreSettings.MS2IsoCyanLossYScore / ScoreSettings.MS2IsoCyanOnLossScoreDivider);
            // TODO: Set score instead of count
            //return cyanlossMz.Count;
            return score;
        }

        /// <summary>
        /// Check if list of isocyanic loss contains a certain m/z-value.
        /// </summary>
        /// <param name="cyanlossMz">The list of isocyanic losses.</param>
        /// <param name="mzVal">The M/Z-value to be checked.</param>
        /// <returns>True, if list contains the iso cyanic loss; Otherwise, false.</returns>
        private static bool IsoListContains(List<double> cyanlossMz, double mzVal)
        {
            // Loop through all of the values
            foreach (double cyanLossMz in cyanlossMz)
            {
                // If an loss m/z is approx. equal to the M/Z-value to be checked, return true
                if (HelperUtilities.IsApproximately(cyanLossMz + 43, mzVal, ScoreSettings.CyanicLossTolerance)) return true;
            }

            // If the item is not found
            return false;
        }
        #endregion

        #region Quantification
        /// <summary>
        /// Add the Cit Spectra to quantification.
        /// </summary>
        /// <param name="scoredSpectra">The spectra to be added.</param>
        private static void AddCitSpectraAboveCutoffToQuantification(Dictionary<XTSpectrum, List<XTSpectrum>> scoredSpectra)
        {
            // Loop through all of the spectra
            foreach (KeyValuePair<XTSpectrum, List<XTSpectrum>> spectrum in scoredSpectra)
            {
                // Check that the CitScore is greater than the CutOff value
                if (spectrum.Key.CitScore >= ScoreSettings.CutOffScore)
                {
                    // If the spectrum passes, add the spectrum to the quantification together with the complementary spectrum with the highest match score
                    Quantification.AddTokvpCitSpecSpecDict(spectrum.Key, spectrum.Value[0]);
                    // Add the spectrum ID to the list
                    Validation.citChosenForQuantList.Add(spectrum.Key.ID);
                }
            }
        }

        /// <summary>
        /// Add the Arg Spectra to quantification.
        /// </summary>
        /// <param name="scoredSpectra">The spectra to be added.</param>
        private static void AddArgSpectraAboveCutOffToQuantification(Dictionary<XTSpectrum, List<RawScan>> scoredSpectra)
        {
            // Loop through all of the spectra
            foreach (KeyValuePair<XTSpectrum, List<RawScan>> spectrum in scoredSpectra)
            {
                // Loop through all of the complementary cit spectra
                foreach (RawScan complSpectra in spectrum.Value)
                {
                    // Check that the CitScore is greater than the CutOff value
                    if (complSpectra.CitScore >= ScoreSettings.CutOffScore && Quantification.argSpecScanDict.ContainsKey(spectrum.Key) == false)
                    {
                        // If the spectrum passes, add the spectrum to the quantification together with the arginine spectra
                        Quantification.AddTokvpArgSpecScanDict(spectrum.Key, complSpectra);
                        Validation.argChosenForQuantList.Add(spectrum.Key.ID);
                    }
                }

            }
        }

        /// <summary>
        /// Add the Lone Cit Spectra to quantification.
        /// </summary>
        /// <param name="scoredSpectra">The spectra to be added.</param>
        private static void AddLoneCitSpectraAboveCutoffToQunatification(List<XTSpectrum> scoredSpectra)
        {
            // Loop through the spectra
            foreach (XTSpectrum spectrum in scoredSpectra)
            {
                // Check that the CitScore is greater than the CutOff value
                if (spectrum.CitScore >= ScoreSettings.CutOffScore)
                {
                    // If the spectrum passes, add the spectrum to the quantification
                    Quantification.AddToLoneCitSpecList(spectrum);
                    Validation.loneCitChosenForQuantList.Add(spectrum.ID);
                }
            }
        }
        #endregion
    }
}
