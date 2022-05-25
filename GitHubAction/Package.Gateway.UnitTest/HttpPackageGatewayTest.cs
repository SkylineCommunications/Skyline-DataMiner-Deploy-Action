using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ArtifactDeploymentInfoApi.Generated;
using ArtifactDeploymentInfoApi.Generated.Models;
using DeployArtifactApi.Generated;
using DeployArtifactApi.Generated.Models;
using Microsoft.Rest;
using Moq;
using NUnit.Framework;
using Package.Domain.Exceptions;
using Package.Domain.Models;
using UploadArtifactApi;
using ValidationException = Microsoft.Rest.ValidationException;

namespace Package.Gateway.UnitTest
{
    [TestFixture]
    public class HttpPackageGatewayTest
    {
        private Mock<IDeployArtifactAPI> _deployArtifactApi = null!;
        private Mock<IArtifactDeploymentInfoAPI> _artifactDeploymentInfoApi = null!;
        private Mock<IArtifactUploadApi> _artifactUploadApi = null!;
        private UploadedPackage _uploadedPackage = null!;
        private string _key = null!;
        private DeployingPackage _deployingPackage = null!;

        [SetUp]
        public void Setup()
        {
            _deployArtifactApi = new Mock<IDeployArtifactAPI>();
            _artifactDeploymentInfoApi = new Mock<IArtifactDeploymentInfoAPI>();
            _artifactUploadApi = new Mock<IArtifactUploadApi>();
            _uploadedPackage = new UploadedPackage(Guid.NewGuid().ToString());
            _key = "MyDummyKey";

            _artifactDeploymentInfoApi = new Mock<IArtifactDeploymentInfoAPI>();
            _deployingPackage = new DeployingPackage(Guid.NewGuid().ToString(), Guid.NewGuid());
        }

        [Test]
        public async Task DeployPackageAsync_ThrowsBodyValidationDeployPackageExceptionTest()
        {
            // Given
            var validationException = new ValidationException(ValidationRules.CannotBeNull, "body");
            var expectedText = $"Couldn't deploy the package {validationException}";

            _deployArtifactApi
                .Setup((x) =>
                    x.DeployArtifactWithApiKeyFunctionWithHttpMessagesAsync(It.IsAny<DeployArtifactAsSystemForm>(),
                            It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(),
                            It.IsAny<CancellationToken>())
                        .Result)
                .Throws(validationException);

            var httpGateway = new HttpPackageGateway(_artifactUploadApi.Object, _artifactDeploymentInfoApi.Object, _deployArtifactApi.Object);
            try
            { // When
                await httpGateway.DeployPackageAsync(_uploadedPackage, _key);
            } // Then
            catch (Exception e)
            {
                Assert.IsTrue(e.GetType() == typeof(DeployPackageException));
                var exceptionMessage = e.Message;
                Assert.IsTrue(exceptionMessage.StartsWith(expectedText));
            }
        }

        [Test]
        public async Task DeployPackageAsync_ThrowsSubscriptionKeyValidationDeployPackageExceptionTest()
        {
            // Given
            var validationException = new ValidationException(ValidationRules.CannotBeNull, "ocpApimSubscriptionKey");
            var expectedText = $"Couldn't deploy the package {validationException}";

            _deployArtifactApi
                .Setup((x) =>
                    x.DeployArtifactWithApiKeyFunctionWithHttpMessagesAsync(It.IsAny<DeployArtifactAsSystemForm>(),
                            It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(),
                            It.IsAny<CancellationToken>())
                        .Result)
                .Throws(validationException);

            var httpGateway = new HttpPackageGateway(_artifactUploadApi.Object, _artifactDeploymentInfoApi.Object, _deployArtifactApi.Object);
            try
            { // When
                await httpGateway.DeployPackageAsync(_uploadedPackage, _key);
            } // Then
            catch (Exception e)
            {
                Assert.IsTrue(e.GetType() == typeof(DeployPackageException));
                var exceptionMessage = e.Message;
                Assert.IsTrue(exceptionMessage.StartsWith(expectedText));
            }
        }

