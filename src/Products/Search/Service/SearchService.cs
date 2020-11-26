using System;
using System.Globalization;
using System.Reflection;
using Aspose.Html;
using GroupDocs.Search.Common;
using GroupDocs.Search.Dictionaries;
using GroupDocs.Search.Highlighters;
using GroupDocs.Search.Options;
using GroupDocs.Search.Results;
using GroupDocs.Search.WebForms.Products.Common.Config;
using GroupDocs.Search.WebForms.Products.Common.Entity.Web;
using GroupDocs.Search.WebForms.Products.Search.Entity.Web.Request;
using GroupDocs.Search.WebForms.Products.Search.Entity.Web.Response;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GroupDocs.Search.WebForms.Products.Common.Entity.Web.Response;
using GroupDocs.Search.WebForms.Products.Common.Entity.Web.Request;

namespace GroupDocs.Search.WebForms.Products.Search.Service
{
    public static class SearchService
    {
        private static Index index;

        internal static Dictionary<string, string> FileIndexingStatusDict { get; } = new Dictionary<string, string>();
        internal static Dictionary<string, string> PassRequiredStatusDict { get; } = new Dictionary<string, string>();

        public static SummarySearchResult Search(SearchPostedData searchRequest, GlobalConfiguration globalConfiguration)
        {
            if (index == null)
            {
                return new SummarySearchResult();
            }
            if (searchRequest == null)
            {
                return new SummarySearchResult();
            }

            SearchOptions searchOptions = new SearchOptions();
            searchOptions.UseCaseSensitiveSearch = searchRequest.CaseSensitiveSearch;
            if (!searchRequest.CaseSensitiveSearch)
            {
                searchOptions.FuzzySearch.Enabled = searchRequest.FuzzySearch;
                searchOptions.FuzzySearch.FuzzyAlgorithm = new TableDiscreteFunction(searchRequest.FuzzySearchMistakeCount);
                searchOptions.FuzzySearch.OnlyBestResults = searchRequest.FuzzySearchOnlyBestResults;
                searchOptions.KeyboardLayoutCorrector.Enabled = searchRequest.KeyboardLayoutCorrection;
                searchOptions.UseSynonymSearch = searchRequest.SynonymSearch;
                searchOptions.UseHomophoneSearch = searchRequest.HomophoneSearch;
                searchOptions.UseWordFormsSearch = searchRequest.WordFormsSearch;
                searchOptions.SpellingCorrector.Enabled = searchRequest.SpellingCorrection;
                searchOptions.SpellingCorrector.MaxMistakeCount = searchRequest.SpellingCorrectionMistakeCount;
                searchOptions.SpellingCorrector.OnlyBestResults = searchRequest.SpellingCorrectionOnlyBestResults;
            }

            var searchQuery = searchRequest.Query;
            SearchResult result;

            var alphabet = index.Dictionaries.Alphabet;
            var containedSeparators = new HashSet<char>();
            foreach (char c in searchQuery)
            {
                var type = alphabet.GetCharacterType(c);
                if (type == CharacterType.Separator)
                {
                    containedSeparators.Add(c);
                }
            }

            if (containedSeparators.Count > 0)
            {
                foreach (char specialChar in containedSeparators)
                {
                    searchQuery = searchQuery.Replace(specialChar, ' ');
                }
                result = index.Search("\"" + searchQuery + "\"", searchOptions);
            }
            else
            {
                result = index.Search(searchQuery, searchOptions);
            }

            SummarySearchResult summaryResult = new SummarySearchResult();
            List<SearchDocumentResult> foundFiles = new List<SearchDocumentResult>();

            HighlightOptions options = new HighlightOptions
            {
                TermsBefore = 5,
                TermsAfter = 5,
                TermsTotal = 13,
            };

            for (int i = 0; i < result.DocumentCount; i++)
            {
                SearchDocumentResult searchDocumentResult = new SearchDocumentResult();

                FoundDocument document = result.GetFoundDocument(i);
                HtmlFragmentHighlighter highlighter = new HtmlFragmentHighlighter();
                index.Highlight(document, highlighter, options);
                FragmentContainer[] fragmentContainers = highlighter.GetResult();

                List<string> foundPhrases = new List<string>();
                for (int j = 0; j < fragmentContainers.Length; j++)
                {
                    FragmentContainer container = fragmentContainers[j];
                    string[] fragments = container.GetFragments();
                    if (fragments.Length > 0)
                    {
                        for (int k = 0; k < fragments.Length; k++)
                        {
                            foundPhrases.Add(fragments[k].Replace("<br>", string.Empty));
                        }
                    }
                }

                searchDocumentResult.SetGuid(document.DocumentInfo.FilePath);
                searchDocumentResult.SetName(Path.GetFileName(document.DocumentInfo.FilePath));
                searchDocumentResult.SetSize(new FileInfo(document.DocumentInfo.FilePath).Length);
                searchDocumentResult.SetOccurrences(document.OccurrenceCount);
                searchDocumentResult.SetFoundPhrases(foundPhrases.ToArray());
                var terms = document.TermSequences
                    .SelectMany(s => s)
                    .Concat(document.Terms)
                    .ToArray();
                searchDocumentResult.SetTerms(terms);

                foundFiles.Add(searchDocumentResult);
            }

            summaryResult.SetFoundFiles(foundFiles.ToArray());
            summaryResult.SetTotalOccurences(result.OccurrenceCount);
            summaryResult.SetTotalFiles(result.DocumentCount);
            string searchDurationString = result.SearchDuration.ToString(@"ss\.ff");
            summaryResult.SetSearchDuration(searchDurationString.Equals("00.00") ? "< 1" : searchDurationString);
            summaryResult.SetIndexedFiles(Directory.GetFiles(globalConfiguration.Search.GetIndexedFilesDirectory(), "*", SearchOption.TopDirectoryOnly).Length);

            return summaryResult;
        }

