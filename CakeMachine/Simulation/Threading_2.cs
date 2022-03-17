using System.Runtime.CompilerServices;
using CakeMachine.Fabrication;
using CakeMachine.Fabrication.Elements;

namespace CakeMachine.Simulation
{
    internal class Threading_2 : Algorithme
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
                var plats = Enumerable.Range(0, usine.OrganisationUsine.ParamètresCuisson.NombrePlaces)
                    .Select(_ => new Plat());

                var gâteauxCrus = plats.Select(postePréparation.Préparer).AsParallel();;
                var gâteauxCuits = posteCuisson.Cuire(gâteauxCrus.ToArray());
                var gâteauxEmballés = gâteauxCuits.Select(posteEmballage.Emballer).AsParallel().ToArray();

                foreach (var gâteauEmballé in gâteauxEmballés)
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
                var plats = Enumerable.Range(0, usine.OrganisationUsine.ParamètresCuisson.NombrePlaces)
                    .Select(_ => new Plat());
                
                var gâteauCruTask = plats.Select(postePréparation.PréparerAsync);

                var gâteauxCrus = await Task.WhenAll(gâteauCruTask);

                var gâteauxCuits = await posteCuisson.CuireAsync(gâteauxCrus);

                var gâteauxEmballés = gâteauxCuits.Select(posteEmballage.EmballerAsync).ToArray();

                await Task.WhenAny(gâteauxEmballés);

                foreach (var gâteauEmballé in gâteauxEmballés)
                    yield return await gâteauEmballé;
            }
        }
    }
}