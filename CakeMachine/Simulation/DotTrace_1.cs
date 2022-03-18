using System.Runtime.CompilerServices;
using CakeMachine.Fabrication;
using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Opérations;
using CakeMachine.Utils;


namespace CakeMachine.Simulation
{
    public class DotTrace_1 : Algorithme
    {
        /// <inheritdoc />
        public override bool SupportsAsync => true;

        /// <inheritdoc />
        public override async IAsyncEnumerable<GâteauEmballé> ProduireAsync(Usine usine, [EnumeratorCancellation] CancellationToken token)
        {
            var capacitéFour = usine.OrganisationUsine.ParamètresCuisson.NombrePlaces;

            var postePréparation = usine.Préparateurs.Single();
            var posteEmballage = usine.Emballeuses.Single();
            var posteCuisson = usine.Fours.Single();

            while (!token.IsCancellationRequested)
            {
                var plats = Enumerable.Range(0, 10).Select(_ => new Plat());

                var gâteauxCrus = plats
                    .Select(postePréparation.PréparerAsync)
                    .EnumerateCompleted();

                var gâteauxCuits = CuireParLotsAsync(gâteauxCrus, posteCuisson, capacitéFour);

                var tâchesEmballage = new List<Task<GâteauEmballé>>();
                await foreach(var gâteauCuit in gâteauxCuits.WithCancellation(token))
                    tâchesEmballage.Add(posteEmballage.EmballerAsync(gâteauCuit));

                await foreach (var gâteauEmballé in tâchesEmballage.EnumerateCompleted().WithCancellation(token))
                    yield return gâteauEmballé;
            }
        }

        private static async IAsyncEnumerable<GâteauCuit> CuireParLotsAsync(
            IAsyncEnumerable<GâteauCru> gâteaux, 
            Cuisson four,
            uint capacitéFour)
        {
            var buffer = new List<GâteauCru>((int) capacitéFour);
            await foreach(var gâteauCru in gâteaux)
            {
                buffer.Add(gâteauCru);

                if (buffer.Count != capacitéFour) continue;

                var gâteauxCuits = await four.CuireAsync(buffer.ToArray());
                foreach (var gâteauCuit in gâteauxCuits)
                    yield return gâteauCuit;

                buffer.Clear();
            }
        }
        
        private static async IAsyncEnumerable<GâteauCuit> TestCuireParLotsAsync(
            IAsyncEnumerable<GâteauCru> gâteaux, 
            Cuisson four,
            uint capacitéFour)
        {
            var listGâteaux = await Task.WhenAll(gâteaux.ToEnumerableAsync());
            var iteration = listGâteaux.First().Count() / capacitéFour;
            for (int i = 0; i < iteration; i++)
            {
                var y = i * 5;
                var gâteauxCuits = await four.CuireAsync(
                    listGâteaux[y].First(), 
                    listGâteaux[1 + y].First(), 
                    listGâteaux[2 + y].First(), 
                    listGâteaux[3 + y].First(), 
                    listGâteaux[4 + y].First()
                );
                foreach (var gâteauCuit in gâteauxCuits)
                    yield return gâteauCuit;
            }
        }
    }
}