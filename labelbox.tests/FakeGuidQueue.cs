namespace labelbox.tests
{
    internal class FakeGuidQueue
    {
        private readonly List<Guid> _list;
        public FakeGuidQueue()
        {
            _list = new List<Guid>();
        }
        public virtual void Add(Guid item, CancellationToken cancellationToken)
        {
            _list.Add(item);
        }
    }
}
