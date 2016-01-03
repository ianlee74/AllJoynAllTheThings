namespace AllJoynCheerlightsController
{
    public class HslColor
    {
        public Windows.UI.Color WindowsColor { get; set; }

        /// <summary>
        /// Hue value = degree_hue * 0x100000000 / 360
        /// degree_hue is described here: http://www.rapidtables.com/convert/color/rgb-to-hsl.htm
        /// </summary>
        public uint Hue { get; set; }
        public uint Saturation { get; set; }
        public uint Level { get; set; }
    }
}
