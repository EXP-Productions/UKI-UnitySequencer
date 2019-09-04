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
            MovePixelsToFixtureOrigin();
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

        /// <summary>
        /// Scaling will stretch the pixels by the factor passed in, in a positive direction away from the fixtures origin.
        /// </summary>
        /// <param name="scaleFactor"></param>
        public void ScaleFixture(float scaleFactor)
        {
            foreach (var ledChain in LedChains)
            {
                foreach (var pixel in ledChain.Pixels)
                {
                    pixel.location.Scale(new Vector3(scaleFactor, scaleFactor, scaleFactor));
                }
            }
        }

        /// <summary>
        /// Pass in a vector that will be used to rotate all Pixels around the fixtures origin point.
        /// </summary>
        /// <param name="rotateBy"></param>
        public void RotateFixture(Vector3 rotateBy)
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
