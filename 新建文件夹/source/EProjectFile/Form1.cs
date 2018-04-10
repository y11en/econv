using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EProjectFile
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private static string InputPassword(string tip)
        {
            var passwordDialog = new InputPasswordDialog();
            string password = null;
            passwordDialog.TipTextBox.Text = tip;
            if (passwordDialog.ShowDialog() == DialogResult.OK)
            {
                password = passwordDialog.PasswordTextBox.Text;
            }
            passwordDialog.Dispose();
            return password;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label2.Text = Encoding.UTF8.GetString(new byte[]
            {
                0x41, 0x44, 0xEF, 0xBC, 0x9A, 0x51, 0x75, 0x69, 0x63, 0x6B, 0x20, 0x41, 0x6E, 0x64, 0x20, 0x53,
                0x69, 0x6D, 0x70, 0x6C, 0x65, 0x20, 0x45, 0x43, 0xE5, 0xAE, 0x98, 0xE6, 0x96, 0xB9, 0x51, 0x51,
                0xE7, 0xBE, 0xA4, 0x36, 0x30, 0x35, 0x33, 0x31, 0x30, 0x39, 0x33, 0x33, 0xEF, 0xBC, 0x8C, 0xE6,
                0xAC, 0xA2, 0xE8, 0xBF, 0x8E, 0xE5, 0x8A, 0xA0, 0xE5, 0x85, 0xA5
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool parseSessionData = checkBox1.Checked;
            bool parseCodeData = checkBox2.Checked;
            string fileName = textBox2.Text;
            textBox1.Text = "正在处理...";

            new Task(() =>
            {
                try
                {
                    var output = new StringBuilder();
                    using (var projectFileReader = new ProjectFileReader(File.OpenRead(fileName),InputPassword))
                    {
                        while (!projectFileReader.IsFinish)
                        {
                            var section = projectFileReader.ReadSection();
                            output.AppendLine("------------------" + section.Name + "------------------");
                            output.AppendLine("***Flags: 0x" + section.Flags.ToString("X8"));
                            output.AppendLine("***Key: " + section.Key.ToHexString());
                            output.AppendLine();
                            if (!parseSessionData)
                            {
                                continue;
                            }
                            switch (section.Name)
                            {
                                case ESystemInfo.SectionName:
                                    {
                                        var systemInfo = ESystemInfo.Parse(section.Data);
                                        output.AppendLine(systemInfo.ToString());
                                    }
                                    break;
                                case ProjectConfigInfo.SectionName:
                                    {
                                        var projectConfig = ProjectConfigInfo.Parse(section.Data);
                                        output.AppendLine(projectConfig.ToString());
                                    }
                                    break;
                                case CodeSectionInfo.SectionName:
                                    {
                                        var codeSectionInfo = CodeSectionInfo.Parse(section.Data, projectFileReader.CryptEc);
                                        output.AppendLine(codeSectionInfo.ToString());

                                        if (parseCodeData)
                                        {
                                            output.AppendLine("~~~~~~~~~~~~~~解析代码~~~~~~~~~~~~~~");
                                            foreach (var method in codeSectionInfo.Methods)
                                            {
                                                output.AppendLine($"TryToParseCode: {method.Name}(Id: {method.Id})");
                                                try
                                                {
                                                    var reader = new BinaryReader(new MemoryStream(method.CodeData[5], false));
                                                    var lineOffestStream = new MemoryStream();
                                                    var block = CodeDataParser.ParseStatementBlock(reader, new BinaryWriter(lineOffestStream));
                                                    output.AppendLine($"LineOffest(生成): {lineOffestStream.ToArray().ToHexString()}");
                                                    output.AppendLine($"BlockOffest(生成): {CodeDataParser.GenerateBlockOffest(block).ToHexString()}");
                                                    output.AppendLine(block.ToString());
                                                }
                                                catch (Exception exception)
                                                {
                                                    output.AppendLine("出现错误：");
                                                    output.AppendLine(exception.ToString());
                                                    output.AppendLine();
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case EPackageInfo.SectionName:
                                    {
                                        var packageInfo = EPackageInfo.Parse(section.Data);
                                        output.AppendLine(packageInfo.ToString());
                                    }
                                    break;
                                case ResourceSectionInfo.SectionName:
                                    {
                                        var resourceSectionInfo = ResourceSectionInfo.Parse(section.Data);
                                        output.AppendLine(resourceSectionInfo.ToString());
                                    }
                                    break;

                                case InitEcSectionInfo.SectionName:
                                    {
                                        var initEcSectionInfo = InitEcSectionInfo.Parse(section.Data);
                                        output.AppendLine(initEcSectionInfo.ToString());
                                    }
                                    break;
                                default:
                                    output.Append("Unknown: ");
                                    output.AppendLine(section.Data.ToHexString());
                                    break;
                            }
                            }
                        }
                    Invoke(new Action(() =>
                    {
                        textBox1.Text = output.ToString();
                    }));
                }
                catch (Exception exception)
                {
                    Invoke(new Action(() =>
                    {
                        textBox1.Text = $"出现错误：\r\n{exception}\r\n请加群后将文件发送给作者以便修复此问题";
                    }));
                }
            })
            .Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = openFileDialog1.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string fileName = textBox2.Text;
            string output = textBox3.Text;
            textBox1.Text = "正在处理...";
            new Task(() => 
            {
                try
                {

                    FixEProjectFile(fileName, output, InputPassword);
                    Invoke(new Action(() =>
                    {
                        textBox1.Text = $"处理完成，修复后的文件：{output}";
                    }));
                }
                catch (Exception exception)
                {
                    Invoke(new Action(() =>
                    {
                        textBox1.Text = $"出现错误：\r\n{exception}\r\n请加群后将文件发送给作者以便修复此问题";
                    }));
                }

            }).Start();
        }

        private const int IdMask = 0xFFFFFF;
        private static void FixEProjectFile(string source, string target, ProjectFileReader.OnInputPassword inputPassword = null)
        {
            ESystemInfo systemInfo = null;
            CodeSectionInfo codeSectionInfo = null;
            ResourceSectionInfo resourceSectionInfo = null;
            InitEcSectionInfo initEcSectionInfo = null;
            var sections = new List<SectionInfo>();
            using (var projectFileReader = new ProjectFileReader(File.OpenRead(source), inputPassword))
            {
                while (!projectFileReader.IsFinish)
                {
                    var section = projectFileReader.ReadSection();
                    switch (section.Name)
                    {
                        case ESystemInfo.SectionName:
                            systemInfo = ESystemInfo.Parse(section.Data);
                            break;
                        case CodeSectionInfo.SectionName:
                            codeSectionInfo = CodeSectionInfo.Parse(section.Data, projectFileReader.CryptEc);
                            break;
                        case ResourceSectionInfo.SectionName:
                            resourceSectionInfo = ResourceSectionInfo.Parse(section.Data);
                            break;
                        case InitEcSectionInfo.SectionName:
                            initEcSectionInfo = InitEcSectionInfo.Parse(section.Data);
                            break;
                        default:
                            break;
                    }
                    sections.Add(section);
                }
            }
            systemInfo.FileType = 1;
            foreach (var classInfo in codeSectionInfo.Classes)
            {
                if (!ValidEplName(classInfo.Name))
                {
                    classInfo.Name = ParseDebugComment(classInfo.Comment);
                    if (classInfo.Name == null)
                    {
                        if (classInfo.Comment == "_-@M<>")
                        {
                            classInfo.Comment = "";
                        }
                        classInfo.Name = (classInfo.BaseClass == 0 ? "_程序集" : "_类") + (classInfo.Id & IdMask).ToString("X");
                    }
                    else
                    {
                        classInfo.Comment = "";
                    }
                }
                FixVariablesName(classInfo.Variables, classInfo.BaseClass == 0 ? "_程序集变量" : "_成员");
            }
            FixVariablesName(codeSectionInfo.GlobalVariables, "_全局");
            foreach (var method in codeSectionInfo.Methods)
            {
                if (!ValidEplName(method.Name))
                {
                    method.Name = ParseDebugComment(method.Comment);
                    if (method.Name == null)
                    {
                        if (method.Comment == "_-@S<>")
                        {
                            method.Comment = "";
                        }
                        method.Name = $"_子程序{(method.Id & IdMask).ToString("X")}";
                    }
                    else
                    {
                        method.Comment = "";
                    }
                }
                FixVariablesName(method.Parameters, "_参数", true);
                FixVariablesName(method.Variables, "_局部", true);

                var reader = new BinaryReader(new MemoryStream(method.CodeData[5], false));
                var lineOffestStream = new MemoryStream();
                CodeDataParser.StatementBlock block = null;
                try
                {
                    block = CodeDataParser.ParseStatementBlock(reader, new BinaryWriter(lineOffestStream));
                }
                catch (Exception)
                {
                    method.Comment = $"[修复失败]{method.Comment}";
                    continue;
                }

                method.CodeData[0] = lineOffestStream.ToArray();
                method.CodeData[1] = CodeDataParser.GenerateBlockOffest(block);
            }
            foreach (var structInfo in codeSectionInfo.Structs)
            {
                if (!ValidEplName(structInfo.Name))
                {
                    structInfo.Name = $"_结构{(structInfo.Id & IdMask).ToString("X")}";
                }
                FixVariablesName(structInfo.Member, "_成员", false);
            }
            foreach (var dll in codeSectionInfo.DllDeclares)
            {
                if (!ValidEplName(dll.Name))
                {
                    dll.Name = dll.NameInLibrary;
                    if (dll.Name.StartsWith("@"))
                    {
                        dll.Name = dll.Name.Substring(1);
                    }
                    dll.Name = "_" + dll.Name;
                    if (!ValidEplName("_" + dll.Name))
                    {
                        dll.Name = "";
                    }
                    dll.Name = $"_DLL命令{(dll.Id & IdMask).ToString("X")}{dll.Name}";
                }
                FixVariablesName(dll.Parameters, "_参数", true);
            }
            foreach (var constant in resourceSectionInfo.Constants)
            {
                if (!ValidEplName(constant.Name))
                {
                    constant.Name = $"_常量{(constant.Id & IdMask).ToString("X")}";
                }
            }
            foreach (var formInfo in resourceSectionInfo.Forms)
            {
                if (!ValidEplName(formInfo.Name))
                {
                    formInfo.Name = $"_窗口{(formInfo.Id & IdMask).ToString("X")}";
                }
                foreach (var elem in formInfo.Elements)
                {
                    if (elem is FormMenuInfo)
                    {
                        var menu = (FormMenuInfo)elem;
                        MethodInfo eventMethod = null;
                        if (menu.ClickEvent != 0)
                        {
                            eventMethod = Array.Find(codeSectionInfo.Methods, x => x.Id == menu.ClickEvent);
                        }
                        if (string.IsNullOrEmpty(menu.Name))
                        {
                            if (ValidEplName("_" + menu.Text))
                            {
                                menu.Name = $"_菜单{(menu.Id & IdMask).ToString("X")}_{menu.Text}";
                            }
                            else
                            {
                                menu.Name = $"_菜单{(menu.Id & IdMask).ToString("X")}";
                            }
                            if (eventMethod != null && eventMethod.Name != null
                            && eventMethod.Name.StartsWith("_") && eventMethod.Name.EndsWith("_被选择"))//尝试从事件子程序名还原名称
                            {
                                menu.Name = eventMethod.Name.Substring(1, eventMethod.Name.Length - 5);
                            }
                        }
                        if (eventMethod != null)
                        {
                            eventMethod.Name = $"_{menu.Name}_被选择";
                        }
                    }
                    else if (elem is FormControlInfo)
                    {
                        var control = (FormControlInfo)elem;
                        var elemName = control.Name;

                        if (!ValidEplName(elemName))
                        {
                            if (control.Events.Length > 0)//尝试从子程序名恢复窗口名
                            {
                                var eventItem = control.Events[0];
                                MethodInfo eventMethod = Array.Find(codeSectionInfo.Methods, x => x.Id == eventItem.Value);//TODO:使用哈希表提高效率
                                if (eventMethod != null)
                                {
                                    var eventName = GetEventName(codeSectionInfo.Libraries, control.DataType, eventItem.Key);
                                    if (eventName != null && eventMethod.Name.StartsWith("_") && eventMethod.Name.EndsWith($"_{eventName}"))
                                    {
                                        formInfo.Name = eventMethod.Name.Substring(1, eventMethod.Name.Length - 1 - eventName.Length - 1);
                                    }
                                }
                            }
                            elemName = formInfo.Name;
                        }
                        foreach (var eventItem in control.Events)
                        {
                            MethodInfo eventMethod = Array.Find(codeSectionInfo.Methods, x => x.Id == eventItem.Value);//TODO:使用哈希表提高效率
                            if (eventMethod != null)
                            {
                                var eventName = GetEventName(codeSectionInfo.Libraries, control.DataType, eventItem.Key);
                                if (eventName != null)
                                {
                                    eventMethod.Name = $"_{elemName}_{eventName}";
                                }
                                else if (!eventMethod.Name.StartsWith($"_{elemName}_"))
                                {
                                    eventName = $"事件{eventItem.Key.ToString("X8")}";
                                    eventMethod.Name = $"_{elemName}_{eventName}";
                                }
                            }
                        }
                    }
                }
                var formClass = Array.Find(codeSectionInfo.Classes, x => x.Id == formInfo.Class);//TODO:使用哈希表提高效率
                if (formClass != null)
                {
                    var prefix = $"[“{formInfo.Name}”的窗口程序集]";
                    if (!formClass.Comment.StartsWith(prefix))
                    {
                        formClass.Comment = $"{prefix}{formClass.Comment}";
                    }
                }
            }
            {
                //TODO:使用哈希表提高效率
                var mainMethod = Array.Find(codeSectionInfo.Methods, x => x.Id == codeSectionInfo.MainMethod);
                if (mainMethod != null)
                {
                    mainMethod.Name = "_启动子程序";
                    if (initEcSectionInfo.InitMethod.Length > 0)
                    {
                        var prefix = "[注意：本子程序将在 初始模块_X 后调用]";
                        if (!mainMethod.Comment.StartsWith(prefix))
                        {
                            mainMethod.Comment = $"{prefix}{mainMethod.Comment}";
                        }
                    }
                }
                for (int i = 0; i < initEcSectionInfo.InitMethod.Length; i++) 
                {
                    var initMethod = Array.Find(codeSectionInfo.Methods, x => x.Id == initEcSectionInfo.InitMethod[i]);
                    initMethod.Name = $"初始模块_{i + 1}";
                    if(ValidEplName("_" + initEcSectionInfo.EcName[i]))
                    {
                        initMethod.Name +=  "_" + initEcSectionInfo.EcName[i];
                    }

                    var prefix = $"[禁止删除][注意：本子程序将自动在启动时被调用，且早于 _启动子程序 被调用][为内联的模块“{initEcSectionInfo.EcName[i]}”做初始化工作]";
                    if (!initMethod.Comment.StartsWith(prefix))
                    {
                        initMethod.Comment = $"{prefix}{initMethod.Comment}";
                    }
                }
            }
            
            using (var projectFileWriter = new ProjectFileWriter(File.OpenWrite(target)))
            {
                foreach (var section in sections)
                {
                    switch (section.Name)
                    {
                        case ESystemInfo.SectionName:
                            section.Data = systemInfo.ToBytes();
                            break;
                        case CodeSectionInfo.SectionName:
                            section.Data = codeSectionInfo.ToBytes();
                            break;
                        case ResourceSectionInfo.SectionName:
                            section.Data = resourceSectionInfo.ToBytes();
                            break;
                        case InitEcSectionInfo.SectionName:
                            section.Data = initEcSectionInfo.ToBytes();
                            break;
                        default:
                            break;
                    }
                    projectFileWriter.WriteSection(section);
                }
            }
        }
        private static void FixVariablesName(VariableInfo[] variables, string prefix, bool useIndexInsteadOfId = false)
        {
            int i = 1;
            foreach (var variable in variables)
            {
                if (string.IsNullOrEmpty(variable.Name))
                {
                    variable.Name = prefix + (useIndexInsteadOfId ? i.ToString() : (variable.Id & IdMask).ToString("X"));
                }
                i++;
            }
        }

        private static Regex validEplNameRegex = new Regex(@"^[_A-Za-z\u0080-\uFFFF][_0-9A-Za-z\u0080-\uFFFF]*$", RegexOptions.Compiled);
        private static bool ValidEplName(string name)
        {
            return validEplNameRegex.IsMatch(name);
        }
        private static Regex debugCommentMatchRegex = new Regex(@"^_-@[MS]<([_A-Za-z\u0080-\uFFFF][_0-9A-Za-z\u0080-\uFFFF]*)>$", RegexOptions.Compiled);
        private static string ParseDebugComment(string comment)
        {
            var matchItem = debugCommentMatchRegex.Match(comment);
            if (matchItem == null || !matchItem.Success)
            {
                return null;
            }
            return matchItem.Groups[1].Value;
        }
        private static string GetEventName(LibraryInfo[] libraries, int dataType, int eventId)
        {
            //TODO 效率优化
            //TODO 提供一种机制将信息事先存储到json，然后在不能加载dll的Linux平台使用

            int libraryIndex = ((dataType >> 16) & 0xFFFF) - 1;
            int dataTypeIndex = (dataType & 0xFFFF) - 1;
            string result = null;
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "GetEventName.exe";
                    process.StartInfo.Arguments = $"\"{libraries[libraryIndex].FileName}\" {dataTypeIndex} {eventId}";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();
                    result = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                }
                if("".Equals(result))
                {
                    result = null;
                }
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }
    }
}
