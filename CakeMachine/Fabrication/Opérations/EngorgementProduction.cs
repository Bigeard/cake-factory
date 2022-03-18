namespace CakeMachine.Fabrication.Opérations
{
    internal class EngorgementProduction
    {
        private readonly SemaphoreSlim _lock;

        public EngorgementProduction(ushort places)
        {
            _lock = new SemaphoreSlim(places);
        }

        public void Wait() => _lock.Wait();

        public async Task WaitAsync() => await _lock.WaitAsync().ConfigureAwait(false);

        public void Release() => _lock.Release();

        public int PlacesRestantes => _lock.CurrentCount;
    }
}