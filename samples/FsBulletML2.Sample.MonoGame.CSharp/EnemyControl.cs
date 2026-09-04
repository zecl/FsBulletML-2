using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FsBulletML2.Sample.MonoGame.CSharp
{
    public static class EnemyControl
    {
        public static IEnumerable<BulletmlInfo> Bullets()
        {
            return FsBulletML2.Bullets.Dsl.All.bullets;
        }
    }
}
