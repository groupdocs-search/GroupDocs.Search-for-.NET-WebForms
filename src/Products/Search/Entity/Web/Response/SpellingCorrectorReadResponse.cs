using Newtonsoft.Json;

namespace GroupDocs.Search.WebForms.Products.Search.Entity.Web.Response
{
    internal class SpellingCorrectorReadResponse
    {
        [JsonProperty]
        public string[] Words { get; set; }
    }
}
