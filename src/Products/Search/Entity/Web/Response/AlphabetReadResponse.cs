using Newtonsoft.Json;

namespace GroupDocs.Search.WebForms.Products.Search.Entity.Web.Response
{
    internal class AlphabetReadResponse
    {
        [JsonProperty]
        public AlphabetCharacter[] Characters { get; set; }
    }
}
