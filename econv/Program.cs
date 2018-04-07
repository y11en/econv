using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using EProjectFile;

namespace econv
{
    class Program
    {
        enum ExitCode : int
        {
            Success = 0,
            NoInputFile = 1,
            ConvertError = 2,
        }

        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("用法: {0} 易语言文件名 [密码]",
                    System.AppDomain.CurrentDomain.FriendlyName);
                return (int)ExitCode.NoInputFile;
            }

            var path = args[0];
            var password = args.Length >= 2 ? args[1] : null;

            try
            {
                Console.Write(Convert(path, password));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return (int)ExitCode.ConvertError;
            }

            return (int)ExitCode.Success;
        }

        static string Convert(string path, string password)
        {
            using (var project = new ProjectFileReader(File.OpenRead(path), password))
            {
                var data = new EData();
 
                while (!project.IsFinish())
                {
                    SectionInfo section = project.ReadSection();
                    switch (section.Name)
                    {
                        case "系统信息段":
                            {
                                data.ESystemInfo = ESystemInfo.Parse(section.Data);
                                break;
                            }
                        case "用户信息段":
                            {
                                data.ProjectConfigInfo = ProjectConfigInfo.Parse(section.Data);
                                break;
                            }
                        case "程序段":
                            {
                                break;
                            }
                        case "易包信息段1":
                            {
                                data.EPackageInfo = EPackageInfo.Parse(section.Data);
                                break;
                            }
                        case "程序资源段":
                            {
                                data.ResourceSectionInfo = ResourceSectionInfo.Parse(section.Data);
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }

                return data.ToString();
            }
        }
    }
}
