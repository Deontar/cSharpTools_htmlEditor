using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace cSharpTools
{
    /// <summary>
    /// 
    /// </summary>
    public class traductor
    {

        private string html;
        private string htmlTranslated;
        private string[] lang;
        private int referenceLangNum;
        private int translateLangNum;
        private string[] referenceLang;
        private string[] translateLang;

        //MMM edu nuevos tags th y h5
        private List<string> tagsToTranslate = new List<string> { "a", "label", "th", "h1", "h2", "h3", "h4", "h5", "button", "th" };

        private List<string> notTranslatedWords = new List<string>();

        private Log LogNotTranslated;
        private Log LogErrors;

        //VARIABLES PARA  DEPURACIÓN###################################################################
        private Log LogDebug;
        private const bool enableDebugLog = true;
        private const string debugLogPath = "C:/Debuger";
        private const string debugLogName = "debug";
        private const string debugLogFormat = "txt";
        //#############################################################################################

        /// <summary>
        /// Inicializa una nueva instancia de la clase cSharpTools<br></br>
        /// - html: HTML que se va a traducir<br></br>
        /// - lang: Array de idiomas para referencia y traducción<br></br>
        /// - referenceLangNum: Índice del idioma de referencia en el array de idiomas<br></br>
        /// - translateLangNum: Índice del idioma al que se va a traducir en el array de idiomas<br></br>
        /// </summary>
        /// 
        public traductor(string html, string[] lang, int referenceLangNum, int translateLangNum)
        {
            //MMM edu averiguar directorio de runtime y usarlo de prefijo del fichero
            string actuallDirectoryPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
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


            if (enableDebugLog)
            {
                this.LogDebug = new Log("C:/Debuger", "debug", "txt");
            }
        }


        /// <summary>
        /// - tagsToTranslate: String list de tags HTML que se va a traducir. Si no se establece se usran las predeterminadas<br></br>
        /// Default { "a", "label", "th", "h1", "h2", "h3", "h4", "h5", "button", "th" }
        /// </summary>
        public void SetTagsToTranslate(List<String> tagsToTranslate)
        {
            this.tagsToTranslate = tagsToTranslate;
        }

        /// <summary>
        /// Traduce el HTML proporcionado utilizando los idiomas de referencia y destino
        /// </summary>
        /// <returns>HTML traducido</returns>
        public string TranslateHTML()
        {
            if(checkForErrors() == true) { return html; }

            try
            {
                InnerTextEditor innerTextEditor = new InnerTextEditor(html, tagsToTranslate);
                BigTagCollection bigTagCollection = innerTextEditor.GetBigTagCollection();

                foreach (TagCollection tagCollection in bigTagCollection.TagCollection)
                {
                    foreach (IndividualTag tag in tagCollection.Tag)
                    {
                        string actualInnerText = tag.InnerText.ActualValue;
                        int indexTranslate = IndexOfTextInArray(actualInnerText, referenceLang);
                        if (indexTranslate != -1)
                        {
                            LogDebug?.Writte($"{referenceLang[indexTranslate].ToLower()} =? {actualInnerText.Trim().ToLower()}");
                            if (referenceLang[indexTranslate].Trim().ToLower() == actualInnerText.Trim().ToLower())
                            {
                                LogDebug?.Writte($"{actualInnerText.Trim().ToLower()} -> {translateLang[indexTranslate]}");
                                innerTextEditor.AddNewValue(actualInnerText, translateLang[indexTranslate]);
                            }
                            else
                            {
                                LogNotTranslated.WritteWeeklyLog(actualInnerText);
                            }
                        }
                        else
                        {
                            LogNotTranslated.WritteWeeklyLog(actualInnerText);
                        }
                    }
                }

                htmlTranslated = innerTextEditor.ReplaceInnerText();
            }
            catch (Exception e)
            {
                LogErrors.WritteWeeklyLog(e.Message);
                return html;
            }

            return htmlTranslated;
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

        private bool checkForErrors()
        {

            //Control de errores
            if (lang == null)
            {
                LogErrors.WritteWeeklyLog("El array de idiomas no puede ser nulo.");
                return true;
            }
            else if (html == null)
            {
                LogErrors.WritteWeeklyLog("El HTML no puede ser nulo.");
                return true;
            }
            else if (referenceLangNum == translateLangNum)
            {
                LogErrors.WritteWeeklyLog("El idioma de referencia no puede ser el mismo que el idioma de traducción.");
                return true;
            }
            else if (referenceLangNum < 0)
            {
                LogErrors.WritteWeeklyLog("El idioma de referencia no puede ser negativo");
                return true;
            }
            else if (translateLangNum < 0)
            {
                LogNotTranslated.WritteWeeklyLog("El idioma de traducción no puede ser negativo");
                return true;
            }
            return false;
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