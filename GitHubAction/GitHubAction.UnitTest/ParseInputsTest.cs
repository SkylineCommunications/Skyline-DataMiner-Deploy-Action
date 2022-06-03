using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace GitHubAction.UnitTest;

public class ParseInputsTest
{
    private Mock<ILogger> _loggerMock = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger>();
    }

    [Test]
    public void ParseAndValidateInputs_HappyFlow()
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
            stage,
            "--artifact-id",
            ""
        };

        // When
        var inputs = ParseInputs.ParseAndValidateInputs(args, _loggerMock.Object);

        // Then
        Assert.IsNotNull(inputs);
    }

    [Test]
    public void ParseAndValidateInputs_InvalidArgs()
    {
        // Given
        string[] args = null!;


        Assert.Throws<ArgumentNullException>(delegate { ParseInputs.ParseAndValidateInputs(args, _loggerMock.Object); });
    }


    [Test]
    public void ParseAndValidateInputs_GivesWarning()
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
            stage,
            "--artifact-id",
            "",
            "--Some-unknown-key",
            ""
        };

        // When
        var inputs = ParseInputs.ParseAndValidateInputs(args, _loggerMock.Object);

        // Then
        _loggerMock.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Unknown argument \"Some-unknown-key\"" && @type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
