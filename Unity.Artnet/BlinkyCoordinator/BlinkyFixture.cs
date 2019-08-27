using System.Collections.Generic;

namespace Unity.BlinkyLights
{
    public class BlinkyFixture
    {
        public string Name;
        public List<Pixel> pixels = new List<Pixel>();

        public BlinkyFixture(Fixture fixture)
        {
            Name = fixture.Name;
            fixture.LedChains.ForEach(chain => pixels.AddRange(chain.Pixels));
        }
    }
}
