using Newtonsoft.Json;

namespace GroupDocs.Search.WebForms.Products.Search.Entity.Web.Response
{
    internal class StopWordsReadResponse
    {
        [JsonProperty]
        public string[] StopWords { get; set; }
    }
}
