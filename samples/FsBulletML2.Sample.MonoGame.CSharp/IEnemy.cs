using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FsBulletML2.MonoGame;

namespace FsBulletML2.Sample.MonoGame.CSharp
{
    interface IEnemy : IBullet
    {
        int Life { get; set; }
        void Shoot();
    }
}
