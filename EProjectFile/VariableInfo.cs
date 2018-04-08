using Newtonsoft.Json;
using System.IO;

namespace EProjectFile
{
    public class VariableInfo : IHasId
    {
        private int id;

        public uint DataType;

        public int Flags;

        public int[] UBound;

        public string Name;

        public string Comment;

        public int Id => id;

        public VariableInfo(int id)
        {
            this.id = id;
        }

        public static VariableInfo[] ReadVariables(BinaryReader r)
        {
            return r.ReadBlocksWithIdAndOffest((BinaryReader reader, int id) => new VariableInfo(id)
            {
                DataType = reader.ReadUInt32(),
                Flags = reader.ReadInt16(),
                UBound = reader.ReadInt32sWithFixedLength(reader.ReadByte()),
                Name = reader.ReadCStyleString(),
                Comment = reader.ReadCStyleString()
            });
        }

        public static void WriteVariables(BinaryWriter w, VariableInfo[] variables)
        {
            w.WriteBlocksWithIdAndOffest(variables, delegate (BinaryWriter writer, VariableInfo elem)
            {
                writer.Write(elem.DataType);
                writer.Write((short)elem.Flags);
                writer.Write((byte)elem.UBound.Length);
                writer.WriteInt32sWithoutLengthPrefix(elem.UBound);
                writer.WriteCStyleString(elem.Name);
                writer.WriteCStyleString(elem.Comment);
            });
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject((object)this, Formatting.Indented);
        }
    }
}
