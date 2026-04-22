using ProjectMovie.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace ProjectMovie
{
    [TestFixture]
    public class Tests

    {
        private RestClient client;
        private static string? LastCreatedId;
        private const string BaseUrl = "http://144.91.123.158:5000/";
        private const string? StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI3NTJmZmNlZi03MWEzLTQ3OGItYWRmMC04ODBjMTJmMjcxNzYiLCJpYXQiOiIwNC8yMi8yMDI2IDEyOjU3OjM3IiwiVXNlcklkIjoiZGVkNzdiN2MtMmY2Mi00MTA0LTY0ZDAtMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJEaW1pNDMyMUBleGFtcGxlLmNvbSIsIlVzZXJOYW1lIjoiRGltaTQzMjEiLCJleHAiOjE3NzY4ODQyNTcsImlzcyI6Ik1vdmllQ2F0YWxvZ19BcHBfU29mdFVuaSIsImF1ZCI6Ik1vdmllQ2F0YWxvZ19XZWJBUElfU29mdFVuaSJ9.QSAnfWoNr3oN1jgjTEp7JmID6dkoo3G-AfBTtEBwuJs";
        private const string LoginEmail = "Dimi4321@example.com";
        private const string LoginPassword = "123456";

        public JwtAuthenticator Authentcator { get; private set; }

        [OneTimeSetUp]
        public void Setup()
        // using token and credentional
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            //post metod with authentication and body
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            // executing
            var response = tempClient.Execute(request);

            // response checking and deserialization
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();
                // ressponse checking
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }
        [Test, Order(1)]
        public void CreateNewMovieWithTheRequiredFields_ReturnMovieCreatedSuccessfully()
        {
            // Arrange
            var movieData = new MovieDTO
            {
                Title = "Movie Titanic",
                Description = "Nice Movie."
            };

            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);

            // Act
            var response = this.client.Execute(request);

            APIResponseDTO? createResponse = null;

            if (!string.IsNullOrEmpty(response.Content))
            {
                createResponse = JsonSerializer.Deserialize<APIResponseDTO>(response.Content);
            }

            // Assert - Status Code
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Assert - Response not null
            Assert.That(createResponse, Is.Not.Null);

            // Assert - Message
            Assert.That(createResponse!.Msg, Is.EqualTo("Movie created successfully!"));

            // Assert - Movie exists (FIXED)
            Assert.That(createResponse.Movie, Is.Not.Null);

            // Assert - Id exists
            Assert.That(createResponse.Movie.Id, Is.Not.Null.And.Not.Empty);

            // Store ID for next tests
            LastCreatedId = createResponse.Movie.Id;
        }
        [Test, Order(2)]
        public void EditCreatedMovie_ReturnsSuccessMessage()
        {
            // Arrange
            Assert.That(LastCreatedId, Is.Not.Null.And.Not.Empty, "No movie Id available from previous test.");

            var updatedMovie = new MovieDTO
            {
                Id = LastCreatedId,
                Title = "Movie Titanic - Edited",
                Description = "Updated Description"
            };

            var request = new RestRequest($"/api/Movie/Edit?movieId={LastCreatedId}", Method.Put);
            request.AddJsonBody(updatedMovie);

            // Act
            var response = this.client.Execute(request);

            APIResponseDTO? editResponse = null;

            if (!string.IsNullOrEmpty(response.Content))
            {
                editResponse = JsonSerializer.Deserialize<APIResponseDTO>(response.Content);
            }

            // Assert - Status Code
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Assert - Response exists
            Assert.That(editResponse, Is.Not.Null);

            // Assert - Message
            Assert.That(editResponse!.Msg, Is.EqualTo("Movie edited successfully!"));
        }
        [Test, Order(3)]
        public void GetAllMovies_ReturnsNonEmptyList()
        {
            // Arrange
            var request = new RestRequest("/api/Catalog/All", Method.Get);

            // Act
            var response = this.client.Execute(request);

            List<MovieDTO>? movies = null;

            if (!string.IsNullOrEmpty(response.Content))
            {
                movies = JsonSerializer.Deserialize<List<MovieDTO>>(response.Content);
            }

            // Assert - Status Code
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Assert - Response not null
            Assert.That(movies, Is.Not.Null);

            // Assert - Non-empty array
            Assert.That(movies!.Count, Is.GreaterThan(0));
        }
        [Test, Order(4)]
        public void DeleteTheMovieThatYouCreated_ResponseSuccessful()
        {
            // Arrange
            Assert.That(LastCreatedId, Is.Not.Null.And.Not.Empty, "No movie Id available from previous test.");

            var request = new RestRequest($"/api/Movie/Delete?movieId={LastCreatedId}", Method.Delete);

            // Act
            var response = this.client.Execute(request);

            APIResponseDTO? deleteResponse = null;

            if (!string.IsNullOrEmpty(response.Content))
            {
                deleteResponse = JsonSerializer.Deserialize<APIResponseDTO>(response.Content);
            }

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deleteResponse, Is.Not.Null);
            Assert.That(deleteResponse!.Msg, Is.EqualTo("Movie deleted successfully!"));
        }
        [Test, Order(5)]
        public void CreateMovieWithoutRequiredFields_ReturnsBadRequest()
        {
            // Arrange
            var movieData = new MovieDTO
            {
                Title = "",
                Description = ""
            };

            var request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);

            // Act
            var response = this.client.Execute(request);

            // Debug (??? ????? ?? ????? ????? ????? API-??)
            TestContext.WriteLine($"Status: {response.StatusCode}");
            TestContext.WriteLine($"Response: {response.Content}");

            // Assert - Status Code
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test, Order(6)]
        public void EditNonExistingMovie_ReturnsBadRequest()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid().ToString(); // ??????????? ?????????????

            var updatedMovie = new MovieDTO
            {
                Id = nonExistingId,
                Title = "Fake Movie",
                Description = "This should not be edited"
            };

            var request = new RestRequest($"/api/Movie/Edit?movieId={nonExistingId}", Method.Put);
            request.AddJsonBody(updatedMovie);

            // Act
            var response = this.client.Execute(request);

            APIResponseDTO? editResponse = null;

            if (!string.IsNullOrEmpty(response.Content))
            {
                editResponse = JsonSerializer.Deserialize<APIResponseDTO>(response.Content);
            }

            // Debug (?? ???????)
            TestContext.WriteLine($"Status: {response.StatusCode}");
            TestContext.WriteLine($"Response: {response.Content}");

            // Assert - Status Code
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            // Assert - Response exists
            Assert.That(editResponse, Is.Not.Null);

            // Assert - Message
            Assert.That(editResponse!.Msg,
                Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }
        [Test, Order(7)]
        public void DeleteNonExistingMovie_ReturnsBadRequest()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid().ToString(); // ??????????? ?????????????

            var request = new RestRequest($"/api/Movie/Delete?movieId={nonExistingId}", Method.Delete);

            // Act
            var response = this.client.Execute(request);

            APIResponseDTO? deleteResponse = null;

            if (!string.IsNullOrEmpty(response.Content))
            {
                deleteResponse = JsonSerializer.Deserialize<APIResponseDTO>(response.Content);
            }

            // Debug (?? ???????)
            TestContext.WriteLine($"Status: {response.StatusCode}");
            TestContext.WriteLine($"Response: {response.Content}");

            // Assert - Status Code
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));


            // Assert - Response exists
            Assert.That(deleteResponse, Is.Not.Null);

            // Assert - Message
            Assert.That(deleteResponse!.Msg,
                Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }
        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}