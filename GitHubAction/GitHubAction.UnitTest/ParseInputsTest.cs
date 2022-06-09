using System;
using GitHubAction.Domain.Entities;
using GitHubAction.Factories;
using GitHubAction.Factories.Impl;
using GitHubAction.Presenters;
using Moq;
using NUnit.Framework;

namespace GitHubAction.UnitTest;

public class ParseInputsTest
{
    private Mock<IInputFactoryPresenter> _inputParserPresenterMock = null!;
    private IInputFactory _inputParserService = null!;

    [SetUp]
    public void Setup()
    {
        _inputParserPresenterMock = new Mock<IInputFactoryPresenter>();

        _inputParserService = new InputFactory(_inputParserPresenterMock.Object);

    }

    [Test]
    public void ParseAndValidateInputs_HappyFlow_All()
    {
        // Given
        var key = "some key";
        var solutionFile = "some file";
        var packageName = "TestPackage";
        var version = "1.0.2";
        var timeOut = "12:00";
        var stage = "All";
        var artifactId = "some string";

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
            artifactId
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        _inputParserPresenterMock.VerifyNoOtherCalls();

        Assert.IsNotNull(inputs);
        Assert.AreEqual(key, inputs.ApiKey);
        Assert.AreEqual(solutionFile, inputs.SolutionPath);
        Assert.AreEqual(packageName, inputs.PackageName);
        Assert.AreEqual(version, inputs.Version);
        Assert.AreEqual(TimeSpan.Parse(timeOut), inputs.TimeOut);
        Assert.AreEqual(Enum.Parse<Stage>(stage), inputs.Stage);
        Assert.IsNull(inputs.ArtifactId);
    }

    [Test]
    public void ParseAndValidateInputs_HappyFlow_Upload()
    {
        // Given
        var key = "some key";
        var solutionFile = "some file";
        var packageName = "TestPackage";
        var version = "1.0.2";
        var timeOut = "12:00";
        var stage = "Upload";
        var artifactId = "some string";

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
            artifactId
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        _inputParserPresenterMock.VerifyNoOtherCalls();

        Assert.IsNotNull(inputs);
        Assert.AreEqual(key, inputs.ApiKey);
        Assert.AreEqual(solutionFile, inputs.SolutionPath);
        Assert.AreEqual(packageName, inputs.PackageName);
        Assert.AreEqual(version, inputs.Version);
        Assert.AreEqual(TimeSpan.Parse(timeOut), inputs.TimeOut);
        Assert.AreEqual(Enum.Parse<Stage>(stage), inputs.Stage);
        Assert.IsNull(inputs.ArtifactId);
    }

    [Test]
    public void ParseAndValidateInputs_HappyFlow_Deploy()
    {
        // Given
        var key = "some key";
        var solutionFile = "some file";
        var packageName = "TestPackage";
        var version = "1.0.2";
        var timeOut = "12:00";
        var stage = "Deploy";
        var artifactId = "some string";

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
            artifactId
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        _inputParserPresenterMock.VerifyNoOtherCalls();

        Assert.IsNotNull(inputs);
        Assert.AreEqual(key, inputs.ApiKey);
        Assert.IsNull(inputs.SolutionPath);
        Assert.IsNull(inputs.PackageName);
        Assert.IsNull(inputs.Version);
        Assert.AreEqual(TimeSpan.Parse(timeOut), inputs.TimeOut);
        Assert.AreEqual(Enum.Parse<Stage>(stage), inputs.Stage);
        Assert.AreEqual(artifactId, inputs.ArtifactId);
    }

    [Test]
    public void ParseAndValidateInputs_InvalidArgs()
    {
        // Given
        string[] args = null!;

        Assert.Throws<ArgumentNullException>(delegate { _inputParserService.ParseAndValidateInputs(args); });
    }

    [Test]
    public void ParseAndValidateInputs_EmptyStage()
    {
        // Given
        var key = "some key";
        var solutionFile = "some file";
        var packageName = "TestPackage";
        var version = "1.0.2";
        var timeOut = "12:00";
        var stage = "";
        var artifactId = "some string";

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
            artifactId
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        _inputParserPresenterMock.Verify(p => p.PresentMissingArgument(InputArgurments.Stage), Times.Once);
        _inputParserPresenterMock.VerifyNoOtherCalls();

        Assert.IsNull(inputs);
    }

