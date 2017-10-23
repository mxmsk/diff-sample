using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiffSample.Integration.Test
{
    [TestClass]
    public class WebTest
    {
        private static readonly string _DataDir = Path.Combine(
            Path.GetTempPath(), "diff-sample-integration-test");

        private TestServer _server;
        private HttpClient _client;

        [TestInitialize]
        public void Init()
        {
            var configValues = new Dictionary<string, string>
            {
                {"Diffs:DataDir", _DataDir},
            };
            
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(configValues);
            var config = configBuilder.Build();

            // Prepare clean directory for a test.
            Directory.Delete(_DataDir, true);

            _server = new TestServer(new WebHostBuilder()
                                     .UseStartup<Startup>()
                                     .UseConfiguration(config));
            _client = _server.CreateClient();
        }
        
        [DataTestMethod]
        [DataRow("/v1/diff/1/left")]
        [DataRow("/v1/diff/1/right")]
        public async Task PostReturnsBadRequestWhenContentIsMissing(string url)
        {
            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [DataTestMethod]
        [DataRow("/v1/diff/2/left")]
        [DataRow("/v1/diff/2/right")]
        public async Task PostReturnsBadRequestWhenDataIsNotBase64(string url)
        {
            var content = new StringContent("{ \"data\": \"__.\" }", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [DataTestMethod]
        [DataRow("/v1/diff/3/left")]
        [DataRow("/v1/diff/3/right")]
        public async Task PostReturnsOkWhenDataIsBase64(string url)
        {
            var content = new StringContent("{ \"data\": \"MTIz\" }", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [DataTestMethod]
        [DataRow("/v1/diff/4")]
        [DataRow("/v1/diff/5")]
        public async Task GetDiffReturnsNotFoundWithoutAnySource(string diffUrl)
        {
            var content = new StringContent("{ \"data\": \"MTIz\" }", Encoding.UTF8, "application/json");
            var response = await _client.GetAsync(diffUrl);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [DataTestMethod]
        [DataRow("/v1/diff/6/left", "/v1/diff/6")]
        [DataRow("/v1/diff/7/right", "/v1/diff/7")]
        public async Task GetDiffReturnsNoContentWhenOnlyOneSourceUploaded(string contentUrl, string diffUrl)
        {
            var content = new StringContent("{ \"data\": \"MTIz\" }", Encoding.UTF8, "application/json");
            await _client.PostAsync(contentUrl, content);
            var response = await _client.GetAsync(diffUrl);

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [TestMethod]
        public async Task GetDiffReturnsDiffWhenBothSourcesUploaded()
        {
            var content1 = new StringContent("{ \"data\": \"MTIzNDU=\" }", Encoding.UTF8, "application/json");
            var content2 = new StringContent("{ \"data\": \"MTc3NDg=\" }", Encoding.UTF8, "application/json");

            await _client.PostAsync("/v1/diff/8/left", content1);
            await _client.PostAsync("/v1/diff/8/right", content2);

            // Poll server 10 times because we have async system.
            HttpResponseMessage response = null;
            foreach (var url in Enumerable.Repeat("/v1/diff/8", 10))
            {
                response = await _client.GetAsync(url);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    break;
                }
                Thread.Sleep(100);
            }

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.ReadAsStringAsync().Result.Should().Be(
                "{\"type\":\"Detailed\",\"details\":" + 
                "[{\"leftOffset\":1,\"leftLength\":2,\"rightOffset\":1,\"rightLength\":2}," +
                "{\"leftOffset\":4,\"leftLength\":1,\"rightOffset\":4,\"rightLength\":1}]}");
        }
    }
}
