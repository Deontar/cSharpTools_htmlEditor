using System;
using System.Collections.Generic;
using System.IO;

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
            //InnerTextEditor innerTextEditor = new InnerTextEditor(html, text);
            //TagCollection tagCollection = innerTextEditor.GetTagCollection();
            //foreach (IndividualTag tag in tagCollection.Tag)
            //{
            //    string oldInnerText = innerTextEditor.GetInnerNextText();
            //    innerTextEditor.AddNewValue(oldInnerText, "palabras");
            //}
            //string newHTML = innerTextEditor.ReplaceInnerText();
            //
            //return newHTML;
        }

        private void StartLog()
        {
            this.LogTesting_traductor = new Log("C:/Testing", "Testing", "txt");
        }
    }
}
