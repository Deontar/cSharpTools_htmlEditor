using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace cSharpTools
{
    /// <summary>
    /// 
    /// </summary>
    public class Testing_traductor
    {

        /// <summary>
        /// 
        /// </summary>
        public bool CheckForBlackListed(string text, string[] blackListedHtmlAtributes)
        {
            foreach (string atribute in blackListedHtmlAtributes)
            {
                if (text.Contains(atribute)) return true;
            }
            return false;
        }


        /// <summary>
        /// Encuentra el índice de un texto dentro de un arreglo de strings.<br></br>
        /// - text El texto que estás buscando.<br></br>
        /// - arrayOfStrings El arreglo donde buscar.<br></br>
        /// <returns>El índice del texto si se encuentra, o -1 si no.</returns>
        /// </summary>
        public int IndexOfTextInArray(string text, string[] arrayOfStrings)
        {
            if (text == null) throw new ArgumentNullException("text can not be null");
            if (arrayOfStrings == null) throw new ArgumentNullException("array can not be null");
            if (arrayOfStrings.Length == 0) throw new ArgumentNullException("array can not be of 0 lenght");

            text = text.Trim().ToLower();
            try
            {
                // Initialize the start and end indices for binary search
                int desde = 0;
                int hasta = arrayOfStrings.Length - 1;

                // Perform binary search
                while (desde <= hasta)
                {
                    // Calculate the middle index
                    int medio = desde + (hasta - desde) / 2;

                    // Check if the middle element matches the search text
                    if (arrayOfStrings[medio].Trim().ToLower() == text)
                    {
                        return medio; // Return the index if found
                    }
                    // If the middle element is less than the search text, search in the right half
                    else if (string.Compare(arrayOfStrings[medio].Trim().ToLower(), text) < 0)
                    {
                        desde = medio + 1;
                    }
                    // If the middle element is greater than the search text, search in the left half
                    else
                    {
                        hasta = medio - 1;
                    }
                }

                // If not found, return -1
                return -1;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
