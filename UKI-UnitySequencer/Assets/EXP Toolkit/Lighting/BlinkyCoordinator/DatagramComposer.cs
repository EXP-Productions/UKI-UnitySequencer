using System.Collections.Generic;
using Unity.BlinkyLights;
using Unity.BlinkyShared.DMX;
using UnityEngine;

namespace Unity.BlinkyBlinky
{
    public static class DatagramComposer
    {
        public static List<DMXDatagram> GetDMXDatagrams(Fixture fixture)
        {
            var datagrams = new List<DMXDatagram>();

            foreach (var chain in fixture.LedChains)
            {
                short universesProcessed = 0;
                int remainingPixelsToProcess = chain.Pixels.Count % 170;

                //PROCESS FULL UNIVERSES (more than 170 pixels) 
                while (universesProcessed < chain.DmxUniversesRequired)
                {
                    var buffer = new byte[510];
                    var pixelIndex = 0;

                    while (pixelIndex < 170)
                    {
                        var universePixelIndex = pixelIndex + (universesProcessed * 170);
                        var idx = pixelIndex * 3;

                        buffer[idx] = (byte)chain.Pixels[universePixelIndex].R;
                        buffer[idx + 1] = (byte)chain.Pixels[universePixelIndex].G;
                        buffer[idx + 2] = (byte)chain.Pixels[universePixelIndex].B;

                        pixelIndex++;
                    }

                    datagrams.Add(new DMXDatagram(buffer, (short)(universesProcessed + chain.DMXStartingUniverse), fixture.DmxDevice.NetworkName));

                    universesProcessed++;
                }

                //PROCESSS REMAINING DATA IN THE LAST UNIVERSE 
                var remainingBuffer = new byte[510];
                var pixelIndexLast = 0;

                while (pixelIndexLast < remainingPixelsToProcess)
                {
                    var pixelIndexFinal = pixelIndexLast + (  universesProcessed * 170);
                    var idx = pixelIndexLast * 3;

                    remainingBuffer[idx] = (byte)chain.Pixels[pixelIndexFinal].R;
                    remainingBuffer[idx + 1] = (byte)chain.Pixels[pixelIndexFinal].G;
                    remainingBuffer[idx + 2] = (byte)chain.Pixels[pixelIndexFinal].B;

                    pixelIndexLast++;
                }

                datagrams.Add(new DMXDatagram(remainingBuffer, (short)(universesProcessed + chain.DMXStartingUniverse), fixture.DmxDevice.NetworkName));

            }

            return datagrams;

        }
    }
}

