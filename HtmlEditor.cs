using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace cSharpTools
{
    public class HtmlEditor
    {
        private string html;

        public HtmlEditor(string html)
        {
            this.html = html;
        }

        /// <returns>A fully build Tag variable of the first match in html</returns>
        public Tag GetTag(string tagName)
        {
            //Original regex (not adapted to code) <\s*{texto}[^>]*>.*?<\/\s*{texto}\s*>
            string pattern = $"<\\s*{tagName}([^>]*)>(.*?)<\\/\\s*{tagName}\\s*>";
            Match match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Tag _tag = Tag.Build(match);
            return _tag;
        }

        /// <returns>A fully build TagCollection variable of all the matches in html</returns>
        public TagCollection GetTagCollection(string tagName)
        {
            //Original regex (not adapted to code) <\s*TEXT([^>]*)>(.*?)<\/\s*TEXT\s*>
            string pattern = $"<\\s*{tagName}([^>]*)>(.*?)<\\/\\s*{tagName}\\s*>";
            MatchCollection matchCollection = Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            TagCollection tagCollection = TagCollection.Build(matchCollection);

            return tagCollection;
        }
    }

    public class InnerTextEditor_old
    {
        private string html;
        private string pattern;
        TagInEditCollection tagInEditCollection;
        private int currentIndex = 0;

        public InnerTextEditor_old(string html, string tagName)
        {
            this.html = html;
            //Original regex (not adapted to code) <\s*TEXT([^>]*)>(.*?)<\/\s*TEXT\s*>
            this.pattern = $"<\\s*{tagName}([^>]*)>(.*?)<\\/\\s*{tagName}\\s*>";
        }

        public TagInEditCollection GetInTagCollection()
        {
            MatchCollection matchCollection = Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            TagInEditCollection tagInEditCollection = TagInEditCollection.Build(matchCollection, html);
            this.tagInEditCollection = tagInEditCollection;
            return tagInEditCollection;
        }

        public string ReplaceInnerText(TagInEditCollection tagInEditCollection)
        {
            string[] htmlLines = html.Split('\n');

            foreach (TagInEdit tagInEdit in tagInEditCollection.Tags)
            {
                if (tagInEdit.Tag != null)
                {
                    string newText = tagInEdit.Tag.InnerText.NewValue;
                    if (!string.IsNullOrEmpty(newText))
                    {
                        for (int i = tagInEdit.StartLine - 1; i < tagInEdit.EndLine; i++)
                        {
                            int startIndex = htmlLines[i].IndexOf(tagInEdit.Tag.InnerText.Value);
                            if (startIndex != -1)
                            {
                                // Reemplazar el contenido completo con el nuevo valor
                                htmlLines[i] = htmlLines[i].Remove(startIndex, tagInEdit.Tag.InnerText.Value.Length).Insert(startIndex, newText);
                                break; // Reemplazar solo la primera ocurrencia exacta
                            }
                        }
                    }
                }
            }

            html = string.Join("\n", htmlLines);
            return html;
        }

        public string GetInnerText()
        {
            if (tagInEditCollection == null || currentIndex >= tagInEditCollection.Tags.Count) { return string.Empty; }

            TagInEdit currentTagInEdit = tagInEditCollection.Tags[currentIndex];
            currentIndex++;
            return currentTagInEdit.Tag.InnerText.Value;
        }

        public TagInEditCollection AddNewValue(TagInEditCollection tagInEditCollection,string oldValue, string newValue)
        {
            foreach (TagInEdit tagInEdit in tagInEditCollection.Tags)
            {
                if (tagInEdit.Tag != null && tagInEdit.Tag.InnerText != null && tagInEdit.Tag.InnerText.Value == oldValue)
                {
                    tagInEdit.Tag.InnerText.NewValue = newValue;
                }
            }
            return tagInEditCollection;
        }
    }

    public class TagInEditCollection
    {
        public List<TagInEdit> Tags { get; set; } = new List<TagInEdit>();
        public int Count { get; set; } = 0;

        public static TagInEditCollection Build(MatchCollection matchCollection, string html)
        {
            TagInEditCollection tagInEditCollection = new TagInEditCollection();
            foreach (Match match in matchCollection)
            {
                tagInEditCollection.Tags.Add(TagInEdit.Build(match,html));
                tagInEditCollection.Count++;
            }
            return tagInEditCollection;
        }
    }

    public class TagInEdit
    {
        internal Tag Tag { get; set; } = new Tag();
        internal int StartLine { get; set; } = 0;
        internal int EndLine { get; set; } = 0;

        public static TagInEdit Build(Match match, string html)
        {
            TagInEdit tagInEdit = new TagInEdit();

            tagInEdit.Tag = Tag.BuildIn(match);

            // Determinar las líneas de inicio y fin de la etiqueta
            tagInEdit.StartLine = GetLineNumber(html, match.Index);
            tagInEdit.EndLine = GetLineNumber(html, match.Index + match.Length);

            return tagInEdit;
        }

        private static int GetLineNumber(string text, int index)
        {
            return text.Substring(0, index).Split('\n').Length;
        }
    }

    public class TagCollection
    {
        internal List<Tag> Tags { get; set; } = new List<Tag>();
        internal int Count { get; set; } = 0;

        public TagCollection()
        {
            Tags = new List<Tag>();
            Count = 0;
        }

        /// <summary>
        /// Builds a list of Tag variables using a MatchCollection made to a HTML<br></br>
        /// </summary>
        /// <returns>A list of fully build Tag variables</returns>
        internal static TagCollection Build(MatchCollection matchCollection)
        {
            TagCollection tagCollection = new TagCollection();

            foreach (Match match in matchCollection)
            {
                // Construye cada Tag en el orden en que aparece
                tagCollection.Count++;
                Tag tag = Tag.Build(match);
                tagCollection.Tags.Add(tag);
            }
            return tagCollection;
        }
    }

    public class Tag
    {
        internal string Name { get; set; } = null;
        internal InnerText InnerText { get; set; } = new InnerText();
        internal AttributeCollection Attributes { get; set; } = new AttributeCollection();
        internal TagCollection NestedTags { get; set; } = new TagCollection();
        public bool IsSelfClosing { get; set; } = false;

        public Tag()
        {
            string Name = null;
            InnerText InnerText = new InnerText();
            AttributeCollection Attributes = new AttributeCollection();
            TagCollection NestedTags = new TagCollection();
            bool IsSelfClosing = false;
        }

        public static Tag Build(Match match)
        {
            var tag = new Tag();

            // La primera parte del Match contiene la apertura del Tag y sus atributos
            string openingTag = match.Groups[1].Value;

            // La segunda parte del Match contiene el contenido interno y los Tags anidados
            string innerContent = match.Groups[2].Value;

            // Captura el nombre del Tag
            tag.Name = GetTagName(match.Value);

            //Determinar si la Tag es autocontenida o no
            if (openingTag.EndsWith("/>"))
            {
                tag.IsSelfClosing = true;
                return tag;  // Termina aquí si la Tag es autocontenida
            }

            tag.Attributes = AttributeCollection.Build(openingTag);  // Pasa solo los atributos

            // Si la Tag no es autocontenida, captura el InnerText y Tags anidados

            tag.InnerText.Value = InnerText.GetTrueInnerText(innerContent);
            List<String> nestedTagsNameList = GetNestedTagsNames(innerContent);
            if (nestedTagsNameList != null)
            {
                HtmlEditor nestedTagsEditor = new HtmlEditor(innerContent);
                foreach (string nestedTagName in nestedTagsNameList)
                {
                    tag.NestedTags = nestedTagsEditor.GetTagCollection(nestedTagName);
                }
            }

            return tag;
        }

        public static Tag BuildIn(Match match)
        {
            var tag = new Tag();

            // La primera parte del Match contiene la apertura del Tag y sus atributos
            string openingTag = match.Groups[1].Value;

            // La segunda parte del Match contiene el contenido interno y los Tags anidados
            string innerContent = match.Groups[2].Value;

            // Captura el nombre del Tag
            tag.Name = GetTagName(match.Value);

            tag.Attributes = AttributeCollection.Build(openingTag);

            //Determinar si la Tag es autocontenida o no
            if (openingTag.EndsWith("/>"))
            {
                tag.IsSelfClosing = true;
                return null;  // Termina aquí si la Tag es autocontenida
            }

            tag.InnerText.Value = InnerText.GetTrueInnerText(innerContent);

            return tag;
        }

        public static List<String> GetNestedTagsNames(string innerContent)
        {
            string pattern = @"<\w+([^>]*)>";
            List<String> result = new List<String>();
            MatchCollection matchCollection = Regex.Matches(innerContent, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match match in matchCollection)
            {
                if (match.Success)
                {
                    result.Add(GetTagName(match.Value));
                }
            }
            return result;
        }

        public static string GetTagName(string matchValue)
        {
            string patternName = @"(?<=<)\s*(\w+)";
            Match match = Regex.Match(matchValue, patternName);
            if (match.Success)
            {
                return match.Value;
            }
            else
            {
                return null;
            }
        }
    }

    public class InnerText
    {
        internal string Value { get; set; } = null;
        internal string NewValue { get; set; } = null;
        internal AttributeCollection Attribute { get; set; } = new AttributeCollection();

        public static String GetTrueInnerText(String InnerTextString)
        {
            //CASE 1
            //Original pattern (not code)   /^(.*?)\<[^<>]+\>.*$
            //Pattern to get text in text<test></test>
            string patternCase1 = @"^(.*?)\<[^<>]+\>.*$";
            Match regexCase1 = Regex.Match(InnerTextString, patternCase1, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            string valueRegexCase1 = regexCase1.Groups[1].Value;
            if (!String.IsNullOrEmpty(valueRegexCase1) && !(valueRegexCase1.Contains("<") || valueRegexCase1.Contains(">")))
            {
                return valueRegexCase1;
            }

            //CASE 2
            //Original pattern (not code)   ^\<[^<>]+\>(.*?)\<\/[^<>]+\>$
            //Pattern to get text in <test>text</test>
            string patternCase2 = @"^\<[^<>]+\>(.*?)\<\/[^<>]+\>$";
            Match regexCase2 = Regex.Match(InnerTextString, patternCase2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            string valueRegexCase2 = regexCase2.Groups[1].Value;
            if (!String.IsNullOrEmpty(valueRegexCase2) && !(valueRegexCase2.Contains("<") || valueRegexCase2.Contains(">")))
            {
                return valueRegexCase2;
            }

            //CASE 3
            //Original pattern (not code)   ^\<[^<>]+\>\<\/[^<>]+\>(.*?)$
            //Pattern to get text in <test></test>text
            string patternCase3 = @"^\<[^<>]+\>\<\/[^<>]+\>(.*?)$";
            Match regexCase3 = Regex.Match(InnerTextString, patternCase3, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            string valueRegexCase3 = regexCase3.Groups[1].Value;
            if (!String.IsNullOrEmpty(valueRegexCase3) && !(valueRegexCase3.Contains("<") || valueRegexCase3.Contains(">")))
            {
                return valueRegexCase3;
            }

            return InnerTextString;
        }
    }

    public class AttributeCollection
    {
        internal List<Attribute> Attributes { get; set; } = new List<Attribute>();
        internal int Count { get; set; } = 0;

        public static AttributeCollection Build(String inputText)
        {
            AttributeCollection attributeCollection = new AttributeCollection();

            string pattern = @"(\w+)\s*=\s*['""]([^'""]*)['""]";

            MatchCollection matchCollection = Regex.Matches(inputText, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match match in matchCollection)
            {
                string subName = match.Groups[1].Value;
                string subValue = match.Groups[2].Value;
                attributeCollection.Attributes.Add(new Attribute(subName, subValue));
                attributeCollection.Count++;
            }
            return attributeCollection;
        }
    }

    public class Attribute
    {
        internal string Name { get; set; }
        internal string Value { get; set; }
        internal AttributeCollection SubAttributes { get; set; }

        public Attribute(string name = null, string value = null, AttributeCollection subAttributes = null)
        {
            if (name != null) Name = name;
            else Name = null;
            if (value != null) Value = value;
            else Value = null;
            if (subAttributes != null) SubAttributes = subAttributes;
            else SubAttributes = new AttributeCollection();
        }

        internal static Attribute Build(String inputText)
        {
            string pattern = @"(\w+)\s*=\s*[""']([^""']*)[""']";
            Match match = Regex.Match(inputText, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (match.Success)
            {
                string name = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                Attribute attribute = new Attribute(name, value);

                if (attribute.CountValues(value) > 0)
                {
                    attribute.SubAttributes = AttributeCollection.Build(value);
                }

                return attribute;
            }
            return null;
        }

        public int CountAttributes(string text)
        {
            //Pattern to get the individual attributes
            //Original pattern (not code)   (\w+)\s*=\s*"(.*?)"
            string pattern = @"(\w+)\s*=\s*""(.*?)""";
            MatchCollection attributesMatches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return attributesMatches.Count;
        }

        public int CountValues(string text)
        {
            //Pattern to get the individual values
            //Original pattern (not code)   \w+[-\w]*\s*:\s*([^;]+)
            string pattern = @"\w+[-\w]*\s*:\s*([^;]+)";
            MatchCollection valuesMatches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            return valuesMatches.Count;
        }
    }
}