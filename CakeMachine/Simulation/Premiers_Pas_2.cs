using CakeMachine.Fabrication;
using CakeMachine.Fabrication.Elements;

namespace CakeMachine.Simulation
{
    internal class Premiers_Pas_2 : Algorithme
    {
        /// <inheritdoc />
        public override bool SupportsSync => true;

        /// <inheritdoc />
        public override IEnumerable<GâteauEmballé> Produire(Usine usine, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var arrayGateauCru = new GâteauCru[5];
                for (int i = 0; i < 5; i++)
                {
                    var plat = new Plat();
                    arrayGateauCru[i] = usine.Préparateurs.First().Préparer(plat);
                }
                
                var gâteauCuit = usine.Fours.First().Cuire(arrayGateauCru);

                for (int y = 0; y < 5; y++)
                {
                    var gâteauEmballé = usine.Emballeuses.First().Emballer(gâteauCuit[y]);
                    yield return gâteauEmballé;
                }
            }
        }
    }
}
