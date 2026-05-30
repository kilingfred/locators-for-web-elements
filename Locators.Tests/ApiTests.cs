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
    [Category("API")]
    public class ApiTests
    {
        private ApiClient _restClient = new ApiClient("https://jsonplaceholder.typicode.com/");
       
        [Test]
        public void ValidateThatTheListOfUsersCanBeReceivedSuccessfully()
        {
            var users = _restClient.GetUsers();
            Assert.That(users.IsSuccessStatusCode && users.ErrorException == null);
            Logger.Info("Response Successful. Extracting list of users from responce");
            foreach (UserModel user in users.Data) {
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
            HeaderParameter contentType = new("Content-Type", "application/json; charset=utf-8");
            var users = _restClient.GetUsers();
            Assert.That(users.IsSuccessStatusCode && users.ErrorException == null);
            Logger.Info("Response Successful. Validating header");
            Assert.That(users.ContentHeaders.Contains(contentType));
        }

        [Test]
        public void ValidateResponseDataForAListOfUsers()
        {
            var users = _restClient.GetUsers();
            List<int> ids = new List<int>();
            Assert.That(users.IsSuccessStatusCode && users.ErrorException == null);
            Logger.Info("Response Successful. Validating list of Users");
            Assert.That(users.Data.Length == 10);
            Logger.Info("List of Users has 10 objects. Checking one at a time");
            foreach (var user in users.Data)
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
            var addedUser = _restClient.PostUser(requestBody); 
            Assert.IsNull(addedUser.ErrorException);
            Logger.Info("No errors occured");
            Assert.NotNull(addedUser);
            Assert.NotNull(addedUser.Data);
            Assert.NotNull(addedUser.Data.Id);
            Logger.Info("Response is not empty. Checking header");
            Assert.That(addedUser.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public void ValidateThatUserIsNotifiedIfResourceDoesNotExist()
        {
            var users = _restClient.GetFromInvalidEndpoint();
            //Assert.IsNull(users.ErrorException);
            Logger.Info("No errors occured");
            Assert.That(users.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