        internal static void InitIndex(GlobalConfiguration globalConfiguration)
        {
            if (index == null)
            {
                string indexedFilesDirectory = globalConfiguration.Search.GetIndexedFilesDirectory();
                var settings = new IndexSettings();
                settings.UseRawTextExtraction = false;
                index = new Index(globalConfiguration.Search.GetIndexDirectory(), settings, true);

                index.Events.OperationProgressChanged += (sender, args) =>
                {
                    if (PassRequiredStatusDict.ContainsKey(args.LastDocumentPath) &&
                        args.LastDocumentStatus.ToString() == DocumentStatus.SuccessfullyProcessed.ToString())
                    {
                        PassRequiredStatusDict.Remove(args.LastDocumentPath);
                    }

                    if (FileIndexingStatusDict.ContainsKey(args.LastDocumentPath))
                    {
                        if (args.LastDocumentStatus.ToString() == DocumentStatus.ProcessedWithError.ToString() &&
                            PassRequiredStatusDict.ContainsKey(args.LastDocumentPath))
                        {
                            FileIndexingStatusDict[args.LastDocumentPath] = "PasswordRequired";
                        }
                        else
                        {
                            FileIndexingStatusDict[args.LastDocumentPath] = args.LastDocumentStatus.ToString();
                        }
                    }
                    else
                    {
                        if (args.LastDocumentStatus.ToString() == DocumentStatus.ProcessedWithError.ToString() &&
                            PassRequiredStatusDict.ContainsKey(args.LastDocumentPath))
                        {
                            FileIndexingStatusDict.Add(args.LastDocumentPath, "PasswordRequired");
                        }
                        else
                        {
                            FileIndexingStatusDict.Add(args.LastDocumentPath, args.LastDocumentStatus.ToString());
                        }
                    }
                };

                index.Events.PasswordRequired += (sender, args) =>
                {
                    if (PassRequiredStatusDict.ContainsKey(args.DocumentFullPath))
                    {
                        PassRequiredStatusDict[args.DocumentFullPath] = "PasswordRequired";
                    }
                    else
                    {
                        PassRequiredStatusDict.Add(args.DocumentFullPath, "PasswordRequired");
                    }
                };

                index.Add(indexedFilesDirectory);
            }
        }

        internal static IndexPropertiesResponse GetIndexProperties()
        {
            var indexProperties = new IndexPropertiesResponse();
            if (index == null) return indexProperties;

            indexProperties.IndexVersion = index.IndexInfo.Version;
            indexProperties.IndexType = index.IndexSettings.IndexType.ToString();
            indexProperties.UseStopWords = index.IndexSettings.UseStopWords;
            indexProperties.UseCharacterReplacements = index.IndexSettings.UseCharacterReplacements;
            indexProperties.AutoDetectEncoding = index.IndexSettings.AutoDetectEncoding;
            indexProperties.UseRawTextExtraction = index.IndexSettings.UseRawTextExtraction;
            var tss = index.IndexSettings.TextStorageSettings;
            indexProperties.TextStorageCompression = tss == null ? "No storage" : tss.Compression.ToString();

            return indexProperties;
        }

        internal static AlphabetReadResponse GetAlphabetDictionary()
        {
            var response = new AlphabetReadResponse();

            var alphabet = index.Dictionaries.Alphabet;
            int count = alphabet.Count;

            response.Characters = new AlphabetCharacter[count];
            int order = 0;
            for (int i = char.MinValue; i <= char.MaxValue; i++)
            {
                var characterType = alphabet.GetCharacterType((char)i);
                if (characterType != CharacterType.Separator)
                {
                    response.Characters[order] = new AlphabetCharacter()
                    {
                        Character = i,
                        Type = (int)characterType,
                    };
                    order++;
                }
            }

            return response;
        }

