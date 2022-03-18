namespace CakeMachine.Fabrication.Elements
{
    public class GâteauCuit : IConforme
    {
        public GâteauCuit(GâteauCru gâteauCru, bool estBienCuit)
        {
            EstConforme = gâteauCru.EstConforme && estBienCuit;
        }

        /// <inheritdoc />
        public bool EstConforme { get; }
    }
}
