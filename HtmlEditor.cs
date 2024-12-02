using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace cSharpTools
{
    public class HtmlEditor
    {
        private string html;
        private Log logHtmlEditor;
        private bool debug = true;

        public HtmlEditor(string html)
        {
            this.html = html;
            if (debug)
            {
                this.logHtmlEditor = new Log("C:/Testing", "LogHtmlEditor", "txt");
            }
        }

        /// <returns>A fully build Tag variable of the first match in html</returns>
        public Tag GetTag(string tagName)
        {
            //Original regex (not adapted to code) <\s*{texto}[^>]*>.*?<\/\s*{texto}\s*>
            string pattern = $"<\\s*{tagName}([^>]*)>(.*?)<\\/\\s*{tagName}\\s*>";
            Match _match = Regex.Match(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Tag _tag = new Tag().Build(_match);
            return _tag;
        }

        /// <returns>A fully build TagCollection variable of all the matches in html</returns>
        public TagCollection GetTagCollection(string tagName)
        {
            //Original regex (not adapted to code) <\s*TEXT([^>]*)>(.*?)<\/\s*TEXT\s*>
            string pattern = $"<\\s*{tagName}([^>]*)>(.*?)<\\/\\s*{tagName}\\s*>";
            MatchCollection matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            TagCollection _tagCollection = new TagCollection().Build(matches);

            return _tagCollection;
        }
    }
    
    /// <summary>
    /// A list of Tag variables
    /// </summary>
    public class TagCollection
    {
        internal List<Tag> Tags { get; set; } = new List<Tag>();
        internal int Count { get; set; } = 0;

        /// <summary>
        /// Builds a list of Tag variables using a MatchCollection made to a HTML<br></br>
        /// </summary>
        /// <returns>A list of fully build Tag variables</returns>
        internal TagCollection Build(MatchCollection tagMatchCollection)
        {
            TagCollection tagCollection = new TagCollection();
            foreach (Match individualTagMatch in tagMatchCollection)
            {
                try
                {
                    tagCollection.Tags.Add(new Tag().Build(individualTagMatch));
                    tagCollection.Count = Count++;
                }
                catch
                {
                    tagCollection.Tags.Add(new Tag());
                    tagCollection.Count = Count++;
                }
            }
            return tagCollection;
        }
    }

    /// <summary>
    /// 1. Tag<br></br>
    /// 1.2 Name: Name of tag<br></br>
    /// 1.3 InnerText: Inerr text of tag<br></br>
    /// 1.4 Attribute: {code, value}<br></br>
    /// 1.4.1 Name: Name of attribute<br></br>
    /// 1.4.1 Value: Value or list of Values of the code<br></br>
    /// </summary>
    public class Tag
    {
        internal string Name { get; set; } = null;
        internal InnerText InnerText { get; set; } = null;
        internal List<Attribute> Attribute { get; set; } = new List<Attribute>();

        ///<summary>
        /// Builds a Tag variable using the match of a tag made to an HTML<br></br>
        /// -match: a match of a tag made to a HTML
        ///</summary>
        /// <returns>Fully build Tag variable</returns>
        public Tag Build(Match individualTagMatch)
        {
            if (!individualTagMatch.Success) throw new Exception($"Buil.Tag failed to execute due to failed match");

            Tag varTag = new Tag();
            //Pattern to get the text right after <
            varTag.Name = Regex.Match(individualTagMatch.Value, @"(?<=<)\s*(\w+)").Value;
            //regex para innerText y attribute

            varTag.InnerText = new InnerText().Build(individualTagMatch.Groups[2].Value);
            varTag.Attribute = new Attribute().Build(individualTagMatch.Groups[1].Value);
            return varTag;
        }
    }

    public class InnerText
    {
        internal string Value { get; set; } = null;
        internal List<Attribute> Attribute { get; set; } = new List<Attribute>();

        /// <summary>
        /// -TagMatch: A regex Match done to a html tag
        /// </summary>
        /// <returns>A fully build InnerText variable and its attributes if it has</returns>
        public InnerText Build(String InnerTextString)
        {
            InnerText varInnerText = new InnerText();
            //Pattern to get attribute + value of the tagMatch 
            //Original pattern (not code)   \s+\w+(?:\s*=\s*(['"]).*?\1|\s*=\s*[^>\s]+)?
            string pattern = @"<\s*(\w+)(\s+\w+="".*?"")+\s*>(.*?)<\/\1>";
            //Equivalent to if (match.success)
            if (Regex.IsMatch(InnerTextString, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                varInnerText.Value = GetTrueInnerText(InnerTextString);
                varInnerText.Attribute = new Attribute().Build(InnerTextString);
            }
            else
            {
                varInnerText.Value = InnerTextString;
            }
            return varInnerText;
        }

        /// <summary>
        /// -TagMatch: A regex Match done to a html tag
        /// </summary>
        /// <returns>The true InnerText of a tag that is with the attributes</returns>
        public String GetTrueInnerText(String InnerTextString)
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

            return "";
        }
    }

    /// <summary>
    /// - Code: code of attribute<br></br>
    /// - Value: Value of the attribute<br></br>
    /// </summary>
    public class Attribute
    {
        internal string Name { get; set; } = null;
        internal List<string> Value { get; set; } = default(List<string>);
        internal int Count { get; set; } = 0;

        ///<summary>
        /// Builds a list of Attribute variables using the match of a tag made to an HTML<br></br>
        /// -match: a match of a tag made to a HTML
        ///</summary>
        /// <returns>Fully built list of Attribute variables</returns>
        internal List<Attribute> Build(String InnerTextString)
        {
            List<Attribute> attributes = new List<Attribute>();
            //Pattern to get attribute + value of the tagMatch 
            //Original pattern (not code)   \s+\w+(?:\s*=\s*(['"]).*?\1|\s*=\s*[^>\s]+)?
            string pattern = @"\s+\w+(?:\s*=\s*([""']).*?\1|\s*=\s*[^>\s]+)?";
            MatchCollection atrrMatch = Regex.Matches(InnerTextString, pattern, RegexOptions.IgnoreCase);
            int atrrCount = 0;
            foreach (Match individualMatch in atrrMatch)
            {
                if (individualMatch.Success)
                {
                    //Pattern to individualy get the name and value of atrrMatch
                    //Original pattern (not code) (\w+)\s*=\s*"([^"]*)"
                    string nameValuePattern = @"(\w+)\s*=\s*['""]([^'""]*)['""]";
                    Match nameValueMatch = Regex.Match(InnerTextString, nameValuePattern, RegexOptions.IgnoreCase);
                    string name = nameValueMatch.Groups[1].Value;
                    string value = nameValueMatch.Groups[2].Value;

                    if (nameValueMatch.Success && CountValues(nameValueMatch.Groups[2].Value) > 1)
                    {
                        Attribute attribute = new Attribute();
                        atrrCount++;

                        attribute.Name = name;
                        attribute.Value = BuildMutipleValues(nameValueMatch.Groups[2].Value);
                        attribute.Count = atrrCount;

                        attributes.Add(attribute);
                    }
                    else if (nameValueMatch.Success)
                    {
                        Attribute attribute = new Attribute();
                        atrrCount++;

                        attribute.Name = name;
                        attribute.Value = new List<string> { value };
                        attribute.Count = atrrCount;

                        attributes.Add(attribute);
                    }
                }
            }
            return attributes;
        }

        /// <summary>
        /// Builds a string list of all the values in a Attribute.code<br></br>
        /// -atrrValue: Value[2] of Match done to an Attribute and its Value<br></br>
        /// </summary>
        /// <returns>String List of all the values in atrrValue</returns>
        internal List<string> BuildMutipleValues(String atrrValue)
        {
            // Pattern to get the attributes of Style and its values
            string stylePattern = @"(\w+[-\w]*)\s*:\s*([^;]+);?";
            // Creates a collection of all the matches of the pattern in atrrValue
            MatchCollection valuesMatches = Regex.Matches(atrrValue, stylePattern, RegexOptions.IgnoreCase);
            List<string> StyleStringList = new List<string>();
            foreach (Match individualMatch in valuesMatches)
            {
                if (individualMatch.Success)
                {
                    StyleStringList.Add(individualMatch.Groups[2].Value);
                }
            }
            return StyleStringList;
        }

        /// <summary>
        /// -values: Value of an attribute<br></br>
        /// </summary>
        /// <returns>Ammount of values</returns>
        internal int CountValues(string values)
        {
            // Pattern to get the individual values in case it has multiple values
            string stylePattern = @"(\w+[-\w]*)\s*:\s*([^;]+);?";
            MatchCollection styleMatches = Regex.Matches(values, stylePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            return styleMatches.Count;
        }
    }

    public class ValueCollection
    {
        internal List<ValueSingle> Value { get; set; } = new List<ValueSingle>();
        internal int Count { get; set; } = 0;

        public ValueCollection Build(string atrrValue)
        {
            // Pattern to get the attributes of Style and its values
            string stylePattern = @"(\w+[-\w]*)\s*:\s*([^;]+);?";
            // Creates a collection of all the matches of the pattern in atrrValue
            MatchCollection valuesMatches = Regex.Matches(atrrValue, stylePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            ValueCollection valueCollection = new ValueCollection();
            foreach (Match individualMatch in valuesMatches)
            {
                if (individualMatch.Success)
                {
                    valueCollection.Value.Add(new ValueSingle().Build(individualMatch.Groups[2].Value));
                    valueCollection.Count++;
                }
            }
            return valueCollection;
        }
    }

    public class ValueSingle
    {
        internal string Name { get; set; } = "";
        internal string Value { get; set; } = "";

        public ValueSingle Build(string test)
        {
            return null;
        }
    }
}