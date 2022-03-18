using System.Runtime.CompilerServices;
using CakeMachine.Fabrication;
using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Opérations;
using CakeMachine.Utils;

namespace CakeMachine.Simulation
{
    internal class Threading_3 : Algorithme
    {
        /// <inheritdoc />
        public override bool SupportsSync => false;
        /// <inheritdoc />
        public override bool SupportsAsync => false;
        
        /// <inheritdoc />
        public override IEnumerable<GâteauEmballé> Produire(Usine usine, CancellationToken token)
        {
            var capacitéFour = usine.OrganisationUsine.ParamètresCuisson.NombrePlaces;

            var postePréparation = usine.Préparateurs.Single();
            var posteEmballage = usine.Emballeuses.Single();
            var posteCuisson = usine.Fours.Single();

            while (!token.IsCancellationRequested)
            {
                var plats = Enumerable.Range(0, 10).Select(_ => new Plat()); 

                var gâteauxCrus = plats.Select(postePréparation.Préparer).AsParallel().ToArray();

                var gâteauxCuits = CuireParLots(gâteauxCrus, posteCuisson, capacitéFour);
                var gâteauxEmballés = gâteauxCuits.Select(posteEmballage.Emballer).AsParallel().ToArray();

                foreach (var gâteauEmballé in gâteauxEmballés)
                    yield return gâteauEmballé;
            }
        }

        private static IEnumerable<GâteauCuit> CuireParLots(IEnumerable<GâteauCru> gâteaux, Cuisson four, uint capacitéFour)
        {
            var queue = new Queue<GâteauCru>(gâteaux);

            while (queue.Any())
            {
                var gâteauxCuits = four.Cuire(queue.Dequeue());
                foreach (var gâteauCuit in gâteauxCuits)
                    yield return gâteauCuit;
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
                var plats = Enumerable.Range(0, 10).Select(_ => new Plat());
                
                var gâteauCruTask = plats.Select(postePréparation.PréparerAsync);

                var gâteauxCrus = await Task.WhenAll(gâteauCruTask);
                
                var tempArrayGâteauCru = gâteauxCrus.Chunk(5).ToArray();
                
                var gâteauCuits1 = await posteCuisson.CuireAsync(tempArrayGâteauCru.First());
                var gâteauCuits2 = await posteCuisson.CuireAsync(tempArrayGâteauCru.Last());

                var gâteauxEmballés1 = gâteauCuits1.Select(posteEmballage.EmballerAsync).ToArray();
                var gâteauxEmballés2 = gâteauCuits2.Select(posteEmballage.EmballerAsync).ToArray();

                var gâteauxEmballés = gâteauxEmballés1.Concat(gâteauxEmballés2);
                await Task.WhenAny(gâteauxEmballés);
                
                foreach (var gâteauEmballé in gâteauxEmballés)
                    yield return await gâteauEmballé;
            }
        }
    }
}