    [Test]
    [TestCase("All")]
    [TestCase("Upload")]
    [TestCase("Deploy")]
    public void ParseAndValidateInputs_EmptyApiKey(string stage)
    {
        // Given
        var key = "";
        var solutionFile = "some file";
        var packageName = "TestPackage";
        var version = "1.0.2";
        var timeOut = "12:00";
        var artifactId = "some string";

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
            artifactId
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        _inputParserPresenterMock.Verify(p => p.PresentMissingArgument(InputArgurments.ApiKey), Times.Once);
        _inputParserPresenterMock.VerifyNoOtherCalls();

        Assert.IsNull(inputs);
    }

    [Test]
    [TestCase("All")]
    [TestCase("Upload")]
    [TestCase("Deploy")]
    public void ParseAndValidateInputs_EmptyTimeOut(string stage)
    {
        // Given
        var key = "some key";
        var solutionFile = "some file";
        var packageName = "TestPackage";
        var version = "1.0.2";
        var timeOut = "";
        var artifactId = "some string";

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
            artifactId
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        _inputParserPresenterMock.Verify(p => p.PresentMissingArgument(InputArgurments.Timeout), Times.Once);
        _inputParserPresenterMock.VerifyNoOtherCalls();

        Assert.IsNull(inputs);
    }

    [Test]
    [TestCase("All", true)]
    [TestCase("Upload", true)]
    [TestCase("Deploy", false)]
    public void ParseAndValidateInputs_EmptySolutionFile(string stage, bool required)
    {
        // Given
        var key = "some key";
        var solutionFile = "";
        var packageName = "TestPackage";
        var version = "1.0.2";
        var timeOut = "12:00";
        var artifactId = "some string";

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
            artifactId
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        if (required) _inputParserPresenterMock.Verify(p => p.PresentMissingArgument(InputArgurments.SolutionPath), Times.Once);
        _inputParserPresenterMock.VerifyNoOtherCalls();

        if (required)
        {
            Assert.IsNull(inputs);
        }
        else
        {
            Assert.IsNotNull(inputs);
        }
    }

    [Test]
    [TestCase("All", true)]
    [TestCase("Upload", true)]
    [TestCase("Deploy", false)]
    public void ParseAndValidateInputs_EmptyPackageName(string stage, bool required)
    {
        // Given
        var key = "some key";
        var solutionFile = "some file";
        var packageName = "";
        var version = "1.0.2";
        var timeOut = "12:00";
        var artifactId = "some string";

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
            artifactId
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        if (required) _inputParserPresenterMock.Verify(p => p.PresentMissingArgument(InputArgurments.PackageName), Times.Once);
        _inputParserPresenterMock.VerifyNoOtherCalls();

        if (required)
        {
            Assert.IsNull(inputs);
        }
        else
        {
            Assert.IsNotNull(inputs);
        }
    }

    [Test]
    [TestCase("All", true)]
    [TestCase("Upload", true)]
    [TestCase("Deploy", false)]
    public void ParseAndValidateInputs_EmptyVersion(string stage, bool required)
    {
        // Given
        var key = "some key";
        var solutionFile = "some file";
        var packageName = "TestPackage";
        var version = "";
        var timeOut = "12:00";
        var artifactId = "some string";

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
            artifactId
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        if (required) _inputParserPresenterMock.Verify(p => p.PresentMissingArgument(InputArgurments.Version), Times.Once);
        _inputParserPresenterMock.VerifyNoOtherCalls();

        if (required)
        {
            Assert.IsNull(inputs);
        }
        else
        {
            Assert.IsNotNull(inputs);
        }
    }

