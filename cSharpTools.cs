﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class cSharpTools
{
    private string[] lang;
    private int referenceLangNum;
    private int translateLangNum;
    private string[] referenceLang;
    private string[] translateLang;
    private string[] tagsToTranslate = { "a", "label", "span" };
    private string html;
    private string htmlTranslated;
    private string debugFilePath = Path.Combine("C:\\Debuger", $"debug_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
    private int wordsIdentifiedCount = 0;
    private int wordsTranslatedCount = 0;
    private int wordsNotTranslatedCount = 0;
    private List<string> translatedWords = new List<string>();
    private List<string> notTranslatedWords = new List<string>();
    private const string HtmlAttributes = "href|target|download|hreflang|rel|type|for|form|style|class|lang|role|tabindex";

    // Procesa una etiqueta específica dentro del HTML usando Regex para identificar sus atributos y su contenido.
    // Regex pattern explanation:
    // - `<\s*{tag}`: Busca la etiqueta de apertura, permitiendo espacios en blanco después del símbolo '<'.
    // - `([^>]*)`: Captura todos los atributos de la etiqueta en el primer grupo de captura (Group 1).
    // - `>(.*?)<\/\s*{tag}\s*>`: Captura el contenido entre las etiquetas de apertura y cierre en el segundo grupo de captura (Group 2).
    //   Usa `.*?` para hacer una búsqueda no codiciosa y capturar el mínimo necesario.
    public cSharpTools(string html, string[] lang, int referenceLangNum, int translateLangNum, string[] tagsToTranslate = null)
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

        // Crear directorio para debuger si no existe
        Directory.CreateDirectory(Path.GetDirectoryName(debugFilePath));
        LogDebug($"html:\r{html}\r");
        LogDebug($"referenceLangNum: {referenceLangNum}");
        LogDebug($"translateLangNum: {translateLangNum}");
        if (tagsToTranslate == null) LogDebug("usando tags predeterminadas");
        else LogDebug($"Usando tags personalizadas: {string.Join(", ", tagsToTranslate)}");
    }

    public string TranslateHTML()
    {
        LogDebug("TranslateHTML start");

        foreach (string tag in tagsToTranslate)
        {
            ProcessTag(tag);
        }

        LogDebug($"Palabras identificadas: {wordsIdentifiedCount}");
        LogDebug($"Palabras traducidas: {wordsTranslatedCount}");
        LogDebug($"Palabras no traducidas: {wordsNotTranslatedCount}");
        LogDebug($"largo html: {html.Length}\rlargo htmlTranslated: {htmlTranslated.Length}");
        // Devolver el HTML traducido
        LogDebug("TranslateHTML end");
        GenerateSummary();
        return htmlTranslated;
    }

    private void ProcessTag(string tag)
    {
        LogDebug($"\tProcessing tag: {tag}");
        string pattern = $"<\\s*{tag}([^>]*)>(.*?)<\\/\\s*{tag}\\s*>";
        LogDebug($"\tpattern: {pattern}");

        htmlTranslated = Regex.Replace(htmlTranslated, pattern, match =>
        {
            LogDebug($"\r\tCoincidencia encontrada: {match.Value}");
            string attributes = match.Groups[1].Value;
            string originalText = match.Groups[2].Value;
            LogDebug($"\t\toriginalText start");
            LogDebug($"\t\t\t{originalText}");
            LogDebug($"\t\toriginalText end");
            wordsIdentifiedCount++;

            string updatedAttributes = ProcessAttributes(attributes);
            return TranslateText(tag, updatedAttributes, originalText, match);
        }, RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    private string ProcessAttributes(string attributes)
    {
        return Regex.Replace(attributes, @"(\b(?:" + HtmlAttributes + @")\s*=\s*"")([^""]+)(\""|\')", attrMatch =>
        {
            string attributeName = attrMatch.Groups[1].Value;
            string attributeValue = attrMatch.Groups[2].Value;
            LogDebug($"\t\tAtributo encontrado: {attributeName} con valor: {attributeValue}");

            int attrIndex = Array.FindIndex(referenceLang, text => text.Trim().ToLower() == attributeValue.ToLower());
            if (attrIndex != -1 && attrIndex < translateLang.Length && !string.IsNullOrWhiteSpace(attributeValue))
            {
                string translatedAttrValue = translateLang[attrIndex];
                LogDebug($"\t\t\t'{attributeValue}' --> '{translatedAttrValue}'");
                return $"{attributeName}{translatedAttrValue}{attrMatch.Groups[3].Value}";
            }
            return attrMatch.Value;
        }, RegexOptions.IgnoreCase);
    }

    private string TranslateText(string tag, string updatedAttributes, string originalText, Match match)
    {
        int index = Array.FindIndex(referenceLang, text => text.Trim().ToLower() == originalText.ToLower());
        if (index != -1 && index < translateLang.Length && !string.IsNullOrWhiteSpace(originalText))
        {
            string translatedText = translateLang[index];
            LogDebug($"\t\t\t --> Reemplazando '{originalText}' con '{translatedText}'");
            translatedWords.Add(originalText);
            wordsTranslatedCount++;
            return $"<{tag}{updatedAttributes}>{translatedText}</{tag}>";
        }
        else
        {
            LogDebug($"\t\tNo se encontró traducción para el texto: '{originalText}'");
            notTranslatedWords.Add(originalText);
            wordsNotTranslatedCount++;
            return match.Value;
        }
    }

    public string[] LangToArray(int position)
    {
        LogDebug("LangToArray start");
        var result = new List<string>();
        foreach (var item in lang)
        {
            // Validar que cada item dentro de lang no sea null
            if (item != null)
            {
                var parts = item.Split(new[] { "|" }, StringSplitOptions.None);
                if (position < parts.Length)
                {
                    result.Add(parts[position]);
                }
            }
        }
        LogDebug($"\tPosition: {position}");
        LogDebug($"\tLargo result: {result.Count}");
        LogDebug("LangToArray end");
        return result.ToArray(); // Convertir la lista a un array antes de devolverla
    }

    private void LogDebug(string message)
    {
        try
        {
            File.AppendAllText(debugFilePath, message + Environment.NewLine);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al escribir en el archivo de depuración: {e.Message}");
        }
    }

    private void GenerateSummary()
    {
        LogDebug("\r\n--- Resumen de Traducción ---");
        LogDebug($"Palabras identificadas: {wordsIdentifiedCount}");
        LogDebug($"Palabras traducidas: {wordsTranslatedCount}");
        LogDebug($"Palabras no traducidas: {wordsNotTranslatedCount}");

        LogDebug("\r\n--- Palabras traducidas ---");
        LogDebug("[palabra tratada para comparación]\t\t\t[palabra en formato original]");
        foreach (var translated in translatedWords)
        {
            LogDebug($"{translated.ToLower().Trim()}\t\t\t{translated}");
        }

        LogDebug("\r\n--- Palabras no traducidas ---");
        LogDebug("[palabra tratada para comparación]\t\t\t[palabra en formato original]");
        foreach (var notTranslated in notTranslatedWords)
        {
            LogDebug($"{notTranslated.ToLower().Trim()}\t\t\t{notTranslated}");
        }
    }
}
