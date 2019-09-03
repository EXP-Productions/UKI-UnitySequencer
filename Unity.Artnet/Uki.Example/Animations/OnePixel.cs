
using System.Linq;
using System.Threading;
using Unity.BlinkyBlinky;

namespace Uki.Example.Animations
{
    public class OnePixel : IBlinkyAnimation
    {
        private int Itterations = 0;

        public void Run()
        {

            BlinkyBlinky.Model.Fixtures.First(x => x.Name == "LeftWing").LedChains[0].Pixels[128].R = 255;


            //BlinkyBlinky.fixtures[1].pixels.ForEach(f => f.G = 255);
            BlinkyBlinky.UpdateLights();
            Thread.Sleep(500);

            BlinkyBlinky.AllBlack();
            BlinkyBlinky.UpdateLights();
            Thread.Sleep(100);
        }
    }
}
