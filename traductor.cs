using cSharpTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace cSharpTools
{
    /// <summary>
    /// 
    /// </summary>
    public class Traductor
    {

        private string html;
        private string htmlTranslated;
        private string[] lang;
        private int referenceLangNum;
        private int translateLangNum;
        private string[] referenceLang;
        private string[] translateLang;

        //MMM edu nuevos tags th y h5
        private string[] tagsToTranslate = { "a", "label", "th", "h1", "h2", "h3", "h4", "h5", "button", "th" };
        private string[] htmlAttributes = { "href", "a", "label", "th", "h1", "h2", "h3", "h4", "h5", "button", "th" };
        private string htmlAttributesRegex;
        private string[] blackListedHtmlAtributes = { };

        private int wordsIdentifiedCount;
        private int wordsTranslatedCount;
        private int wordsNotTranslatedCount;
        private List<string> notTranslatedWords = new List<string>();

        private Log LogNotTranslated;
        private Log LogErrors;

        //VARIABLES PARA  DEPURACIÓN###################################################################
        private Log LogDebug;
        private Log LogNewTranslatorFunction;
        private const bool enableDebugLog = true;
        private const string debugLogPath = "C:/Debuger";
        private const string debugLogName = "debug";
        private const string debugLogFormat = "txt";
        private Testing_traductor Testing_traductor = new Testing_traductor();
        //#############################################################################################

        /* Procesa una etiqueta específica dentro del HTML usando Regex para identificar sus atributos y su contenido.
         Regex pattern explanation:
         - `<\s*{tag}`: Busca la etiqueta de apertura, permitiendo espacios en blanco después del símbolo '<'.
         - `([^>]*)`: Captura todos los atributos de la etiqueta en el primer grupo de captura (Group 1).
         - `>(.*?)<\/\s*{tag}\s*>`: Captura el contenido entre las etiquetas de apertura y cierre en el segundo grupo de captura (Group 2).
            Usa `.*?` para hacer una búsqueda no codiciosa y capturar el mínimo necesario.
        */

        /// <summary>
        /// Inicializa una nueva instancia de la clase cSharpTools<br></br>
        /// - html: HTML que se va a traducir<br></br>
        /// - lang: Array de idiomas para referencia y traducción<br></br>
        /// - referenceLangNum: Índice del idioma de referencia en el array de idiomas<br></br>
        /// - translateLangNum: Índice del idioma al que se va a traducir en el array de idiomas<br></br>
        /// </summary>
        public Traductor(string html, string[] lang, int referenceLangNum, int translateLangNum)
        {
            //MMM edu averiguar directorio de runtime y usarlo de prefijo del fichero
            string actuallDirectoryPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            string translatedFilePath = Path.Combine(actuallDirectoryPath + "/../log/traductor/translated");
            string notTranslatedFilePath = Path.Combine(actuallDirectoryPath + "/../log/traductor/Nottranslated");
            string errorsFilePath = Path.Combine(actuallDirectoryPath + "/../log/traductor/errors");

            this.LogNotTranslated = new Log(notTranslatedFilePath, "NOT_translated_words_", "txt");
            this.LogErrors = new Log(errorsFilePath, "error_traductor", "txt");

            this.lang = lang;
            this.referenceLangNum = referenceLangNum;
            this.translateLangNum = translateLangNum;
            this.referenceLang = LangToIndividualArray(referenceLangNum);
            this.translateLang = LangToIndividualArray(translateLangNum);
            this.html = html;
            this.htmlTranslated = html;
            htmlAttributesRegex = string.Join("|", htmlAttributes);

            if (enableDebugLog)
            {
                this.LogDebug = new Log("C:/Debuger", "debug", "txt");
                this.LogNewTranslatorFunction = new Log("C:/Testing", "new_translator_function", "txt");
            }
        }

        /// <summary>
        /// - tagsToTranslate: Array de tags HTML que se va a traducir. Si no se establece se usran las predeterminadas<br></br>
        /// Default { "a", "label", "th", "h1", "h2", "h3", "h4", "h5", "button", "th" }
        /// </summary>
        public void SetTagsToTranslate(string[] tagsToTranslate)
        {
            this.tagsToTranslate = tagsToTranslate;
        }

        /// <summary>
        /// - htmlAttributes: Array de atributos HTML que se va a traducir. Si no se establece se usran los predeterminados<br></br>
        /// Default { "href", "a", "label", "th", "h1", "h2", "h3", "h4", "h5", "button", "th" };
        /// </summary>
        public void SetHtmlAttributes(string[] htmlAttributes)
        {
            htmlAttributesRegex = "";
            this.htmlAttributes = htmlAttributes;
            htmlAttributesRegex = string.Join("|", htmlAttributes);
        }

        /// <summary>
        /// - blackListedHtmlAtributes: Array de atributos HTML que seran ignorados.<br></br>
        /// </summary>
        public void SetBlackListeAttributes(string[] blackListedHtmlAtributes)
        {
            this.blackListedHtmlAtributes = blackListedHtmlAtributes;
        }

        /// <summary>
        /// Traduce el HTML proporcionado utilizando los idiomas de referencia y destino
        /// </summary>
        /// <returns>HTML traducido</returns>
        public string TranslateHTML()
        {
            //Control de errores
            if (lang == null)
            {
                LogErrors.WritteWeeklyLog("El array de idiomas no puede ser nulo.");
                return html;
            }
            else if (html == null)
            {
                LogErrors.WritteWeeklyLog("El HTML no puede ser nulo.");
                return html;
            }
            else if (referenceLangNum == translateLangNum)
            {
                LogErrors.WritteWeeklyLog("El idioma de referencia no puede ser el mismo que el idioma de traducción.");
                return html;
            }

            try
            {
                LogDebug?.Writte($"TranslateHTML START");
                string tagsInString = null;
                foreach (string tag in tagsToTranslate) { tagsInString += tag + " "; }
                LogDebug?.Writte($"\t{tagsInString}");
                //ELIMINAR PARA PRUEBA EN VIVO----------
                foreach (string tag in tagsToTranslate)
                {
                    LogDebug?.Writte($"\ttag:\t{tag}");

                    ProcessTag(tag);
                }
                LogDebug?.Writte("\r\n--- Resumen de Traducción ---");
                LogDebug?.Writte($"Palabras identificadas: {wordsIdentifiedCount}");
                LogDebug?.Writte($"Palabras traducidas: {wordsTranslatedCount}");
                LogDebug?.Writte($"Palabras no traducidas: {wordsNotTranslatedCount}");
                LogDebug?.Writte(Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine);

                //NO TOCAR-----
                LogNotTranslated.UniqueWords(notTranslatedWords);
                //NO TOCAR-----

                return htmlTranslated;
            }
            catch (Exception e) { LogErrors.WritteWeeklyLog(e.ToString()); return html; }
            //ELIMINAR PARA PRUEBA EN VIVO----------
            finally
            {
                LogDebug?.Writte($"TranslateHTML END");
            }
            //ELIMINAR PARA PRUEBA EN VIVO----------
        }

        /// <summary>
        /// Procesa una etiqueta HTML específica, traduciendo su contenido y atributos<br></br>
        /// - tag: La etiqueta HTML que se va a procesar
        /// </summary>
        /// <summary>
        /// Procesa una etiqueta HTML específica, traduciendo su contenido y atributos.
        /// </summary>
        /// <param name="tag">La etiqueta HTML que se va a procesar.</param>
        private void ProcessTag(string tag)
        {
            try
            {
                LogDebug?.Writte($"\r\t\tProcesstag START");
                string pattern = $"<\\s*{tag}([^>]*)>(.*?)<\\/\\s*{tag}\\s*>";
                htmlTranslated = Regex.Replace(htmlTranslated, pattern, match =>
                {
                    LogDebug?.Writte($"\t\t\tpattern1:\t{pattern}");
                    LogDebug?.Writte($"\t\t\tmatch1:\t{match}");
                    LogDebug?.Writte($"\t\t\tatributes1:\t{match.Groups[1].Value}");
                    LogDebug?.Writte($"\t\t\tinnerText1:\t{match.Groups[2].Value}");

                    string attributes = match.Groups[1].Value;
                    string innerText = match.Groups[2].Value;
                    wordsIdentifiedCount++;
                    LogDebug?.Writte($"\t\t\tID:{wordsIdentifiedCount}");
                    decimal nbr;
                    if (!decimal.TryParse(innerText, out nbr)) // MMMedu no traducir numeros
                    {
                        return TranslateInnerTextOfTagAndAttribute(tag, match);
                    }
                    else
                    {
                        return match.ToString();
                    }
                }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
            catch (Exception e) { LogErrors.WritteWeeklyLog(e.ToString()); }
            finally
            {
                LogDebug?.Writte($"\t\tProcesstag END" + Environment.NewLine + Environment.NewLine + Environment.NewLine);
            }
        }

        private string TranslateInnerTextOfTagAndAttribute(string tag, Match match)
        {
            //declaraciones temporales, borrar para versión final
            string htmlAttrRegex = "";

            string attribute = match.Groups[1].Value;
            string innerText = match.Groups[2].Value;
            string originalText = innerText;
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

        /// <summary>
        /// Convierte el array de idiomas en un array de strings basado en la posición especificada<br></br>
        /// - position: La posición en el array de idiomas
        /// </summary>
        /// <returns>Array de strings para el idioma especificado</returns>
        public string[] LangToIndividualArray(int position)
        {
            try
            {
                var result = new List<string>();
                foreach (var item in lang)
                {
                    if (item != null)
                    {
                        var parts = item.Split(new[] { "|" }, StringSplitOptions.None);
                        if (position < parts.Length)
                        {
                            result.Add(parts[position]);
                        }
                    }
                }
                return result.ToArray();
            }
            catch (Exception e) { LogErrors.WritteWeeklyLog(e.ToString()); return null; }
        }
    }
}