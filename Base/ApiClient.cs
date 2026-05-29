using Base.Models;
using Base.Utils;
using RestSharp;
using RestSharp.Serializers;
using RestSharp.Serializers.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Base
{
    public class ApiClient
    {
        private IRestClient _restClient;

        public ApiClient(string path)
        {
            _restClient = new RestClient(
               options: new() { BaseUrl = new(path) },
               configureSerialization: s => s.UseSystemTextJson(options: options));
        }

        private static JsonSerializerOptions options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public RestResponse<UserModel[]> GetUsers()
        {
            var getUsersRequest = new RestRequest("/users", Method.Get);
            Logger.Info("Initiating GET on https://jsonplaceholder.typicode.com/users");
            return _restClient.ExecuteGet<UserModel[]>(getUsersRequest);
        }

        public RestResponse<UserModel> PostUser(UserModel user)
        {
            var postUsersRequest = new RestRequest("/users", Method.Post);
            postUsersRequest.AddBody(user);
            Logger.Info("Initiating POST on https://jsonplaceholder.typicode.com/users");
            return _restClient.ExecutePost<UserModel>(postUsersRequest);
        }
    }
}
