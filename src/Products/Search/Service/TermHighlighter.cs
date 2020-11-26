using Aspose.Html;
using Aspose.Html.Dom;
using GroupDocs.Search.Dictionaries;
using System;
using System.Collections.Generic;

namespace GroupDocs.Search.WebForms.Products.Search.Service
{
    internal class TermHighlighter
    {
        private readonly HTMLDocument document;
        private readonly TermFinder termFinder;
        private List<Text> result;

        public TermHighlighter(bool isCaseSensitive, Alphabet alphabet, HTMLDocument document, string[] terms)
        {
            this.document = document;
            termFinder = new TermFinder(isCaseSensitive, alphabet, terms);
        }

        public void Run()
        {
            if (result != null)
            {
                throw new InvalidOperationException("The operation cannot be run twice.");
            }

            result = new List<Text>();
            foreach (var child in document.Children)
            {
                Find(child);
            }

            foreach (var node in result)
            {
                termFinder.Start(node);
            }
        }

        private void Find(Node node)
        {
            if (node.NodeName.Equals("STYLE", StringComparison.InvariantCultureIgnoreCase) ||
                node.NodeName.Equals("TITLE", StringComparison.InvariantCultureIgnoreCase) ||
                node.NodeName.Equals("HEAD", StringComparison.InvariantCultureIgnoreCase) ||
                node.NodeName.Equals("SCRIPT", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (node.NodeType == 3)
            {
                var text = node.TextContent;
                if (!string.IsNullOrEmpty(text))
                {
                    result.Add((Text)node);
                }
            }

            foreach (var child in node.ChildNodes)
            {
                Find(child);
            }
        }
    }
}
