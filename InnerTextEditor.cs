using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace cSharpTools
{
    public class InnerTextEditor
    {
        private string html; // Stores the HTML content
        private List<string> patternList = new List<string>();
        private List<string> tagsToReplace;
        private BigTagCollection bigTagCollection = new BigTagCollection(); // Collection of tags found in the HTML
        private int currentIndexBITAGCLL = 0; // Tracks the current index for iteration on a BigTagCollection
        private int currentIndexTAGCLL = 0; // Tracks the current index for iteration on a TagCollection

        //VARIABLES PARA  DEPURACIÓN###################################################################
        private Log LogDebug;
        private bool enableDebugLog = false;
        private const string debugLogPath = "C:/Debuger/InnerTextEditor";
        private const string debugLogName = "InnerTextEditor";
        private const string debugLogFormat = "txt";
        //#############################################################################################

        /// <summary>
        /// -html: html a editar<br></br>
        /// -tagsToReplace: Lista string de tags a reeplazar
        /// </summary>
        public InnerTextEditor(string html, List<string> tagsToReplace)
        {
            this.html = html;
            this.tagsToReplace = tagsToReplace;
            // Create a regex pattern based on the provided tag name to match tags in the HTML
            foreach (string tag in tagsToReplace)
            {
                this.patternList.Add($@"<\s*{tag}\b([^>]*)>(.*?)<\/\s*{tag}\s*>");
            }

            if (enableDebugLog)
            {
                this.LogDebug = new Log(debugLogPath, debugLogName, debugLogFormat);
            }
        }

        public BigTagCollection GetBigTagCollection(List<string> userPatternList = null)
        {
            BigTagCollection bigTagCollection = new BigTagCollection();
            List<string> localPatternList;

            if (userPatternList != null)
            {
                localPatternList = userPatternList;
            }
            else
            {
                localPatternList = this.patternList;
            }

            // Find all the match collections
            BigMatchCollection bigMatchCollection = new BigMatchCollection().Build(html, localPatternList);
            // Build the BigTagCollection from the BigMatchCollection
            bigTagCollection = bigTagCollection.Build(bigMatchCollection, html);
            this.bigTagCollection = bigTagCollection;
            return bigTagCollection;
        }

        /// <summary>
        /// Extracts the collection of tags that matches the pattern<br></br>
        /// -pattern: Pattern to get tag matches from the html
        /// </summary>
        /// <returns>Fully builded TagCollection object</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public TagCollection GetTagCollection(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                throw new InvalidOperationException("pattern can not be null or empty");
            }
            // Find all matches using the regex pattern
            MatchCollection matchCollection = Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            // Build the TagCollection from the MatchCollection
            TagCollection tagCollection = new TagCollection();
            return tagCollection.Build(matchCollection, html);
        }

        /// <summary>
        /// Replaces the inner text of matched tags with new values
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public string ReplaceInnerText()
        {
            if (bigTagCollection == null || bigTagCollection.TagCollections.Count == 0)
            {
                throw new InvalidOperationException("Big tag collection is not initialized or empty. Call GetBigTagCollection() first.");
            }

            foreach (TagCollection tagCollection in bigTagCollection.TagCollections)
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
                            if ( String.Compare(originalContent , originalText, StringComparison.OrdinalIgnoreCase) == 0)
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

        /// <summary>
        /// Retrieves the inner text of the next tag in the collection<br></br>
        /// OPTIONAL -tagCollection: Declared TagCollection object<br></br>
        /// OPTIONAL -bigTagCollection: Declared BigTagCollection object<br></br>
        /// </summary>
        /// <returns>inner text value of actuall bigTagcollection</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public string GetNextInnerText(TagCollection tagCollection = null, BigTagCollection bigTagCollection = null)
        {
            if (tagCollection != null)
            {
                if (bigTagCollection == null)
                {
                    throw new ArgumentNullException("Big tag collection is not initialized or empty. Call GetBigTagCollection() first.");
                }
                else if (currentIndexBITAGCLL >= bigTagCollection.TagCollections.Count)
                {
                    throw new InvalidOperationException("Index of BigTagCollection has reached the limit at " + currentIndexBITAGCLL);
                }

                // Select the BigTagCollection to use
                BigTagCollection localBigTagCollection = bigTagCollection ?? this.bigTagCollection;

                // Get the current tag and increment the index
                IndividualTag currentTagInEdit = localBigTagCollection.TagCollections[currentIndexBITAGCLL].Tag[0];
                currentIndexBITAGCLL++;
                return currentTagInEdit.InnerText.ActualValue;
            }
            else
            {
                if (currentIndexTAGCLL >= tagCollection.Count)
                {
                    throw new InvalidOperationException("Index of TagCollection has reached the limit at " + currentIndexTAGCLL);
                }

                // Get the current tag and increment the index
                IndividualTag currentTagInEdit = tagCollection.Tag[currentIndexTAGCLL];
                currentIndexTAGCLL++;
                return currentTagInEdit.InnerText.ActualValue;
            }
        }

        /// <returns>Curent index of BigTagCollection</returns>
        public int GetCurentIndexOfBigTagCollection() { return currentIndexBITAGCLL; }

        /// <summary>Resetest the index of BigTagCollection</summary>
        public void ResetIndexOfBigTagCollection() { currentIndexBITAGCLL = 0; }

        /// <returns>Curent index of TagCollection</returns>
        public int GetCurentIndexOfTagCollection() { return currentIndexTAGCLL; }

        /// <summary>Resetest the index of TagCollection</summary>
        public void ResetIndexOfTagCollection() { currentIndexTAGCLL = 0; }

        /// <summary>
        /// Adds a new value to a tag's inner text if it matches the old value<br></br>
        /// -oldValue: Value to replace<br></br>
        /// -newValue: New value to use in ReplaceInnerText();<br></br>
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void AddNewValue(string oldValue, string newValue)
        {
            if (oldValue == null)
            {
                throw new InvalidOperationException("oldValue can't be null");
            }
            else if (newValue == null)
            {
                throw new InvalidOperationException("newValue can't be null");
            }
            else if (bigTagCollection == null)
            {
                throw new InvalidOperationException("Tag collection is not initialized. Call GetBigTagCollection() first.");
            }

            // Iterate through each tag collection in the big tag collection
            foreach (TagCollection tagCollection in bigTagCollection.TagCollections)
            {
                foreach (IndividualTag tagInEdit in tagCollection.Tag)
                {
                    string actualValue = tagInEdit.InnerText.ActualValue;
                    // If the tag's inner text matches the old value, set the new value
                    if (tagInEdit.InnerText != null && actualValue.ToLower().Trim() == oldValue.ToLower().Trim())
                    {
                        LogDebug?.Writte("Setting new value for tag: " + tagInEdit.InnerText.TagName + " from: " + oldValue + " to: " + newValue);
                        tagInEdit.InnerText.NewValue = newValue;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents a collection of MatchCollections forund in the given text<br></br>
    /// -matchCollection: //List of MatchCollections forund in the given text<br></br>
    /// -Count: Number of MatchCollections in BigMatchCollection<br></br>
    /// </summary>
    public class BigMatchCollection
    {
        public List<MatchCollection> MatchCollection { get; set; } = new List<MatchCollection>(); //List of MatchCollections forund in the given text
        public int Count { get; set; } = 0;

        /// <summary>
        /// Builds a BigMatchCollection object from a list of patterns and the text content<br></br>
        /// -text: text to do a Regex pattern search<br></br>
        /// -patternList: List of patters to apply to text<br></br>
        /// </summary>
        /// <returns></returns>
        public BigMatchCollection Build(string text, List<string> patternList)
        {
            BigMatchCollection bigMatchCollection = new BigMatchCollection();

            foreach (string pattern in patternList)
            {
                if(string.IsNullOrEmpty(pattern))
                {
                    throw new InvalidOperationException("pattern can not be null or empty");
                }
                MatchCollection matchCollection = Regex.Matches(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                bigMatchCollection.MatchCollection.Add(matchCollection);
                bigMatchCollection.Count++;
            }

            return bigMatchCollection;
        }
    }

    /// <summary>
    /// Represents a collection of TagCollections found in the HTML<br></br>
    /// -TagCollections: List of TagCollection of tags found in the HTML<br></br>
    /// -Count: Number of TagCollections in the collection<br></br>
    /// </summary>
    public class BigTagCollection
    {
        public List<TagCollection> TagCollections = new List<TagCollection>(); // List of TagCollection of tags found in the HTML
        public int Count { get; set; } = 0; // Number of TagCollections in the collection

        /// <summary>
        /// Builds a BigTagCollection object from a list of patterns and the HTML content
        /// -patterList: A list of patterns<br></br>
        /// -html: html to analice<br></br>
        /// </summary>
        /// <returns></returns>
        public  BigTagCollection Build(BigMatchCollection bigMatchCollection, string html)
        {
            BigTagCollection bigTagCollection = new BigTagCollection();
            
            foreach (MatchCollection matchCollection in bigMatchCollection.MatchCollection)
            {
                TagCollection tagCollection = new TagCollection();
                bigTagCollection.TagCollections.Add(tagCollection.Build(matchCollection, html));
                bigTagCollection.Count++;
            }
            return bigTagCollection;
        }
    }

    /// <summary>
    /// Represents a collection of IndividualTag objects found in the HTML<br></br>
    /// -Tag (List&lt;IndividualTag&gt;): List of tags found in the HTML<br></br>
    /// -Count (int): Number of tags in the collection<br></br>
    /// </summary>
    public class TagCollection
    {
        public List<IndividualTag> Tag { get; set; } = new List<IndividualTag>(); // List of tags found in the HTML
        public int Count { get; set; } = 0; // Number of tags in the collection

        /// <summary>
        /// Builds a TagCollection from a collection of regex matches<br></br>
        /// -matchCollection: A Regex.Matches of a pattern to get tag matches<br></br>
        /// -html: html to edit<br></br>
        /// </summary>
        /// <remarks>
        /// match of matchCollection to give:<br></br>
        /// &lt;a href='/central/nuevo'&gt;nuevo servicio&lt;/a&gt; ---><br></br>
        /// match.Value = &lt;a href='/central/nuevo'&gt;nuevo servicio&lt;/a&gt;<br></br>
        /// group[1] = href='/central/nuevo<br></br>group[2] = nuevo servicio<br></br>
        /// </remarks>
        /// <returns>A fully builded TagCollection object</returns>
        public TagCollection Build(MatchCollection matchCollection, string html)
        {
            IndividualTag individualTag = new IndividualTag();
            TagCollection tagCollection = new TagCollection();
            foreach (Match match in matchCollection)
            {
                tagCollection.Tag.Add(individualTag.Build(match, html));
                tagCollection.Count++;
            }
            return tagCollection;
        }
    }

    /// <summary>
    /// -InnerText (InnerText): Stores the inner text of the tag<br></br>
    /// -StartLine (int): Line number where the tag starts<br></br>
    /// -EndLine (int): Line number where the tag ends<br></br>
    /// </summary>
    public class IndividualTag
    {
        internal InnerText InnerText { get; set; } = new InnerText(); // Stores the inner text of the tag
        internal int StartLine { get; set; } = 0; // Line number where the tag starts
        internal int EndLine { get; set; } = 0; // Line number where the tag ends

        /// <summary>
        /// Builds a Tag object from a regex match<br></br>
        /// -match: Regex Match done to a tag<br></br>
        /// -html: html to edit<br></br>
        /// </summary>
        /// <remarks>
        /// match to give:<br></br>
        /// &lt;a href='/central/nuevo'&gt;nuevo servicio&lt;/a&gt; ---><br></br>
        /// match.Value = &lt;a href='/central/nuevo'&gt;nuevo servicio&lt;/a&gt;<br></br>
        /// group[1] = href='/central/nuevo<br></br>group[2] = nuevo servicio<br></br>
        /// </remarks>
        /// <returns>Fully builded IndividualTag object</returns>
        public IndividualTag Build(Match match, string html)
        {
            IndividualTag tagInEdit = new IndividualTag();

            // Set the inner text of the tag
            tagInEdit.InnerText = InnerText.Build(match);

            // Determine the start and end lines of the tag in the HTML
            tagInEdit.StartLine = GetLineNumber(html, match.Index);
            tagInEdit.EndLine = GetLineNumber(html, match.Index + match.Length);

            return tagInEdit;
        }

        /// <summary>
        /// Determines the line number of a character index in the text<br></br>
        /// -text: text to analice<br></br>
        /// -index: index of the match<br></br>
        /// </summary>
        /// <remarks>The text must also be in the match</remarks>
        /// <returns>Position in the html</returns>
        public int GetLineNumber(string text, int index)
        {
            return text.Substring(0, index).Split('\n').Length;
        }
    }

    /// <summary>
    /// -ActualValue (string): Original value of the inner text<br></br>
    /// -NewValue (string): New value to replace the original inner text<br></br>
    /// -TagName (string): The name of the tag<br></br>
    /// </summary>
    public class InnerText
    {
        internal string ActualValue { get; set; } = null; // Original value of the inner text
        internal string NewValue { get; set; } = null; // New value to replace the original inner text
        internal string TagName { get; set; } = null; // The name of the tag

        /// <summary>
        /// Builds an InnerText object from the provided match<br></br>
        /// -match: Regex Match donde to a tag<br></br>
        /// </summary>
        /// <remarks>
        /// match to give:<br></br>
        /// &lt;a href='/central/nuevo'&gt;nuevo servicio&lt;/a&gt; ---><br></br>
        /// match.Value = &lt;a href='/central/nuevo'&gt;nuevo servicio&lt;/a&gt;<br></br>
        /// group[1] = href='/central/nuevo<br></br>group[2] = nuevo servicio<br></br>
        /// </remarks>
        /// <returns>Fully builded InnerText object</returns>
        public InnerText Build(Match match)
        {
            InnerText innerText = new InnerText();
            innerText.TagName = GetTagName(match.Value);
            innerText.ActualValue = GetTrueInnerText(match.Groups[2].Value);
            return innerText;
        }

        /// <summary>
        /// Uses a generic regex pattern to get the name of the given tag as a string<br></br>
        /// pattern used: (?&lt;=&lt;)\s*(\w+)<br></br>
        /// -tagValue: string of a tag<br></br>
        /// </summary>
        /// <remarks>
        /// Tag.Value to give: &lt;a href='/central/nuevo'&gt;nuevo servicio&lt;/a&gt;<br></br>
        /// </remarks>
        /// <returns>Name of the tag</returns>
        public string GetTagName(string tagValue)
        {
            string patternName = @"(?<=<)\s*(\w+)";
            Match match = Regex.Match(tagValue, patternName);
            if (match.Success)
            {
                return match.Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Filters the given string though 3 posible cases using a generic regex patterns<br></br>
        /// -tagCompleteInnerText: The complete inner text of a tag<br></br>
        /// </summary>
        /// <remarks>
        /// CASE 1: text&lt;test&gt;&lt;/test&gt; pattern: /^(.*?)&lt;[^&lt;&gt;]+&gt;.*$<br></br>
        /// CASE 2: &lt;test&gt;text&lt;/test&gt; pattern: ^&lt;[^&lt;&gt;]+&gt;(.*?)&lt;/[^&lt;&gt;]+&gt;$<br></br>
        /// CASE 3: &lt;test&gt;&lt;/test&gt;text pattern: ^&lt;[^&lt;&gt;]+&gt;&lt;/[^&lt;&gt;]+&gt;(.*?)$<br></br>
        /// If a case succes then return<br></br>
        /// </remarks>
        /// <returns>Inner text</returns>
        public String GetTrueInnerText(String tagCompleteInnerText)
        {
            //Original pattern (not code)   /^(.*?)\<[^<>]+\>.*$
            // CASE 1: Pattern to get text before a tag like text<test></test>
            string patternCase1 = @"^(.*?)\<[^<>]+\>.*$";
            Match regexCase1 = Regex.Match(tagCompleteInnerText, patternCase1, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            string valueRegexCase1 = regexCase1.Groups[1].Value;
            if (!String.IsNullOrEmpty(valueRegexCase1) && !(valueRegexCase1.Contains("<") || valueRegexCase1.Contains(">")))
            {
                return valueRegexCase1;
            }

            //Original pattern (not code)   ^\<[^<>]+\>(.*?)\<\/[^<>]+\>$
            // CASE 2: Pattern to get text enclosed within tags like <test>text</test>
            string patternCase2 = @"^\<[^<>]+\>(.*?)\<\/[^<>]+\>$";
            Match regexCase2 = Regex.Match(tagCompleteInnerText, patternCase2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            string valueRegexCase2 = regexCase2.Groups[1].Value;
            if (!String.IsNullOrEmpty(valueRegexCase2) && !(valueRegexCase2.Contains("<") || valueRegexCase2.Contains(">")))
            {
                return valueRegexCase2;
            }

            //Original pattern (not code)   ^\<[^<>]+\>\<\/[^<>]+\>(.*?)$
            // CASE 3: Pattern to get text after empty tags like <test></test>text
            string patternCase3 = @"^\<[^<>]+\>\<\/[^<>]+\>(.*?)$";
            Match regexCase3 = Regex.Match(tagCompleteInnerText, patternCase3, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            string valueRegexCase3 = regexCase3.Groups[1].Value;
            if (!String.IsNullOrEmpty(valueRegexCase3) && !(valueRegexCase3.Contains("<") || valueRegexCase3.Contains(">")))
            {
                return valueRegexCase3;
            }

            // If no pattern matches, return the original string
            return tagCompleteInnerText;
        }
    }
}