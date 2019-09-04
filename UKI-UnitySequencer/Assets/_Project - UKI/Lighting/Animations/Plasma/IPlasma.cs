
using UnityEngine;

namespace Unity.BlinkyBlinky.Animations
{
    interface IPlasma
    {
        void SetParameters(int _brightness, int size, bool _showRed, bool _showGreen, bool _showBlue, bool _morphGreen, bool _morphBlue, bool _showPink);
        Color RenderPlasmaPixel(int x, int y, double movement);

    }
}
