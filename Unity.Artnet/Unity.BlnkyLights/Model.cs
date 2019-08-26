using System.Collections.Generic;

namespace Unity.BlinkyLights
{
    public class BlinkyModel
    {
        public List<Fixture> Fixtures;

        public BlinkyModel()
        {
            Fixtures = new List<Fixture>();
        }

        public void AddFixture(Fixture fixture)
        {
            Fixtures.Add(fixture);
        }

        public void SendAll() 
        {
           
        }
    }
}
