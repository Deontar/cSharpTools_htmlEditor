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


        //Attributes =  match.Groups[1].Value
        private string TranslateInnerTextOfAtributes(string attributes, string htmlAttributesRegex, string[] referenceLang, string[] translateLang, List<string> notTranslatedWords, int wordsNotTranslatedCount)
        {
            string pattern = @"(\b(?:" + htmlAttributesRegex + @")\s*=\s*"")([^""]+)(\""|\')";
            return Regex.Replace(attributes, pattern, attrMatch =>
            {
                string attributeName = attrMatch.Groups[1].Value;
                string attributeValue = attrMatch.Groups[2].Value;
                int index = IndexOfTextInArray(attributeValue, referenceLang);
                if (index != -1 && index < translateLang.Length && !string.IsNullOrWhiteSpace(attributeValue))
                {
                    string translatedAttrValue = translateLang[index];
                    return $"{attributeName}{translatedAttrValue}{attrMatch.Groups[3].Value}";
                }
                else
                {
                    notTranslatedWords.Add(attributeValue.Trim().ToLower());
                    wordsNotTranslatedCount++;
                }
                return attrMatch.Value;
            }, RegexOptions.IgnoreCase);
        }

        private string TranslateInnerTextOfTagAndAttribute(string tag, Match match, string[] referenceLang, string[] translateLang, List<string> notTranslatedWords, int wordsNotTranslatedCount)
        {
            //declaraciones temporales, borrar para versión final
            string htmlAttrRegex = "";

            string attribute = match.Groups[1].Value;
            string innerText = match.Groups[2].Value;
            string originalText = innerText;
            //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            string regexPatternAttr = @"(\b(?:" + htmlAttrRegex + @")\s*=\s*"")([^""]+)(\""|\')";
            string updatedAttributes = Regex.Replace(attribute, regexPatternAttr, attrMatch =>
            {
                string codeOfAtribute = attrMatch.Groups[1].Value;
                string valueOfAtribute = attrMatch.Groups[2].Value;
                int indexOfValueOfAttr = IndexOfTextInArray(valueOfAtribute, referenceLang);
                if (indexOfValueOfAttr != -1 && indexOfValueOfAttr < translateLang.Length && !string.IsNullOrWhiteSpace(valueOfAtribute))
                {
                    string translatedValueOfAtribute = translateLang[indexOfValueOfAttr];
                    return $"{codeOfAtribute}{translatedValueOfAtribute}{attrMatch.Groups[3].Value}";
                }
                else
                {
                    notTranslatedWords.Add(valueOfAtribute.Trim().ToLower());
                    wordsNotTranslatedCount++;
                }
                return attrMatch.Value;
            }, RegexOptions.IgnoreCase);
            //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            int index = IndexOfTextInArray(originalText, referenceLang);

            //traduccir innertext de tag
            if (index != -1 && index < translateLang.Length && !string.IsNullOrWhiteSpace(originalText))
            {
                string translatedText = translateLang[index];

                return $"<{tag}{updatedAttributes}>{translatedText}</{tag}>";
            }
            else
            {
                notTranslatedWords.Add(originalText.Trim().ToLower());
                wordsNotTranslatedCount++;
                return match.Value;
            }
        }
    }
}
