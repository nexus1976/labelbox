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
        public virtual bool Any()
        {
            return _list.Any();
        }
        public virtual Guid Remove(CancellationToken cancellation)
        {
            Guid item = Guid.Empty;
            if (_list.Any())
            {
                item = _list.First();
                _list.Remove(item);
            }
            return item;
        }
    }
}
