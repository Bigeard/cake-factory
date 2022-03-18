using System.Runtime.CompilerServices;
using CakeMachine.Fabrication;
using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Opérations;
using CakeMachine.Utils;

namespace CakeMachine.Simulation
{
    public class Parallel_1 : Algorithme
    {
        /// <inheritdoc />
        public override bool SupportsAsync => true;

        /// <inheritdoc />
        public override async IAsyncEnumerable<GâteauEmballé> ProduireAsync(Usine usine,
            [EnumeratorCancellation] CancellationToken token)
        {
            var postePréparation = usine.Préparateurs;
            var posteCuisson = usine.Fours.Single();
            var posteEmballage = usine.Emballeuses.Single();

            while (!token.IsCancellationRequested)
            {
                var gâteauxCuits = ProduireEtCuireParBains(
                    postePréparation,
                    posteCuisson,
                    usine.OrganisationUsine.ParamètresCuisson.NombrePlaces, 
                    6,
                    token);

                var tâchesEmballage = new List<Task<GâteauEmballé>>(usine.OrganisationUsine.ParamètresCuisson.NombrePlaces * 6);
                await foreach(var gâteauCuit in gâteauxCuits.WithCancellation(token))
                    tâchesEmballage.Add(posteEmballage.EmballerAsync(gâteauCuit));
                
                await foreach (var gâteauEmballé in tâchesEmballage.EnumerateCompleted().WithCancellation(token))
                    yield return gâteauEmballé;
            }
        }

        private static async IAsyncEnumerable<GâteauCuit> ProduireEtCuireParBains(
            IEnumerable<Préparation> postePréparation,
            Cuisson posteCuisson,
            ushort nombrePlacesParFour,
            ushort nombreBains,
            [EnumeratorCancellation] CancellationToken token)
        {
            var gâteauxCrus = PréparerConformesParBainAsync(postePréparation, nombrePlacesParFour, nombreBains, token);

            await foreach (var bainGâteauxCrus in gâteauxCrus.WithCancellation(token))
            {
                var gâteauxCuits = await posteCuisson.CuireAsync(bainGâteauxCrus);
                foreach (var gâteauCuit in gâteauxCuits.Where(gâteau => gâteau.EstConforme))
                    yield return gâteauCuit;
            }
        }
        
        private static async IAsyncEnumerable<GâteauCru[]> PréparerConformesParBainAsync(
            IEnumerable<Préparation> postePréparation, ushort gâteauxParBain, ushort bains, [EnumeratorCancellation] CancellationToken token)
        {
            var productionGâteauxConformes =
                PréparerConformes(postePréparation, (ushort)(bains * gâteauxParBain), token);

            var buffer = new List<GâteauCru>(gâteauxParBain);
            await foreach(var gâteauCru in productionGâteauxConformes.WithCancellation(token))
            {
                buffer.Add(gâteauCru);

                if (buffer.Count != gâteauxParBain) continue;

                yield return buffer.ToArray();
                buffer.Clear();
            }
        }

        private static async IAsyncEnumerable<GâteauCru> PréparerConformes(
            IEnumerable<Préparation> postePréparation, ushort total, [EnumeratorCancellation] CancellationToken token)
        {
            var gâteauxConformes = 0;

            do
            {
                var plats = Enumerable.Range(0, total - gâteauxConformes)
                    .Select(_ => new Plat());

                var gâteauxCrus = plats.Select(postePréparation.First().PréparerAsync).EnumerateCompleted();
                
                await foreach(var gâteauCru in gâteauxCrus.WithCancellation(token))
                {
                    if (!gâteauCru.EstConforme) continue;
                    gâteauxConformes++;
                    yield return gâteauCru;
                }

            } while (gâteauxConformes < total);
        }
    }
}