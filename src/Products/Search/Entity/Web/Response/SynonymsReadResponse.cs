using Newtonsoft.Json;

namespace GroupDocs.Search.WebForms.Products.Search.Entity.Web.Response
{
    internal class SynonymsReadResponse
    {
        [JsonProperty]
        public string[][] SynonymGroups { get; set; }
    }
}
