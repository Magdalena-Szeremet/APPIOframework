using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Oppo.ObjectModel.CommandStrategies.BuildCommands;

namespace Oppo.ObjectModel.Tests
{
    public class BuildNameStrategyTests
    {
        private static string[][] InvalidInputs()
        {
            return new[]
            {
                new []{""},
                new string[0],
            };
        }

        private static string[][] ValidInputs()
        {
            return new[]
            {
                new []{"hugo"}
            };
        }      

        private static bool[][] FailingExecutableStates()
        {
            return new[]
            {
                new[] {false, false},
                new[] {true, false},
                new[] {false, true},
            };
        }

        [SetUp]
        public void Setup()
        {
        }
        

        [Test]
        public void BuildStrategy_Should_SucceedOnBuildableProject([ValueSource(nameof(ValidInputs))] string[] inputParams)
        {
            // Arrange
            var projectDirectoryName = inputParams.ElementAt(0);
            var projectBuildDirectory = Path.Combine(projectDirectoryName, Constants.DirectoryName.MesonBuild);

            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(x => x.CombinePaths(It.IsAny<string>(), It.IsAny<string>())).Returns(projectBuildDirectory);
            fileSystemMock.Setup(x => x.CallExecutable(Constants.ExecutableName.Meson, projectDirectoryName, Constants.DirectoryName.MesonBuild)).Returns(true);
            fileSystemMock.Setup(x => x.CallExecutable(Constants.ExecutableName.Ninja, projectBuildDirectory, string.Empty)).Returns(true);
            var buildStrategy = new BuildNameStrategy(fileSystemMock.Object);

            // Act
            var strategyResult = buildStrategy.Execute(inputParams);

            // Assert
            Assert.AreEqual(strategyResult, Constants.CommandResults.Success);
            fileSystemMock.VerifyAll();
        }

        [Test]
        public void ShouldExecuteStrategy_Fail_MissingParameter([ValueSource(nameof(InvalidInputs))] string[] inputParams)
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();

            var buildStrategy = new BuildNameStrategy(fileSystemMock.Object);

            // Act
            var strategyResult = buildStrategy.Execute(inputParams);

            // Assert
            Assert.AreEqual(strategyResult, Constants.CommandResults.Failure);
        }

        [Test]
        public void BuildStrategy_ShouldFail_DueToFailingExecutableCalls([ValueSource(nameof(FailingExecutableStates))] bool[] executableStates)
        {
            // Arrange
            var mesonState = executableStates.ElementAt(0);
            var ninjaState = executableStates.ElementAt(1);

            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(x => x.CallExecutable(Constants.ExecutableName.Meson, It.IsAny<string>(), It.IsAny<string>())).Returns(mesonState);
            fileSystemMock.Setup(x => x.CallExecutable(Constants.ExecutableName.Ninja, It.IsAny<string>(), It.IsAny<string>())).Returns(ninjaState);

            var buildStrategy = new BuildNameStrategy(fileSystemMock.Object);

            // Act
            var strategyResult = buildStrategy.Execute(new[] {"hugo"});

            // Assert
            Assert.AreEqual(Constants.CommandResults.Failure, strategyResult);
        }
    }
}