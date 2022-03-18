using CakeMachine.Fabrication;
using CakeMachine.Fabrication.Elements;

namespace CakeMachine.Simulation
{
    internal class Premiers_Pas_1 : Algorithme
    {
        /// <inheritdoc />
        public override bool SupportsSync => false;

        /// <inheritdoc />
        public override IEnumerable<GâteauEmballé> Produire(Usine usine, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var plat1 = new Plat();
                var plat2 = new Plat();

                var gâteauCru1 = usine.Préparateurs.First().Préparer(plat1);
                var gâteauCru2 = usine.Préparateurs.First().Préparer(plat2);

                var gâteauCuit = usine.Fours.First().Cuire(gâteauCru1, gâteauCru2);

                var gâteauEmballé1 = usine.Emballeuses.First().Emballer(gâteauCuit.First());
                var gâteauEmballé2 = usine.Emballeuses.First().Emballer(gâteauCuit.Last());

                yield return gâteauEmballé1;
                yield return gâteauEmballé2;
            }
        }
    }
}
