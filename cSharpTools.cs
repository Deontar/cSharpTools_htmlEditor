using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class cSharpTools
{
    private string[] lang;
    private int referenceLangNum;
    private int translateLangNum;
    private string[] tagsToTranslate = { "a", "label" };
    private string html;

    // Constructor para inicializar los datos necesarios para la traducción
    public cSharpTools(string html, string[] lang, int referenceLangNum, int translateLangNum, string[] tagsToTranslate = null)
    {
        this.lang = lang;
        this.referenceLangNum = referenceLangNum;
        this.translateLangNum = translateLangNum;
        if (tagsToTranslate != null)
        {
            this.tagsToTranslate = tagsToTranslate;
        }
        this.html = html;
    }

    public string TranslateHTML()
    {
        string[] referenceLang = LangToArray(referenceLangNum);
        string[] translateLang = LangToArray(translateLangNum);

        var referenceDict = new Dictionary<string, string>();
        for (int i = 0; i < referenceLang.Length; i++)
        {
            if (i < translateLang.Length)
            {
                referenceDict[referenceLang[i]] = translateLang[i];
            }
        }

        foreach (var tag in tagsToTranslate)
        {
            // Crear un patrón Regex para encontrar las etiquetas especificadas
            string pattern = $@"<({tag})\b[^>]*>(.*?)</\1>";
            html = Regex.Replace(html, pattern, match =>
            {
                string innerText = match.Groups[2].Value;

                // Buscar si el innerText coincide con una entrada en el diccionario de referencia
                if (referenceDict.ContainsKey(innerText))
                {
                    // Obtener la traducción desde el diccionario de traducción
                    string translatedText = referenceDict[innerText];
                    return $"<{match.Groups[1].Value}>{translatedText}</{match.Groups[1].Value}>";
                }
                return match.Value;
            }, RegexOptions.Singleline);
        }

        // Devolver el HTML traducido
        return html;
    }

    public string[] LangToArray(int position)
    {
        var result = new List<string>();

        // Validar que lang no sea null
        if (lang == null)
        {
            throw new ArgumentNullException(nameof(lang), "El arreglo lang no puede ser null.");
        }

        foreach (var item in lang)
        {
            // Validar que cada item dentro de lang no sea null
            if (item != null)
            {
                var parts = item.Split(new[] { "|" }, StringSplitOptions.None);
                if (position < parts.Length)
                {
                    result.Add(parts[position].Trim());
                }
            }
            else
            {
                Console.WriteLine("Advertencia: Se encontró un elemento null en el arreglo lang.");
            }
        }

        return result.ToArray(); // Convertir la lista a un array antes de devolverla
    }
}