        [Test]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task DeployPackageAsync_ThrowsBadRequestStatusCodeDeployPackageExceptionTest(HttpStatusCode statusCode)
        {
            // Given
            const string responseContentText = "Something went wrong.";
            var expectedText = $"The deploy API returned a response with status code {statusCode}, content: {responseContentText}";
            var result = new HttpOperationResponse<DeploymentModel>();
            var httpResponse = new HttpResponseMessage(statusCode);
            httpResponse.Content = new StringContent(responseContentText);
            result.Response = httpResponse;

            _deployArtifactApi
                .Setup((x) =>
                    x.DeployArtifactWithApiKeyFunctionWithHttpMessagesAsync(It.IsAny<DeployArtifactAsSystemForm>(),
                            It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(),
                            It.IsAny<CancellationToken>())
                        .Result)
                .Returns(result);

            var httpGateway = new HttpPackageGateway(_artifactUploadApi.Object, _artifactDeploymentInfoApi.Object, _deployArtifactApi.Object);
            try
            { // When
                await httpGateway.DeployPackageAsync(_uploadedPackage, _key);
            } // Then
            catch (Exception e)
            {
                Assert.IsTrue(e.GetType()==typeof(DeployPackageException));
                var exceptionMessage = e.Message;
                Assert.IsTrue(exceptionMessage.StartsWith(expectedText));
            }
        }

