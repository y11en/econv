using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EProjectFile
{
    class MethodInfo : IHasId
    {
        private int id;
        public int Id => id;
        public MethodInfo(int id)
        {
            this.id = id;
        }

        [JsonIgnore]
        public int UnknownAfterId;
        public int Type;
        public int Flags;
        public int ReturnDataType;
        public string Name;
        public string Comment;
        public VariableInfo[] Variables;
        public VariableInfo[] Parameters;
        [JsonProperty(ItemConverterType = typeof(HexConverter))]
        public byte[][] CodeData;
        public static MethodInfo[] ReadMethods(BinaryReader reader)
        {
            var headerSize = reader.ReadInt32();
            int count = headerSize / 8;
            var ids = reader.ReadInt32sWithFixedLength(count);
            var unknownsAfterIds = reader.ReadInt32sWithFixedLength(count);
            var methods = new MethodInfo[count];
            for (int i = 0; i < count; i++)
            {
                var methodInfo = new MethodInfo(ids[i])
                {
                    UnknownAfterId = unknownsAfterIds[i],
                    Type = reader.ReadInt32(),
                    Flags = reader.ReadInt32(),
                    ReturnDataType = reader.ReadInt32(),
                    Name = reader.ReadStringWithLengthPrefix(),
                    Comment = reader.ReadStringWithLengthPrefix(),
                    Variables = VariableInfo.ReadVariables(reader),
                    Parameters = VariableInfo.ReadVariables(reader),
                    CodeData = new byte[6][]
                };
                for (int j = 0; j < methodInfo.CodeData.Length; j++)
                {
                    methodInfo.CodeData[j] = reader.ReadBytesWithLengthPrefix();
                }
                methods[i] = methodInfo;
            }

            return methods;
        }
        public static void WriteMethods(BinaryWriter writer, MethodInfo[] methods)
        {
            writer.Write(methods.Length * 8);
            Array.ForEach(methods, x => writer.Write(x.Id));
            Array.ForEach(methods, x => writer.Write(x.UnknownAfterId));
            foreach (var method in methods)
            {
                writer.Write(method.Type);
                writer.Write(method.Flags);
                writer.Write(method.ReturnDataType);
                writer.WriteStringWithLengthPrefix(method.Name);
                writer.WriteStringWithLengthPrefix(method.Comment);
                VariableInfo.WriteVariables(writer, method.Variables);
                VariableInfo.WriteVariables(writer, method.Parameters);
                Array.ForEach(method.CodeData, x => writer.WriteBytesWithLengthPrefix(x));
            }
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
