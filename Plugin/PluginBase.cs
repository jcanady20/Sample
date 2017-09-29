using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin
{
    public class PluginBase : IPlugin
    {
        public PluginBase()
        { }

        public virtual void Initialize()
        { }

        public virtual void OnMessageRecieved()
        { }
    }
}
