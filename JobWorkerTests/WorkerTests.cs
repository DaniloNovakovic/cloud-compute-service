using NUnit.Framework;
using JobWorker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobWorker.Tests
{
    [TestFixture]
    public class WorkerTests
    {
        private Worker worker;

        [SetUp]
        public void SetUp()
        {
            worker = new Worker();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Start_IsNullEmptyOrWhiteSpace_ThrowArgumentException(string containerId)
        {
            Assert.That(() => worker.Start(containerId), Throws.Exception.InstanceOf<ArgumentException>());
        }

        [Test]
        public void Stop_IsCalledBeforeStop_ThrowInvalidOperationException()
        {
            Assert.That(() => worker.Stop(), Throws.Exception.InstanceOf<InvalidOperationException>());
        }
    }
}