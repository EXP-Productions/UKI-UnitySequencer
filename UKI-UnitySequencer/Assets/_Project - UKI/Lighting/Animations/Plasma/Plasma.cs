using System.Diagnostics;
using Uki.Example.Animations;
using UnityEngine;

namespace Unity.BlinkyBlinky.Animations
{
    public class PlasmaAnimation : IBlinkyAnimation
    {
        private Stopwatch stopWatch;

        IPlasma plasmaGenerator;
        
        private int currentItternation;
        private double movement = 0;
        private double speed;
        
        public PlasmaAnimation()
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();
        }

        public void Initialize( int _brightness, int size, double _speed,  bool _showRed, bool _showGreen, bool _showBlue, bool _morphGreen, bool _morphBlue, bool _pink)
        {
            speed = _speed;

            plasmaGenerator = ChoosePlasma();

            plasmaGenerator.SetParameters(_brightness, size, _showRed, _showGreen, _showBlue, _morphGreen, _morphBlue, _pink);

        }

        public void Run()
        { 
            //itterate all LEDS to generate the next frame of plasma 
            foreach (var pixel in BlinkyBlinky.pixels)
            {
                
                //Color.Lerp(Color.blue, Color.red, Time.time % 1);// 
                pixel.Color = plasmaGenerator.RenderPlasmaPixel((int)pixel.X, (int)pixel.Y, movement);
               
            }

            movement += speed / 100000;
            currentItternation++;
        }

        private IPlasma ChoosePlasma()
        {
            //replace with case when more exist
            return new PlasmaSinXY( );
        }
    }
}
