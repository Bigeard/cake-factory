using CakeMachine.Fabrication;
using CakeMachine.Fabrication.Elements;

namespace CakeMachine.Simulation
{
    internal class Premiers_Pas_3 : Algorithme
    {
        /// <inheritdoc />
        public override bool SupportsSync => false;

        /// <inheritdoc />
        public override IEnumerable<GâteauEmballé> Produire(Usine usine, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var arrayGâteauCru = new List<GâteauCru>();

                for (int i = 0; i < 10; i++)
                {
                    var plat = new Plat();
                    arrayGâteauCru.Add(usine.Préparateurs.First().Préparer(plat));
                }

                var tempArrayGâteauCru = arrayGâteauCru.Chunk(5);
                
                var gâteauCuit1 = usine.Fours.First().Cuire(tempArrayGâteauCru.First().ToArray());
                var gâteauCuit2 = usine.Fours.First().Cuire(tempArrayGâteauCru.Last().ToArray());
                
                for (int y = 0; y < 5; y++)
                {
                    var gâteauEmballé1 = usine.Emballeuses.First().Emballer(gâteauCuit1[y]);
                    yield return gâteauEmballé1;
                }
                
                for (int z = 0; z < 5; z++)
                {
                    var gâteauEmballé2 = usine.Emballeuses.First().Emballer(gâteauCuit2[z]);
                    yield return gâteauEmballé2;
                }
            }
        }
    }
}
