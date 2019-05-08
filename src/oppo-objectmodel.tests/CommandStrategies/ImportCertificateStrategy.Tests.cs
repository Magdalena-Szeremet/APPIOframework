using System.IO;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Oppo.ObjectModel.CommandStrategies.ImportCommands;
using Oppo.Resources.text.logging;
using Oppo.Resources.text.output;

namespace Oppo.ObjectModel.Tests.CommandStrategies
{
    public class ImportCertificateStrategyTests
    {
        private ImportCertificateStrategy _objectUnderTest;
        private Mock<IFileSystem> _fileSystemMock;
        private Mock<ILoggerListener> _loggerListenerMock;
        
        private static object[] ValidInputsDER =
        {
            new [] {"--key", "myfile.der", "--certificate", "mycert.der", "-p", "myproject"}
        };
        
        private static object[] ValidInputsPEMFormat =
        {
            new [] {"--key", "myfile.key", "--certificate", "mycert.pem", "-p", "myproject"},
            new [] {"-k", "myfile.key", "-c", "mycert.pem", "-p", "myproject"},
            new [] {"-k", "myfile.key", "--certificate", "mycert.pem", "-p", "myproject"},
            new [] {"-k", "myfile.key", "--certificate", "mycert.crt", "-p", "myproject"}
        };

        private static object[] InvalidInputsCaughtByCommandLineParser =
        {
            new[] {"-Key", "myfile.der", "--certificate", "mycert.der", "-p", "myproject"},
            new[] {"-k", "myfile.der", "--certificate", "mycert.der"},
        };

        private static object[] ValidInputsDERClientServer =
        {
            new [] {"--client", "--key", "myfile.der", "--certificate", "mycert.der", "-p", "myproject"},
            new [] {"--server", "--key", "myfile.der", "--certificate", "mycert.der", "-p", "myproject"}
        };
        
        private static object[] InvalidBothClientServer =
        {
            new [] {"--client", "--server", "--key", "myfile.der", "--certificate", "mycert.der", "-p", "myproject"}
        };
        
        private static readonly string SampleClientServerAppProject = $"{{\"name\": \"App\",\"type\": \"{Constants.ApplicationType.ClientServer}\"}}";
        private static readonly string SampleClientAppProject = $"{{\"name\": \"App\",\"type\": \"{Constants.ApplicationType.Client}\"}}";

        const string OppoprojPath = "oppoproj-path";

        [SetUp]
        public void Setup()
        {
            _fileSystemMock = new Mock<IFileSystem>();
            _objectUnderTest = new ImportCertificateStrategy(_fileSystemMock.Object);
                    
            _loggerListenerMock = new Mock<ILoggerListener>();
            OppoLogger.RegisterListener(_loggerListenerMock.Object);
        }

        [TearDown]
        public void Cleanup()
        {
            OppoLogger.RemoveListener(_loggerListenerMock.Object);
        }

        [TestCaseSource(nameof(ValidInputsDER))]
        public void CopiesOverFilesWithRightFileType(string[] inputParams)
        {
            // arrange
            var keyFile = inputParams[1];
            var certFile = inputParams[3];
            var project = inputParams[5];
            
            MockPathCombine(project);
            var stream = MockFileRead(OppoprojPath, SampleClientAppProject);
            
            // act
            var result = _objectUnderTest.Execute(inputParams);

            // assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(OutputText.ImportCertificateCommandSuccess,result.OutputMessages.First().Key);
            _fileSystemMock.Verify(fs => fs.CopyFile(keyFile, project), Times.Once);
            _fileSystemMock.Verify(fs => fs.CopyFile(certFile, project), Times.Once);
            _loggerListenerMock.Verify(l => l.Info(string.Format(LoggingText.ImportCertificateSuccess, certFile, keyFile)));
            _loggerListenerMock.VerifyNoOtherCalls();
            stream.Close();
        }

        [TestCaseSource(nameof(InvalidInputsCaughtByCommandLineParser))]
        public void FailsWithInvalidInputs(string[] inputParams)
        {
            // arrange
            
            // act
            var result = _objectUnderTest.Execute(inputParams);

            // assert
            Assert.IsFalse(result.Success);
        }
        
