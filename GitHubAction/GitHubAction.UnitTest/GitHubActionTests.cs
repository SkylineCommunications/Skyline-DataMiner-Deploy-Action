using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Package.Builder;
using Package.Domain.Enums;
using Package.Domain.Exceptions;
using Package.Domain.Models;
using Package.Domain.Services;

namespace GitHubAction.UnitTest
{
    public class GitHubActionTests
    {
        private Mock<IPackageService> _packageServiceMock = null!;
        private Mock<IPackagePresenter> _packagePresenterMock = null!;
        private GitHubAction _gitHubAction = null!;

        [SetUp]
        public void Setup()
        {
            _packageServiceMock = new Mock<IPackageService>();
            _packagePresenterMock = new Mock<IPackagePresenter>();

            _gitHubAction = new GitHubAction(_packageServiceMock.Object, _packagePresenterMock.Object);
        }

        [Test]
        public async Task RunAsync_HappyFlow()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(@"C:\TestEnv");
            var packageName = "TestPackage";
            var version = "1.0.2";

            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.Automation);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(localPackageConfig);

            var createdPackage = new CreatedPackage(new FileInfo("something.txt"), "name", "type", "version");

            _packageServiceMock
                .Setup(createPackageAsync)
                .ReturnsAsync(createdPackage);

            Expression<Func<IPackageService, Task<UploadedPackage>>> uploadPackageAsync = s =>
                s.UploadPackageAsync(It.IsAny<CreatedPackage>(), key);

            var uploadedPackage = new UploadedPackage(id);

            _packageServiceMock
                .Setup(uploadPackageAsync)
                .ReturnsAsync(uploadedPackage);

            Expression<Func<IPackageService, Task<DeployingPackage>>> deployPackageAsync = s =>
                s.DeployPackageAsync(uploadedPackage, key);

            var deploymentId = Guid.NewGuid();
            var deployingPackage = new DeployingPackage(id, deploymentId);

            _packageServiceMock
                .Setup(deployPackageAsync)
                .ReturnsAsync(deployingPackage);

            Expression<Func<IPackageService, Task<DeployedPackage>>> getDeployedPackageAsync = s =>
                s.GetDeployedPackageAsync(
                    deployingPackage,
                    key);

            var status = "Succeeded";
            var deployedPackage = new DeployedPackage(id, deploymentId, status);

            _packageServiceMock
                .Setup(getDeployedPackageAsync)
                .ReturnsAsync(deployedPackage);

