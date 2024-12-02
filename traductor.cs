using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

public class traductor
{
    private string html;
    private string htmlTranslated;
    private string[] lang;
    private int referenceLangNum;
    private int translateLangNum;
    private string[] referenceLang;
    private string[] translateLang;

    private string[] tagsToTranslate = { "a", "label", "th", "h5"}; //MMM edu nuevos tags th y h5
    private const string HtmlAttributes = "href|target|download|hreflang|rel|type|for|form|style|class|lang|role|tabindex|span";

    private int wordsIdentifiedCount = 0;
    private int wordsTranslatedCount = 0;
    private int wordsNotTranslatedCount = 0;
    private List<string> translatedWords = new List<string>();
    private List<string> notTranslatedWords = new List<string>();

    private Log LogTranslated;
    private Log LogNotTranslated;
    private Log LogErrors;
    private Log LogDebug;

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
    /// - tagsToTranslate: Array de etiquetas HTML que se van a traducir. Si es null, se usarán las etiquetas predeterminadas<br></br>
    /// </summary>
    public traductor(string html, string[] lang, int referenceLangNum, int translateLangNum, string[] tagsToTranslate = null)
    {
        //MMM edu averiguar directorio de runtime y usarlo de prefijo del fichero
        string actuallDirectoryPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

        string translatedFilePath = Path.Combine(actuallDirectoryPath + "/../log/traductor/translated");
        string notTranslatedFilePath = Path.Combine(actuallDirectoryPath + "/../log/traductor/Nottranslated");
        string errorsFilePath = Path.Combine(actuallDirectoryPath + "/../log/traductor/errors");
        this.LogTranslated = new Log(translatedFilePath, "translated_words_", "txt");
        this.LogNotTranslated = new Log(notTranslatedFilePath, "NOT_translated_words_", "txt");
        this.LogErrors = new Log(errorsFilePath, "error_traductor", "txt");

        //Control de errores
        if (lang == null) Console.WriteLine("El array de idiomas no puede ser nulo.");
        else if (html == null) Console.WriteLine("El HTML no puede ser nulo.");
        else if (referenceLangNum == translateLangNum) Console.WriteLine("El idioma de referencia no puede ser el mismo que el idioma de traducción.");

        this.lang = lang;
        try
        {
            this.referenceLangNum = referenceLangNum;
            this.translateLangNum = translateLangNum;
        }
        catch (Exception e) { LogErrors.WritteWeeklyLog(e.ToString()); return; }

        this.referenceLang = LangToIndividualArray(referenceLangNum);
        this.translateLang = LangToIndividualArray(translateLangNum);
        if (tagsToTranslate != null) this.tagsToTranslate = tagsToTranslate;
        this.html = html;
        this.htmlTranslated = html;

        //ELIMINAR PARA PRUEBA EN VIVO----------
        this.LogDebug = new Log("C:/Debuger", $"debug", "txt");
        //ELIMINAR PARA PRUEBA EN VIVO----------
    }

