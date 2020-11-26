using GroupDocs.Search.WebForms.Products.Search.Entity.Web.Response;
using Newtonsoft.Json;

namespace GroupDocs.Search.WebForms.Products.Search.Entity.Web.Request
{
    public class AlphabetUpdateRequest
    {
        [JsonProperty]
        internal AlphabetCharacter[] Characters { get; set; }
    }
}
