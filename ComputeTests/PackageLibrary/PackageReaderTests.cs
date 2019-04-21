using System;
using System.Xml.Linq;
using Moq;
using NUnit.Framework;

namespace Compute.Tests
{
    [TestFixture]
    public class PackageReaderTests
    {
        private readonly string defaultPackagePath = "myConfig.xml";
        private readonly string defaultAssemblyName = "MyAssembly.dll";
        private readonly int defaultNumberOfInstances = 2;
        private PackageReader packageReader;
        private Mock<IXDocumentLoader> xDocLoaderMock;
        private XDocument xDocValid;

        [Test]
        public void ReadPackage_WhenCalled_LoadsConfigFile()
        {
            this.packageReader.ReadPackage(this.defaultPackagePath);
            this.xDocLoaderMock.Verify(loader => loader.Load(this.defaultPackagePath));
        }

        [Test]
        public void ReadPackage_ConfigIsValid_ReturnProperlyFilledPackageReader()
        {
            var result = this.packageReader.ReadPackage(this.defaultPackagePath);
            Assert.That(result.AssemblyName, Is.EqualTo(this.defaultAssemblyName));
            Assert.That(result.NumberOfInstances, Is.EqualTo(this.defaultNumberOfInstances));
        }

        [Test]
        public void ReadPackage_ConfigIsInvalid_ThrowException()
        {
            this.xDocLoaderMock.Setup(loader => loader.Load(It.IsAny<string>())).Returns(new XDocument(new XElement("doc")));
            Assert.That(() => this.packageReader.ReadPackage(this.defaultPackagePath), Throws.Exception);
        }

        [SetUp]
        public void SetUp()

        {
            this.xDocLoaderMock = new Mock<IXDocumentLoader>();
            this.packageReader = new PackageReader(this.xDocLoaderMock.Object);
            this.xDocValid = new XDocument(new XElement("doc",
                new XElement("assembly", new XElement("name", this.defaultAssemblyName)),
                new XElement("numberOfInstances", new XAttribute("value", this.defaultNumberOfInstances))));
            this.xDocLoaderMock.Setup(loader => loader.Load(It.Is<string>(str => string.IsNullOrWhiteSpace(str)))).Throws<ArgumentNullException>();
            this.xDocLoaderMock.Setup(loader => loader.Load(It.Is<string>(str => !string.IsNullOrWhiteSpace(str)))).Returns(this.xDocValid);
        }
    }
}