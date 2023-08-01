namespace Aristocrat.Monaco.RobotApiServer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Net;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Aristocrat.Monaco.RobotApiServer.Controllers;
    using Aristocrat.Monaco.Kernel;

    internal class RobotApiServerCore
    {
        public static void StartApiServer()
        {
            var builder = WebApplication.CreateBuilder();

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Listen(IPAddress.Any, 8300);
            });

            builder.Services.AddControllers()
                            .AddApplicationPart(typeof(AutomationController).Assembly);

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            // app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
