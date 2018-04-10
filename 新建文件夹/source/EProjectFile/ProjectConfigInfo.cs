using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics;
using System.IO;

namespace EProjectFile
{
    class ProjectConfigInfo
    {
        public const string SectionName = "用户信息段";

        public string Name;
        public string Description;
        public string Author;
        public string ZipCode;
        public string Address;
        public string TelephoneNumber;
        public string FaxNumber;
        public string Email;
        public string Homepage;
        public string CopyrightNotice;
        [JsonConverter(typeof(VersionConverter))]
        public Version Version;
        public bool WriteVersion;
        public string CompilePlugins;
        public bool ExportPublicClassMethod;
        public static ProjectConfigInfo Parse(byte[] data)
        {
            var projectConfig = new ProjectConfigInfo();
            using (var reader = new BinaryReader(new MemoryStream(data, false)))
            {
                projectConfig.Name = reader.ReadStringWithLengthPrefix();
                projectConfig.Description = reader.ReadStringWithLengthPrefix();
                projectConfig.Author = reader.ReadStringWithLengthPrefix();
                projectConfig.ZipCode = reader.ReadStringWithLengthPrefix();
                projectConfig.Address = reader.ReadStringWithLengthPrefix();
                projectConfig.TelephoneNumber = reader.ReadStringWithLengthPrefix();
                projectConfig.FaxNumber = reader.ReadStringWithLengthPrefix();
                projectConfig.Email = reader.ReadStringWithLengthPrefix();
                projectConfig.Homepage = reader.ReadStringWithLengthPrefix();
                projectConfig.CopyrightNotice = reader.ReadStringWithLengthPrefix();
                projectConfig.Version = new Version(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                projectConfig.WriteVersion = reader.ReadInt32() == 0;
                projectConfig.CompilePlugins = reader.ReadStringWithFixedLength(20);
                projectConfig.ExportPublicClassMethod = reader.ReadInt32() != 0;

            }
            return projectConfig;
        }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}