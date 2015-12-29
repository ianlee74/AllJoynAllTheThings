using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using NeoPixelLib;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.SocketInterfaces;

namespace WS2812Controller
{
    public partial class Program
    {
        private static WS2811Led _ws2812Strip;
        private const int _numberOfLeds = 5; // Modify here the led number of your strip !
        private byte[] _color = new byte[3] {0, 0, 0};
        private uint _colorCode = 0;

        private DigitalInput _colorPin0;
        private DigitalInput _colorPin1;
        private DigitalInput _colorPin2;
        private DigitalInput _colorPin3;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");

            _colorPin0 = colorSelector.CreateDigitalInput(GT.Socket.Pin.Three, GlitchFilterMode.Off, ResistorMode.PullDown);
            _colorPin1 = colorSelector.CreateDigitalInput(GT.Socket.Pin.Four, GlitchFilterMode.Off, ResistorMode.PullDown);
            _colorPin2 = colorSelector.CreateDigitalInput(GT.Socket.Pin.Five, GlitchFilterMode.Off, ResistorMode.PullDown);
            _colorPin3 = colorSelector.CreateDigitalInput(GT.Socket.Pin.Six, GlitchFilterMode.Off, ResistorMode.PullDown);

            // Setup the strip. I'm using a Cerb so must use SPI1
            _ws2812Strip = new WS2811Led(_numberOfLeds, SPI.SPI_module.SPI1, WS2811Led.WS2811Speed.S800KHZ, 2.25);

            // Setup a new thread to drive the NeoPixels
            var driveThread = new Thread(DriveStrip);
            // Start the thread
            driveThread.Start();
            // Wait forever
            Thread.Sleep(Timeout.Infinite);
        }

        private void DriveStrip()
        {
            SweepLeds1();
        }

        private void SweepLeds1()
        {
            // Setup a delay time between each transmit
            const int sleepTime = 20;
            // Setup two byte arrays to hold the RGB of the pixels
            var n = 0;

            while (true)
            {
                CheckForColorChange();
                // Setup the initial four pixels in positions 0, 8, 16 and 24
                _ws2812Strip.Set(0, _color[0], _color[1], _color[2]);
                // Send the first arrangement to the NeoPixel rings
                _ws2812Strip.Transmit();
                // Step through the LEDs.
                for (n = 0; n < _numberOfLeds; n++)
                {
                    _ws2812Strip.Rotate(true);
                    _ws2812Strip.Transmit();
                    Thread.Sleep(sleepTime);
                }
                // Do this forever...
            }
        }

        private void CheckForColorChange()
        {
            _colorCode = (uint)((_colorPin3.Read() ? 8 : 0 )
                              + (_colorPin2.Read() ? 4 : 0)
                              + (_colorPin1.Read() ? 2 : 0)
                              + (_colorPin0.Read() ? 1 : 0));
            SetColor();
        }

        private void SetColor()
        {
            switch (_colorCode)
            {
                case 1: // red
                    _color = new byte[3] { 255, 0, 0 };
                    break;
                case 2: // green
                    _color = new byte[3] { 0, 128, 0 };
                    break;
                case 3: // blue
                    _color = new byte[3] { 0, 0, 255 };
                    break;
                case 4: // cyan
                    _color = new byte[3] { 0, 255, 255 };
                    break;
                case 5: // white
                    _color = new byte[3] { 255, 255, 255 };
                    break;
                case 6: // oldlace
                    _color = new byte[3] { 253, 245, 230 };
                    break;
                case 7: // purple
                    _color = new byte[3] { 128, 0, 128 };
                    break;
                case 8: // magenta
                    _color = new byte[3] { 255, 0, 255 };
                    break;
                case 9: // yellow
                    _color = new byte[3] { 255, 255, 0 };
                    break;
                case 10: // orange
                    _color = new byte[3] { 255, 165, 0 };
                    break;
                case 11: // pink
                    _color = new byte[3] { 255, 192, 203 };
                    break;
                default: // off
                    _color = new byte[3] { 0, 0, 0 };
                    break;
            }
            
        }
    }
}
