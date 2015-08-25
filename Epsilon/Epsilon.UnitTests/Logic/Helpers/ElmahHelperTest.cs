using Epsilon.Logic.Helpers;
using NUnit.Framework;
using System;

namespace Epsilon.UnitTests.Logic.Helpers
{
    [TestFixture]
    public class ElmahHelperTest
    {
        [Test]
        public void RaiseDoesNotThrow()
        {
            var helper = new ElmahHelper();
            var exception = new Exception();

            // There is no context when calling from the test, so it should throw if it
            // is not wrapped in a try catch that swallows the exception.
            Assert.DoesNotThrow(() => helper.Raise(exception));
        }
    }
}
