using System.Collections.Generic;
using System.IO;

namespace EProjectFile
{
    public class PublicClassesInfo
    {
        public const string SectionName = "辅助信息段2";

        public HashSet<uint> Classes;

        public static PublicClassesInfo Parse(SectionInfo sectionInfo, bool cryptEc = false)
        {
            byte[] data = sectionInfo.Data;

            PublicClassesInfo publicClassesInfo = new PublicClassesInfo();
            using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(data, false)))
            {
                uint method = binaryReader.ReadUInt32();
                uint is_public = binaryReader.ReadUInt32();

                HashSet<uint> set = new HashSet<uint>();

                if (is_public == 1)
                {
                    set.Add(method);
                }
                publicClassesInfo.Classes = set;
            }
            return publicClassesInfo;
        }
    }
}
