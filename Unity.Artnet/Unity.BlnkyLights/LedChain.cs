using System.Collections.Generic;

namespace Unity.BlinkyLights
{
    public class LedChain
    {
        public string Name { get; private set; }
        public List<Pixel> Pixels { get; set; }
        public int DMXStartingUniverse;

        public string GetInfo => "This class represents a string of " +
            "LEDs that are connected to one driver output of a controller.";

        public LedChain(string name, int dmxStartingUniverse)
        {
            Name = name;
            DMXStartingUniverse = dmxStartingUniverse;
            Pixels = new List<Pixel>();
        }

        public void AddPixel(Pixel p) => Pixels.Add(p);

        public int DmxUniversesRequired => CalcuateRequiredUniverses();

        private int CalcuateRequiredUniverses()
        {
            float divisions = Pixels.Count / 170;
            return divisions % 1 > 0 ? (int)divisions + 1 : (int)divisions;
        }
    }
}
