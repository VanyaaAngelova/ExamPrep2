using ExamPrep2.DTOs;
using NUnit.Framework;
using NUnit.Framework.Internal;
using RestSharp;
using RestSharp.Authenticators;
using System.ComponentModel.Design;
using System.Net;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace ExamPrep2
{
    public class Tests
    {
        private RestClient client;
        private string foodId;
        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJWTToken("VanyaTest1", "123456");
            RestClientOptions options = new RestClientOptions("http://144.91.123.158:81")
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            this.client = new RestClient(options);
        }
        private string GetJWTToken(string username, string password)
        {
            RestSharp.RestClient client = new RestSharp.RestClient("http://144.91.123.158:81");
            RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });
            RestResponse response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");


                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}");
            }



        }


        [Order(1)]
        [Test]
        public void CreateFood_WithRequiredFields_ShouldSuccess()
        {
            FoodDTO food = new FoodDTO
            {
                Name = "Test Food",
                Description = "Test Description",
                Url = ""
            };

            RestRequest request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            var readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            foodId = readyResponse.FoodId;



        }
        [Order(2)]
        [Test]
        public void EditFoodTitle_WithRequiredFields_ShouldSuccess()
        {
            RestRequest request = new RestRequest($"/api/Food/Edit/{foodId}", Method.Patch);
            request.AddJsonBody(new[]
{
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "Edited Test Food"
                }
            }, "application/json-patch+json");
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(readyResponse.Msg, Is.EqualTo("Successfully edited"));
        }


        [Order(3)]
        [Test]
        public void GetAllFoods_ShouldReturnNonEmptyArray()
        {
            RestRequest request = new RestRequest("/api/Food/All", Method.Get);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));

            List<FoodDTO> readyResponse = JsonSerializer.Deserialize<List<FoodDTO>>(response.Content);
            Assert.That(readyResponse, Is.Not.Null);
            Assert.That(readyResponse, Is.Not.Empty);
            Assert.That(readyResponse.Count, Is.GreaterThanOrEqualTo(1));

        }
        [Test]
        [Order(4)]
        public void DeleteExistingFood_ShouldSuccess()
        {
            RestRequest request = new RestRequest($"/api/Food/Delete/{foodId}", Method.Delete);
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            
            Assert.That(readyResponse.Msg, Is.EqualTo("Deleted successfully!"));
        }






        [Test]
        [Order(5)]
        public void CreateFood_WithInvalidFields_ShouldReturnBadRequest()
        {
            FoodDTO food = new FoodDTO
            {
                Name = "",
                Description = "Test Description",
                Url = ""
            };
            RestRequest request = new RestRequest($"/api/Food/Create", Method.Post);
            request.AddBody(food);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }
        [Test]
        [Order(6)]
        public void EditTitleOfNonExistingFood_ShouldReturnNotFound()
        {
            string nonExistingFoodId = "12345";
            RestRequest request = new RestRequest($"/api/Food/Edit/{nonExistingFoodId}", Method.Patch);
            request.AddJsonBody(new[]
{
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "Edited Test Food"
                }
            }, "application/json-patch+json");
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(readyResponse.Msg, Is.EqualTo("No food revues..."));
        }
        [Test]
        [Order(7)]
        public void DeleteNonExistingFood_ShouldReturnNotFound()
        {
            string nonExistingFoodId = "12345";
            RestRequest request = new RestRequest($"/api/Food/Delete/{nonExistingFoodId}", Method.Delete);
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            
            ApiResponseDTO readyResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            
            Assert.That(readyResponse.Msg, Is.EqualTo("Unable to delete this food revue!"));


        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}





