using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BookCompare.Startup))]
namespace BookCompare
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