        [TestCaseSource(nameof(ValidInputsPEMFormat))]
        public void ConvertsFilesWithExplicitParameter(string[] inputParams)
        {
            // arrange
            var keyFile = inputParams[1];
            var certFile = inputParams[3];
            var project = inputParams[5];
            
            MockPathCombine(project);
            var stream = MockFileRead(OppoprojPath, SampleClientAppProject);
            
            const string targetCertPath = "certificate-path";
            const string targetKeyPath = "key-path";
            _fileSystemMock.Setup(fs => fs.CombinePaths(project, Constants.FileName.Certificate)).Returns(targetCertPath);
            _fileSystemMock.Setup(fs => fs.CombinePaths(project, Constants.FileName.PrivateKeyDER)).Returns(targetKeyPath);
            var openSSLCertArgs = string.Format(Constants.ExternalExecutableArguments.OpenSSLConvertCertificateFromPEM, certFile, targetCertPath);
            var openSSLKeyArgs = string.Format(Constants.ExternalExecutableArguments.OpenSSLConvertKeyFromPEM, keyFile, targetKeyPath);
            
            // act
            var result = _objectUnderTest.Execute(inputParams);

            // assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(OutputText.ImportCertificateCommandSuccess,result.OutputMessages.First().Key);
            _fileSystemMock.Verify(fs => fs.CallExecutable(Constants.ExecutableName.OpenSSL, null, openSSLCertArgs), Times.Once);
            _fileSystemMock.Verify(fs => fs.CallExecutable(Constants.ExecutableName.OpenSSL, null, openSSLKeyArgs), Times.Once);
            _fileSystemMock.Verify(fs => fs.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _loggerListenerMock.Verify(l => l.Info(string.Format(LoggingText.ImportCertificateSuccess, certFile, keyFile)));
            _loggerListenerMock.VerifyNoOtherCalls();
            
            stream.Close();
        }
        
        [Test]
        public void CorrectHelpText()
        {
            Assert.AreEqual(string.Empty, _objectUnderTest.GetHelpText());
        }

        [TestCaseSource(nameof(ValidInputsDERClientServer))]
        public void ImportAsCorrectFileInClientServer(string[] inputParams)
        {
            var keyFile = inputParams[2];
            var certFile = inputParams[4];
            var project = inputParams[6];

            // arrange
            MockPathCombine(project);
            
            var stream = MockFileRead(OppoprojPath, SampleClientServerAppProject);
            
            // act
            var result = _objectUnderTest.Execute(inputParams);
            
            // assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(OutputText.ImportCertificateCommandSuccess,result.OutputMessages.First().Key);
            _loggerListenerMock.Verify(l => l.Info(string.Format(LoggingText.ImportCertificateSuccess, certFile, keyFile)));
            _loggerListenerMock.VerifyNoOtherCalls();
            
            _fileSystemMock.Verify(fs => fs.CopyFile(keyFile, project), Times.Once);
            _fileSystemMock.Verify(fs => fs.CopyFile(certFile, project), Times.Once);
            
            stream.Close();
        }

        [TestCaseSource(nameof(ValidInputsDERClientServer))]
        public void FailWhenSpecifyingInstanceOnClientApp(string[] inputParams)
        {
            // arrange
            var isServer = inputParams[0] == "--server";
            var keyFile = inputParams[2];
            var certFile = inputParams[4];
            var project = inputParams[6];
            
            MockPathCombine(project);
            var stream = MockFileRead(OppoprojPath, SampleClientAppProject);
            
            // act
            var result = _objectUnderTest.Execute(inputParams);
            
            // assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(OutputText.ImportCertificateCommandWrongServerClient, result.OutputMessages.First().Key);
            _fileSystemMock.Verify(fs => fs.CallExecutable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _fileSystemMock.Verify(fs => fs.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _loggerListenerMock.Verify(l => l.Warn(LoggingText.ImportCertificateFailureWrongClientServer));
            _loggerListenerMock.VerifyNoOtherCalls();
            
            stream.Close();
        }
        
        [TestCaseSource(nameof(ValidInputsDER))]
        public void FailWhenServerClientNotSpecified(string[] inputParams)
        {
            // arrange
            var project = inputParams[5];
            
            MockPathCombine(project);
            var stream = MockFileRead(OppoprojPath, SampleClientServerAppProject);
            
            // act
            var result = _objectUnderTest.Execute(inputParams);
            
            // assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(OutputText.ImportCertificateCommandWrongServerClient, result.OutputMessages.First().Key);
            _fileSystemMock.Verify(fs => fs.CallExecutable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _fileSystemMock.Verify(fs => fs.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _loggerListenerMock.Verify(l => l.Warn(LoggingText.ImportCertificateFailureMissingClientServer));
            _loggerListenerMock.VerifyNoOtherCalls();
            stream.Close();
        }
        
        [TestCaseSource(nameof(InvalidBothClientServer))]
        public void FailWhenBothServerClientSpecified(string[] inputParams)
        {
            // arrange
            var project = inputParams[7];
            
            MockPathCombine(project);
            var stream = MockFileRead(OppoprojPath, SampleClientServerAppProject);
            
            // act
            var result = _objectUnderTest.Execute(inputParams);
            
            // assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(OutputText.ImportCertificateCommandWrongServerClient, result.OutputMessages.First().Key);
            _fileSystemMock.Verify(fs => fs.CallExecutable(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _fileSystemMock.Verify(fs => fs.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _loggerListenerMock.Verify(l => l.Warn(LoggingText.ImportCertificateFailureWrongClientServer));
            _loggerListenerMock.VerifyNoOtherCalls();
            stream.Close();
        }
        
        private MemoryStream MockFileRead(string path, string contents)
        {
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(contents));
            _fileSystemMock.Setup(fs => fs.ReadFile(path)).Returns(stream);
            return stream;
        }

        private void MockPathCombine(string project)
        {
            _fileSystemMock.Setup(fs => fs.CombinePaths(project, project + Constants.FileExtension.OppoProject)).Returns(OppoprojPath);
        }
    }
}