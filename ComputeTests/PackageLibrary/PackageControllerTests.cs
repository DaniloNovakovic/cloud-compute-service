using System;
using Moq;
using NUnit.Framework;

namespace Compute.Tests
{
    [TestFixture()]
    public class PackageControllerTests
    {
        private readonly string defaultAssemblyName = "MyAssembly.dll";
        private readonly string defaultConfigPath = "myPath.xml";
        private readonly int defaultNumberOfInstances = 2;
        private readonly int maxAllowedNumberOfInstances = 4;
        private Mock<IFileIO> fileIOMock;
        private PackageController packageController;
        private Mock<IPackageReader> readerMock;
        private PackageReaderResult validReaderResult;

        [Test()]
        public void CopyFile_WhenCalled_CopiesFile()
        {
            const string fromPath = "myOriginalFile.dll";
            const string toPath = "myCopiedFile.dll";
            this.packageController.CopyFile(fromPath, toPath);
            this.fileIOMock.Verify(writer => writer.CopyFile(fromPath, toPath));
        }

        [Test()]
        public void DeletePackage_WhenCalled_DeletesPackage()
        {
            const string packageFolder = "myPackageFolder";
            this.packageController.DeletePackage(packageFolder);
            this.fileIOMock.Verify(writer => writer.ClearFolder(packageFolder));
        }

        [Test]
        public void ReadPackage_AssemblyFileDoesNotExist_ThrowException()
        {
            this.fileIOMock.Setup(io => io.FileExists(It.IsAny<string>())).Returns(false);
            Assert.That(() => this.packageController.ReadPackage(this.defaultConfigPath, this.defaultNumberOfInstances), Throws.Exception);
        }

        [Test()]
        public void ReadPackage_NumberOfInstancesIsGreaterThenAllowed_ThrowException()
        {
            this.readerMock.Setup(reader => reader.ReadPackage(this.defaultConfigPath)).Returns(new PackageReaderResult()
            {
                AssemblyName = defaultAssemblyName,
                NumberOfInstances = this.maxAllowedNumberOfInstances + 1
            });
            Assert.That(() => this.packageController.ReadPackage(this.defaultConfigPath, this.maxAllowedNumberOfInstances), Throws.Exception);
        }

        [Test]
        public void ReadPackage_NumberOfInstancesIsLessThanZero_ThrowException()
        {
            this.readerMock.Setup(reader => reader.ReadPackage(this.defaultConfigPath)).Returns(new PackageReaderResult()
            {
                AssemblyName = defaultAssemblyName,
                NumberOfInstances = -5
            });
            Assert.That(() => this.packageController.ReadPackage(this.defaultConfigPath, this.maxAllowedNumberOfInstances), Throws.Exception);
        }

        [Test()]
        public void ReadPackage_PackageIsValid_ReturnsPackage()
        {
            var result = this.packageController.ReadPackage(this.defaultConfigPath, this.maxAllowedNumberOfInstances);
            Assert.That(result.AssemblyName, Is.EqualTo(this.validReaderResult.AssemblyName));
            Assert.That(result.NumberOfInstances, Is.EqualTo(this.validReaderResult.NumberOfInstances));
        }

        [Test()]
        public void ReadPackage_WhenCalled_CallsPackageReader()
        {
            this.packageController.ReadPackage(this.defaultConfigPath);
            this.readerMock.Verify(reader => reader.ReadPackage(this.defaultConfigPath));
        }

        [SetUp]
        public void SetUp()
        {
            this.validReaderResult = new PackageReaderResult()
            {
                AssemblyName = defaultAssemblyName,
                NumberOfInstances = defaultNumberOfInstances
            };
            this.readerMock = new Mock<IPackageReader>();
            this.fileIOMock = new Mock<IFileIO>();
            this.packageController = new PackageController(this.readerMock.Object, this.fileIOMock.Object);

            this.readerMock.Setup(reader => reader.ReadPackage(It.Is<string>(str => string.IsNullOrWhiteSpace(str)))).Throws<Exception>();
            this.readerMock.Setup(reader => reader.ReadPackage(It.Is<string>(str => !string.IsNullOrWhiteSpace(str)))).Returns(this.validReaderResult);

            this.fileIOMock.Setup(io => io.FileExists(It.Is<string>(str => !string.IsNullOrWhiteSpace(str)))).Returns(true);
        }
    }
}