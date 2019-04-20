using System;
using Common;
using Moq;
using NUnit.Framework;

namespace Container.Tests

{
    [TestFixture]
    public class ContainerManagementTests
    {
        private readonly string defaultAssemblyName = "myAssembly.dll";
        private Mock<IAssemblyLoader> assemblyLoaderMock;
        private ContainerManagement management;
        private Mock<IWorker> workerMock;

        [Test]
        public void CheckHealth_WhenCalled_ReturnHealthy()
        {
            Assert.That(this.management.CheckHealth(), Does.Contain("Healthy").IgnoreCase);
        }

        [Test]
        public void Constructor_OnContainerManagementIsLoaded_ContainerIdIsNotNullOrWhitespace()
        {
            Assert.That(string.IsNullOrWhiteSpace(ContainerManagement.ContainerId), Is.False);
        }

        [Test]
        public void Load_AssemblyIsLoaded_CallsStartMethodFromAssemblyWithContainerId()
        {
            this.management.Load(this.defaultAssemblyName);
            this.workerMock.Verify(worker => worker.Start(It.Is<string>(id => id == ContainerManagement.ContainerId)));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Load_AssemblyNameIsNullEmptyOrWhitespace_ReturnErrorMessage(string assemblyName)
        {
            var result = this.management.Load(assemblyName);
            Assert.That(result, Does.Contain("error").IgnoreCase);
        }

        [Test]
        public void Load_OnFailure_ReturnErrorMessage()
        {
            this.assemblyLoaderMock.Setup(exp => exp.LoadAssembly(defaultAssemblyName)).Throws<Exception>();
            string result = this.management.Load(this.defaultAssemblyName);
            Assert.That(result, Does.Contain("error").IgnoreCase);
        }

        [Test]
        public void Load_OnSuccess_ReturnSuccessMessage()
        {
            string result = this.management.Load(this.defaultAssemblyName);
            Assert.That(result, Does.Contain("success").IgnoreCase);
        }

        [Test]
        public void Load_WhenCalled_LoadsAssembly()
        {
            this.management.Load(this.defaultAssemblyName);
            this.assemblyLoaderMock.Verify(loader => loader.LoadAssembly(It.Is<string>(str => str == this.defaultAssemblyName)));
        }

        [SetUp]
        public void SetUp()
        {
            this.workerMock = new Mock<IWorker>();
            this.assemblyLoaderMock = new Mock<IAssemblyLoader>();
            this.assemblyLoaderMock.Setup(loader => loader.LoadAssembly(It.IsAny<string>())).Returns(this.workerMock.Object);
            this.management = new ContainerManagement(this.assemblyLoaderMock.Object);
        }
    }
}