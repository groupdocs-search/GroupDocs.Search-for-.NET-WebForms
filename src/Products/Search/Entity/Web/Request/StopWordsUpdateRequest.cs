using Newtonsoft.Json;

namespace GroupDocs.Search.WebForms.Products.Search.Entity.Web.Request
{
    public class StopWordsUpdateRequest
    {
        [JsonProperty]
        internal string[] StopWords { get; set; }
    }
}
