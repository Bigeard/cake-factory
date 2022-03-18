namespace CakeMachine.Fabrication.Elements
{
    public class GâteauCru : IConforme
    {
        public GâteauCru(Plat plat, bool estCorrectementPréparé)
        {
            EstConforme = plat.EstConforme && estCorrectementPréparé;
        }

        /// <inheritdoc />
        public bool EstConforme { get; }
    }
}
