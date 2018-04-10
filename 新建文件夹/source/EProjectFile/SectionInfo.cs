using System;
using System.Text;

namespace EProjectFile
{
    class SectionInfo
    {
        public string Name;
        public byte[] Key = new byte[] { 25, 115, 0, 7 };
        public byte[] Data;
        public int Flags;
    }
}