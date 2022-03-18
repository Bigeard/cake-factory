namespace CakeMachine.Fabrication.Elements
{
    public class GâteauEmballé : IConforme
    {
        public GâteauEmballé(GâteauCuit gâteau, bool estCorrectementEmballé)
        {
            EstConforme = gâteau.EstConforme && estCorrectementEmballé;
        }

        /// <inheritdoc />
        public bool EstConforme { get; }
    }
}
