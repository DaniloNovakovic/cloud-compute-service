using System;
using NUnit.Framework;

namespace JobWorker.Tests
{
    [TestFixture]
    public class WorkerTests
    {
        private Worker worker;

        [SetUp]
        public void SetUp()
        {
            this.worker = new Worker();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Start_ContainerIdIsNullEmptyOrWhiteSpace_ThrowArgumentException(string containerId)
        {
            Assert.That(() => this.worker.Start(containerId), Throws.Exception.InstanceOf<ArgumentException>());
        }

        [Test]
        public void Stop_WhenCalledBeforeStart_ThrowInvalidOperationException()
        {
            Assert.That(() => this.worker.Stop(), Throws.Exception.InstanceOf<InvalidOperationException>());
        }
    }
}