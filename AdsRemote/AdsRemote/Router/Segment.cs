namespace AdsRemote.Router
{
    internal static class Segment
    {
        public static readonly byte[] Header = { 0x03, 0x66, 0x14, 0x71 };
        public static readonly byte[] End = { 0, 0, 0, 0 };
        public static readonly byte[] Amsnetid = { 0, 0, 0, 0, 1, 1 };
        public static readonly byte[] Port = { 0x10, 0x27 };

        public static readonly byte[] RequestAddroute = { 6, 0, 0, 0 };
        public static readonly byte[] RequestDiscover = { 1, 0, 0, 0 };
        public static readonly byte[] RoutetypeTemp = { 6, 0, 0, 0 };
        public static readonly byte[] RoutetypeStatic = { 5, 0, 0, 0 };
        public static readonly byte[] TemprouteTail = { 9, 0, 4, 0, 1, 0, 0, 0 };
        public static readonly byte[] RoutenameL = { 0x0c, 0, 0, 0 };
        public static readonly byte[] UsernameL = { 0x0d, 0, 0, 0 };
        public static readonly byte[] PasswordL = { 2, 0, 0, 0 };
        public static readonly byte[] LocalhostL = { 5, 0, 0, 0 };
        public static readonly byte[] AmsnetidL = { 7, 0, 6, 0 };

        public static readonly byte[] ResponseAddroute = { 6, 0, 0, 0x80 };
        public static readonly byte[] ResponseDiscover = { 1, 0, 0, 0x80 };
        // ReSharper disable once UnusedMember.Global
        public static readonly byte[] TcattypeEngineering = { 4, 0, 0x94, 0, 0x94, 0, 0, 0 };
        public static readonly byte[] TcattypeRuntime = { 4, 0, 0x14, 1, 0x14, 1, 0, 0 };

        public static readonly int LNamelength = 4;
        public static readonly int LOsversion = 12;
        public static readonly int LDescriptionmarker = 4;
        public static readonly int LRouteack = 4;
    }
}
