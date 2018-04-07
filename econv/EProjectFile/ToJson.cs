using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EProjectFile
{
    class ToJson
    {
        public override string ToString()
        {
#if DEBUG
            return JsonConvert.SerializeObject((object)this, Formatting.Indented);
#else
            return JsonConvert.SerializeObject((object)this, Formatting.None);
#endif
        }
    }
}