    /// <summary>
    /// Traduce el HTML proporcionado utilizando los idiomas de referencia y destino
    /// </summary>
    /// <returns>HTML traducido</returns>
    public string TranslateHTML()
    {
        try
        {
            foreach (string tag in tagsToTranslate)
            {
                ProcessTag(tag);
            }

            //ELIMINAR PARA PRUEBA EN VIVO----------
            //Resumen de la traducción, SOLO USAR DURANTE DEPURACIÓN
            LogDebug.WritteInLogDebug("\r\n--- Resumen de Traducción ---");
            LogDebug.WritteInLogDebug($"Palabras identificadas: {wordsIdentifiedCount}");
            LogDebug.WritteInLogDebug($"Palabras traducidas: {wordsTranslatedCount}");
            LogDebug.WritteInLogDebug($"Palabras no traducidas: {wordsNotTranslatedCount}");
            //ELIMINAR PARA PRUEBA EN VIVO----------

            //NO TOCAR-----
            LogTranslated.UniqueWords(translatedWords);
            LogNotTranslated.UniqueWords(notTranslatedWords);
            //NO TOCAR-----

            return htmlTranslated;
        }
        catch (Exception e){ LogErrors.WritteWeeklyLog(e.ToString());  return html; }
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
            string pattern = $"<\\s*{tag}([^>]*)>(.*?)<\\/\\s*{tag}\\s*>";

            htmlTranslated = Regex.Replace(htmlTranslated, pattern, match =>
            {
                try
                {
                    string attributes = match.Groups[1].Value;
                    string originalText = match.Groups[2].Value;
                    wordsIdentifiedCount++;
                    decimal nbr;
                    if (!decimal.TryParse(originalText, out nbr)) // MMMedu no traducir numeros
                    {
                        string updatedAttributes = ProcessAttributes(attributes);
                        return TranslateText(tag, updatedAttributes, originalText, match);
                    }
                    else
                    {
                        return match.ToString();
                    }
                }
                catch (Exception e) { LogErrors.WritteWeeklyLog(e.ToString()); return null; }
            }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
        catch (Exception e) { LogErrors.WritteWeeklyLog(e.ToString()); }
    }

    /// <summary>
    /// Procesa y traduce los atributos de una etiqueta HTML si corresponde<br></br>
    /// - attributes: Los atributos de la etiqueta HTML
    /// </summary>
    /// <returns>Atributos traducidos.</returns>
    private string ProcessAttributes(string attributes)
    {
        try
        {
            return Regex.Replace(attributes, @"(\b(?:" + HtmlAttributes + @")\s*=\s*"")([^""]+)(\""|\')", attrMatch =>
            {
                try
                {
                    string attributeName = attrMatch.Groups[1].Value;
                    string attributeValue = attrMatch.Groups[2].Value;

                    int attrIndex = Array.FindIndex(referenceLang, text => text.Trim().ToLower() == attributeValue.ToLower());
                    if (attrIndex != -1 && attrIndex < translateLang.Length && !string.IsNullOrWhiteSpace(attributeValue))
                    {
                        string translatedAttrValue = translateLang[attrIndex];
                        return $"{attributeName}{translatedAttrValue}{attrMatch.Groups[3].Value}";
                    }
                    return attrMatch.Value;
                }
                catch (Exception e) { LogErrors.WritteWeeklyLog(e.ToString()); return null; }
            }, RegexOptions.IgnoreCase);
        }
        catch (Exception e) { LogErrors.WritteWeeklyLog(e.ToString()); return null; }
    }

    /// <summary>
    /// Traduce el texto contenido dentro de una etiqueta HTML específica<br></br>
    /// - tag: La etiqueta HTML procesada<br></br>
    /// - updatedAttributes: Los atributos de la etiqueta después de ser procesados<br></br>
    /// - originalText: El texto original dentro de la etiqueta HTML<br></br>
    /// - match: La coincidencia de la etiqueta HTML en el contenido
    /// </summary>
    /// <returns>Texto traducido junto con la etiqueta HTML</returns>  
    private string TranslateText(string tag, string updatedAttributes, string originalText, Match match)
    {
        try
        {
            int index = Array.FindIndex(referenceLang, text => text.Trim().ToLower() == originalText.ToLower());
            if (index != -1 && index < translateLang.Length && !string.IsNullOrWhiteSpace(originalText))
            {
                string translatedText = translateLang[index];
                translatedWords.Add(originalText);
                wordsTranslatedCount++;
                return $"<{tag}{updatedAttributes}>{translatedText}</{tag}>";
            }
            else
            {
                notTranslatedWords.Add(originalText);
                wordsNotTranslatedCount++;
                return match.Value;
            }
        }
        catch (Exception e) { LogErrors.WritteWeeklyLog(e.ToString()); return null; }
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