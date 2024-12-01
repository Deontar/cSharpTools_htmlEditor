using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cSharpToolsNamespace
{
    public class cSharpTools
    {
        private string[] lang;
        private int referenceLangNum;
        private int translatedWordsNum;
        private string[] tagsToReplace = { "a", "label" };
        private string html;

        // Constructor para inicializar los datos necesarios para la traducción
        public cSharpTools(string html, string[] lang, int referenceLangNum, int translatedWordsNum, string[] tagsToReplace = null)
        {
            this.lang = lang;
            this.referenceLangNum = referenceLangNum;
            this.translatedWordsNum = translatedWordsNum;
            this.tagsToReplace = tagsToReplace;
            this.html = html;
        }

        public string TranslateHTML()
        {
            List<string> referenceLangList = LangToList(lang, referenceLangNum);
            List<string> translatedWordsList = LangToList(lang, translatedWordsNum);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Crear una expresión XPath para las etiquetas especificadas por el usuario
            string xpath = string.Join(" | ", tagsToReplace.Select(tag => "//" + tag));
            var tags = doc.DocumentNode.SelectNodes(xpath);

            if (tags != null)
            {
                // Iterar sobre cada etiqueta seleccionada
                foreach (var tagValue in tags)
                {
                    // Iterar sobre la lista de palabras de referencia en referenceLangList
                    for (int j = 0; j < referenceLangList.Count; j++)
                    {
                        if (tagValue.InnerHtml.Trim().ToLower() == referenceLangList[j].Trim().ToLower())
                        {
                            // Si hay coincidencia, reemplazar el contenido con la traducción correspondiente
                            tagValue.InnerHtml = translatedWordsList[j];
                            break;
                        }
                    }
                }
            }

            return doc.DocumentNode.InnerHtml;
        }

        private List<string> LangToList(string[] lang, int position)
        {
            var result = new List<string>();

            // Iterar sobre cada elemento de la lista combinada
            foreach (var item in lang)
            {
                // Dividir la cadena en partes usando '||' como delimitador
                var parts = item.Split(new[] { "|" }, StringSplitOptions.None);
                if (position < parts.Length)
                {
                    // Agregar la parte correspondiente a la posición especificada
                    result.Add(parts[position].Trim());
                }
            }

            return result;
        }
    }
}