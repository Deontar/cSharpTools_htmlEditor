using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace cSharpTools
{
    public class InnerTextEditor
    {
        private string html; // Stores the HTML content
        private List<string> patternList = new List<string>();
        private List<string> tagsToReplace;
        private BigTagCollection bigTagCollection = new BigTagCollection(); // Collection of tags found in the HTML
        private int currentIndex = 0; // Tracks the current index for iteration


        //VARIABLES PARA  DEPURACIÓN###################################################################
        private Log LogDebug;
        private const bool enableDebugLog = true;
        private const string debugLogPath = "C:/Debuger/InnerTextEditor";
        private const string debugLogName = "InnerTextEditor";
        private const string debugLogFormat = "txt";
        //#############################################################################################

        public InnerTextEditor(string html, List<string> tagsToReplace)
        {
            this.html = html;
            this.tagsToReplace = tagsToReplace;
            // Create a regex pattern based on the provided tag name to match tags in the HTML
            foreach (string tag in tagsToReplace)
            {
                this.patternList.Add($"<\\s*{tag}([^>]*)>(.*?)<\\/\\s*{tag}\\s*>");
            }

            if (enableDebugLog)
            {
                this.LogDebug = new Log(debugLogPath, debugLogName, debugLogFormat);
            }
        }

        public BigTagCollection GetBigTagCollection()
        {
            foreach (string pattern in this.patternList)
            {
                bigTagCollection.TagCollection.Add(GetTagCollection(pattern));
            }
            return bigTagCollection;
        }

        // Extracts the collection of tags that match the pattern
        public TagCollection GetTagCollection(string pattern)
        {
            // Find all matches using the regex pattern
            MatchCollection matchCollection = Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Build the TagCollection from the matches
            TagCollection tagCollection = TagCollection.Build(matchCollection, html);
            return tagCollection;
        }

        // Replaces the inner text of matched tags with new values
        public string ReplaceInnerText()
        {
            if (bigTagCollection == null || bigTagCollection.TagCollection.Count == 0)
            {
                throw new InvalidOperationException("Tag collection is not initialized or empty. Call GetBigTagCollection() first.");
            }

            foreach (TagCollection tagCollection in bigTagCollection.TagCollection)
            {
                foreach (IndividualTag tag in tagCollection.Tag)
                {
                    if (tag != null && !string.IsNullOrEmpty(tag.InnerText.NewValue))
                    {
                        string newText = tag.InnerText.NewValue;
                        string originalText = tag.InnerText.ActualValue;

                        LogDebug?.Writte("Processing tag: " + tag.InnerText.TagName);
                        LogDebug?.Writte("Original text: " + originalText);
                        LogDebug?.Writte("New text: " + newText);

                        string tagPattern = $"(<\\s*{tag.InnerText.TagName}[^>]*>)(.*?)(<\\/\\s*{tag.InnerText.TagName}\\s*>)";
                        Regex regex = new Regex(tagPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        LogDebug?.Writte("Using tag pattern: " + tagPattern);

                        html = regex.Replace(html, m =>
                        {
                            string openingTag = m.Groups[1].Value;
                            string originalContent = m.Groups[2].Value;
                            string closingTag = m.Groups[3].Value;

                            LogDebug?.Writte("Found match: " + m.Value);
                            LogDebug?.Writte("Opening tag: " + openingTag);
                            LogDebug?.Writte("Original content in match: " + originalContent);
                            LogDebug?.Writte("Closing tag: " + closingTag);

                            // Verify that the original content matches the expected value before replacing
                            if (NormalizeText(originalContent) == NormalizeText(originalText))
                            {
                                LogDebug?.Writte("Original content matches. Proceeding with replacement.");
                                LogDebug?.Writte("Replacing: " + originalContent + " with: " + newText);
                                return $"{openingTag}{newText}{closingTag}";
                            }
                            else
                            {
                                LogDebug?.Writte("Original content does not match the expected value. Skipping replacement.");
                                return m.Value; // Return the original value if it doesn't match the expected value
                            }
                        });
                    }
                }
            }

            LogDebug?.Writte("Final HTML content after all replacements:");
            LogDebug?.Writte(html);
            return html;
        }

        // Helper method to normalize text for comparison
        private string NormalizeText(string text)
        {
            return text.Trim().ToLower();
        }


        // Retrieves the inner text of the next tag in the collection
        public string GetInnerNextText()
        {
            if (bigTagCollection == null || currentIndex >= bigTagCollection.TagCollection.Count) { return string.Empty; }

            // Get the current tag and increment the index
            IndividualTag currentTagInEdit = bigTagCollection.TagCollection[currentIndex].Tag[0];
            currentIndex++;
            return currentTagInEdit.InnerText.ActualValue;
        }

        // Adds a new value to a tag's inner text if it matches the old value
        public void AddNewValue(string oldValue, string newValue)
        {
            if (bigTagCollection == null)
            {
                throw new InvalidOperationException("Tag collection is not initialized. Call GetBigTagCollection() first.");
            }

            foreach (TagCollection tagCollection in bigTagCollection.TagCollection)
            {
                foreach (IndividualTag tagInEdit in tagCollection.Tag)
                {
                    if (tagInEdit.InnerText != null && tagInEdit.InnerText.ActualValue == oldValue)
                    {
                        LogDebug?.Writte("Setting new value for tag: " + tagInEdit.InnerText.TagName + " from: " + oldValue + " to: " + newValue);
                        tagInEdit.InnerText.NewValue = newValue;
                    }
                }
            }
        }
    }
        public class BigTagCollection
        {
            public List<TagCollection> TagCollection = new List<TagCollection>();

            public static BigTagCollection Build()
            {
                return null;
            }
        }

        public class TagCollection
        {
            public List<IndividualTag> Tag { get; set; } = new List<IndividualTag>(); // List of tags found in the HTML
            public int Count { get; set; } = 0; // Number of tags in the collection

            // Builds a TagCollection from a collection of regex matches
            public static TagCollection Build(MatchCollection matchCollection, string html)
            {
                TagCollection tagCollection = new TagCollection();
                foreach (Match match in matchCollection)
                {
                    tagCollection.Tag.Add(IndividualTag.Build(match, html));
                    tagCollection.Count++;
                }
                return tagCollection;
            }
        }

        public class IndividualTag
        {
            internal InnerText InnerText { get; set; } = new InnerText(); // Stores the inner text of the tag
            internal int StartLine { get; set; } = 0; // Line number where the tag starts
            internal int EndLine { get; set; } = 0; // Line number where the tag ends

            // Builds a Tag object from a regex match
            internal static IndividualTag Build(Match match, string html)
            {
                IndividualTag tagInEdit = new IndividualTag();

                // Set the inner text of the tag
                tagInEdit.InnerText = InnerText.Build(match);

                // Determine the start and end lines of the tag in the HTML
                tagInEdit.StartLine = GetLineNumber(html, match.Index);
                tagInEdit.EndLine = GetLineNumber(html, match.Index + match.Length);

                return tagInEdit;
            }

            // Determines the line number of a character index in the text
            private static int GetLineNumber(string text, int index)
            {
                return text.Substring(0, index).Split('\n').Length;
            }
        }

        public class InnerText
        {
            internal string ActualValue { get; set; } = null; // Original value of the inner text
            internal string NewValue { get; set; } = null; // New value to replace the original inner text
            internal string TagName { get; set; } = null; // The name of the tag

            // Builds an InnerText object from the provided content
            public static InnerText Build(Match match)
            {
                InnerText innerText = new InnerText();
                innerText.TagName = GetTagName(match.Value);
                innerText.ActualValue = GetTrueInnerText(match.Groups[2].Value);
                return innerText;
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

            // Extracts the true inner text from the provided string, ignoring tags
            public static String GetTrueInnerText(String InnerTextString)
            {
                //Original pattern (not code)   /^(.*?)\<[^<>]+\>.*$
                // CASE 1: Pattern to get text before a tag like text<test></test>
                string patternCase1 = @"^(.*?)\<[^<>]+\>.*$";
                Match regexCase1 = Regex.Match(InnerTextString, patternCase1, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                string valueRegexCase1 = regexCase1.Groups[1].Value;
                if (!String.IsNullOrEmpty(valueRegexCase1) && !(valueRegexCase1.Contains("<") || valueRegexCase1.Contains(">")))
                {
                    return valueRegexCase1;
                }

                //Original pattern (not code)   ^\<[^<>]+\>(.*?)\<\/[^<>]+\>$
                // CASE 2: Pattern to get text enclosed within tags like <test>text</test>
                string patternCase2 = @"^\<[^<>]+\>(.*?)\<\/[^<>]+\>$";
                Match regexCase2 = Regex.Match(InnerTextString, patternCase2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                string valueRegexCase2 = regexCase2.Groups[1].Value;
                if (!String.IsNullOrEmpty(valueRegexCase2) && !(valueRegexCase2.Contains("<") || valueRegexCase2.Contains(">")))
                {
                    return valueRegexCase2;
                }

                //Original pattern (not code)   ^\<[^<>]+\>\<\/[^<>]+\>(.*?)$
                // CASE 3: Pattern to get text after empty tags like <test></test>text
                string patternCase3 = @"^\<[^<>]+\>\<\/[^<>]+\>(.*?)$";
                Match regexCase3 = Regex.Match(InnerTextString, patternCase3, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                string valueRegexCase3 = regexCase3.Groups[1].Value;
                if (!String.IsNullOrEmpty(valueRegexCase3) && !(valueRegexCase3.Contains("<") || valueRegexCase3.Contains(">")))
                {
                    return valueRegexCase3;
                }

                // If no pattern matches, return the original string
                return InnerTextString;
            }
        }
}