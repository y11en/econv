using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EProjectFile;

namespace econv
{
    class EData : ToJson
    {
        public ESystemInfo ESystemInfo;
        public ProjectConfigInfo ProjectConfigInfo;
        public ResourceSectionInfo ResourceSectionInfo;
        public EPackageInfo EPackageInfo;
        public List<SectionInfo> UnknownInfos;
        public CodeSectionInfo CodeSectionInfo;
    }
}