    [Test]
    [TestCase("All", true)]
    [TestCase("Upload", true)]
    [TestCase("Deploy", false)]
    public void ParseAndValidateInputs_InvalidVersion(string stage, bool required)
    {
        // Given
        var key = "some key";
        var solutionFile = "some file";
        var packageName = "TestPackage";
        var version = "sqdfsdg";
        var timeOut = "12:00";
        var artifactId = "some string";

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
            artifactId
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        if (required) _inputParserPresenterMock.Verify(p => p.PresentInvalidVersionFormat(), Times.Once);
        _inputParserPresenterMock.VerifyNoOtherCalls();

        if (required)
        {
            Assert.IsNull(inputs);
        }
        else
        {
            Assert.IsNotNull(inputs);
        }
    }

    [Test]
    public void ParseAndValidateInputs_InvalidTimeOut_ToLow()
    {
        // Given
        var key = "some key";
        var solutionFile = "some file";
        var packageName = "TestPackage";
        var version = "sqdfsdg";
        var timeOut = "00:00";
        var stage = "All";
        var artifactId = "some string";

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
            artifactId
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        _inputParserPresenterMock.Verify(p => p.PresentTimeOutToLow(), Times.Once);
        _inputParserPresenterMock.VerifyNoOtherCalls();

        Assert.IsNull(inputs);
    }

    [Test]
    public void ParseAndValidateInputs_InvalidTimeOut_ToHigh()
    {
        // Given
        var key = "some key";
        var solutionFile = "some file";
        var packageName = "TestPackage";
        var version = "sqdfsdg";
        var timeOut = "15:00";
        var stage = "All";
        var artifactId = "some string";

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
            artifactId
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        _inputParserPresenterMock.Verify(p => p.PresentTimeOutToHigh(), Times.Once);
        _inputParserPresenterMock.VerifyNoOtherCalls();

        Assert.IsNull(inputs);
    }

    [Test]
    [TestCase("All", false)]
    [TestCase("Upload", false)]
    [TestCase("Deploy", true)]
    public void ParseAndValidateInputs_EmptyArtifactId(string stage, bool required)
    {
        // Given
        var key = "some key";
        var solutionFile = "some file";
        var packageName = "TestPackage";
        var version = "1.0.2";
        var timeOut = "12:00";
        var artifactId = "";

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
            artifactId
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        if (required) _inputParserPresenterMock.Verify(p => p.PresentMissingArgument(InputArgurments.ArtifactId), Times.Once);
        _inputParserPresenterMock.VerifyNoOtherCalls();

        if (required)
        {
            Assert.IsNull(inputs);
        }
        else
        {
            Assert.IsNotNull(inputs);
        }
    }

    [Test]
    public void ParseAndValidateInputs_InvalidArgument()
    {
        // Given
        var key = "some key";
        var solutionFile = "some file";
        var packageName = "TestPackage";
        var version = "1.0.2";
        var timeOut = "12:00";
        var stage = "All";
        var artifactId = "";

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
            artifactId,
            "--some-unknown-key",
            ""
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        _inputParserPresenterMock.Verify(p => p.PresentUnkownArgument("some-unknown-key"), Times.Once);
        _inputParserPresenterMock.VerifyNoOtherCalls();

        Assert.IsNotNull(inputs);

    }

    [Test]
    public void ParseAndValidateInputs_MissingArgument()
    {
        // Given
        var key = "some key";
        var solutionFile = "some file";
        var packageName = "TestPackage";
        var version = "1.0.2";
        var timeOut = "12:00";
        var stage = "All";

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
            stage
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        _inputParserPresenterMock.Verify(p => p.PresentKeyNotFound(It.Is<string>(s => s.Contains(InputArgurments.ArtifactId))), Times.Once);
        _inputParserPresenterMock.VerifyNoOtherCalls();

        Assert.IsNotNull(inputs);

    }

    [Test]
    public void ParseAndValidateInputs_InvalidStage()
    {
        // Given
        var key = "some key";
        var solutionFile = "some file";
        var packageName = "TestPackage";
        var version = "1.0.2";
        var timeOut = "12:00";
        var stage = "doesn't exist";

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
            ""
        };

        // When
        var inputs = _inputParserService.ParseAndValidateInputs(args)!;

        // Then
        _inputParserPresenterMock.Verify(p => p.PresentInvalidStage(), Times.Once);
        _inputParserPresenterMock.VerifyNoOtherCalls();

        Assert.IsNull(inputs);
    }
}
