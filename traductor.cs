using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    //MMM edu nuevos tags th y h5
    private string[] tagsToTranslate = { "a", "label", "th", "h1", "h2", "h3", "h4", "h5", "button", "th" };
    private string[] htmlAttributes = { "href", "a", "label", "th", "h1", "h2", "h3", "h4", "h5", "button", "th" };
    private string htmlAttributesRegex;
    private string[] blackListedHtmlAtributes = { };

    private int wordsIdentifiedCount;
    private int wordsTranslatedCount;
    private int wordsNotTranslatedCount;
    private List<string> translatedWords = new List<string>();
    private List<string> notTranslatedWords = new List<string>();

    private Log LogNotTranslated;
    private Log LogErrors;

    //VARIABLES PARA GENERAR ARCHIVOS DE DEPURACIÓN################################################
    private Log LogDebug;
    private const bool enableDebugLog = false;
    private const string debugLogPath = "C:/Debuger";
    private const string debugLogName = "debug";
    private const string debugLogFormat = "txt";
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
    public traductor(string html, string[] lang, int referenceLangNum, int translateLangNum)
    {
        
        //MMM edu averiguar directorio de runtime y usarlo de prefijo del fichero
        string actuallDirectoryPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
        string translatedFilePath = Path.Combine(actuallDirectoryPath + "/../log/traductor/translated");
        string notTranslatedFilePath = Path.Combine(actuallDirectoryPath + "/../log/traductor/Nottranslated");
        string errorsFilePath = Path.Combine(actuallDirectoryPath + "/../log/traductor/errors");

        this.LogNotTranslated = new Log(notTranslatedFilePath, "NOT_translated_words_", "txt");
        this.LogErrors = new Log(errorsFilePath, "error_traductor", "txt");

        //Control de errores
        if (lang == null)
        {
            LogErrors.WritteWeeklyLog("El array de idiomas no puede ser nulo.");
            return;
        }
        if (html == null)
        {
            LogErrors.WritteWeeklyLog("El HTML no puede ser nulo.");
            return;
        }
        if (referenceLangNum == translateLangNum)
        {
            LogErrors.WritteWeeklyLog("El idioma de referencia no puede ser el mismo que el idioma de traducción.");
            return;
        }

        this.lang = lang;
        try
        {
            this.referenceLangNum = referenceLangNum;
            this.translateLangNum = translateLangNum;
        }
        catch (Exception e)
        {
            LogErrors.WritteWeeklyLog(e.ToString());
            return;
        }

        this.referenceLang = LangToIndividualArray(referenceLangNum);
        this.translateLang = LangToIndividualArray(translateLangNum);
        this.html = html;
        this.htmlTranslated = html;
        htmlAttributesRegex = string.Join("|", htmlAttributes);

        if (enableDebugLog)
        {
            this.LogDebug = new Log("C:/Debuger", $"debug", "txt");
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
    public void SetHtmlAttributes (string[] htmlAttributes)
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
        catch (Exception e){ LogErrors.WritteWeeklyLog(e.ToString());  return html; }
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
                try
                {
                    if (!decimal.TryParse(innerText, out nbr)) // MMMedu no traducir numeros
                    {
                        string updatedAttributes = TranslateInnerTextOfAtributes(attributes);
                        return TranslateInnerTextOfTag(tag, updatedAttributes, innerText, match);
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
        //ELIMINAR PARA PRUEBA EN VIVO----------
        finally
        {
            LogDebug?.Writte($"\t\tProcesstag END" + Environment.NewLine + Environment.NewLine + Environment.NewLine);
        }
        //ELIMINAR PARA PRUEBA EN VIVO----------
    }

    /// <summary>
    /// Procesa y traduce los atributos de una etiqueta HTML si corresponde<br></br>
    /// - attributes: Los atributos de la etiqueta HTML
    /// </summary>
    /// <returns>Atributos traducidos.</returns>
    private string TranslateInnerTextOfAtributes(string attributes)
    {
        try
        {
            LogDebug?.Writte($"\r\t\t\t\tTranslateInnerTextOfAtributes START");
            string pattern = @"(\b(?:" + htmlAttributesRegex + @")\s*=\s*"")([^""]+)(\""|\')";
            return Regex.Replace(attributes, pattern, attrMatch =>
            {
                LogDebug?.Writte($"\t\t\t\t\treferenceText:\t{attributes}");
                LogDebug?.Writte($"\t\t\t\t\tpattern2:\t{pattern}");
                LogDebug?.Writte($"\t\t\t\t\tMatch2:\t{attrMatch}");
                LogDebug?.Writte($"\t\t\t\t\tatributes2:\t{attrMatch.Groups[1].Value}");
                LogDebug?.Writte($"\t\t\t\t\tinnerText2:\t{attrMatch.Groups[2].Value}");

                string attributeName = attrMatch.Groups[1].Value;
                string attributeValue = attrMatch.Groups[2].Value;
                //Early leave si encuentra un atributo de la lista negra en el attrMatch.Value (lo que ha encontrado)
                if (Array.Exists(blackListedHtmlAtributes, attr => attr.Equals(attrMatch.Value, StringComparison.OrdinalIgnoreCase)))
                {
                    return attrMatch.Value;
                }
                try
                {

                    int attrIndex = Array.FindIndex(referenceLang, text => text.Trim().ToLower() == attributeValue.Trim().ToLower());
                    if (attrIndex != -1 && attrIndex < translateLang.Length && !string.IsNullOrWhiteSpace(attributeValue))
                    {
                        string translatedAttrValue = translateLang[attrIndex];
                        return $"{attributeName}{translatedAttrValue}{attrMatch.Groups[3].Value}";
                    }
                    else
                    {
                        notTranslatedWords.Add(attributeValue.Trim().ToLower());
                        wordsNotTranslatedCount++;
                    }
                    return attrMatch.Value;
                }
                catch (Exception e) { LogErrors.WritteWeeklyLog(e.ToString()); return null; }
            }, RegexOptions.IgnoreCase);
        }
        catch (Exception e) { LogErrors.WritteWeeklyLog(e.ToString()); return null; }
        finally
        {
            LogDebug?.Writte($"\t\t\t\tTranslateInnerTextOfAtributes END");
        }
    }

    /// <summary>
    /// Traduce el texto contenido dentro de una etiqueta HTML específica<br></br>
    /// - tag: La etiqueta HTML procesada<br></br>
    /// - updatedAttributes: Los atributos de la etiqueta después de ser procesados<br></br>
    /// - originalText: El texto original dentro de la etiqueta HTML<br></br>
    /// - match: La coincidencia de la etiqueta HTML en el contenido
    /// </summary>
    /// <returns>Texto traducido junto con la etiqueta HTML</returns>  
    private string TranslateInnerTextOfTag(string tag, string updatedAttributes, string innerText, Match match)
    {
        string originalText = innerText;
        LogDebug?.Writte($"\r\t\t\t\tTranslateInnerTextOfTag START");
        try
        {
            //Obtiene el indice de referenceLang con el texto originalText que coincida con el primer resultado de referenceLang
            int index = Array.FindIndex(referenceLang, text => text.Trim().ToLower() == originalText.Trim().ToLower());
            if (index != -1 && index < translateLang.Length && !string.IsNullOrWhiteSpace(originalText))
            {
                string translatedText = translateLang[index];
                translatedWords.Add(originalText);
                wordsTranslatedCount++;

                LogDebug?.Writte($"\t\t\t\t\t{originalText} = {translateLang[index]}");

                return $"<{tag}{updatedAttributes}>{translatedText}</{tag}>";
            }
            else
            {
                notTranslatedWords.Add(originalText.Trim().ToLower());
                wordsNotTranslatedCount++;
                return match.Value;
            }
        }
        catch (Exception e) { LogErrors.WritteWeeklyLog(e.ToString()); return null; }
        finally
        {
            LogDebug?.Writte($"\t\t\t\tTranslateInnerTextOfTag END");
        }
    }


    private bool checkForBlackListed(string text)
    {
        foreach (string atribute in blackListedHtmlAtributes) {
            if (text.Contains(atribute)) return true;
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