using Aspose.Html.Dom;
using GroupDocs.Search.Dictionaries;
using System;

namespace GroupDocs.Search.WebForms.Products.Search.Service
{
    public class TermFinder
    {
        private enum State
        {
            Separators,
            Term,
        }

        private readonly bool isCaseSensitive;
        private readonly Alphabet alphabet;
        private readonly string[] terms;
        private Text node;
        private int index;
        private State lastState;
        private int termStart;

        public TermFinder(bool isCaseSensitive, Alphabet alphabet, string[] terms)
        {
            this.isCaseSensitive = isCaseSensitive;
            this.alphabet = alphabet;
            this.terms = terms;
        }

        public void Start(Text textNode)
        {
            node = textNode;
            index = 0;
            lastState = State.Separators;

            while (index < node.TextContent.Length)
            {
                char character = node.TextContent[index];
                var type = alphabet.GetCharacterType(character);
                if (type == CharacterType.Separator)
                {
                    if (lastState == State.Term)
                    {
                        lastState = State.Separators;
                        int termLength = index - termStart;
                        HandleTerms(termStart, termLength);
                    }
                }
                else
                {
                    if (lastState == State.Separators)
                    {
                        lastState = State.Term;
                        termStart = index;
                    }
                }
                index++;
            }

            if (lastState == State.Term)
            {
                int termLength = node.TextContent.Length - termStart;
                HandleTerms(termStart, termLength);
            }
        }

        private void HandleTerms(int termStart, int termLength)
        {
            var text = node.TextContent;
            for (int i = 0; i < terms.Length; i++)
            {
                var term = terms[i];
                if (term.Length == termLength)
                {
                    bool match = true;
                    for (int j = 0; j < termLength; j++)
                    {
                        int index = termStart + j;
                        if (isCaseSensitive)
                        {
                            if (term[j] != text[index])
                            {
                                match = false;
                                break;
                            }
                        }
                        else
                        {
                            if (char.ToUpperInvariant(term[j]) != char.ToUpperInvariant(text[index]))
                            {
                                match = false;
                                break;
                            }
                        }
                    }
                    if (match)
                    {
                        HighlightTerm(termStart, termLength);
                        return;
                    }
                }
            }
        }

        private void HighlightTerm(int termStart, int termLength)
        {
            node = node.SplitText(termStart);
            var lastTextNode = node.SplitText(termLength);

            var span = node.OwnerDocument.CreateElement("span");
            span.ClassName = "gd-found-term";

            node.ParentNode.ReplaceChild(span, node);
            span.AppendChild(node);

            node = lastTextNode;
            index = 0;
        }
    }
}
