using Base;
using Base.Models;
using Base.Utils;
using FluentAssertions;
using Newtonsoft.Json;
using OpenQA.Selenium.DevTools.V146.Emulation;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Locators.Tests
{
    public class ApiTests
    {
        private IRestClient _restClient;
       
        [OneTimeSetUp]
        public void Before()
        {
            _restClient = Base.ApiClient.GetRestClient();
        }

        [OneTimeTearDown]
        public void After()
        {
            _restClient.Dispose();
        }

        [Test]
        public void ValidateThatTheListOfUsersCanBeReceivedSuccessfully()
        {
            var getUsersRequest = new RestRequest("/users", Method.Get);
            Logger.Info("Initiating GET on https://jsonplaceholder.typicode.com/users");
            var getUsersResponse = _restClient.ExecuteGet<UserModel[]>(getUsersRequest);
            Assert.That(getUsersResponse.IsSuccessStatusCode && getUsersResponse.ErrorException == null);
            Logger.Info("Response Successful. Extracting list of users from responce");
            foreach (UserModel user in getUsersResponse.Data) {
                Assert.NotNull(user.Id);
                Logger.Info("Reading properties of user " + user.Id);
                Assert.NotNull(user.Name);
                Assert.NotNull(user.Email);
                Assert.NotNull(user.Username);
                Assert.NotNull(user.Website);
                Assert.NotNull(user.Phone);
                Assert.NotNull(user.Company);
                Assert.NotNull(user.Address);
            }            
        }

        [Test]
        public void ValidateResponseHeaderForAListOfUsers()
        {
            var getUsersRequest = new RestRequest("/users", Method.Get);
            HeaderParameter contentType = new("Content-Type", "application/json; charset=utf-8");
            Logger.Info("Initiating GET on https://jsonplaceholder.typicode.com/users");
            var getUsersResponse = _restClient.Execute(getUsersRequest);
            Assert.That(getUsersResponse.IsSuccessStatusCode && getUsersResponse.ErrorException == null);
            Logger.Info("Response Successful. Validating header");
            Assert.That(getUsersResponse.ContentHeaders.Contains(contentType));
        }

        [Test]
        public void ValidateResponseDataForAListOfUsers()
        {
            var getUsersRequest = new RestRequest("/users", Method.Get);
            Logger.Info("Initiating GET on https://jsonplaceholder.typicode.com/users");
            var getUsersResponse = _restClient.ExecuteGet<UserModel[]>(getUsersRequest);
            List<int> ids = new List<int>();
            Assert.That(getUsersResponse.IsSuccessStatusCode && getUsersResponse.ErrorException == null);
            Logger.Info("Response Successful. Validating list of Users");
            Assert.That(getUsersResponse.Data.Length == 10);
            Logger.Info("List of Users has 10 objects. Checking one at a time");
            foreach (var user in getUsersResponse.Data)
            {
                int userId = user.Id;
                string company = user.Company.Name;
                Assert.IsFalse(ids.Contains(userId));
                Logger.Info("User has unique id of " + userId);
                ids.Add(userId);
                Assert.NotNull(user.Name);
                Assert.NotNull(user.Username);
                Logger.Info("\tName and Username properties are not null");
                Assert.NotNull(company);
                Logger.Info("\tCompany name is" + company);
            }
        }

        [Test]
        public void ValidateThatUserCanBeCreated()
        {
            var requestBody = new UserModel
            {
                Name = "TestName",
                Username = "TestUsername",
            };
            var postUsersRequest = new RestRequest("/users", Method.Post);
            postUsersRequest.AddBody(requestBody);
            Logger.Info("Initiating POST on https://jsonplaceholder.typicode.com/users");
            var postUsersResponse = _restClient.ExecutePost<UserModel>(postUsersRequest);
            Assert.IsNull(postUsersResponse.ErrorException);
            Logger.Info("No errors occured");
            Assert.NotNull(postUsersResponse);
            Assert.NotNull(postUsersResponse.Data);
            Assert.NotNull(postUsersResponse.Data.Id);
            Logger.Info("Response is not empty. Checking header");
            Assert.That(postUsersResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public void ValidateThatUserIsNotifiedIfResourceDoesNotExist()
        {
            var getUsersRequest = new RestRequest("/invalidendpoint", Method.Get);
            Logger.Info("Initiating GET on https://jsonplaceholder.typicode.com/users");
            var getUsersResponse = _restClient.Execute(getUsersRequest);
            //Assert.IsNull(getUsersResponse.ErrorException);
            Logger.Info("No errors occured");
            Assert.That(getUsersResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
