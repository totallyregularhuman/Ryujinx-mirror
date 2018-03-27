using System;
using System.Collections.ObjectModel;

namespace Ryujinx.Graphics.Gpu
{
    public struct NsGpuPBEntry
    {
        public int Method { get; private set; }

        public int SubChannel { get; private set; }

        private int[] m_Arguments;

        public ReadOnlyCollection<int> Arguments => Array.AsReadOnly(m_Arguments);

        public NsGpuPBEntry(int Method, int SubChannel, params int[] Arguments)
        {
            this.Method      = Method;
            this.SubChannel  = SubChannel;
            this.m_Arguments = Arguments;
        }
    }
}