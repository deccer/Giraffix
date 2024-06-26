﻿namespace Giraffix.Native.Glfw;

public static unsafe partial class Glfw
{
    public struct VideoMode
    {
        public int Width;

        public int Height;

        public int RedBits;

        public int GreenBits;

        public int BlueBits;

        public int RefreshRate;
    }

    public struct Image
    {
        public int Width;
        public int Height;
        public byte* PixelPtr;
    }
}