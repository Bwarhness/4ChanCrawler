using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(_4ChanCrawler.Startup))]
namespace _4ChanCrawler
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
