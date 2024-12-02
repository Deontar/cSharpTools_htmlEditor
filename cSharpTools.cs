using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

public class cSharpTools
{
    private string[] lang;
    private int referenceLangNum;
    private int translateLangNum;
    private string[] referenceLang;
    private string[] translateLang;
    private string[] tagsToTranslate = { "a", "label"};
    private string html;
    private string htmlTranslated;
    private int wordsIdentifiedCount = 0;
    private int wordsTranslatedCount = 0;
    private int wordsNotTranslatedCount = 0;
    private List<string> translatedWords = new List<string>();
    private List<string> notTranslatedWords = new List<string>();
    private const string HtmlAttributes = "href|target|download|hreflang|rel|type|for|form|style|class|lang|role|tabindex|span";


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
    public cSharpTools(string html, string[] lang, int referenceLangNum, int translateLangNum, string[] tagsToTranslate = null)
    {
        try
        {
            if (lang == null) throw new ArgumentNullException(nameof(lang), "El array de idiomas no puede ser nulo.");
            if (html == null) throw new ArgumentNullException(nameof(html), "El HTML no puede ser nulo.");
            if (referenceLangNum == translateLangNum) throw new ArgumentException("El idioma de referencia no puede ser el mismo que el idioma de traducción.");

            this.lang = lang;
            this.referenceLangNum = referenceLangNum;
            this.translateLangNum = translateLangNum;
            this.referenceLang = LangToArray(referenceLangNum);
            this.translateLang = LangToArray(translateLangNum);
            if (tagsToTranslate != null) this.tagsToTranslate = tagsToTranslate;
            this.html = html;
            this.htmlTranslated = html;

            string debugFilePath = Path.Combine("C:/Debuger", $"debug_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            var debug = new Debug(
                debugFilePath, wordsIdentifiedCount, wordsTranslatedCount, wordsNotTranslatedCount,
                translatedWords,
                notTranslatedWords
                );
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error en el constructor cSharpTools: {e.Message}");
            throw;
        }
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
            //LogTranslation();
            return htmlTranslated;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error en TranslateHTML: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// Procesa una etiqueta HTML específica, traduciendo su contenido y atributos<br></br>
    /// - tag: La etiqueta HTML que se va a procesar
    /// </summary>
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

                    string updatedAttributes = ProcessAttributes(attributes);
                    return TranslateText(tag, updatedAttributes, originalText, match);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error en ProcessTag '{tag}': {e.Message}");
                    throw;
                }
            }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error en ProcessTag: {e.Message}");
            throw;
        }
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
                catch (Exception e)
                {
                    Console.WriteLine($"Error en el procesamiento de atributos: {e.Message}");
                    throw;
                }
            }, RegexOptions.IgnoreCase);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error en ProcessAttributes: {e.Message}");
            throw;
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
        catch (Exception e)
        {
            Console.WriteLine($"Error en TranslateText: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// Convierte el array de idiomas en un array de strings basado en la posición especificada<br></br>
    /// - position: La posición en el array de idiomas
    /// </summary>
    /// <returns>Array de strings para el idioma especificado</returns>
    public string[] LangToArray(int position)
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
        catch (Exception e)
        {
            Console.WriteLine($"Error en LangToArray: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// Registra las palabras traducidas y no traducidas en archivos de texto
    /// </summary>
    private void LogTranslation()
    {
        try
        {
            string translatedFilePath = Path.Combine("../logs", $"translated_words_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllLines(translatedFilePath, translatedWords);

            string notTranslatedFilePath = Path.Combine("../logs", $"not_translated_words_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllLines(notTranslatedFilePath, notTranslatedWords);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error en GenerateTranslationFiles: {e.Message}");
            throw;
        }
    }
}