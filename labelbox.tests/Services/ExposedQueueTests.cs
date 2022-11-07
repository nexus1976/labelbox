using labelbox.Services;

namespace labelbox.tests.Services
{
    [TestClass]
    public class ExposedQueueTests
    {
        private readonly IExposedQueue _exposedQueue;

        public ExposedQueueTests()
        {
            _exposedQueue = new ExposedQueue();
        }

        [TestMethod]
        public void WhenCalling_Enqueue()
        {
            // Arrange
            Guid expectedGUID = Guid.NewGuid();

            // Act
            _exposedQueue.Enqueue(expectedGUID, CancellationToken.None);

            // Assert
            Assert.IsTrue(_exposedQueue.HasItemsInQueue());
        }

        [TestMethod]
        public void WhenCalling_Dequeue()
        {
            // Arrange
            Guid expectedGUID = Guid.NewGuid();

            // Act
            _exposedQueue.Enqueue(expectedGUID, CancellationToken.None);
            var response = _exposedQueue.Dequeue(CancellationToken.None);

            // Assert
            Assert.IsFalse(_exposedQueue.HasItemsInQueue());
            Assert.AreEqual(expectedGUID, response);
        }
    }
}
