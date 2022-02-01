using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeterHan.PLib.Options
{
    public interface IManualConfig
    {
        object ReadSettings();

        void WriteSettings(object settings);

        string GetConfigPath();
    }
}
