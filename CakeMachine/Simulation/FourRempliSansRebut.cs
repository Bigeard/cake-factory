using System.Runtime.CompilerServices;
using CakeMachine.Fabrication;
using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Opérations;
using CakeMachine.Utils;

namespace CakeMachine.Simulation
{
    internal class FourRempliSansRebut : Algorithme
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
                var gâteauxCrus = PréparerNConformes(postePréparation,
                    usine.OrganisationUsine.ParamètresCuisson.NombrePlaces);

                var gâteauxCuits = posteCuisson.Cuire(gâteauxCrus);
                var gâteauxCuitsConformes = gâteauxCuits.Where(gâteau => gâteau.EstConforme);
                
                var gâteauxEmballés = gâteauxCuitsConformes
                    .AsParallel()
                    .Select(posteEmballage.Emballer)
                    .ToArray();

                foreach (var gâteauEmballé in gâteauxEmballés)
                    yield return gâteauEmballé;
            }
        }

        private static GâteauCru[] PréparerNConformes(Préparation postePréparation, ushort gâteaux)
        {
            var gâteauxConformes = new List<GâteauCru>(gâteaux);

            do
            {
                var plats = Enumerable.Range(0, gâteaux - gâteauxConformes.Count)
                    .Select(_ => new Plat());

                var gâteauxCrus = plats.AsParallel().Select(postePréparation.Préparer);
                gâteauxConformes.AddRange(gâteauxCrus.Where(gâteau => gâteau.EstConforme));
            } while (gâteauxConformes.Count < gâteaux);

            return gâteauxConformes.ToArray();
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<GâteauEmballé> ProduireAsync(Usine usine,
            [EnumeratorCancellation] CancellationToken token)
        {
            var postePréparation = usine.Préparateurs.Single();
            var posteCuisson = usine.Fours.Single();
            var posteEmballage = usine.Emballeuses.Single();

            while (!token.IsCancellationRequested)
            {
                var gâteauxCrus = await PréparerNConformesAsync(postePréparation,
                    usine.OrganisationUsine.ParamètresCuisson.NombrePlaces);

                var gâteauxCuits = await posteCuisson.CuireAsync(gâteauxCrus);

                var gâteauxEmballés = gâteauxCuits
                    .Select(posteEmballage.EmballerAsync)
                    .EnumerateCompleted();

                await foreach(var gâteauEmballé in gâteauxEmballés.WithCancellation(token)) 
                    yield return gâteauEmballé;
            }
        }

        private static async Task<GâteauCru[]> PréparerNConformesAsync(
            Préparation postePréparation, ushort gâteaux)
        {
            var gâteauxConformes = new List<GâteauCru>(gâteaux);

            do
            {
                var plats = Enumerable.Range(0, gâteaux - gâteauxConformes.Count)
                    .Select(_ => new Plat());

                var gâteauxCrus = await Task.WhenAll(plats.Select(postePréparation.PréparerAsync));
                gâteauxConformes.AddRange(gâteauxCrus.Where(gâteau => gâteau.EstConforme));
            } while (gâteauxConformes.Count < gâteaux);

            return gâteauxConformes.ToArray();
        }
    }
}
