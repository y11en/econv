using Newtonsoft.Json;
using System.IO;

namespace EProjectFile
{
	public class CodeSectionInfo
    {
		public const string SectionName = "程序段";

		[JsonIgnore]
		public byte[] UnknownBeforeLibrary;

		public LibraryInfo[] Libraries;

		[JsonIgnore]
		public byte[] UnknownBeforeIconData;

		public byte[] IconData;

		public string DebugCommandParameters;

		public ClassInfo[] Classes;

		public MethodInfo[] Methods;

		public VariableInfo[] GlobalVariables;

		public StructInfo[] Structs;

		public DllDeclareInfo[] DllDeclares;

		public static CodeSectionInfo Parse(SectionInfo sectionInfo, bool cryptEc = false)
		{
            byte[] data = sectionInfo.Data;
            CodeSectionInfo codeSectionInfo = new CodeSectionInfo();
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(data, false)))
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream()))
				{
					using (BinaryWriter binaryWriter2 = new BinaryWriter(new MemoryStream()))
					{
						binaryWriter.Write(binaryReader.ReadInt32());
						binaryWriter.Write(binaryReader.ReadInt32());
						binaryWriter.WriteBytesWithLengthPrefix(binaryReader.ReadBytesWithLengthPrefix());
						int num;
						int value;
						if (cryptEc)
						{
							binaryReader.ReadInt32();
							binaryReader.ReadInt32();
							binaryWriter.WriteBytesWithLengthPrefix(binaryReader.ReadBytesWithLengthPrefix());
							num = binaryReader.ReadInt32();
							value = binaryReader.ReadInt32();
							codeSectionInfo.Libraries = LibraryInfo.ReadLibraries(binaryReader);
							binaryWriter.WriteBytesWithLengthPrefix(binaryReader.ReadBytesWithLengthPrefix());
						}
						else
						{
							binaryWriter.WriteBytesWithLengthPrefix(binaryReader.ReadBytesWithLengthPrefix());
							binaryWriter.WriteBytesWithLengthPrefix(binaryReader.ReadBytesWithLengthPrefix());
							codeSectionInfo.Libraries = LibraryInfo.ReadLibraries(binaryReader);
							num = binaryReader.ReadInt32();
							value = binaryReader.ReadInt32();
						}
						binaryWriter2.Write(num);
						binaryWriter2.Write(value);
						if ((num & 1) != 0)
						{
							binaryWriter2.Write(binaryReader.ReadBytes(16));
						}
						codeSectionInfo.UnknownBeforeLibrary = ((MemoryStream)binaryWriter.BaseStream).ToArray();
						codeSectionInfo.UnknownBeforeIconData = ((MemoryStream)binaryWriter2.BaseStream).ToArray();
					}
				}
				codeSectionInfo.IconData = binaryReader.ReadBytesWithLengthPrefix();
				codeSectionInfo.DebugCommandParameters = binaryReader.ReadStringWithLengthPrefix();
				if (cryptEc)
				{
					binaryReader.ReadBytes(12);
					codeSectionInfo.Methods = MethodInfo.ReadMethods(binaryReader);
					codeSectionInfo.DllDeclares = DllDeclareInfo.ReadDllDeclares(binaryReader);
					codeSectionInfo.GlobalVariables = VariableInfo.ReadVariables(binaryReader);
					codeSectionInfo.Classes = ClassInfo.ReadClasses(binaryReader);
					codeSectionInfo.Structs = StructInfo.ReadStructs(binaryReader);
				}
				else
				{
					codeSectionInfo.Classes = ClassInfo.ReadClasses(binaryReader);
					codeSectionInfo.Methods = MethodInfo.ReadMethods(binaryReader);
					codeSectionInfo.GlobalVariables = VariableInfo.ReadVariables(binaryReader);
					codeSectionInfo.Structs = StructInfo.ReadStructs(binaryReader);
					codeSectionInfo.DllDeclares = DllDeclareInfo.ReadDllDeclares(binaryReader);
				}
			}

            foreach (var method in codeSectionInfo.Methods)
            {
                BinaryReader reader = new BinaryReader(new MemoryStream(method.CodeData[5], false));
                CodeDataParser.StatementBlock statement = CodeDataParser.ParseStatementBlock(reader, codeSectionInfo, method.Id);
                method.Code = statement.ToString();
            }
			return codeSectionInfo;
		}

		public byte[] ToBytes()
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream()))
			{
				WriteTo(binaryWriter);
				binaryWriter.Flush();
				return ((MemoryStream)binaryWriter.BaseStream).ToArray();
			}
		}

		public void WriteTo(BinaryWriter writer)
		{
			writer.Write(UnknownBeforeLibrary);
			LibraryInfo.WriteLibraries(writer, Libraries);
			writer.Write(UnknownBeforeIconData);
			writer.WriteBytesWithLengthPrefix(IconData);
			writer.WriteStringWithLengthPrefix(DebugCommandParameters);
			ClassInfo.WriteClasses(writer, Classes);
			MethodInfo.WriteMethods(writer, Methods);
			VariableInfo.WriteVariables(writer, GlobalVariables);
			StructInfo.WriteStructs(writer, Structs);
			DllDeclareInfo.WriteDllDeclares(writer, DllDeclares);
			writer.Write(new byte[40]);
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject((object)this, Formatting.Indented);
		}
	}
}
