using System.Collections.Generic;
using System.Linq;

namespace Unity.BlinkyLights
{
    public class BlinkyModel
    {
        public List<Fixture> Fixtures;

        public BlinkyModel()
        {
            Fixtures = new List<Fixture>();
        }

        public Fixture AddFixture(Fixture fixture)
        {
            Fixtures.Add(fixture);
            return Fixtures.Last();
        }

        public void AllBlack()
        {
            Fixtures.ForEach(fixture => fixture.LedChains.ForEach(chain => chain.Pixels.ForEach(pixel => pixel.Black())));
        }

        public int longestChainCount => Fixtures.Max(fixture => fixture.LedChains.Max(p=>p.Pixels.Count));
    }
}
