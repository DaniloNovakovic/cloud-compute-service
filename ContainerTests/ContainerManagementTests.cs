using NUnit.Framework;
using Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Container.Tests
{
    [TestFixture]
    public class ContainerManagementTests
    {
        private ContainerManagement management;

        [SetUp]
        public void SetUp()
        {
            management = new ContainerManagement();
        }

        [Test]
        public void CheckHealth_WhenCalled_ReturnHealthy()
        {
            Assert.That(management.CheckHealth(), Is.EqualTo("Healthy").IgnoreCase);
        }

        [Test]
        public void LoadTest()
        {
            Assert.Fail();
        }
    }
}