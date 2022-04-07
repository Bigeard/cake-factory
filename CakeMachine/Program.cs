using System.Runtime.CompilerServices;
using CakeMachine.Simulation;

[assembly:InternalsVisibleTo("CakeMachine.Test")]

const int nombreGâteaux = 1500;

var runner = new MultipleAlgorithmsRunner();
await runner.ProduireNGâteaux(nombreGâteaux);