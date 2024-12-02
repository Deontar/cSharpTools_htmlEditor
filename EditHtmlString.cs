using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using static cSharpTools.EditHtmlString;

namespace cSharpTools
{
    public class EditHtmlString
    {
        public string html;
        private TagCollection tagCollection = null;
        public EditHtmlString(string html)
        {
            this.html = html;
        }

        public class Get()
        {
            public Tag Tag (string tagName)
            {
                string pattern = $"<\\s*{tagName}([^>]*)>(.*?)<\\/\\s*{tagName}\\s*>";
                Match _match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
            }
            
            public TagCollection TagCollection ()
            {
                return null;
            }
        }
    

        public class Set
        {

        }

internal class Build
        {
            ///<summary>
            /// Builds a Tag variable using the match of a tag made to an HTML<br></br>
            /// -match: a match of a tag made to a HTML
            ///</summary>
            /// <returns>Fully build Tag variable</returns>
            internal Tag Tag(Match _match)
            {
                if (!_match.Success) throw new Exception($"Buil.Tag failed to execute due to failed match {_match.Success}");

                Tag varTag = new Tag();
                varTag.Name = _match.Groups[1].Value;
                varTag.InnerText = _match.Groups[2].Value;
                varTag.Attribute = new List<Attribute>();
                varTag.Style = new List<Style>();

                //Pattern to get attribute and value of the tag match
                string pattern = @"\s+\w+(?:\s*=\s*([""']).*?\1|\s*=\s*[^>\s]+)?";
                MatchCollection _matchCollection = Regex.Matches(_match.Groups[1].Value, pattern, RegexOptions.IgnoreCase);
                foreach (Match individualMatch in _matchCollection)
                {
                    if (individualMatch.Success)
                    {
                        Attribute _attribute = Attribute(individualMatch);
                        if (_attribute != null)
                        {
                            if (_attribute.Code.ToLower() == "style")
                            {
                                varTag.Style = Style(_attribute);
                            }
                            else
                            {
                                varTag.Attribute.Add(_attribute);
                            }
                        }
                    }
                }

                return varTag;
            }

            ///<summary>
            /// Builds a Attribute variable using the match of a tag made to an HTML<br></br>
            /// -match: a match of a tag made to a HTML
            ///</summary>
            /// <returns>Fully build Attribute variable</returns>
            internal Attribute Attribute(Match _match)
            {
                if (!_match.Success) return null;

                //Pattern to get the code and its value
                //Original pattern (not code) /(\w+)\s*=\s*"([^"]*)"
                string patternCodeValue = @"(\w+)\s*=\s*['""]([^'""]*)['""]";
                Match matchCodeValue = Regex.Match(_match.Value, patternCodeValue, RegexOptions.IgnoreCase);

                if (matchCodeValue.Success)
                {
                    Attribute attribute = new Attribute
                    {
                        Code = matchCodeValue.Groups[1].Value,
                        Value = matchCodeValue.Groups[2].Value
                    };

                    return attribute;
                }

                return null;
            }

            /// <summary>
            /// Builds a Style variable using fully build Attribbute variable
            /// -Attribute: A Attribute variable build using build.attribute
            /// </summary>
            /// <returns>Fully build Style variable</returns>
            internal List<Style> Style(Attribute _attribute)
            {
                List<Style> styles = new List<Style>();
                // Pattern to get the attributes of Style and its values
                string stylePattern = @"(\w+[-\w]*)\s*:\s*([^;]+);?";
                // Creates a collection of all the matches of the pattern in styleValue
                MatchCollection styleMatches = Regex.Matches(_attribute.Value, stylePattern, RegexOptions.IgnoreCase);
                foreach (Match styleMatch in styleMatches)
                {
                    if (styleMatch.Success)
                    {
                        Style style = new Style
                        {
                            Code = styleMatch.Groups[1].Value.Trim(),
                            Value = styleMatch.Groups[2].Value.Trim()
                        };
                        styles.Add(style);
                    }
                }
                return styles;
            }
 
            /// <summary>
            /// Builds a list of Tag variables using a MatchCollection made to a HTML<br></br>
            /// </summary>
            /// <returns>A list of fully build Tag variables</returns>
            internal TagCollection TagCollection(MatchCollection _matchCollection)
            {
                TagCollection _tagCollection = new TagCollection();
                foreach (Match _match in _matchCollection)
                {
                    if (_match.Success)
                    {
                        _tagCollection.Tag.Add(Tag(_match));
                    }
                }
                return _tagCollection;
            }   
        }
    }

    /// <summary>
    /// A list of Tag variables
    /// </summary>
    public class TagCollection
    {
        internal List<Tag> Tag { get; set; }
    }

    /// <summary>
    /// - Name: Name of tag<br></br>
    /// - InnerText: Inerr text of tag<br></br>
    /// - Attribute: {code, value}<br></br>
    /// -- Code: code of attribute<br></br>
    /// -- Value: Value of the attribute<br></br>
    /// - Style: List of style properties for the tag<br></br>
    /// -- Code: code of attribute<br></br>
    /// -- Value: Value of the attribute<br></br>
    /// </summary>
    public class Tag
    {
        internal string Name { get; set; }
        internal string InnerText { get; set; }
        internal List<Attribute> Attribute { get; set; }
        internal List<Style> Style { get; set; }
    }

    /// <summary>
    /// - Code: code of attribute<br></br>
    /// - Value: Value of the attribute<br></br>
    /// </summary>
    public class Attribute
    {
        internal string Code { get; set; }
        internal string Value { get; set; }
    }

    /// <summary>
    /// - Code: code of attribute<br></br>
    /// - Value: Value of the attribute<br></br>
    /// </summary>
    public class Style
    {
        internal string Code { get; set; }
        internal string Value { get; set; }
    }
}
