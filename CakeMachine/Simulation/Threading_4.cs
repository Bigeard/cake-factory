using System.Runtime.CompilerServices;
using CakeMachine.Fabrication;
using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Opérations;
using CakeMachine.Utils;

namespace CakeMachine.Simulation
{
    internal class Threading_4 : Algorithme
    {
        /// <inheritdoc />
        public override bool SupportsSync => true;
        /// <inheritdoc />
        public override bool SupportsAsync => true;
        
        /// <inheritdoc />
        public override IEnumerable<GâteauEmballé> Produire(Usine usine, CancellationToken token)
        {
            var postePréparation = usine.Préparateurs.Single();
            var posteCuisson = usine.Fours.Single();
            var posteEmballage = usine.Emballeuses.Single();

            while (!token.IsCancellationRequested)
            {
                var plat = new Plat();

                GâteauCru gâteauCru;
                do gâteauCru = postePréparation.Préparer(plat);
                while (!gâteauCru.EstConforme);
                
                var gâteauCuit = posteCuisson.Cuire(gâteauCru).Single();
                if(!gâteauCuit.EstConforme) continue;

                var gâteauEmballé = posteEmballage.Emballer(gâteauCuit);
                if (!gâteauEmballé.EstConforme) continue;

                yield return gâteauEmballé;
            }
        }
        /// <inheritdoc />
        public override async IAsyncEnumerable<GâteauEmballé> ProduireAsync(Usine usine, [EnumeratorCancellation] CancellationToken token)
        {
            var postePréparation = usine.Préparateurs.Single();
            var posteCuisson = usine.Fours.Single();
            var posteEmballage = usine.Emballeuses.Single();

            while (!token.IsCancellationRequested)
            {
                var plat = new Plat();

                GâteauCru gâteauCru;
                do gâteauCru = await postePréparation.PréparerAsync(plat);
                while (!gâteauCru.EstConforme);
                
                var arrayGâteauCuit = await posteCuisson.CuireAsync(gâteauCru);

                var gâteauCuit = arrayGâteauCuit.First();
                if(!gâteauCuit.EstConforme) continue;

                var gâteauEmballé = await posteEmballage.EmballerAsync(gâteauCuit);
                if (!gâteauEmballé.EstConforme) continue;

                yield return gâteauEmballé;
            }
        }
    }
}