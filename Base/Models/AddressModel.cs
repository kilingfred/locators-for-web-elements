using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Base.Models
{
    public class AddressModel
    {
        [JsonPropertyName("street")]
        public string Street { get; set; }
        [JsonPropertyName("suite")]
        public string Suite { get; set; }
        [JsonPropertyName("city")]
        public string City { get; set; }
        [JsonPropertyName("zipcode")]
        public string Zipcode { get; set; }
        [JsonPropertyName("geo")]
        public GeoModel Geo { get; set; }

        public class GeoModel
        {
            [JsonPropertyName("lat")]
            public string Latitude { get; set; }
            [JsonPropertyName("lng")]
            public string Lng { get; set; }
        }
    }
}
