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
                Convert(path, password);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return (int)ExitCode.ConvertError;
            }

            return (int)ExitCode.Success;
        }

        static void Convert(string path, string password)
        {
            using (var project = new ProjectFileReader(File.OpenRead(path), password))
            {
                while (!project.IsFinish())
                {
                    SectionInfo section = project.ReadSection();
                    Console.WriteLine(section.Name);
                }
            }
        }
    }
}
