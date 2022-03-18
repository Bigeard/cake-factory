using System.Runtime.CompilerServices;
using CakeMachine.Fabrication;
using CakeMachine.Fabrication.Elements;

namespace CakeMachine.Simulation
{
    internal class AntiRebut : Algorithme
    {
        /// <inheritdoc />
        public override bool SupportsSync => false;

        /// <inheritdoc />
        public override bool SupportsAsync => false;

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

                var gâteauCuit = (await posteCuisson.CuireAsync(gâteauCru)).Single();
                if (!gâteauCuit.EstConforme) continue;

                var gâteauEmballé = await posteEmballage.EmballerAsync(gâteauCuit);
                if (!gâteauEmballé.EstConforme) continue;

                yield return gâteauEmballé;
            }
        }
    }
}