        internal static void SetAlphabetDictionary(AlphabetUpdateRequest request)
        {
            var alphabet = index.Dictionaries.Alphabet;
            var separator = Enumerable.Range(char.MinValue, char.MaxValue)
                .Select(v => (char)v);
            {
                int letterType = (int)CharacterType.Letter;
                var letter = request.Characters
                    .Where(ac => ac.Type == letterType)
                    .Select(ac => (char)ac.Character)
                    .ToArray();
                alphabet.SetRange(letter, CharacterType.Letter);
                separator = separator.Except(letter);
            }
            {
                int blendedType = (int)CharacterType.Blended;
                var blended = request.Characters
                    .Where(ac => ac.Type == blendedType)
                    .Select(ac => (char)ac.Character)
                    .ToArray();
                alphabet.SetRange(blended, CharacterType.Blended);
                separator = separator.Except(blended);
            }
            {
                int separateWordType = (int)CharacterType.SeparateWord;
                var separateWord = request.Characters
                    .Where(ac => ac.Type == separateWordType)
                    .Select(ac => (char)ac.Character)
                    .ToArray();
                alphabet.SetRange(separateWord, CharacterType.SeparateWord);
                separator = separator.Except(separateWord);
            }
            alphabet.SetRange(separator.ToArray(), CharacterType.Separator);
        }

        internal static StopWordsReadResponse GetStopWordDictionary()
        {
            var response = new StopWordsReadResponse();

            var dictionary = index.Dictionaries.StopWordDictionary;
            response.StopWords = dictionary.ToArray();

            return response;
        }

        internal static void SetStopWordDictionary(StopWordsUpdateRequest request)
        {
            var dictionary = index.Dictionaries.StopWordDictionary;
            dictionary.Clear();
            dictionary.AddRange(request.StopWords);
        }

        internal static HighlightTermsResponse HighlightTerms(HighlightTermsRequest request, string baseDirectory)
        {
            if (index == null)
            {
                throw new InvalidOperationException("The index has not yet been created.");
            }

            using (var document = new HTMLDocument(request.Html, string.Empty))
            {
                var highlighter = new TermHighlighter(request.CaseSensitive, index.Dictionaries.Alphabet, document, request.Terms);
                highlighter.Run();

                var response = new HighlightTermsResponse();
                response.Html = document.DocumentElement.OuterHTML;
                return response;
            }
        }

        internal static void AddFilesToIndex(PostedDataEntity[] postedData, GlobalConfiguration globalConfiguration)
        {
            string indexedFilesDirectory = globalConfiguration.Search.GetIndexedFilesDirectory();

            foreach (var entity in postedData)
            {
                string fileName = Path.GetFileName(entity.guid);
                string destFileName = Path.Combine(indexedFilesDirectory, fileName);

                if (!File.Exists(destFileName))
                {
                    File.Copy(entity.guid, destFileName);
                }

                if (!string.IsNullOrEmpty(entity.password))
                {
                    if (!index.Dictionaries.DocumentPasswords.Contains(entity.guid))
                    {
                        index.Dictionaries.DocumentPasswords.Add(entity.guid, entity.password);
                    }
                    else
                    {
                        index.Dictionaries.DocumentPasswords.Remove(entity.guid);
                        index.Dictionaries.DocumentPasswords.Add(entity.guid, entity.password);
                    }
                }
            }

            index.Update(GetUpdateOptions());
            index.Optimize();
        }

        internal static void RemoveFileFromIndex(string guid)
        {
            if (File.Exists(guid))
            {
                File.Delete(guid);

                if (FileIndexingStatusDict.ContainsKey(guid))
                {
                    FileIndexingStatusDict.Remove(guid);
                }
            }

            index.Update(GetUpdateOptions());
            index.Optimize();
        }

        internal static SynonymsReadResponse GetSynonymGroups()
        {
            if (index == null)
            {
                throw new InvalidOperationException("The index has not yet been created.");
            }

            var response = new SynonymsReadResponse();
            response.SynonymGroups = index.Dictionaries.SynonymDictionary.GetAllSynonymGroups();
            return response;
        }

        internal static void SetSynonymGroups(SynonymsUpdateRequest request)
        {
            var dictionary = index.Dictionaries.SynonymDictionary;
            dictionary.Clear();
            dictionary.AddRange(request.SynonymGroups);
        }

        internal static HomophonesReadResponse GetHomophoneGroups()
        {
            if (index == null)
            {
                throw new InvalidOperationException("The index has not yet been created.");
            }

            var response = new HomophonesReadResponse();
            response.HomophoneGroups = index.Dictionaries.HomophoneDictionary.GetAllHomophoneGroups();
            return response;
        }

        internal static void SetHomophoneGroups(HomophonesUpdateRequest request)
        {
            var dictionary = index.Dictionaries.HomophoneDictionary;
            dictionary.Clear();
            dictionary.AddRange(request.HomophoneGroups);
        }

        internal static SpellingCorrectorReadResponse GetSpellingCorrectorWords()
        {
            if (index == null)
            {
                throw new InvalidOperationException("The index has not yet been created.");
            }

            var response = new SpellingCorrectorReadResponse();
            response.Words = index.Dictionaries.SpellingCorrector.GetWords();
            return response;
        }

        internal static void SetSpellingCorrectorWords(SpellingCorrectorUpdateRequest request)
        {
            var dictionary = index.Dictionaries.SpellingCorrector;
            dictionary.Clear();
            dictionary.AddRange(request.Words);
        }

        private static UpdateOptions GetUpdateOptions()
        {
            return new UpdateOptions
            {
                Threads = 2,
                IsAsync = true,
            };
        }
    }
}
