using NUnit.Framework;

namespace AkkaChat.Web.Tests
{
    /// <summary>
    /// Ensures that the DB is setup before all specs are run
    /// and cleaned up after the run is finished.
    /// </summary>
    [SetUpFixture]
    public class AkkaChatSetup
    {
        [SetUp]
        public void SetUp()
        {
            DbSupport.Initialize();
        }

        [TearDown]
        public void Teardown()
        {
            DbSupport.Clean();
        }
    }
}