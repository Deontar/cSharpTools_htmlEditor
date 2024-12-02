using System;
using System.Collections.Generic;

namespace cSharpTools
{
    public class Testing_traductor
    {
        Log LogTesting_traductor;

        public bool CheckForBlackListed(string text, string[] blackListedHtmlAtributes)
        {
            foreach (string atribute in blackListedHtmlAtributes)
            {
                if (text.Contains(atribute)) return true;
            }
            return false;
        }

        public void TestingFunction(string text, string html)
        {
            StartLog();

            HtmlEditor htmlEditor = new HtmlEditor(html);

            PrintTags(htmlEditor.GetTagCollection(text));
        }

        private void StartLog()
        {
            this.LogTesting_traductor = new Log("C:/Testing", "PrintTags", "txt");
        }

        private void PrintTags(TagCollection varTagCollection)
        {
            /*Estructura de variable TagCollection
             * TagCollection ─┐
             *              Tag ─┐
             *                Name: Name of the Tag
             *                InnerText: Innertext of the tag
             *                InnerText Attribute─┐
             *                                 Name: Name of the attribute
             *                                 Value: Value of the name
             *                                 ValueCollection─┐
             *                                                Name:  Name of the value
             *                                                Value: Value or List of Values of the attribute
             *                TAG Attribute ─┐
             *                            Name: Name of the attribute
             *                            Value: Value of the name
             *                                 ValueCollection─┐
             *                                                Name:  Name of the value
             *                                                Value: Value or List of Values of the attribute
             */
            LogTesting_traductor.Writte($"Numero de tags: {varTagCollection.Count - 1}");
            int index1 = 0;
            foreach (var tag in varTagCollection.Tags)
            {
                LogTesting_traductor.Writte($"TAG{index1}─┐");
                LogTesting_traductor.Writte($"  name: {tag.Name}");
                LogTesting_traductor.Writte($"  InnerText: {tag.InnerText.Value}");
                foreach (Attribute attribute in tag.InnerText.Attribute)
                {
                    LogTesting_traductor.Writte($"  INNERTEXT ATTRIBUTE{attribute.Count}─┐");
                    LogTesting_traductor.Writte($"                   Name: {attribute.Name}");
                    foreach (string individualValue in attribute.Value)
                    {
                        LogTesting_traductor.Writte($"                   Value{attribute.Count}: {individualValue}");
                    }
                }
                if (tag.Attribute != null && tag.Attribute.Count > 0)
                {
                    foreach (var attribute in tag.Attribute)
                    {
                        LogTesting_traductor.Writte($"  ATTRIBUTE{attribute.Count}─┐");
                        LogTesting_traductor.Writte($"              Name: {attribute.Name}");
                        foreach (string individualValue in  attribute.Value)
                        {
                            LogTesting_traductor.Writte($"              Value{attribute.Count}: {individualValue}");
                        }
                    }
                }
                LogTesting_traductor.Writte("\n\n");
                index1++;
            }
        }
    }
}
