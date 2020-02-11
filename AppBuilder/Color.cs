using System;
namespace AppBuilder
{
    sealed class Color
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;

        public Color(string color)
        {
            var values = color.Split(',');
            R = byte.Parse(values[0]);
            G = byte.Parse(values[1]);
            B = byte.Parse(values[2]);
        }

        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public string StringValue
        {
            get
            {
                return $"{R},{G},{B}";
            }
        }

        public string HexValue
        {
            get
            {
                return $"#{R.ToString("X2")}{G.ToString("X2")}{B.ToString("X2")}";
            }
        }

        public string DoubleValueR
        {
            get
            {
                return (R / 255.0).ToString().Replace(",", ".");
            }
        }

        public string DoubleValueG
        {
            get
            {
                return (G / 255.0).ToString().Replace(",", ".");
            }
        }

        public string DoubleValueB
        {
            get
            {
                return (B / 255.0).ToString().Replace(",", ".");
            }
        }
    }
}