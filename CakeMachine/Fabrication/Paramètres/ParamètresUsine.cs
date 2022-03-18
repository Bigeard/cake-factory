namespace CakeMachine.Fabrication.Paramètres
{
    public record ParamètresUsine(
        ushort NombrePréparateurs, 
        ushort NombreFours, 
        ushort NombreEmballeuses,
        ParamètresPréparation ParamètresPréparation,
        ParamètresCuisson ParamètresCuisson,
        ParamètresEmballage ParamètresEmballage);
}
