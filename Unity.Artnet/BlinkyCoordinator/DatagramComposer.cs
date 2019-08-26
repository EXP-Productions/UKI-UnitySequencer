using System.Collections.Generic;
using Unity.BlinkyLights;
using Unity.BlinkyShared.DMX;

namespace Unity.BlinkyLightsCoordinator
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
                        var idx = pixelIndex * 3;

                        buffer[idx] = (byte)chain.Pixels[pixelIndex].r;
                        buffer[idx + 1] = (byte)chain.Pixels[pixelIndex].g;
                        buffer[idx + 2] = (byte)chain.Pixels[pixelIndex].b;

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
                    var idx = pixelIndexLast * 3;

                    remainingBuffer[idx] = (byte)chain.Pixels[pixelIndexLast].r;
                    remainingBuffer[idx + 1] = (byte)chain.Pixels[pixelIndexLast].g;
                    remainingBuffer[idx + 2] = (byte)chain.Pixels[pixelIndexLast].b;

                    pixelIndexLast++;
                }

                datagrams.Add(new DMXDatagram(remainingBuffer, (short)(universesProcessed + chain.DMXStartingUniverse), fixture.DmxDevice.NetworkName));

            }

            return datagrams;
        }

    }
}

