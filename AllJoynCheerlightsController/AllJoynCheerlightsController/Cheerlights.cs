using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Windows.UI;

namespace AllJoynCheerlightsController
{
    public class Cheerlights
    {
        public Dictionary<string, HslColor> Colors = new Dictionary<string, HslColor>()
        {
            {"red", new HslColor() {WindowsColor = Color.FromArgb(255, 255, 0, 0), Hue = 0x00000000, Saturation = 0xFFFFFFFF}},
            {"green", new HslColor() {WindowsColor = Color.FromArgb(255, 0, 128, 0), Hue = 0x55555555, Saturation = 0xFFFFFFFF}},
            {"blue", new HslColor() {WindowsColor = Color.FromArgb(255, 0, 0, 255), Hue = 0xAAAAAAAA, Saturation = 0xFFFFFFFF}},
            {"cyan", new HslColor() {WindowsColor = Color.FromArgb(255, 0, 255, 255), Hue = 0x80000000, Saturation = 0xFFFFFFFF}},
            {"white", new HslColor() {WindowsColor = Color.FromArgb(255, 255, 255, 255), Hue = 0x78E37900, Saturation = 0x0}},
            {"oldlace", new HslColor() {WindowsColor = Color.FromArgb(255, 253, 245, 230), Hue = 0x2B602B80, Saturation = 0xCCCCCD00}},
            {"purple", new HslColor() {WindowsColor = Color.FromArgb(255, 128, 0, 128), Hue = 0xBFFFC000, Saturation = 0xFFFFFFFF}},
            {"magenta", new HslColor() {WindowsColor = Color.FromArgb(255, 255, 0, 255), Hue = 0xDC70DC00, Saturation = 0xFFFFFFFF}},
            {"yellow", new HslColor() {WindowsColor = Color.FromArgb(255, 255, 255, 0), Hue = 0x349F3480, Saturation = 0xFFFFFFFF}},
            {"orange", new HslColor() {WindowsColor = Color.FromArgb(255, 255, 165, 0), Hue = 0x238E2380, Saturation = 0xFFFFFFFF}},
            {"pink", new HslColor() {WindowsColor = Color.FromArgb(255, 255, 192, 203), Hue = 0xD554D500, Saturation = 0x47AD4780}},
            {"black", new HslColor() {WindowsColor = Color.FromArgb(255, 0, 0, 0), Hue = 0, Saturation = 0}}
        };

        public async Task<string> GetCurrentColorAsync()
        {
            var request = WebRequest.Create("http://api.thingspeak.com/channels/1417/field/1/last.txt");

            using (var response = (HttpWebResponse)(await request.GetResponseAsync()))
            using (var dataStream = response.GetResponseStream())
            using (var reader = new StreamReader(dataStream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
