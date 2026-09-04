using System;
using System.Collections.Generic;
using System.Linq;
using FsBulletML2;

namespace FsBulletML2.Sample.MonoGame.CSharp
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            FsBulletML2.BulletMLManager.Init(new BulletFunctions());
            using (var game = new FsBulletML2SampleGame())
                game.Run();
        }
    }
}
