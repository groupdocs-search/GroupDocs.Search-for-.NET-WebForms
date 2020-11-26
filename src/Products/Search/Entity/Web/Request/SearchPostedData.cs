using GroupDocs.Search.WebForms.Products.Common.Entity.Web;
using Newtonsoft.Json;

namespace GroupDocs.Search.WebForms.Products.Search.Entity.Web.Request
{
    public class SearchPostedData : PostedDataEntity
    {
        [JsonProperty()]
        public string Query { get; set; }

        [JsonProperty()]
        public bool CaseSensitiveSearch { get; set; }

        [JsonProperty()]
        public bool FuzzySearch { get; set; }

        [JsonProperty()]
        public int FuzzySearchMistakeCount { get; set; }

        [JsonProperty()]
        public bool FuzzySearchOnlyBestResults { get; set; }

        [JsonProperty()]
        public bool KeyboardLayoutCorrection { get; set; }

        [JsonProperty()]
        public bool SynonymSearch { get; set; }

        [JsonProperty()]
        public bool HomophoneSearch { get; set; }

        [JsonProperty()]
        public bool WordFormsSearch { get; set; }

        [JsonProperty()]
        public bool SpellingCorrection { get; set; }

        [JsonProperty()]
        public int SpellingCorrectionMistakeCount { get; set; }

        [JsonProperty()]
        public bool SpellingCorrectionOnlyBestResults { get; set; }
    }
}
