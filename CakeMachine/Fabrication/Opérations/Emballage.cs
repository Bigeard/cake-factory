using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Paramètres;
using CakeMachine.Utils;

namespace CakeMachine.Fabrication.Opérations
{
    public class Emballage
    {
        private readonly SemaphoreSlim _lock;
        private readonly TimeSpan _tempsEmballage;
        private readonly ThreadSafeRandomNumberGenerator _rng;
        private readonly double _defectRate;

        public Emballage(ThreadSafeRandomNumberGenerator rng, ParamètresEmballage paramètres)
        {
            var (nombrePlaces, defectRate, tempsEmballage) = paramètres;
            _lock = new SemaphoreSlim(nombrePlaces);
            _tempsEmballage = tempsEmballage;
            _defectRate = defectRate;

            _rng = rng;
        }

        public int PlacesRestantes => _lock.CurrentCount;

        public GâteauEmballé Emballer(GâteauCuit gâteau)
        {
            _lock.Wait();

            try
            {
                Thread.Sleep(_tempsEmballage);
                return new GâteauEmballé(gâteau, _rng.NextBoolean(1 - _defectRate));
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<GâteauEmballé> EmballerAsync(GâteauCuit gâteau)
        {
            await _lock.WaitAsync();

            try
            {
                await Task.Delay(_tempsEmballage);
                return new GâteauEmballé(gâteau, _rng.NextBoolean(1 - _defectRate));
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
