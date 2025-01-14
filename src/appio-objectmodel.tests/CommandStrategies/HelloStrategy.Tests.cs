﻿using Moq;
using NUnit.Framework;
using Appio.ObjectModel.CommandStrategies.HelloCommands;
using System.Linq;

namespace Appio.ObjectModel.Tests.CommandStrategies
{
    public class HelloStrategyTests
    {
        [Test]
        public void HelloStrategy_Should_ImplementICommandOfObjectModel()
        {
            // Arrange

            // Act
            var objectUnderTest = new HelloStrategy();

            // Assert
            Assert.IsInstanceOf<ICommand<ObjectModel>>(objectUnderTest);
        }

        [Test]
        public void HelloStrategy_Should_WriteHelloMessage()
        {
            // Arrange
            var objectUnderTest = new HelloStrategy();
            var inputArgsMock = new string[0];

            // Act
            var result = objectUnderTest.Execute(inputArgsMock);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.OutputMessages);
            Assert.AreEqual(Constants.HelloString, result.OutputMessages.First().Key);
            Assert.AreEqual(string.Empty, result.OutputMessages.First().Value);
        }

        [Test]
        public void ShouldReturnHelpText()
        {
            // Arrange
            var helloStrategy = new HelloStrategy();

            // Act
            var helpText = helloStrategy.GetHelpText();

            // Assert
            Assert.AreEqual(helpText, Resources.text.help.HelpTextValues.HelloCommand);
        }

        [Test]
        public void ShouldReturnCommandName()
        {
            // Arrange
            var helloStrategy = new HelloStrategy();

            // Act
            var commandName = helloStrategy.Name;

            // Assert
            Assert.AreEqual(commandName, Constants.CommandName.Hello);
        }
    }
}