using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(THI_Analysis.Startup))]
namespace THI_Analysis
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
