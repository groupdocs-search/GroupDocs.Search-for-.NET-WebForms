using Newtonsoft.Json;

namespace GroupDocs.Search.WebForms.Products.Search.Entity.Web.Request
{
    public class HomophonesUpdateRequest
    {
        [JsonProperty]
        internal string[][] HomophoneGroups { get; set; }
    }
}
