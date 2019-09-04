using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.BlinkyShared.DMX;
using UnityEngine;

namespace Unity.BlinkyLights
{
    /// <summary>
    /// A fixture contians LED chains which have LEDS with 3D positions.
    /// Each fixture has an origin position to position the leds. TODO: replace with a transform so the leds can be transformed in 3D space
    /// </summary>

    public class Fixture 
    {
        public string Name;
        public List<LedChain> LedChains;
        public Vector3 Origin { get; private set; }
        public readonly DMXDeviceDetail DmxDevice;

        public Fixture(string name, DMXDeviceDetail device)
        {
            Name = name;
            DmxDevice = device;
            LedChains = new List<LedChain>();
        }

        public string NetworkName => DmxDevice.NetworkName;

        public void SetFixtureOrigin(Vector3 origin)
        {
            Origin = origin;
        }

        public void AddLedChain(LedChain chain)
        {
            LedChains.Add(chain);
        }

        /// <summary>
        /// Attempt to load all the pixels based on an x,y index file. 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="universe"></param>
        public void TryLoadLedChainFromFile(string filePath, short universe)
        {         
            try
            {
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
               Debug.Log("Loading an led mapping file fialed. " + e.ToString());
            }
          
        }

        /// <summary>If using consecutive strings for a fixture, this helper will tell you what the next universe for the next string is.
        /// </summary>
        public short GetNextUniverse()
        {
            if (!LedChains.Any()) return 0;
            return (short)(LedChains.Last().DMXStartingUniverse + LedChains.Last().DmxUniversesRequired);
        }

        public void UpdateTransform(Transform parent)
        {
            Origin = parent.transform.position;
            foreach (var ledChain in LedChains)
            {
                foreach (var pixel in ledChain.Pixels)
                {
                    pixel.UpdateLocation(parent);// = parent.transform.position;// + parent.TransformPoint(pixel.originalLocation);
                }
            }
        }
    }
}
