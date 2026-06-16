namespace test_00_prepare
{
    [TestClass]
    public sealed class Test00Prepare
    {
        [TestMethod]
        public void Test00_Welcome()
        {
            bool isEnvironmentReady = true;
            Assert.IsTrue(isEnvironmentReady, "Welcome to your dotnet-workshop journey!");
        }
    }
}
