﻿using Moq;
using NUnit.Framework;
using Oppo.ObjectModel.CommandStrategies.CleanCommands;

namespace Oppo.ObjectModel.Tests.CommandStrategies
{
    public class CleanStrategyTests
    {
        private Mock<ICommandFactory<ObjectModel>> _factoryMock;
        private CleanStrategy _objectUnderTest;

        [SetUp]
        public void SetUp_ObjectUnderTest()
        {
            _factoryMock = new Mock<ICommandFactory<ObjectModel>>();
            _objectUnderTest = new CleanStrategy(_factoryMock.Object);
        }

        [Test]
        public void CleanStrategy_Should_ImplementICommandOfObjectModel()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsInstanceOf<ICommand<ObjectModel>>(_objectUnderTest);
        }

        [Test]
        public void CleanStrategy_Should_HaveCorrectCommandName()
        {
            // Arrange

            // Act
            var name = _objectUnderTest.Name;

            // Assert
            Assert.AreEqual(Constants.CommandName.Clean, name);
        }

        [Test]
        public void CleanStrategy_Should_ProvideSomeHelpText()
        {
            // Arrange

            // Act
            var helpText = _objectUnderTest.GetHelpText();

            // Assert
            Assert.AreEqual(Resources.text.help.HelpTextValues.CleanCommand, helpText);
        }

        [Test]
        public void CleanStrategy_Should_ExecuteCommand()
        {
            // Arrange
            var inputParams = new[] { "--any-param", "any-value" };
            var commandResultMock = new CommandResult(true, "any-message");

            var commandMock = new Mock<ICommand<ObjectModel>>();
            commandMock.Setup(x => x.Execute(It.IsAny<string[]>())).Returns(commandResultMock);

            _factoryMock.Setup(x => x.GetCommand("--any-param")).Returns(commandMock.Object);

            // Act
            var result = _objectUnderTest.Execute(inputParams);

            // Assert
            Assert.AreEqual(commandResultMock, result);
        }
    }
}
