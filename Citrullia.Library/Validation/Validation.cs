using Citrullia.Library.MassSpectra;
using Citrullia.Library.SpectraHandling;
using Citrullia.Library.XTandem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Citrullia.Library.Validation
{
    /// <summary>
    /// Utilities class for validations.
    /// </summary>
    internal static class Validation
    {
        /// <summary>List of citrullination spectra validation.</summary>
        internal static List<ValidationResult> citValResults = new List<ValidationResult>();
        /// <summary>List of arginine spectra validation.</summary>
        internal static List<ValidationResult> argValResults = new List<ValidationResult>();
        /// <summary>List of lonely citrullination spectra validation.</summary>
        internal static List<ValidationResult> loneCitValResults = new List<ValidationResult>();

        /// <summary>List of IDs for citrullination spectra chosen for quantification.</summary>
        internal static List<int> citChosenForQuantList = new List<int>();
        /// <summary>List of IDs for arginine spectra chosen for quantification.</summary>
        internal static List<int> argChosenForQuantList = new List<int>();
        /// <summary>List of IDs for lonely citrullination spectra chosen for quantification.</summary>
        internal static List<int> loneCitChosenForQuantList = new List<int>();

        #region Citrullination spectra
        /// <summary>
        /// Find citrullinated spectra.
        /// </summary>
        internal static void FindCitrullinatedSpectra()
        {
            // Create an empty list of validation results
            var vResultList = new List<ValidationResult>();
            // Set the search parameters. Amino acid and mass change
            string residue = "R";
            double mChange = 0.984;
            // Loop through the list of X!Tandem search results
            foreach (XTResult res in FileReader.XTandemSearchResults)
            {
                // Create an empty instance of validation result
                ValidationResult vResult = new ValidationResult();
                // Create a empty list of spectra
                var specModPeptides = new List<XTSpectrum>();
                // Set the current X!Tandem result as the parent result of the validation result
                vResult.ParentResult = res;

                // Loop through each of the spectra in the result
                foreach (XTSpectrum spec in res.XSpectrums)
                {
                    // Loop through each of the proteins in the spectrum
                    foreach (XTProtein prot in spec.Proteins)
                    {
                        // Loop through each of the modification in the protein
                        foreach (XTModification mod in prot.Modifications)
                        {
                            // Check that the modification matches the citrullination
                            if (mod.AminoAcid == residue & mod.MassChange == mChange)
                            {
                                // If so.
                                // Calculate the ions
                                XTSpectrum scanMS2 = IonCalculation.CalculateAllIons(spec);
                                // Get the original filename
                                scanMS2 = ReturnOriginalFileNameSpectrum(scanMS2, vResult);
                                // Get the original scan ID
                                scanMS2 = GetOriginalScanID(scanMS2);
                                // Find the original MS1 informaton
                                scanMS2 = FindMS1.FindOriginalMS1Info(scanMS2);
                                // Handle orphan spectra
                                scanMS2 = FindMS1.HandleOrphanSpecs(scanMS2);
                                // Add the spectra to the list of modified spectra
                                specModPeptides.Add(scanMS2);
                            }
                        }
                    }
                }
                // Set the modified spectra to the temporary list filled with data
                vResult.ModifiedSpectra = specModPeptides;
                // Add the result to the result list
                vResultList.Add(vResult);
            }
            // Set the result
            ReturnVResultCit(vResultList);
        }

        /// <summary>
        /// Find complementary arginine spectra.
        /// </summary>
        /// <param name="error">The maximum parent mass error allowed.</param>
        internal static void FindComplementaryArginineSpectra(double error)
        {
            // Set the mass change
            double mChange = 0.984;
            // Set the validation results
            var vResultList = citValResults;
            // Loop through the validation results
            foreach (ValidationResult result in vResultList)
            {
                // Create a temporary dictionary of complementary spectra
                Dictionary<XTSpectrum, List<XTSpectrum>> compPepDic = new Dictionary<XTSpectrum, List<XTSpectrum>>();
                // Loop through all of the modified spectra
                foreach (XTSpectrum specMod in result.ModifiedSpectra)
                {
                    // Create an temporary list of complementary spectra
                    var compPeptides = new List<XTSpectrum>();
                    // Loop through all of the complementary validation result
                    foreach (ValidationResult compResult in vResultList)
                    {
                        // TODO: Understand Can be taken out of compResult loop if necessary:??
                        // Loop through al of the spectra in the complementary result's parent
                        foreach (XTSpectrum spec in compResult.ParentResult.XSpectrums)
                        {
                            // Calculate the parent mass error
                            double delta = Math.Abs(spec.ParentMass - (specMod.ParentMass - mChange));
                            // Check if the parent mass error is less than the max allowed error AND the peptide sequence match
                            if (delta <= error & spec.Proteins[0].DoaminSeq == specMod.Proteins[0].DoaminSeq)
                            {
                                // Calculate the ions
                                XTSpectrum scanMS2 = IonCalculation.CalculateAllIons(spec);
                                // Get the original filename 
                                scanMS2 = ReturnOriginalFileNameSpectrum(scanMS2, compResult);
                                // Get the original scan ID
                                scanMS2 = GetOriginalScanID(scanMS2);
                                // Find the original MS1 information
                                scanMS2 = FindMS1.FindOriginalMS1Info(scanMS2);
                                // Handle the orphan spetra
                                scanMS2 = FindMS1.HandleOrphanSpecs(scanMS2);
                                // Add the complementary spectra to the temporary list
                                compPeptides.Add(scanMS2);
                            }
                        }

                    }
                    // If no complementary spectra has been found, continue
                    if (compPeptides.Count() == 0)
                    {
                        continue;
                    }
                    else
                    {
                        // If the dictionary of compmentary spectra does not have the modified spectra, add it to the dictionary
                        if (compPepDic.ContainsKey(specMod) == false)
                        {
                            compPepDic.Add(specMod, compPeptides);
                        }
                    }
                }
                // Set the complementary spectra dictionary to the result's spectra dicitonary
                result.CitSpectraDictionary = compPepDic;
            }
            ReturnVResultCit(vResultList);
        }
        private static List<ValidationResult> ReturnVResultCit(List<ValidationResult> vResultList)
        {
            citValResults = vResultList;
            return citValResults;
        }

        /// <summary>
        /// Set the original filename to the MS2 spectrum.
        /// </summary>
        /// <param name="specMS2">The MS2 spectrum.</param>
        /// <param name="result">The validation result.</param>
        /// <returns>The MS2 spectrum with the original filename.</returns>
        private static XTSpectrum ReturnOriginalFileNameSpectrum(XTSpectrum specMS2, ValidationResult result)
        {
            specMS2.OriginalFileName = result.ParentResult.OriginalInputFile;
            return specMS2;
        }
        #endregion

        #region Arginine spectra
        /// <summary>
        /// Find the arginine spectra.
        /// </summary>
        internal static void FindArginineSpectra()
        {
            // Create a temporary list of validation results
            var vResultList = new List<ValidationResult>();
            // Set the residue and mass change for the citrullination
            var residue = "R";
            double mChange = 0.984;
            // Loop through all of the X!Tandem results
            foreach (XTResult res in FileReader.XTandemSearchResults)
            {
                // Create an empty validation result
                ValidationResult vResult = new ValidationResult();
                // Create an temporary list of arginine spectra
                var specArgPeptides = new List<XTSpectrum>();
                // Set the X!Tandem result as the parent result of the validation
                vResult.ParentResult = res;

                Console.WriteLine("Spectras of result: {0}", res.XSpectrums.Count());
                // Loop through all of the spectra in the X!Tandem result
                foreach (XTSpectrum spec in res.XSpectrums)
                {
                    Console.WriteLine("Proteins of Spectrum: {0}", spec.Proteins.Count());
                    // Loop through all of the proteins in the spectrum
                    foreach (XTProtein prot in spec.Proteins)
                    {
                        // Create an indicator for weather or not the spectrum contains a citrullination. Default: False
                        bool containsCit = false;
                        // Loop through all of the protein modifications
                        foreach (XTModification mod in prot.Modifications)
                        {
                            // Check if the modification matches the citrullination. If so set the citrullination indicator to true
                            if (mod.AminoAcid == residue & mod.MassChange == mChange)
                            {
                                containsCit = true;
                            }
                        }

                        // Check if the protein has an arginine, but does not contain a citrulline.
                        if (prot.DoaminSeq.Contains(residue) & containsCit == false)
                        {
                            // Get the spectrum with the original filename
                            XTSpectrum specMS2 = ReturnOriginalFileNameSpectrum(spec, vResult);
                            // Get the original filename
                            specMS2 = GetOriginalScanID(specMS2);
                            // Find the original MS1 data
                            specMS2 = FindMS1.FindOriginalMS1Info(specMS2);
                            // Handle the orphan spectra
                            specMS2 = FindMS1.HandleOrphanSpecs(specMS2);
                            // Add the spectrum to the list of arginine spectra
                            specArgPeptides.Add(specMS2);
                        }
                    }
                }
                // Set the list of arginine spectra to the results list
                vResult.ArginineSpectra = specArgPeptides;
                // Add the result to the result list
                vResultList.Add(vResult);
                Console.WriteLine("Number of Citrullinations in Result: {0}", vResult.ArginineSpectra.Count());
            }
            // Set the result
            ReturnValResultArg(vResultList);
        }

        /// <summary>
        /// Find complementary citrullinated scans.
        /// </summary>
        /// <param name="error">The maximum parent mass error allowed.</param>
        internal static void FindComplimentaryCitrullinationScans(double error)
        {
            // Get the list of used citrullination spectra
            List<int> usedIDList = GetListOfUsedIDs();
            // Set the proton mass
            double pMass = 1.007276470;
            // Set the arginine validation result list to list of validation results
            var vResultList = argValResults;
            // Loop through all of the vlaidation results
            foreach (ValidationResult result in vResultList)
            {
                // Get the name of the inpput file
                string inputFileName = result.ParentResult.OriginalInputFile;
                // Loop through all of the input results
                foreach (InputData input in FileReader.inputFiles)
                {
                    // TODO: Can be inserted into if statement if it needs only to work on single results
                    // Create a temporary dictionary of spectra and their complementary scans
                    Dictionary<XTSpectrum, List<MgxScan>> compPepDic = new Dictionary<XTSpectrum, List<MgxScan>>();
                    // Loop through all of the arginine spectra
                    foreach (XTSpectrum specArg in result.ArginineSpectra)
                    {
                        // If the arginine spectra is already used, continue
                        if (usedIDList.Contains(specArg.ID))
                        {
                            continue;
                        }
                        else
                        {
                            // If not.
                            // Create a tmeporary list of complementary scans
                            var compPeptides = new List<MgxScan>();
                            // Calculate the ions
                            XTSpectrum scanArg = IonCalculation.CalculateAllIons(specArg);
                            // Loop through all of the MS2 scans
                            foreach (MgxScan mgfScan in input.MS2Scans)
                            {
                                // Calculate the parent mass error
                                double delta = Math.Abs(scanArg.ParentMass - (mgfScan.PreCursorMz * mgfScan.Charge - (mgfScan.Charge - 1) * pMass - 0.984));
                                // Check if the parent mass error is less than the allowed parent mass error
                                if (delta <= error)
                                {
                                    // If so.
                                    // Calculate the relative intenties values
                                    MgxScan normMgfScan = CalcRelativeIntVals(mgfScan);
                                    // Calculate all potential citrullination ions
                                    normMgfScan = IonCalculation.CalculateAllPotentialCitrullinationIons(scanArg, normMgfScan);
                                    // Get the original filename 
                                    normMgfScan = ReturnOriginalFileNameScan(normMgfScan, input);
                                    // Find the potential citrullinated parent in MS1
                                    normMgfScan = FindMS1.FindPotCitParentMS1Scan(normMgfScan);
                                    // Handle orphan scans
                                    normMgfScan = FindMS1.HandleOrphanScans(normMgfScan);
                                    // Add the scan to the list
                                    compPeptides.Add(normMgfScan);
                                }
                            }
                            // If no complementary scan has been found, continue.
                            if (compPeptides.Count() == 0)
                            {
                                continue;
                            }
                            else
                            {
                                // If the dictionary of complementary scans does not already contains the arginine spectra, add it
                                if (compPepDic.ContainsKey(scanArg) == false)
                                {
                                    compPepDic.Add(scanArg, compPeptides);
                                }
                            }
                        }
                    }
                    // Set the dictionary to the result
                    result.ArgSpectraDictionary = compPepDic;
                }
            }
            // Set the result list
            ReturnValResultArg(vResultList);
        }

        private static List<ValidationResult> ReturnValResultArg(List<ValidationResult> vResultList)
        {
            argValResults = vResultList;
            return argValResults;
        }

        /// <summary>
        /// Calculate the relative intensities.
        /// </summary>
        /// <param name="mgfScan">The scan.</param>
        /// <returns>The scan with normalised intensities.</returns>
        private static MgxScan CalcRelativeIntVals(MgxScan mgfScan)
        {
            // Get the maximum intensity in the scan
            double maxInt = mgfScan.Intencities.Max();
            // Create a temporary list of intensities
            List<double> intVals = new List<double>();
            // Create a temporary list of M/Z-values
            List<double> mzVals = new List<double>();

            // Loop through all of the intensities
            for (int i = 0; i <= mgfScan.Intencities.Length - 1; i++)
            {
                // Normalise the intensities
                double normIntVal = Math.Round(mgfScan.Intencities[i] / maxInt * 100);
                // If the normalised intensity is grater than 0, add the notmalised intensity and the current M/Z-value to their respective lists
                if (normIntVal > 0)
                {
                    intVals.Add(normIntVal);
                    mzVals.Add(mgfScan.MzValues[i]);
                }
            }
            // Set the new intensities
            mgfScan.Intencities = intVals.ToArray();
            // Set the new M/Z-values
            mgfScan.MzValues = mzVals.ToArray();
            // Return the new scan
            return mgfScan;
        }

        /// <summary>
        /// Get a list of IDs for the spectra used as complementary spectra in the citrullination validation.
        /// </summary>
        /// <returns>The list of used spectra IDs.</returns>
        private static List<int> GetListOfUsedIDs()
        {
            // Create a temporary list of ids
            List<int> listIDs = new List<int>();
            // Loop through all of citrullination validation results
            foreach (ValidationResult citResult in citValResults)
            {
                // Loop through all of the citrullinated spectra and their complementary spectra.
                foreach (KeyValuePair<XTSpectrum, List<XTSpectrum>> kvp in citResult.CitSpectraDictionary)
                {
                    // Loop through all of the complementary IDs and add the id to the list
                    foreach (XTSpectrum citSpec in kvp.Value)
                    {
                        listIDs.Add(citSpec.ID);
                    }
                }
            }
            // Return the list of ids
            return listIDs;
        }


        /// <summary>
        /// Set the original filename to the MS2 spectrum.
        /// </summary>
        /// <param name="potCitSpecMS2">The MS2 scab.</param>
        /// <param name="input">The input file.</param>
        /// <returns>The MS2 scan with the original filename.</returns>
        private static MgxScan ReturnOriginalFileNameScan(MgxScan potCitSpecMS2, InputData input)
        {
            potCitSpecMS2.OriginalFileName = input.FilePath.Split('\\').Last();
            return potCitSpecMS2;
        }
        #endregion

        #region Lonely citrullination spectra
        /// <summary>
        /// Find lonely citrullinated spectra.
        /// </summary>
        internal static void FindLonelyCitrullinatedSpectra()
        {
            // Create an temporary list of results
            var vResultList = new List<ValidationResult>();
            // Get a list of ids of already used citrullinated spectra
            var usedIDList = GetListOfUsedCitIDs();
            // Set the residue and mass change for the citrullination
            var residue = "R";
            double mChange = 0.984;
            // Loop through all of the X!Tandem results
            foreach (XTResult res in FileReader.XTandemSearchResults)
            {
                // Create an empty validation result
                ValidationResult vResult = new ValidationResult();
                // Create a temporary list of spectra
                var specModPeptides = new List<XTSpectrum>();
                // Set the X!Tandem result as the parent result of the validation
                vResult.ParentResult = res;

                // Loop through all of the spectra in the result
                foreach (XTSpectrum spec in res.XSpectrums)
                {
                    // Loop through all of the proteins in the spectra
                    foreach (XTProtein prot in spec.Proteins)
                    {
                        // Loop through all of the protein modification
                        foreach (XTModification mod in prot.Modifications)
                        {
                            // Check if the modification matches a citrulination
                            if (mod.AminoAcid == residue & mod.MassChange == mChange)
                            {
                                // If so, check if the spectra is already used. If so, continue
                                if (usedIDList.Contains(spec.ID))
                                {
                                    continue;
                                }
                                else
                                {
                                    // If not.
                                    // Calculate the ions
                                    XTSpectrum scanMS2 = IonCalculation.CalculateAllIons(spec);
                                    // Get the original filename
                                    scanMS2 = ReturnOriginalFileNameSpectrum(scanMS2, vResult);
                                    // Get the original scan ID
                                    scanMS2 = GetOriginalScanID(scanMS2);
                                    // Find the original MS1 informaton
                                    scanMS2 = FindMS1.FindOriginalMS1Info(scanMS2);
                                    // Handle orpah spectra
                                    scanMS2 = FindMS1.HandleOrphanSpecs(scanMS2);
                                    // If the list does not already contain the spectra, add it.
                                    if (specModPeptides.Contains(scanMS2) == false)
                                    {
                                        specModPeptides.Add(scanMS2);
                                    }
                                }
                            }
                        }
                    }
                }
                // Set the list of found modified spectra to the result's list of modified spectra
                vResult.ModifiedSpectra = specModPeptides;
                // Add the result to the result list
                vResultList.Add(vResult);
            }
            // Set the results
            ReturnValResultLoneCit(vResultList);
        }

        private static List<ValidationResult> ReturnValResultLoneCit(List<ValidationResult> vResultList)
        {
            loneCitValResults = vResultList;
            return loneCitValResults;
        }
        #endregion

        /// <summary>
        /// Get a list of used citrullination spectra IDs.
        /// </summary>
        /// <returns>A list of IDs for the used citrullination spectra.</returns>
        private static List<int> GetListOfUsedCitIDs()
        {
            // Create a temporary list of ids
            List<int> listIDs = new List<int>();
            // Loop through all of citrullination validation results
            foreach (ValidationResult citResult in citValResults)
            {
                // Loop through all of the citrullinated spectra and their complementary spectra and add the id of the citrullinated scan to the list
                foreach (KeyValuePair<XTSpectrum, List<XTSpectrum>> kvp in citResult.CitSpectraDictionary)
                {
                    listIDs.Add(kvp.Key.ID);
                }
            }
            // Return the list of ids
            return listIDs;
        }

        /// <summary>
        /// Get the orignal scan ID.
        /// </summary>
        /// <param name="scanMS2">The spectrum.</param>
        /// <returns>The spectrum with the original scan ID.</returns>
        private static XTSpectrum GetOriginalScanID(XTSpectrum scanMS2)
        {
            int orgID = int.Parse(scanMS2.SpectrumDescription.Split(';').First().Split(' ').Last());
            scanMS2.ID = orgID;
            return scanMS2;
        }


    }
}
