using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Base.Models
{
    public class CompanyModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("catchPhrase")]
        public string CatchPhrase { get; set; }
        [JsonPropertyName("bs")]
        public string BS {  get; set; }
    }
}
