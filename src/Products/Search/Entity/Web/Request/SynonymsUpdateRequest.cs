using Newtonsoft.Json;

namespace GroupDocs.Search.WebForms.Products.Search.Entity.Web.Request
{
    public class SynonymsUpdateRequest
    {
        [JsonProperty]
        internal string[][] SynonymGroups { get; set; }
    }
}
