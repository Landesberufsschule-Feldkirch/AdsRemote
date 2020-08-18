using System;

namespace AdsRemote.Common
{
    public class LinkedToAttribute : Attribute
    {
        public readonly string To;
        public readonly long Grp;
        public readonly long Offs;
        public readonly int Port;
        public readonly Type As;

        public LinkedToAttribute(string variable, Type @as = null, int port = (int)AmsPort3.PlcRuntime1)
        {
            this.To = variable;
            this.Grp = -1;
            this.Offs = -1;
            this.Port = port;
            this.As = @as;
        }

        public LinkedToAttribute(long grp, long offs, Type @as = null, int port = (int)AmsPort3.PlcRuntime1)
        {
            this.To = null;
            this.Grp = grp;
            this.Offs = offs;
            this.Port = port;
            this.As = @as;
        }
    }
}