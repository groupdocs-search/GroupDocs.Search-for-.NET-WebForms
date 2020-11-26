using Newtonsoft.Json;

namespace GroupDocs.Search.WebForms.Products.Search.Entity.Web.Request
{
    public class SpellingCorrectorUpdateRequest
    {
        [JsonProperty]
        public string[] Words { get; set; }
    }
}