        [Test]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.Unauthorized)]
        public async Task DeployPackageAsync_ThrowsKeyExceptionTest(HttpStatusCode statusCode)
        {
            // Given
            var expectedText = $"The deploy API returned a response with status code {statusCode}";
            var result = new HttpOperationResponse<DeploymentModel>();
            var httpResponse = new HttpResponseMessage(statusCode);
            result.Response = httpResponse;

            _deployArtifactApi
                .Setup((x) =>
                    x.DeployArtifactWithApiKeyFunctionWithHttpMessagesAsync(It.IsAny<DeployArtifactAsSystemForm>(),
                            It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(),
                            It.IsAny<CancellationToken>())
                        .Result)
                .Returns(result);

            var httpGateway = new HttpPackageGateway(_artifactUploadApi.Object, _artifactDeploymentInfoApi.Object, _deployArtifactApi.Object);
            try
            { // When
                await httpGateway.DeployPackageAsync(_uploadedPackage, _key);
            } // Then
            catch (Exception e)
            {
                Assert.IsTrue(e.GetType() == typeof(KeyException));
                var exceptionMessage = e.Message;
                Assert.IsTrue(exceptionMessage.StartsWith(expectedText));
            }
        }

        [Test]
        public async Task DeployPackageAsync_ReturnsDeployingPackageTest()
        {
            // Given
            var result = new HttpOperationResponse<DeploymentModel>();
            result.Response = new HttpResponseMessage();
            result.Response.StatusCode = HttpStatusCode.OK;
            var deploymentId = Guid.NewGuid();
            result.Body = new DeploymentModel(deploymentId.ToString());

            _deployArtifactApi
                .Setup((x) =>
                    x.DeployArtifactWithApiKeyFunctionWithHttpMessagesAsync(It.IsAny<DeployArtifactAsSystemForm>(),
                            It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(),
                            It.IsAny<CancellationToken>())
                        .Result)
                .Returns(result);

            var httpGateway = new HttpPackageGateway(_artifactUploadApi.Object, _artifactDeploymentInfoApi.Object, _deployArtifactApi.Object);
            
            // When
            var deployPackage = await httpGateway.DeployPackageAsync(_uploadedPackage, _key);
            
            // Then
            Assert.IsTrue(deployPackage.ArtifactId.Equals(_uploadedPackage.ArtifactId));
            Assert.IsTrue(deployPackage.DeploymentId.Equals(deploymentId));
        }

        [Test]
        public async Task GetDeployedPackageAsync_ThrowsSubscriptionKeyValidationGetDeploymentPackageExceptionTest()
        {
            // Given
            var validationException = new ValidationException(ValidationRules.CannotBeNull, "ocpApimSubscriptionKey");
            var expectedText = $"Couldn't get the deployed package {validationException}";

            _artifactDeploymentInfoApi.Setup((x) =>
                    x.GetPrivateArtifactDeploymentInfoWithHttpMessagesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(),
                            It.IsAny<CancellationToken>())
                        .Result)
                .Throws(validationException);

            var httpGateway = new HttpPackageGateway(_artifactUploadApi.Object, _artifactDeploymentInfoApi.Object, _deployArtifactApi.Object);

            try
            { // When
                await httpGateway.GetDeployedPackageAsync(_deployingPackage, _key);
            }
            // Then
            catch (Exception e)
            {
                Assert.IsTrue(e.GetType() == typeof(GetDeploymentPackageException));
                var exceptionMessage = e.Message;
                Assert.IsTrue(exceptionMessage.StartsWith(expectedText));
            }
        }

        [Test]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.NotFound)]
        public async Task GetDeployedPackageAsync_ThrowsGetDeploymentPackageExceptionTest(HttpStatusCode statusCode)
        {
            // Given
            const string responseContentText = "Something went wrong.";
            var expectedText = $"The GetDeployedPackage API returned a response with status code {statusCode}, content: {responseContentText}";
            var result = new HttpOperationResponse<IDictionary<string, DeploymentInfoModel>>();
            var httpResponse = new HttpResponseMessage(statusCode);
            httpResponse.Content = new StringContent(responseContentText);
            result.Response = httpResponse;


            _artifactDeploymentInfoApi.Setup((x) =>
                    x.GetPrivateArtifactDeploymentInfoWithHttpMessagesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(),
                            It.IsAny<CancellationToken>())
                        .Result)
                .Returns(result);

            var httpGateway = new HttpPackageGateway(_artifactUploadApi.Object, _artifactDeploymentInfoApi.Object, _deployArtifactApi.Object);

            try
            { // When
                await httpGateway.GetDeployedPackageAsync(_deployingPackage, _key);
            }
            // Then
            catch (Exception e)
            {
                Assert.IsTrue(e.GetType() == typeof(GetDeploymentPackageException));
                var exceptionMessage = e.Message;
                Assert.IsTrue(exceptionMessage.StartsWith(expectedText));
            }
        }

        [Test]
        [TestCase(HttpStatusCode.Forbidden)]
        [TestCase(HttpStatusCode.Unauthorized)]
        public async Task GetDeployedPackageAsync_ThrowsKeyExceptionTest(HttpStatusCode statusCode)
        {
            // Given
            var expectedText = $"The GetDeployedPackage API returned a {statusCode} response";

            const string responseContentText = "Something went wrong.";
            var result = new HttpOperationResponse<IDictionary<string, DeploymentInfoModel>>();
            var httpResponse = new HttpResponseMessage(statusCode);
            httpResponse.Content = new StringContent(responseContentText);
            result.Response = httpResponse;


            _artifactDeploymentInfoApi.Setup((x) =>
                    x.GetPrivateArtifactDeploymentInfoWithHttpMessagesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(),
                            It.IsAny<CancellationToken>())
                        .Result)
                .Returns(result);

            var httpGateway = new HttpPackageGateway(_artifactUploadApi.Object, _artifactDeploymentInfoApi.Object, _deployArtifactApi.Object);

            try
            { // When
                await httpGateway.GetDeployedPackageAsync(_deployingPackage, _key);
            }
            // Then
            catch (Exception e)
            {
                Assert.IsTrue(e.GetType() == typeof(KeyException));
                var exceptionMessage = e.Message;
                Assert.IsTrue(exceptionMessage.StartsWith(expectedText));
            }
        }

        [Test]
        public async Task GetDeployedPackageAsync_ReturnsDeployedPackageTest()
        {
            // Given
            var result = new HttpOperationResponse<IDictionary<string, DeploymentInfoModel>>();
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
            
            var deploymentDic = new Dictionary<string, DeploymentInfoModel>();
            var deploymentInfo = new DeploymentInfoModel
            {
                ArtifactId = Guid.NewGuid().ToString()
            };
            deploymentDic.Add("DeploymentInfo",deploymentInfo);
            result.Body = deploymentDic;
            result.Response = httpResponse;

            _artifactDeploymentInfoApi.Setup((x) =>
                    x.GetPrivateArtifactDeploymentInfoWithHttpMessagesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(),
                            It.IsAny<CancellationToken>())
                        .Result)
                .Returns(result);

            var httpGateway = new HttpPackageGateway(_artifactUploadApi.Object, _artifactDeploymentInfoApi.Object, _deployArtifactApi.Object);

            // When
            var deployedPackage = await httpGateway.GetDeployedPackageAsync(_deployingPackage, _key);
            
            // Then
            Assert.IsTrue(deployedPackage.ArtifactId.Equals(deploymentInfo.ArtifactId));
        }
    }
}