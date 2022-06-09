using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GitHubAction.Domain.Entities;
using GitHubAction.Factories;
using GitHubAction.Presenters;
using Microsoft.Extensions.Logging;
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
        private Mock<IGithubPresenter> _githubPresenterMock = null!;
        private Mock<IInputFactory> _inputParserMock = null!;
        private Mock<ILogger<GitHubAction>> _loggerMock = null!;
        private GitHubAction _gitHubAction = null!;

        [SetUp]
        public void Setup()
        {
            _packageServiceMock = new Mock<IPackageService>();
            _packagePresenterMock = new Mock<IPackagePresenter>();
            _githubPresenterMock = new Mock<IGithubPresenter>();
            _inputParserMock = new Mock<IInputFactory>();
            _loggerMock = new Mock<ILogger<GitHubAction>>();

            _gitHubAction = new GitHubAction(_packageServiceMock.Object, _inputParserMock.Object, _packagePresenterMock.Object, _githubPresenterMock.Object, TimeSpan.Zero, TimeSpan.Zero);
        }

        [Test]
        public async Task RunAsync_HappyFlow_All()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory == "/github/workspace" ? Environment.CurrentDirectory : Path.Join(Environment.CurrentDirectory, "../../../../../"));
            var packageName = "TestPackage";
            var version = "1.0.2";
            var timeOut = "12:00";
            var stage = "All";
            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.DmScript);

            var args = new string[]
            {
                "--api-key",
                key,
                "--solution-path",
                solutionFile.FullName,
                "--package-name",
                packageName,
                "--version",
                version,
                "--timeout",
                timeOut,
                "--stage",
                stage,
                "--artifact-id",
                ""
            };


            Expression<Func<IInputFactory, Inputs?>> parseInputs = s =>
                s.ParseAndValidateInputs(args);

            var inputs = new Inputs
            {
                ApiKey = key,
                PackageName = packageName,
                SolutionPath = solutionFile.FullName,
                Stage = Enum.Parse<Stage>(stage),
                TimeOut = TimeSpan.Parse(timeOut),
                Version = version
            };

            _inputParserMock.Setup(parseInputs).Returns(inputs);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(It.Is<LocalPackageConfig>(config => compareLocalPackageConfig(localPackageConfig, config)));

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
                s.GetDeployedPackageAsync(deployingPackage, key);

            var status = "Succeeded";
            var deployedPackage = new DeployedPackage(id, deploymentId, status);

            _packageServiceMock
                .Setup(getDeployedPackageAsync)
                .ReturnsAsync(deployedPackage);

            // When
            var exitCode = await _gitHubAction.RunAsync(args, CancellationToken.None);

            // Then
            Assert.AreEqual(0, exitCode);

            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);
            _githubPresenterMock.Verify(p => p.PresentOutputVariable("artifact-id", id));

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packageServiceMock.Verify(getDeployedPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentWaitingForFinishedPackageDeployment(0), Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageDeploymentFinished(status), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
            _packageServiceMock.VerifyNoOtherCalls();
            _githubPresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_HappyFlow_Upload()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory == "/github/workspace" ? Environment.CurrentDirectory : Path.Join(Environment.CurrentDirectory, "../../../../../"));
            var packageName = "TestPackage";
            var version = "1.0.2";
            var timeOut = "12:00";
            var stage = "Upload";
            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.DmScript);

            var args = new string[]
            {
                "--api-key",
                key,
                "--solution-path",
                solutionFile.FullName,
                "--package-name",
                packageName,
                "--version",
                version,
                "--timeout",
                timeOut,
                "--stage",
                stage,
                "--artifact-id",
                ""
            };


            Expression<Func<IInputFactory, Inputs?>> parseInputs = s =>
                s.ParseAndValidateInputs(args);

            var inputs = new Inputs
            {
                ApiKey = key,
                PackageName = packageName,
                SolutionPath = solutionFile.FullName,
                Stage = Enum.Parse<Stage>(stage),
                TimeOut = TimeSpan.Parse(timeOut),
                Version = version
            };

            _inputParserMock.Setup(parseInputs).Returns(inputs);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(It.Is<LocalPackageConfig>(config => compareLocalPackageConfig(localPackageConfig, config)));

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
                s.GetDeployedPackageAsync(deployingPackage, key);

            var status = "Succeeded";
            var deployedPackage = new DeployedPackage(id, deploymentId, status);

            _packageServiceMock
                .Setup(getDeployedPackageAsync)
                .ReturnsAsync(deployedPackage);

            // When
            var exitCode = await _gitHubAction.RunAsync(args, CancellationToken.None);

            // Then
            Assert.AreEqual(0, exitCode);

            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);
            _githubPresenterMock.Verify(p => p.PresentOutputVariable("artifact-id", id));

            _packagePresenterMock.VerifyNoOtherCalls();
            _packageServiceMock.VerifyNoOtherCalls();
            _githubPresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_HappyFlow_Deploy()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = "";
            var packageName = "";
            var version = "";
            var timeOut = "12:00";
            var stage = "Deploy";
            var id = Guid.NewGuid().ToString();

            var args = new string[]
            {
                "--api-key",
                key,
                "--solution-path",
                solutionFile,
                "--package-name",
                packageName,
                "--version",
                version,
                "--timeout",
                timeOut,
                "--stage",
                stage,
                "--artifact-id",
                id
            };


            Expression<Func<IInputFactory, Inputs?>> parseInputs = s =>
                s.ParseAndValidateInputs(args);

            var inputs = new Inputs
            {
                ApiKey = key,
                Stage = Enum.Parse<Stage>(stage),
                TimeOut = TimeSpan.Parse(timeOut),
                ArtifactId = id
            };

            _inputParserMock.Setup(parseInputs).Returns(inputs);


            Expression<Func<IPackageService, Task<DeployingPackage>>> deployPackageAsync = s =>
                s.DeployPackageAsync(It.Is<UploadedPackage>(p => p.ArtifactId == id), key);

            var deploymentId = Guid.NewGuid();
            var deployingPackage = new DeployingPackage(id, deploymentId);

            _packageServiceMock
                .Setup(deployPackageAsync)
                .ReturnsAsync(deployingPackage);

            Expression<Func<IPackageService, Task<DeployedPackage>>> getDeployedPackageAsync = s =>
                s.GetDeployedPackageAsync(deployingPackage, key);

            var status = "Succeeded";
            var deployedPackage = new DeployedPackage(id, deploymentId, status);

            _packageServiceMock
                .Setup(getDeployedPackageAsync)
                .ReturnsAsync(deployedPackage);

            // When
            var exitCode = await _gitHubAction.RunAsync(args, CancellationToken.None);

            // Then
            Assert.AreEqual(0, exitCode);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packageServiceMock.Verify(getDeployedPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentWaitingForFinishedPackageDeployment(0), Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageDeploymentFinished(status), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
            _packageServiceMock.VerifyNoOtherCalls();
            _githubPresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_Unauthorized()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory == "/github/workspace" ? Environment.CurrentDirectory : Path.Join(Environment.CurrentDirectory, "../../../../../"));
            var packageName = "TestPackage";
            var version = "1.0.2";
            var timeOut = "12:00";
            var stage = "All";
            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.DmScript);

            var args = new string[]
            {
                "--api-key",
                key,
                "--solution-path",
                solutionFile.FullName,
                "--package-name",
                packageName,
                "--version",
                version,
                "--timeout",
                timeOut,
                "--stage",
                stage,
                "--artifact-id",
                ""
            };


            Expression<Func<IInputFactory, Inputs?>> parseInputs = s =>
                s.ParseAndValidateInputs(args);

            var inputs = new Inputs
            {
                ApiKey = key,
                PackageName = packageName,
                SolutionPath = solutionFile.FullName,
                Stage = Enum.Parse<Stage>(stage),
                TimeOut = TimeSpan.Parse(timeOut),
                Version = version
            };

            _inputParserMock.Setup(parseInputs).Returns(inputs);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(It.Is<LocalPackageConfig>(config => compareLocalPackageConfig(localPackageConfig, config)));

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
            var exitCode = await _gitHubAction.RunAsync(args, CancellationToken.None);

            // Then
            Assert.AreEqual(8, exitCode);

            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);
            _githubPresenterMock.Verify(p => p.PresentOutputVariable("artifact-id", id));

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packageServiceMock.Verify(getDeployedPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentWaitingForFinishedPackageDeployment(0), Times.Once);
            _packagePresenterMock.Verify(p => p.PresentUnauthorizedKey(), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
            _packageServiceMock.VerifyNoOtherCalls();
            _githubPresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_WithRetry_HappyFlow()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory == "/github/workspace" ? Environment.CurrentDirectory : Path.Join(Environment.CurrentDirectory, "../../../../../"));
            var packageName = "TestPackage";
            var version = "1.0.2";
            var timeOut = "12:00";
            var stage = "All";
            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.DmScript);

            var args = new string[]
            {
                "--api-key",
                key,
                "--solution-path",
                solutionFile.FullName,
                "--package-name",
                packageName,
                "--version",
                version,
                "--timeout",
                timeOut,
                "--stage",
                stage,
                "--artifact-id",
                ""
            };


            Expression<Func<IInputFactory, Inputs?>> parseInputs = s =>
                s.ParseAndValidateInputs(args);

            var inputs = new Inputs
            {
                ApiKey = key,
                PackageName = packageName,
                SolutionPath = solutionFile.FullName,
                Stage = Enum.Parse<Stage>(stage),
                TimeOut = TimeSpan.Parse(timeOut),
                Version = version
            };

            _inputParserMock.Setup(parseInputs).Returns(inputs);

            _gitHubAction = new GitHubAction(_packageServiceMock.Object, _inputParserMock.Object, _packagePresenterMock.Object, _githubPresenterMock.Object, TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(2));

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(It.Is<LocalPackageConfig>(config => compareLocalPackageConfig(localPackageConfig, config)));

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
            var exitCode = await _gitHubAction.RunAsync(args, CancellationToken.None);

            // Then
            Assert.AreEqual(0, exitCode);

            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);
            _githubPresenterMock.Verify(p => p.PresentOutputVariable("artifact-id", id));

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packageServiceMock.Verify(getDeployedPackageAsync, Times.Exactly(4));
            _packagePresenterMock.Verify(p => p.PresentWaitingForFinishedPackageDeployment(0), Times.Once);
            _packagePresenterMock.Verify(p => p.PresentWaitingMoreForFinishedPackageDeployment(0), Times.Exactly(3));
            _packagePresenterMock.Verify(p => p.PresentPackageDeploymentFinished(status), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
            _packageServiceMock.VerifyNoOtherCalls();
            _githubPresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_WithRetry_Unauthorized()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory == "/github/workspace" ? Environment.CurrentDirectory : Path.Join(Environment.CurrentDirectory, "../../../../../"));
            var packageName = "TestPackage";
            var version = "1.0.2";
            var timeOut = "12:00";
            var stage = "All";
            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.DmScript);

            var args = new string[]
            {
                "--api-key",
                key,
                "--solution-path",
                solutionFile.FullName,
                "--package-name",
                packageName,
                "--version",
                version,
                "--timeout",
                timeOut,
                "--stage",
                stage,
                "--artifact-id",
                ""
            };


            Expression<Func<IInputFactory, Inputs?>> parseInputs = s =>
                s.ParseAndValidateInputs(args);

            var inputs = new Inputs
            {
                ApiKey = key,
                PackageName = packageName,
                SolutionPath = solutionFile.FullName,
                Stage = Enum.Parse<Stage>(stage),
                TimeOut = TimeSpan.Parse(timeOut),
                Version = version
            };

            _inputParserMock.Setup(parseInputs).Returns(inputs);

            _gitHubAction = new GitHubAction(_packageServiceMock.Object, _inputParserMock.Object, _packagePresenterMock.Object, _githubPresenterMock.Object, TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(2));

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(It.Is<LocalPackageConfig>(config => compareLocalPackageConfig(localPackageConfig, config)));

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
            var exitCode = await _gitHubAction.RunAsync(args, CancellationToken.None);

            // Then
            Assert.AreEqual(8, exitCode);

            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);
            _githubPresenterMock.Verify(p => p.PresentOutputVariable("artifact-id", id));

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packageServiceMock.Verify(getDeployedPackageAsync, Times.Exactly(4));
            _packagePresenterMock.Verify(p => p.PresentWaitingForFinishedPackageDeployment(0), Times.Once);
            _packagePresenterMock.Verify(p => p.PresentWaitingMoreForFinishedPackageDeployment(0), Times.Exactly(3));
            _packagePresenterMock.Verify(p => p.PresentUnauthorizedKey(), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
            _packageServiceMock.VerifyNoOtherCalls();
            _githubPresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        [TestCase("Error")]
        [TestCase("Timeout")]
        public async Task RunAsync_WithRetry_FailedDeployment(string failedStatus)
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory == "/github/workspace" ? Environment.CurrentDirectory : Path.Join(Environment.CurrentDirectory, "../../../../../"));
            var packageName = "TestPackage";
            var version = "1.0.2";
            var timeOut = "12:00";
            var stage = "All";
            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.DmScript);

            var args = new string[]
            {
                "--api-key",
                key,
                "--solution-path",
                solutionFile.FullName,
                "--package-name",
                packageName,
                "--version",
                version,
                "--timeout",
                timeOut,
                "--stage",
                stage,
                "--artifact-id",
                ""
            };


            Expression<Func<IInputFactory, Inputs?>> parseInputs = s =>
                s.ParseAndValidateInputs(args);

            var inputs = new Inputs
            {
                ApiKey = key,
                PackageName = packageName,
                SolutionPath = solutionFile.FullName,
                Stage = Enum.Parse<Stage>(stage),
                TimeOut = TimeSpan.Parse(timeOut),
                Version = version
            };

            _inputParserMock.Setup(parseInputs).Returns(inputs);

            _gitHubAction = new GitHubAction(_packageServiceMock.Object, _inputParserMock.Object, _packagePresenterMock.Object, _githubPresenterMock.Object, TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(2));

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(It.Is<LocalPackageConfig>(config => compareLocalPackageConfig(localPackageConfig, config)));

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

            var FailedDeployedPackage = new DeployedPackage(deployingPackage.ArtifactId, deploymentId, failedStatus);

            _packageServiceMock
                .SetupSequence(getDeployedPackageAsync)
                .ReturnsAsync((DeployedPackage)null!)
                .ReturnsAsync(new DeployedPackage(deployingPackage.ArtifactId, deploymentId, "Pending"))
                .ReturnsAsync(new DeployedPackage(deployingPackage.ArtifactId, deploymentId, "Pending"))
                .ReturnsAsync(FailedDeployedPackage);

            // When
            var exitCode = await _gitHubAction.RunAsync(args, CancellationToken.None);

            // Then
            Assert.AreEqual(7, exitCode);

            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);
            _githubPresenterMock.Verify(p => p.PresentOutputVariable("artifact-id", id));

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packageServiceMock.Verify(getDeployedPackageAsync, Times.Exactly(4));
            _packagePresenterMock.Verify(p => p.PresentWaitingForFinishedPackageDeployment(0), Times.Once);
            _packagePresenterMock.Verify(p => p.PresentWaitingMoreForFinishedPackageDeployment(0), Times.Exactly(3));
            _packagePresenterMock.Verify(p => p.PresentPackageDeploymentFailed(FailedDeployedPackage), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
            _packageServiceMock.VerifyNoOtherCalls();
            _githubPresenterMock.VerifyNoOtherCalls();

        }

        [Test]
        public async Task RunAsync_StartingDeployment_DmsUnavailable()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory == "/github/workspace" ? Environment.CurrentDirectory : Path.Join(Environment.CurrentDirectory, "../../../../../"));
            var packageName = "TestPackage";
            var version = "1.0.2";
            var timeOut = "12:00";
            var stage = "All";
            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.DmScript);

            var args = new string[]
            {
                "--api-key",
                key,
                "--solution-path",
                solutionFile.FullName,
                "--package-name",
                packageName,
                "--version",
                version,
                "--timeout",
                timeOut,
                "--stage",
                stage,
                "--artifact-id",
                ""
            };


            Expression<Func<IInputFactory, Inputs?>> parseInputs = s =>
                s.ParseAndValidateInputs(args);

            var inputs = new Inputs
            {
                ApiKey = key,
                PackageName = packageName,
                SolutionPath = solutionFile.FullName,
                Stage = Enum.Parse<Stage>(stage),
                TimeOut = TimeSpan.Parse(timeOut),
                Version = version
            };

            _inputParserMock.Setup(parseInputs).Returns(inputs);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(It.Is<LocalPackageConfig>(config => compareLocalPackageConfig(localPackageConfig, config)));

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
            var exitCode = await _gitHubAction.RunAsync(args, CancellationToken.None);

            // Then
            Assert.AreEqual(6, exitCode);

            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);

            _githubPresenterMock.Verify(p => p.PresentOutputVariable("artifact-id", id));

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentDataMinerSystemUnavailable(), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
            _packageServiceMock.VerifyNoOtherCalls();
            _githubPresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_StartingDeployment_Unauthorized()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory == "/github/workspace" ? Environment.CurrentDirectory : Path.Join(Environment.CurrentDirectory, "../../../../../"));
            var packageName = "TestPackage";
            var version = "1.0.2";
            var timeOut = "12:00";
            var stage = "All";
            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.DmScript);

            var args = new string[]
            {
                "--api-key",
                key,
                "--solution-path",
                solutionFile.FullName,
                "--package-name",
                packageName,
                "--version",
                version,
                "--timeout",
                timeOut,
                "--stage",
                stage,
                "--artifact-id",
                ""
            };


            Expression<Func<IInputFactory, Inputs?>> parseInputs = s =>
                s.ParseAndValidateInputs(args);

            var inputs = new Inputs
            {
                ApiKey = key,
                PackageName = packageName,
                SolutionPath = solutionFile.FullName,
                Stage = Enum.Parse<Stage>(stage),
                TimeOut = TimeSpan.Parse(timeOut),
                Version = version
            };

            _inputParserMock.Setup(parseInputs).Returns(inputs);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(It.Is<LocalPackageConfig>(config => compareLocalPackageConfig(localPackageConfig, config)));

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
            var exitCode = await _gitHubAction.RunAsync(args, CancellationToken.None);

            // Then
            Assert.AreEqual(8, exitCode);

            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);
            _githubPresenterMock.Verify(p => p.PresentOutputVariable("artifact-id", id));

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentUnauthorizedKey(), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
            _packageServiceMock.VerifyNoOtherCalls();
            _githubPresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_StartingDeployment_Failed()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory == "/github/workspace" ? Environment.CurrentDirectory : Path.Join(Environment.CurrentDirectory, "../../../../../"));
            var packageName = "TestPackage";
            var version = "1.0.2";
            var timeOut = "12:00";
            var stage = "All";
            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.DmScript);

            var args = new string[]
            {
                "--api-key",
                key,
                "--solution-path",
                solutionFile.FullName,
                "--package-name",
                packageName,
                "--version",
                version,
                "--timeout",
                timeOut,
                "--stage",
                stage,
                "--artifact-id",
                ""
            };


            Expression<Func<IInputFactory, Inputs?>> parseInputs = s =>
                s.ParseAndValidateInputs(args);

            var inputs = new Inputs
            {
                ApiKey = key,
                PackageName = packageName,
                SolutionPath = solutionFile.FullName,
                Stage = Enum.Parse<Stage>(stage),
                TimeOut = TimeSpan.Parse(timeOut),
                Version = version
            };

            _inputParserMock.Setup(parseInputs).Returns(inputs);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(It.Is<LocalPackageConfig>(config => compareLocalPackageConfig(localPackageConfig, config)));

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
            var exitCode = await _gitHubAction.RunAsync(args, CancellationToken.None);

            // Then
            Assert.AreEqual(6, exitCode);

            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadSucceeded(), Times.Once);
            _githubPresenterMock.Verify(p => p.PresentOutputVariable("artifact-id", id));

            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeployment(), Times.Once);
            _packageServiceMock.Verify(deployPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentStartingPackageDeploymentFailed(ex), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
            _packageServiceMock.VerifyNoOtherCalls();
            _githubPresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_UploadPackage_Failed()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory == "/github/workspace" ? Environment.CurrentDirectory : Path.Join(Environment.CurrentDirectory, "../../../../../"));
            var packageName = "TestPackage";
            var version = "1.0.2";
            var timeOut = "12:00";
            var stage = "All";
            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.DmScript);

            var args = new string[]
            {
                "--api-key",
                key,
                "--solution-path",
                solutionFile.FullName,
                "--package-name",
                packageName,
                "--version",
                version,
                "--timeout",
                timeOut,
                "--stage",
                stage,
                "--artifact-id",
                ""
            };


            Expression<Func<IInputFactory, Inputs?>> parseInputs = s =>
                s.ParseAndValidateInputs(args);

            var inputs = new Inputs
            {
                ApiKey = key,
                PackageName = packageName,
                SolutionPath = solutionFile.FullName,
                Stage = Enum.Parse<Stage>(stage),
                TimeOut = TimeSpan.Parse(timeOut),
                Version = version
            };

            _inputParserMock.Setup(parseInputs).Returns(inputs);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(It.Is<LocalPackageConfig>(config => compareLocalPackageConfig(localPackageConfig, config)));

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
            var exitCode = await _gitHubAction.RunAsync(args, CancellationToken.None);

            // Then
            Assert.AreEqual(5, exitCode);

            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageUploadFailed(ex), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
            _packageServiceMock.VerifyNoOtherCalls();
            _githubPresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_UploadPackage_Unauthorized()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory == "/github/workspace" ? Environment.CurrentDirectory : Path.Join(Environment.CurrentDirectory, "../../../../../"));
            var packageName = "TestPackage";
            var version = "1.0.2";
            var timeOut = "12:00";
            var stage = "All";
            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.DmScript);

            var args = new string[]
            {
                "--api-key",
                key,
                "--solution-path",
                solutionFile.FullName,
                "--package-name",
                packageName,
                "--version",
                version,
                "--timeout",
                timeOut,
                "--stage",
                stage,
                "--artifact-id",
                ""
            };


            Expression<Func<IInputFactory, Inputs?>> parseInputs = s =>
                s.ParseAndValidateInputs(args);

            var inputs = new Inputs
            {
                ApiKey = key,
                PackageName = packageName,
                SolutionPath = solutionFile.FullName,
                Stage = Enum.Parse<Stage>(stage),
                TimeOut = TimeSpan.Parse(timeOut),
                Version = version
            };

            _inputParserMock.Setup(parseInputs).Returns(inputs);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(It.Is<LocalPackageConfig>(config => compareLocalPackageConfig(localPackageConfig, config)));

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
            var exitCode = await _gitHubAction.RunAsync(args, CancellationToken.None);

            // Then
            Assert.AreEqual(8, exitCode);

            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationSucceeded(), Times.Once);

            _packagePresenterMock.Verify(p => p.PresentStartingPackageUpload(), Times.Once);
            _packageServiceMock.Verify(uploadPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentUnauthorizedKey(), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
            _packageServiceMock.VerifyNoOtherCalls();
            _githubPresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_CreatePackage_Failed()
        {
            // Given
            var key = Guid.NewGuid().ToString();
            var solutionFile = new FileInfo(@"C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln");
            var workingDirectory = new DirectoryInfo(Environment.CurrentDirectory == "/github/workspace" ? Environment.CurrentDirectory : Path.Join(Environment.CurrentDirectory, "../../../../../"));
            var packageName = "TestPackage";
            var version = "1.0.2";
            var timeOut = "12:00";
            var stage = "All";
            var id = Guid.NewGuid().ToString();

            var localPackageConfig = new LocalPackageConfig(
                solutionFile,
                workingDirectory,
                packageName,
                version,
                SolutionType.DmScript);

            var args = new string[]
            {
                "--api-key",
                key,
                "--solution-path",
                solutionFile.FullName,
                "--package-name",
                packageName,
                "--version",
                version,
                "--timeout",
                timeOut,
                "--stage",
                stage,
                "--artifact-id",
                ""
            };


            Expression<Func<IInputFactory, Inputs?>> parseInputs = s =>
                s.ParseAndValidateInputs(args);

            var inputs = new Inputs
            {
                ApiKey = key,
                PackageName = packageName,
                SolutionPath = solutionFile.FullName,
                Stage = Enum.Parse<Stage>(stage),
                TimeOut = TimeSpan.Parse(timeOut),
                Version = version
            };

            _inputParserMock.Setup(parseInputs).Returns(inputs);

            Expression<Func<IPackageService, Task<CreatedPackage>>> createPackageAsync = s =>
                s.CreatePackageAsync(It.Is<LocalPackageConfig>(config => compareLocalPackageConfig(localPackageConfig, config)));

            var createPackageException = new CreatePackageException("this should be thrown in the test");
            _packageServiceMock
                .Setup(createPackageAsync)
                .ThrowsAsync(createPackageException);

            // When
            var exitCode = await _gitHubAction.RunAsync(args, CancellationToken.None);

            // Then
            Assert.AreEqual(4, exitCode);

            _packagePresenterMock.Verify(p => p.PresentStartCreatingPackage(), Times.Once);
            _packageServiceMock.Verify(createPackageAsync, Times.Once);
            _packagePresenterMock.Verify(p => p.PresentPackageCreationFailed(createPackageException), Times.Once);

            _packagePresenterMock.VerifyNoOtherCalls();
            _packageServiceMock.VerifyNoOtherCalls();
            _githubPresenterMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RunAsync_InvalidArguments()
        {
            // Given
            var args = new string[]
            {
                "--api-key",
                "",
                "--solution-path",
                "",
                "--package-name",
                "",
                "--version",
                "",
                "--timeout",
                "",
                "--stage",
                "",
                "--artifact-id",
                ""
            };

            Expression<Func<IInputFactory, Inputs?>> parseInputs = s =>
                s.ParseAndValidateInputs(args);

            _inputParserMock.Setup(parseInputs).Returns((Inputs)null!);

            // When
            var exitCode = await _gitHubAction.RunAsync(args, CancellationToken.None);

            // Then
            Assert.AreEqual(3, exitCode);

            _githubPresenterMock.Verify(p => p.PresentInvalidArguments());

            _packagePresenterMock.VerifyNoOtherCalls();
            _packageServiceMock.VerifyNoOtherCalls();
            _githubPresenterMock.VerifyNoOtherCalls();
        }

        public static bool compareLocalPackageConfig(LocalPackageConfig expected, LocalPackageConfig actual)
        {
            return expected.PackageName == actual.PackageName
                   && expected.Version == actual.Version
                   && expected.ConvertedSolutionWorkingDirectory.Name == actual.ConvertedSolutionWorkingDirectory.Name
                   && expected.SolutionFile.FullName == actual.SolutionFile.FullName
                   && expected.Type == actual.Type;
        }
    }
}