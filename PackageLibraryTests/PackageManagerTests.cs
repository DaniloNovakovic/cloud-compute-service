using System;
using Moq;
using NUnit.Framework;

namespace PackageLibrary.Tests
{
    [TestFixture()]
    public class PackageManagerTests
    {
        private PackageReaderResult validReaderResult;
        private readonly string defaultConfigPath = "myPath.xml";
        private readonly string defaultAssemblyName = "MyAssembly.dll";
        private readonly int defaultNumberOfInstances = 2;
        private PackageManager packageManager;
        private Mock<IPackageReader> readerMock;
        private Mock<IPackageWriter> writerMock;

        [SetUp]
        public void SetUp()
        {
            this.validReaderResult = new PackageReaderResult()
            {
                AssemblyName = defaultAssemblyName,
                NumberOfInstances = defaultNumberOfInstances
            };
            this.readerMock = new Mock<IPackageReader>();
            this.writerMock = new Mock<IPackageWriter>();
            this.packageManager = new PackageManager(this.readerMock.Object, this.writerMock.Object);

            this.readerMock.Setup(reader => reader.ReadPackage(It.Is<string>(str => string.IsNullOrWhiteSpace(str)))).Throws<Exception>();
            this.readerMock.Setup(reader => reader.ReadPackage(It.Is<string>(str => !string.IsNullOrWhiteSpace(str)))).Returns(this.validReaderResult);

            this.writerMock.Setup(writer => writer.Delete(It.Is<string>(str => string.IsNullOrWhiteSpace(str)))).Throws<Exception>();
            this.writerMock.Setup(writer => writer.Copy(It.Is<string>(str => string.IsNullOrWhiteSpace(str)), It.IsAny<string>())).Throws<Exception>();
            this.writerMock.Setup(writer => writer.Copy(It.IsAny<string>(), It.Is<string>(str => string.IsNullOrWhiteSpace(str)))).Throws<Exception>();
        }

        [Test()]
        public void ReadPackage_WhenCalled_CallsPackageReader()
        {
            this.packageManager.ReadPackage(this.defaultConfigPath);
            this.readerMock.Verify(reader => reader.ReadPackage(this.defaultConfigPath));
        }

        [Test()]
        public void ReadPackage_PackageIsValid_ReturnsPackage()
        {
            var result = this.packageManager.ReadPackage(this.defaultConfigPath);
            Assert.That(result.AssemblyName, Is.EqualTo(this.validReaderResult.AssemblyName));
            Assert.That(result.NumberOfInstances, Is.EqualTo(this.validReaderResult.NumberOfInstances));
        }

        [Test()]
        public void ReadPackage_PackageIsInvalid_ThrowException()
        {
            this.readerMock.Setup(reader => reader.ReadPackage(this.defaultConfigPath)).Returns(new PackageReaderResult()
            {
                AssemblyName = string.Empty,
                NumberOfInstances = int.MaxValue
            });
            Assert.That(() => this.packageManager.ReadPackage(this.defaultConfigPath), Throws.Exception);
        }

        [Test()]
        public void DeletePackage_WhenCalled_CallsPackageWriterDelete()
        {
            const string packageFolder = "myPackageFolder";
            this.packageManager.DeletePackage(packageFolder);
            this.writerMock.Verify(writer => writer.Delete(packageFolder));
        }

        [Test()]
        public void CopyPackageDll_WhenCalled_CallsPackageWriterCopy()
        {
            const string fromPath = "myOriginalFile.dll";
            const string toPath = "myCopiedFile.dll";
            this.packageManager.CopyFile(fromPath, toPath);
            this.writerMock.Verify(writer => writer.Copy(fromPath, toPath));
        }
    }
}