using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Griffin.WebServer;
using Griffin.WebServer.Modules;

namespace CameraControl.Core.Classes
{
    public class WebServerModule : IWorkerModule
    {
        #region Implementation of IHttpModule

        public void BeginRequest(IHttpContext context)
        {
            
        }

        public void EndRequest(IHttpContext context)
        {
            
        }

        public void HandleRequestAsync(IHttpContext context, Action<IAsyncModuleResult> callback)
        {
            callback(new AsyncModuleResult(context, HandleRequest(context)));
        }

        #endregion

        public ModuleResult HandleRequest(IHttpContext context)
        {

            return ModuleResult.Continue;
        }
    }
}
