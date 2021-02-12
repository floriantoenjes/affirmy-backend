using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

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
            var httpClient = _clientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Clear();

            httpClient.BaseAddress = new Uri( _configuration["CouchDB:URL"]);
            var dbUserByteArray = Encoding.ASCII.GetBytes(_configuration["CouchDB:User"]);
            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(dbUserByteArray));
            
            var httpContent = new StringContent("affirmycsharp", Encoding.UTF8, "application/json");

            return await httpClient.PutAsync(dbName, httpContent);
        }
    }
}