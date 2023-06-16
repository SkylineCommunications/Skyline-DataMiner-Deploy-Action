using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Catalog.Domain;

using Moq;
using NUnit.Framework;
using Package.Domain.Enums;
using Package.Domain.Models;
using Package.Domain.Services;

namespace Package.Application.UnitTest
{
    public class PackageServiceTest
    {
        private Mock<IPackageGateway> _packageGatewayMock = null!;
        private Mock<IPackageBuilder> _packageBuilderMock = null!;

        private PackageService _packageService = null!;

        [SetUp]
        public void Setup()
        {
            _packageGatewayMock = new Mock<IPackageGateway>();
            _packageBuilderMock = new Mock<IPackageBuilder>();
            _packageService = new PackageService(_packageGatewayMock.Object, _packageBuilderMock.Object);
        }

        [Test]
        public async Task CreatePackage_HappyFlow()
        {
            // Given
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(@"C:\TestEnv");
            var packageName = "TestPackage";
            var version = "1.0.2";

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                packageName,
                version,
                SolutionType.DmScript,
                null);

            Expression<Func<IPackageBuilder, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(localPackageConfig);

            var createdPackage = new CreatedPackage(new byte[0], "name", "type", "version");
            _packageBuilderMock
                .Setup(createPackageAsync)
                .ReturnsAsync(createdPackage);

            // When
            var result = await _packageService.CreatePackageAsync(localPackageConfig);

            // Then
            Assert.AreEqual(createdPackage, result);

            _packageBuilderMock.Verify(createPackageAsync, Times.Once);
            _packageGatewayMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task UploadPackage_HappyFlow()
        {
            // Given
            var createdPackage = new CreatedPackage(new byte[0], "name", "type", "version");
            CatalogData catalog = new CatalogData()
            {
                Version = "version"
            };

            var key = "key";

            Expression<Func<IPackageGateway, Task<UploadedPackage>>> uploadPackageAsync = s =>
                s.UploadPackageAsync(createdPackage, key, catalog);

            var uploadedPackage = new UploadedPackage(Guid.NewGuid().ToString());
            _packageGatewayMock
                .Setup(uploadPackageAsync)
                .ReturnsAsync(uploadedPackage);

            // When
            var result = await _packageService.UploadPackageAsync(createdPackage, key, catalog);

            // Then
            Assert.AreEqual(uploadedPackage, result);

            _packageGatewayMock.Verify(uploadPackageAsync, Times.Once);
            _packageGatewayMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task DeployPackage_HappyFlow()
        {
            // Given
            var uploadedPackage = new UploadedPackage(Guid.NewGuid().ToString());
            var key = "key";

            Expression<Func<IPackageGateway, Task<DeployingPackage>>> deployPackageAsync = s =>
                s.DeployPackageAsync(uploadedPackage, key);

            var deployingPackage = new DeployingPackage(uploadedPackage.ArtifactId, Guid.NewGuid());

            _packageGatewayMock
                .Setup(deployPackageAsync)
                .ReturnsAsync(deployingPackage);

            // When
            var result = await _packageService.DeployPackageAsync(uploadedPackage, key);

            // Then
            Assert.AreEqual(deployingPackage, result);

            _packageGatewayMock.Verify(deployPackageAsync, Times.Once());
            _packageGatewayMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GetDeployedPackage_HappyFlow()
        {
            // Given
            var deployingPackage = new DeployingPackage(Guid.NewGuid().ToString(), Guid.NewGuid());
            var key = "key";

            Expression<Func<IPackageGateway, Task<DeployedPackage>>> getDeployedPackageAsync = s =>
                s.GetDeployedPackageAsync(deployingPackage, key);

            var deployedPackage = new DeployedPackage(deployingPackage.ArtifactId, deployingPackage.DeploymentId, "Succeeded");

            _packageGatewayMock
                .Setup(getDeployedPackageAsync)
                .ReturnsAsync(deployedPackage);

            // When
            var result = await _packageService.GetDeployedPackageAsync(deployingPackage, key);

            // Then
            Assert.AreEqual(deployedPackage, result);

            _packageGatewayMock.Verify(getDeployedPackageAsync, Times.Once);
            _packageGatewayMock.VerifyNoOtherCalls();
        }
    }
}