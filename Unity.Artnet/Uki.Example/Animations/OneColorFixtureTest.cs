
using System.Threading;
using Unity.BlinkyBlinky;

namespace Uki.Example.Animations
{
    public class OneColorFixtureTest : IBlinkyAnimation
    {
        private int Itterations = 0;

        public void Run()
        {

            BlinkyBlinky.fixtures[0].pixels.ForEach(f => f.R = 255);
            BlinkyBlinky.fixtures[1].pixels.ForEach(f => f.G = 255);
            BlinkyBlinky.UpdateLights();
            Thread.Sleep(500);

            BlinkyBlinky.AllBlack();
            BlinkyBlinky.UpdateLights();
            Thread.Sleep(500);
        }
    }
}
