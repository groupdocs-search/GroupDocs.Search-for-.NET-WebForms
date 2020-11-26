using Newtonsoft.Json;

namespace GroupDocs.Search.WebForms.Products.Search.Entity.Web.Response
{
    public class HomophonesReadResponse
    {
        [JsonProperty]
        public string[][] HomophoneGroups { get; set; }
    }
}
