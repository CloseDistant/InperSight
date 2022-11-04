using InperSight.Lib.Config;
using InperSight.ViewModels;
using Stylet;
using StyletIoC;
using System.Collections.Generic;

namespace InperSight
{
    public class Bootstrapper : Bootstrapper<StartPageViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure the IoC container in here
            //ulong
        }

        protected override void Configure()
        {
            // Perform any other configuration before the application starts
        }
    }
}