            // When
            await _gitHubAction.RunAsync(key, localPackageConfig, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

            // Then
            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packageServiceMock.Verify(getDeployedPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentWaitingForFinishedPackageDeployment(0), Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageDeploymentFinished(status), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_Unauthorized()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(@"C:\TestEnv");
            var packageName = "TestPackage";
            var version = "1.0.2";

            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.Automation);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(localPackageConfig);

            var createdPackage = new CreatedPackage(new FileInfo("something.txt"), "name", "type", "version");

            _packageServiceMock
                .Setup(createPackageAsync)
                .ReturnsAsync(createdPackage);

            Expression<Func<IPackageService, Task<UploadedPackage>>> uploadPackageAsync = s =>
                s.UploadPackageAsync(It.IsAny<CreatedPackage>(), key);

            var uploadedPackage = new UploadedPackage(id);

            _packageServiceMock
                .Setup(uploadPackageAsync)
                .ReturnsAsync(uploadedPackage);

            Expression<Func<IPackageService, Task<DeployingPackage>>> deployPackageAsync = s =>
                s.DeployPackageAsync(uploadedPackage, key);

            var deploymentId = Guid.NewGuid();
            var deployingPackage = new DeployingPackage(id, deploymentId);

            _packageServiceMock
                .Setup(deployPackageAsync)
                .ReturnsAsync(deployingPackage);

            Expression<Func<IPackageService, Task<DeployedPackage>>> getDeployedPackageAsync = s =>
                s.GetDeployedPackageAsync(
                    deployingPackage,
                    key);

            _packageServiceMock
                .Setup(getDeployedPackageAsync)
                .ThrowsAsync(new KeyException("this should be thrown in the test"));

            // When
            await _gitHubAction.RunAsync(key, localPackageConfig, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

            // Then
            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packageServiceMock.Verify(getDeployedPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentWaitingForFinishedPackageDeployment(0), Times.Once);
            _packagePresenterMock.Verify(p => p.PresentUnauthorizedKey(), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_WithRetry_HappyFlow()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(@"C:\TestEnv");
            var packageName = "TestPackage";
            var version = "1.0.2";

            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.Automation);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(localPackageConfig);

            var createdPackage = new CreatedPackage(new FileInfo("something.txt"), "name", "type", "version");

            _packageServiceMock
                .Setup(createPackageAsync)
                .ReturnsAsync(createdPackage);

            Expression<Func<IPackageService, Task<UploadedPackage>>> uploadPackageAsync = s =>
                s.UploadPackageAsync(It.IsAny<CreatedPackage>(), key);

            var uploadedPackage = new UploadedPackage(id);

            _packageServiceMock
                .Setup(uploadPackageAsync)
                .ReturnsAsync(uploadedPackage);

            Expression<Func<IPackageService, Task<DeployingPackage>>> deployPackageAsync = s =>
                s.DeployPackageAsync(uploadedPackage, key);

            var deploymentId = Guid.NewGuid();
            var deployingPackage = new DeployingPackage(id, deploymentId);

            _packageServiceMock
                .Setup(deployPackageAsync)
                .ReturnsAsync(deployingPackage);

            Expression<Func<IPackageService, Task<DeployedPackage>>> getDeployedPackageAsync = s =>
                s.GetDeployedPackageAsync(
                    deployingPackage,
                    key);

            var status = "Succeeded";
            var deployedPackage = new DeployedPackage(id, deploymentId, status);

            _packageServiceMock
                .SetupSequence(getDeployedPackageAsync)
                .ReturnsAsync((DeployedPackage)null!)
                .ReturnsAsync((DeployedPackage)null!)
                .ReturnsAsync((DeployedPackage)null!)
                .ReturnsAsync(deployedPackage);

            // When
            await _gitHubAction.RunAsync(
                key,
                localPackageConfig,
                TimeSpan.FromMilliseconds(1), 
                TimeSpan.FromMilliseconds(2), 
                TimeSpan.FromMilliseconds(5));

            // Then
            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packageServiceMock.Verify(getDeployedPackageAsync, Times.Exactly(4));
            _packagePresenterMock.Verify(p => p.PresentWaitingForFinishedPackageDeployment(0), Times.Once);
            _packagePresenterMock.Verify(p => p.PresentWaitingMoreForFinishedPackageDeployment(0), Times.Exactly(3));
            _packagePresenterMock.Verify(p => p.PresentPackageDeploymentFinished(status), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_WithRetry_Unauthorized()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(@"C:\TestEnv");
            var packageName = "TestPackage";
            var version = "1.0.2";

            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.Automation);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(localPackageConfig);

            var createdPackage = new CreatedPackage(new FileInfo("something.txt"), "name", "type", "version");

            _packageServiceMock
                .Setup(createPackageAsync)
                .ReturnsAsync(createdPackage);

            Expression<Func<IPackageService, Task<UploadedPackage>>> uploadPackageAsync = s =>
                s.UploadPackageAsync(It.IsAny<CreatedPackage>(), key);

            var uploadedPackage = new UploadedPackage(id);

            _packageServiceMock
                .Setup(uploadPackageAsync)
                .ReturnsAsync(uploadedPackage);

            Expression<Func<IPackageService, Task<DeployingPackage>>> deployPackageAsync = s =>
                s.DeployPackageAsync(uploadedPackage, key);

            var deploymentId = Guid.NewGuid();
            var deployingPackage = new DeployingPackage(id, deploymentId);

            _packageServiceMock
                .Setup(deployPackageAsync)
                .ReturnsAsync(deployingPackage);

            Expression<Func<IPackageService, Task<DeployedPackage>>> getDeployedPackageAsync = s =>
                s.GetDeployedPackageAsync(
                    deployingPackage,
                    key);

            _packageServiceMock
                .SetupSequence(getDeployedPackageAsync)
                .ReturnsAsync((DeployedPackage)null!)
                .ReturnsAsync((DeployedPackage)null!)
                .ReturnsAsync((DeployedPackage)null!)
                .ThrowsAsync(new KeyException("this should be thrown in the test"));

            // When
            await _gitHubAction.RunAsync(
                key,
                localPackageConfig,
                TimeSpan.FromMilliseconds(1), 
                TimeSpan.FromMilliseconds(2), 
                TimeSpan.FromMilliseconds(5));

            // Then
            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packageServiceMock.Verify(getDeployedPackageAsync, Times.Exactly(4));
            _packagePresenterMock.Verify(p => p.PresentWaitingForFinishedPackageDeployment(0), Times.Once);
            _packagePresenterMock.Verify(p => p.PresentWaitingMoreForFinishedPackageDeployment(0), Times.Exactly(3));
            _packagePresenterMock.Verify(p => p.PresentUnauthorizedKey(), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_WithRetry_Timeout()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(@"C:\TestEnv");
            var packageName = "TestPackage";
            var version = "1.0.2";

            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.Automation);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(localPackageConfig);

            var createdPackage = new CreatedPackage(new FileInfo("something.txt"), "name", "type", "version");

            _packageServiceMock
                .Setup(createPackageAsync)
                .ReturnsAsync(createdPackage);

            Expression<Func<IPackageService, Task<UploadedPackage>>> uploadPackageAsync = s =>
                s.UploadPackageAsync(It.IsAny<CreatedPackage>(), key);

            var uploadedPackage = new UploadedPackage(id);

            _packageServiceMock
                .Setup(uploadPackageAsync)
                .ReturnsAsync(uploadedPackage);

            Expression<Func<IPackageService, Task<DeployingPackage>>> deployPackageAsync = s =>
                s.DeployPackageAsync(uploadedPackage, key);

            var deploymentId = Guid.NewGuid();
            var deployingPackage = new DeployingPackage(id, deploymentId);

            _packageServiceMock
                .Setup(deployPackageAsync)
                .ReturnsAsync(deployingPackage);

            Expression<Func<IPackageService, Task<DeployedPackage>>> getDeployedPackageAsync = s =>
                s.GetDeployedPackageAsync(
                    deployingPackage,
                    key);

            _packageServiceMock
                .SetupSequence(getDeployedPackageAsync)
                .ReturnsAsync((DeployedPackage)null!)
                .ReturnsAsync((DeployedPackage)null!)
                .ReturnsAsync((DeployedPackage)null!)
                .ReturnsAsync((DeployedPackage)null!);

            // When
            await _gitHubAction.RunAsync(
                key,
                localPackageConfig,
                TimeSpan.FromMilliseconds(1), 
                TimeSpan.FromMilliseconds(2), 
                TimeSpan.FromMilliseconds(5));

            // Then
            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packageServiceMock.Verify(getDeployedPackageAsync, Times.Exactly(4));
            _packagePresenterMock.Verify(p => p.PresentWaitingForFinishedPackageDeployment(0), Times.Once);
            _packagePresenterMock.Verify(p => p.PresentWaitingMoreForFinishedPackageDeployment(0), Times.Exactly(3));
            _packagePresenterMock.Verify(p => p.PresentPackageDeploymentTimeout(), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_StartingDeployment_DmsUnavailable()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(@"C:\TestEnv");
            var packageName = "TestPackage";
            var version = "1.0.2";

            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.Automation);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(localPackageConfig);

            var createdPackage = new CreatedPackage(new FileInfo("something.txt"), "name", "type", "version");

            _packageServiceMock
                .Setup(createPackageAsync)
                .ReturnsAsync(createdPackage);

            // Mocks
            Expression<Func<IPackageService, Task<UploadedPackage>>> uploadPackageAsync = s =>
                s.UploadPackageAsync(It.IsAny<CreatedPackage>(), key);

            var uploadedPackage = new UploadedPackage(id);

            _packageServiceMock
                .Setup(uploadPackageAsync)
                .ReturnsAsync(uploadedPackage);

            Expression<Func<IPackageService, Task<DeployingPackage>>> deployPackageAsync = s =>
                s.DeployPackageAsync(uploadedPackage, key);

            _packageServiceMock
                .Setup(deployPackageAsync)
                .ThrowsAsync(new DmsUnavailableException("this should be thrown in the test"));

            // When
            await _gitHubAction.RunAsync(key, localPackageConfig, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

            // Then
            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentDataMinerSystemUnavailable(), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_StartingDeployment_Unauthorized()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(@"C:\TestEnv");
            var packageName = "TestPackage";
            var version = "1.0.2";
            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.Automation);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(localPackageConfig);

            var createdPackage = new CreatedPackage(new FileInfo("something.txt"), "name", "type", "version");

            _packageServiceMock
                .Setup(createPackageAsync)
                .ReturnsAsync(createdPackage);

            // Mocks
            Expression<Func<IPackageService, Task<UploadedPackage>>> uploadPackageAsync = s =>
                s.UploadPackageAsync(It.IsAny<CreatedPackage>(), key);

            var uploadedPackage = new UploadedPackage(id);

            _packageServiceMock
                .Setup(uploadPackageAsync)
                .ReturnsAsync(uploadedPackage);

            Expression<Func<IPackageService, Task<DeployingPackage>>> deployPackageAsync = s =>
                s.DeployPackageAsync(uploadedPackage, key);

            _packageServiceMock
                .Setup(deployPackageAsync)
                .ThrowsAsync(new KeyException("this should be thrown in the test"));

            // When
            await _gitHubAction.RunAsync(key, localPackageConfig, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

            // Then
            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentUnauthorizedKey(), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_StartingDeployment_Failed()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(@"C:\TestEnv");
            var packageName = "TestPackage";
            var version = "1.0.2";

            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.Automation);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(localPackageConfig);

            var createdPackage = new CreatedPackage(new FileInfo("something.txt"), "name", "type", "version");

            _packageServiceMock
                .Setup(createPackageAsync)
                .ReturnsAsync(createdPackage);

            // Mocks
            Expression<Func<IPackageService, Task<UploadedPackage>>> uploadPackageAsync = s =>
                s.UploadPackageAsync(It.IsAny<CreatedPackage>(), key);

            var uploadedPackage = new UploadedPackage(id);

            _packageServiceMock
                .Setup(uploadPackageAsync)
                .ReturnsAsync(uploadedPackage);

            Expression<Func<IPackageService, Task<DeployingPackage>>> deployPackageAsync = s =>
                s.DeployPackageAsync(uploadedPackage, key);

            var ex = new DeployPackageException("this should be thrown in the test", new Exception("inner exception"));

            _packageServiceMock
                .Setup(deployPackageAsync)
                .ThrowsAsync(ex);

            // When
            await _gitHubAction.RunAsync(key, localPackageConfig, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

            // Then
            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeploymentFailed(ex), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_UploadPackage_Failed()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(@"C:\TestEnv");
            var packageName = "TestPackage";
            var version = "1.0.2";

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.Automation);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(localPackageConfig);

            var createdPackage = new CreatedPackage(new FileInfo("something.txt"), "name", "type", "version");

            _packageServiceMock
                .Setup(createPackageAsync)
                .ReturnsAsync(createdPackage);

            Expression<Func<IPackageService, Task<UploadedPackage>>> uploadPackageAsync = s =>
                s.UploadPackageAsync(It.IsAny<CreatedPackage>(), key);

            var ex = new UploadPackageException("this should be thrown in the test", new Exception("inner exception"));

            _packageServiceMock
                .Setup(uploadPackageAsync)
                .ThrowsAsync(ex);

            // When
            await _gitHubAction.RunAsync(key, localPackageConfig, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

            // Then
            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadFailed(ex), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_UploadPackage_Unauthorized()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(@"C:\TestEnv");
            var packageName = "TestPackage";
            var version = "1.0.2";

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.Automation);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(localPackageConfig);

            var createdPackage = new CreatedPackage(new FileInfo("something.txt"), "name", "type", "version");

            _packageServiceMock
                .Setup(createPackageAsync)
                .ReturnsAsync(createdPackage);

            Expression<Func<IPackageService, Task<UploadedPackage>>> uploadPackageAsync = s =>
                s.UploadPackageAsync(It.IsAny<CreatedPackage>(), key);

            _packageServiceMock
                .Setup(uploadPackageAsync)
                .ThrowsAsync(new KeyException("this should be thrown in the test"));

            // When
            await _gitHubAction.RunAsync(key, localPackageConfig, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

            // Then
            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentUnauthorizedKey(), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_CreatePackage_Failed()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(@"C:\TestEnv");
            var packageName = "TestPackage";
            var version = "1.0.2";

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.Automation);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(localPackageConfig);

            var createPackageException = new CreatePackageException("this should be thrown in the test");
            _packageServiceMock
                .Setup(createPackageAsync)
                .ThrowsAsync(createPackageException);

            // When
            await _gitHubAction.RunAsync(key, localPackageConfig, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

            // Then
            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationFailed(createPackageException), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
        }
    }
}