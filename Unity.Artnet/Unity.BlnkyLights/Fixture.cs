using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.BlinkyNetwork.DMX;
using UnityEngine;

namespace Unity.BlinkyLights
{
    public class Fixture 
    {
        public string Name;
        public List<LedChain> LedChains;
        public Vector3 Origin { get; private set; }
        public readonly DMXDeviceDetail DMXDevice;

        public string GetInfo = "If setting more than one chain, set all the chains xyz pixels relative to each other.";

        public Fixture(string name, DMXDeviceDetail device)
        {
            Name = name;
            DMXDevice = device;
            LedChains = new List<LedChain>();
        }

        public void SetFixtureLocation(Vector3 origin)
        {
            Origin = origin;
            MovePixelsToFixtureOrigin();
        }

        public void AddLedChain(LedChain chain)
        {
            LedChains.Add(chain);
        }

        public void TryLoadLedChainFromFile(string filePath, int universe)
        {
         
            try {

                //Name the chain based on the source index file
                var chain = new LedChain(Path.GetFileNameWithoutExtension(filePath), universe);
               

                //read Data Out of the file
                var csv = from line in File.ReadAllLines(filePath)
                          select line.Split(',');

                foreach (var led in csv)
                {
                    var x = float.Parse(led[0]);
                    var y = float.Parse(led[1]);

                    chain.AddPixel(new Pixel(x,y));
                }

                AddLedChain(chain);

            }
            catch (Exception e)
            {
                throw new Exception("Loading an led mapping file fialed. " + e.ToString());
            }
          
        }
        
        public int GetNextUniverse()
        {
            if (!LedChains.Any()) return 0;
            return LedChains.Last().DMXStartingUniverse + LedChains.Last().DmxUniversesRequired;
        }

        private void MovePixelsToFixtureOrigin()
        {
            foreach (var ledChain in LedChains)
            {
                foreach (var pixel in ledChain.Pixels)
                {
                    pixel.Relocate(pixel.origin + Origin);
                }
            }
        }

        public void RotateFixture( Vector3 rotateBy)
        {
            foreach (var ledChain in LedChains)
            {
                foreach (var pixel in ledChain.Pixels)
                {
                    //rotate around the origin Location by an amount specified in Rotation.
                    pixel.Relocate(RotatePointAroundPivot(pixel.location, Origin, rotateBy));
                }
            }
        }

        private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 rotationAngle)
        {
            return Quaternion.Euler(rotationAngle) * (point - pivot) + pivot;
        }
    }
}
