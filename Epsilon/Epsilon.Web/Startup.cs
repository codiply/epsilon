using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Epsilon.Web.Startup))]
namespace Epsilon.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
