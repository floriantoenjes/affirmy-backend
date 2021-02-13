using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AffirmyBackend.Areas.Identity.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace AffirmyBackend.Services
{
    public class CouchDbService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        
        public CouchDbService(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
        }

        public async Task<HttpResponseMessage> CreateDatabases(string dbName)
        {
            var httpClient = HttpClient();

            // var httpContent = new StringContent("affirmycsharp", Encoding.UTF8, "application/json");
            var httpContent = new StringContent(dbName, Encoding.UTF8, "application/json");

            return await httpClient.PutAsync(dbName, httpContent);
        }

        public async Task<HttpResponseMessage> CreateDatabaseUser(AffirmyBackendUser affirmyBackendUser)
        {
            var httpClient = HttpClient();
            
            var newDbUser = JObject.FromObject(new
            {
                name = affirmyBackendUser.Email,
                password = affirmyBackendUser.PasswordHash,
                roles = Array.Empty<string>(),
                type = "user"
            });

            return await httpClient.PutAsync("/_users/org.couchdb.user:" + affirmyBackendUser.Email, 
                new StringContent(newDbUser.ToString(), Encoding.UTF8, "application/json"));
        }

        public async Task<HttpResponseMessage> AssignDatabaseUser(AffirmyBackendUser affirmyBackendUser, string dbName)
        {
            var httpClient = HttpClient();

            var securityObject = JObject.FromObject(new
            {
                admins = JObject.FromObject(new {}),
                members = JObject.FromObject(new
                {
                    names = new List<string>()
                    {
                        affirmyBackendUser.Email
                    },
                    roles = Array.Empty<string>()
                }),
            });

            return await httpClient.PutAsync(dbName + "/_security", new StringContent(securityObject.ToString(), Encoding.UTF8, "application/json"));
        }

        private HttpClient HttpClient()
        {
            var httpClient = _clientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Clear();

            httpClient.BaseAddress = new Uri(_configuration["CouchDB:URL"]);
            var dbUserByteArray = Encoding.ASCII.GetBytes(_configuration["CouchDB:User"]);
            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(dbUserByteArray));
            return httpClient;
        }
    }
}