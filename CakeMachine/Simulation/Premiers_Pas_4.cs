using CakeMachine.Fabrication;
using CakeMachine.Fabrication.Elements;

namespace CakeMachine.Simulation
{
    internal class Premiers_Pas_4 : Algorithme
    {
        /// <inheritdoc />
        public override bool SupportsSync => true;

        /// <inheritdoc />
        public override IEnumerable<GâteauEmballé> Produire(Usine usine, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var plat = new Plat();

                var gâteauCru = usine.Préparateurs.First().Préparer(plat);
                if (!gâteauCru.EstConforme)
                {
                    continue;
                }
                var gâteauCuit = usine.Fours.First().Cuire(gâteauCru).Single();
                if (!gâteauCuit.EstConforme)
                {
                    continue;
                }
                var gâteauEmballé = usine.Emballeuses.First().Emballer(gâteauCuit);
                if (!gâteauEmballé.EstConforme)
                {
                    continue;
                }
                yield return gâteauEmballé;
            }
        }
    }
}
