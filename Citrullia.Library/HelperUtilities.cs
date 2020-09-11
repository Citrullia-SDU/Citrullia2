using System;
using System.Collections.Generic;
using System.Linq;

namespace Citrullia
{
    /// <summary>A static utlitity class with helper functions.</summary>
    internal static class HelperUtilities
    {
        /// <summary>
        /// Get all the indexes of a string.
        /// </summary>
        /// <param name="str">The string to be searched.</param>
        /// <param name="searchstring">The search string.</param>
        /// <returns>The collection of indicies.</returns>
        private static IEnumerable<int> AllIndexesCollectionOf(string str, string searchstring)
        {
            int minIndex = str.IndexOf(searchstring);
            while (minIndex != -1)
            {
                yield return minIndex;
                minIndex = str.IndexOf(searchstring, minIndex + searchstring.Length);
            }
        }

        /// <summary>
        /// Get all the indexes of a string.
        /// </summary>
        /// <param name="str">The string to be searched.</param>
        /// <param name="searchString">The search string.</param>
        /// <returns>The array of indicies.</returns>
        internal static List<int> AllIndexesOf(this string str, string searchString)
        {
            return AllIndexesCollectionOf(str, searchString).ToList();
        }

        /// <summary>
        /// Check that <paramref name="a"/> and <paramref name="tolerance"/> are approximately equal within a <paramref name="tolerance"/>. 
        /// </summary>
        /// <param name="a">Value A.</param>
        /// <param name="b">Value B.</param>
        /// <param name="tolerance">The tolerence.</param>
        /// <returns>True, if <paramref name="a"/> and <paramref name="b"/> are equal within <paramref name="tolerance"/>; Otherwise,false</returns>
        internal static bool IsApproximately(double a, double b, double tolerance)
        {
            return Math.Abs(a - b) < tolerance;
        }

        /// <summary>
        /// Create the AA array from an sequence.
        /// TODO: Can be done in a more simple code.
        /// </summary>
        /// <param name="aaSeq">The aa sequence.</param>
        /// <returns>The AA array.</returns>
        internal static string[] CreateAAArray(string aaSeq)
        {
            // Create an empty list for holding the aa string
            List<string> aaList = new List<string>();

            // Loop through all of the aa chars and add them to the list
            foreach (char aa in aaSeq)
            {
                aaList.Add(aa.ToString());
            }

            // Return the list as an array
            return aaList.ToArray();
        }
    }
}
