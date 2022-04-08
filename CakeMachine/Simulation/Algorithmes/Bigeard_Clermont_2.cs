using System.Runtime.CompilerServices;
using CakeMachine.Fabrication.ContexteProduction;
using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Opérations;
using CakeMachine.Utils;

namespace CakeMachine.Simulation.Algorithmes
{
    internal class Bigeard_Clermont_2 : Algorithme
    {
        /// <inheritdoc />
        public override bool SupportsAsync => false;

        /// <inheritdoc />
        public override void ConfigurerUsine(IConfigurationUsine builder)
        {
            builder.NombrePréparateurs = 20;
            builder.NombreFours = 8;
            builder.NombreEmballeuses = 7;
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<GâteauEmballé> ProduireAsync(
            Usine usine,
            [EnumeratorCancellation] CancellationToken token
        )
        {
            // Optimiser les Rings préparateur entre 5 - 8 
            var _préparatrices = new Ring<Préparation>(usine.Préparateurs);
            var _fours = new Ring<Cuisson>(usine.Fours);
            var _emballeuses = new Ring<Emballage>(usine.Emballeuses);

            while (!token.IsCancellationRequested)
            {
                // PREPARATION 15
                // var gâteauParBoucle = usine.OrganisationUsine.NombrePréparateurs * usine.OrganisationUsine.ParamètresPréparation.NombrePlaces;
                var gâteauCruTask = new List<Task<GâteauCru>>(usine.OrganisationUsine.NombrePréparateurs);

                // var options = new ParallelOptions()
                // {
                //     MaxDegreeOfParallelism = usine.OrganisationUsine.NombrePréparateurs
                // };
                // Parallel.For(0, usine.OrganisationUsine.NombrePréparateurs, options, (i, state) =>
                // {
                //     Console.WriteLine(@"value of i = {0}, thread = {1}", i, Thread.CurrentThread.ManagedThreadId);
                //     var gâteauCru = usine.StockInfiniPlats.First();
                //     // if (gâteauCru.EstConforme)
                //     // {
                //     gâteauCruTask.Add(_préparatrices.Next.PréparerAsync(gâteauCru));
                //     // }
                //     // else
                //     // {
                //     //     i--;
                //     //     usine.MettreAuRebut(gâteauCru);
                //     // }
                // });
                var listGâteauxCrus = new List<GâteauCru>();
                for (int e = 0; e < 50; e++)
                {
                    for (
                        int i = 0;
                        i < usine.OrganisationUsine.NombrePréparateurs;
                        i++
                    )
                    {
                        // token.ThrowIfCancellationRequested();

                        var gâteauCru = usine.StockInfiniPlats.First();
                        if (gâteauCru.EstConforme)
                        {
                            gâteauCruTask.Add(_préparatrices.Next.PréparerAsync(gâteauCru));
                        }
                        else
                        {
                            usine.MettreAuRebut(gâteauCru);
                            i--;
                        }
                    }
                    listGâteauxCrus.AddRange(await Task.WhenAll(gâteauCruTask));
                }

                // var gâteauxCrus = await Task.WhenAll(gâteauCruTask);
                // Console.WriteLine(@"gâteauxCrus.Length: {0}", gâteauxCrus.Length);
                var gâteauxCrus = listGâteauxCrus.ToArray();

                // CUISSON 
                var gâteauCuitTask = new List<Task<GâteauCuit[]>>(gâteauxCrus.Length);

                Parallel.For(0, gâteauxCrus.Length / 5, (i, state) =>
                {
                    gâteauCuitTask.Add(_fours.Next.CuireAsync(
                        gâteauxCrus[i * 5],
                        gâteauxCrus[i * 5 + 1],
                        gâteauxCrus[i * 5 + 2],
                        gâteauxCrus[i * 5 + 3],
                        gâteauxCrus[i * 5 + 4]
                    ));
                });

                // for (
                //     int i = 0;
                //     i < gâteauxCrus.Length;
                //     i += 5
                // )
                // {
                //     // token.ThrowIfCancellationRequested();

                //     var gâteauCuit = _fours.Next.CuireAsync(
                //         gâteauxCrus[i],
                //         gâteauxCrus[i + 1],
                //         gâteauxCrus[i + 2],
                //         gâteauxCrus[i + 3],
                //         gâteauxCrus[i + 4]
                //     );
                //     gâteauCuitTask.Add(gâteauCuit);
                // }
                var gâteauxCuits = await Task.WhenAll(gâteauCuitTask);

                // EMBALLAGE 
                var tâchesEmballage = new List<Task<GâteauEmballé>>(gâteauxCuits.Length);

                for (
                    int i = 0;
                    i < gâteauxCuits.Length;
                    i++
                )
                {
                    // token.ThrowIfCancellationRequested();
                    for (int j = 0; j < gâteauxCuits[i].Length; j++)
                    {
                        if (gâteauxCuits[i][j].EstConforme)
                        {
                            tâchesEmballage.Add(_emballeuses.Next.EmballerAsync(gâteauxCuits[i][j]));
                        }
                        else
                        {
                            usine.MettreAuRebut(gâteauxCuits[i][j]);
                        }
                    }
                    // tâchesEmballage.Add(_emballeuses.Next.EmballerAsync(gâteauxCuits[i][1]));
                    // tâchesEmballage.Add(_emballeuses.Next.EmballerAsync(gâteauxCuits[i][2]));
                    // tâchesEmballage.Add(_emballeuses.Next.EmballerAsync(gâteauxCuits[i][3]));
                    // tâchesEmballage.Add(_emballeuses.Next.EmballerAsync(gâteauxCuits[i][4]));
                }
                await foreach (var gâteauEmballé in tâchesEmballage.EnumerateCompleted().WithCancellation(token))
                    yield return gâteauEmballé;
            }
        }
    }
}