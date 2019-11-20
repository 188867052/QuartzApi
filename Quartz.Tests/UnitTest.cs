using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;

namespace Quartz.Tests
{
    public class UnitTest : UnitTestBase
    {

        static HttpClient client = new HttpClient();

        [Fact]
        public void Test()
        {
            client.BaseAddress = new Uri("http://localhost:8080");

            ListAllProducts();
            ListProduct(1);
        }


        static void ListAllProducts()
        {
            HttpResponseMessage resp = client.GetAsync("api/products").Result;
            resp.EnsureSuccessStatusCode();

            var products = resp.Content.ReadAsStringAsync().Result;
        }

        static void ListProduct(int id)
        {
            var resp = client.GetAsync(string.Format("api/products/{0}", id)).Result;
            resp.EnsureSuccessStatusCode();

            var products = resp.Content.ReadAsStringAsync().Result;
        }


        [Fact]
        public void TestGetToken()
        {
            Assert.NotEmpty(this.Token);
        }

        [Fact]
        public void TestGetWithToken()
        {
            var client = new HttpClient() { BaseAddress = uri };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", this.Token);
            var res = client.GetAsync("/api/Trigger/GetAll");
            var json = res.Result.Content.ReadAsStringAsync().Result;
            Assert.Equal(HttpStatusCode.OK, res.Result.StatusCode);
        }

        [Fact]
        public void TestGetWithoutToken()
        {
            var client = new HttpClient() { BaseAddress = uri };
            var res = client.GetAsync("/api/Trigger/GetAll");
            Assert.Equal(HttpStatusCode.Unauthorized, res.Result.StatusCode);
        }
    }
}
