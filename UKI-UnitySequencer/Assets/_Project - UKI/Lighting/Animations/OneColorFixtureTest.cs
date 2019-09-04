
using System.Linq;
using System.Threading;
using Unity.BlinkyBlinky;

namespace Uki.Example.Animations
{
    public class OneColorFixtureTest : IBlinkyAnimation
    {
        private int Itterations = 0;

        public void Run()
        {

            BlinkyBlinky.Model.Fixtures.First(x => x.Name == "Fixture - Left Wing").LedChains[0].Pixels.ForEach(f => f.R = f.X/8);
            BlinkyBlinky.Model.Fixtures.First(x => x.Name == "LeftWing").LedChains[1].Pixels.ForEach(f => f.B = f.Y/8);

            BlinkyBlinky.Model.Fixtures.First(x => x.Name == "RightWing").LedChains[0].Pixels.ForEach(f => f.G = 64);
            BlinkyBlinky.Model.Fixtures.First(x => x.Name == "RightWing").LedChains[1].Pixels.ForEach(f => f.color = new UnityEngine.Color(64,0,64));

            BlinkyBlinky.fixtures.First(x => x.Name == "Eyes").pixels.ForEach(pixel => pixel.color = new UnityEngine.Color(45, 45, 0));

            BlinkyBlinky.fixtures.First(x => x.Name == "Floods").pixels.ForEach(pixel => pixel.color = new UnityEngine.Color(0, 45, 45));

            BlinkyBlinky.fixtures.First(x => x.Name == "Legs").pixels.ForEach(pixel => pixel.color = new UnityEngine.Color(45, 45, 45));

            //BlinkyBlinky.fixtures[1].pixels.ForEach(f => f.G = 255);
            BlinkyBlinky.UpdateLights();
            Thread.Sleep(500);

            BlinkyBlinky.AllBlack();
            BlinkyBlinky.UpdateLights();
            Thread.Sleep(100);
        }
    }
}
