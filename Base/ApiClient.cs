using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestSharp;
using RestSharp.Serializers;
using RestSharp.Serializers.Json;

namespace Base
{
    public class ApiClient
    {
        private static IRestClient _client;
        private static string BASE_URL = "https://jsonplaceholder.typicode.com/";

        private static JsonSerializerOptions options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static IRestClient GetRestClient()
        {
            _client = new RestClient(
               options: new() { BaseUrl = new(BASE_URL) },
               configureSerialization: s => s.UseSystemTextJson(options: options));
            return _client;
        }
    }
}
