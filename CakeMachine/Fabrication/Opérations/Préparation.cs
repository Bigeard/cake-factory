using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Paramètres;
using CakeMachine.Utils;

namespace CakeMachine.Fabrication.Opérations
{
    public class Préparation
    {
        private readonly (TimeSpan Min, TimeSpan Max) _tempsPréparation;
        private readonly ThreadSafeRandomNumberGenerator _rng;
        private readonly double _defectRate;

        private readonly EngorgementProduction _lock;
        private TimeSpan TempsPréparation => _rng.NextDouble() * _tempsPréparation.Min + (_tempsPréparation.Max - _tempsPréparation.Min);

        public Préparation(ThreadSafeRandomNumberGenerator rng, ParamètresPréparation paramètres)
        {
            _tempsPréparation = (paramètres.TempsMin, paramètres.TempsMax);
            _rng = rng;
            _defectRate = paramètres.DefectRate;
            _lock = new EngorgementProduction(paramètres.NombrePlaces);
        }

        public int PlacesRestantes => _lock.PlacesRestantes;

        public GâteauCru Préparer(Plat plat)
        {
            _lock.Wait();

            try
            {
                AttenteIncompressible.Attendre(TempsPréparation);
                return new GâteauCru(plat, _rng.NextBoolean(1 - _defectRate));
            } 
            finally
            {
                _lock.Release();
            }
        }

        public async Task<GâteauCru> PréparerAsync(Plat plat)
        {
            await _lock.WaitAsync();

            try
            {
                await AttenteIncompressible.AttendreAsync(TempsPréparation);
                return new GâteauCru(plat, _rng.NextBoolean(1 - _defectRate));
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}