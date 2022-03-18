using CakeMachine.Fabrication.Opérations;
using CakeMachine.Fabrication.Paramètres;
using CakeMachine.Utils;

namespace CakeMachine.Fabrication
{
    public class Usine
    {
        public ParamètresUsine OrganisationUsine { get; }
        public const ushort TailleMaxUsine = 35;

        public IEnumerable<Préparation> Préparateurs { get; }
        public IEnumerable<Cuisson> Fours { get; }
        public IEnumerable<Emballage> Emballeuses { get; }

        public Usine(ThreadSafeRandomNumberGenerator rng, ParamètresUsine organisationUsine)
        {
            if (organisationUsine.NombrePréparateurs + organisationUsine.NombreEmballeuses + organisationUsine.NombreFours > TailleMaxUsine)
                throw new InvalidOperationException(
                    $"L'usine est pleine, elle peut compter au maximum {TailleMaxUsine} machines");
            OrganisationUsine = organisationUsine;

            Préparateurs = Enumerable
                .Range(0, organisationUsine.NombrePréparateurs)
                .Select(_ => new Préparation(rng.Fork(), organisationUsine.ParamètresPréparation));

            Fours = Enumerable
                .Range(0, organisationUsine.NombreFours)
                .Select(_ => new Cuisson(rng.Fork(), organisationUsine.ParamètresCuisson));

            Emballeuses = Enumerable
                .Range(0, organisationUsine.NombreEmballeuses)
                .Select(_ => new Emballage(rng.Fork(), organisationUsine.ParamètresEmballage));
        }
    }
}
