using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using FsBulletML2;
using FsBulletML2.MonoGame;
using BulletType = FsBulletML2.DTD.BulletType;
using EnemyBullet = FsBulletML2.MonoGame.EnemyBullet;

namespace FsBulletML2.Sample.MonoGame.CSharp
{
    public class Enemy : FsBulletML2.MonoGame.BaseBullet, IEnemy 
    {
        private FsBulletML2.MonoGame.IBullet self = null;
        private int Timer { get; set; }
        string BulletName { get; set; }
        BulletmlInfo BulletmlInfo { get; set; }
        private bool Second { get; set; }
        EnemyBullet Bullet { get; set; }
        public int Life { get; set; }

        public Enemy() : this(2000) {}
        public Enemy(int life)
        {
            this.self = this;
            this.self.BulletType = BulletType.Enemy;
            self.Init();
            this.self.IsBullet = false;
            this.self.Radius = 18;
            this.Life = life;
        }

        public void Shoot()
        {
          if (this.self.Used)
          {
            this.Bullet = new EnemyBullet();
            ((IBullet)this.Bullet).IsBullet = true;
            Manager.AddEnemyBulletPos(this.Bullet, new Vector2(self.X, self.Y));
            this.Bullet.SetScript(this.BulletmlInfo.Script(BulletmlLoad.loadEnv()));
          }
        }

        void IBullet.Update() { this.Update(); }

        public void Update()
        { 
            this.Timer += 1;

            if (!this.Second || IsFinish()) 
            {
                this.Second = true;
                this.Timer = 0;
                this.Shoot();
            }
            base.RunTask((x, y) => { this.self.X = this.self.X + x; this.self.Y = this.self.Y + y; });
        }

        private bool IsFinish()
        {
            if (this.Bullet == null)
            {
                return false;
            }
            else
            {
                return ((IBullet)this.Bullet).Finished;
            }
       
        }

        public void SetMoveBulletmlInfo(BulletmlInfo bulletmlInfo) 
        {
            ((IEnemy)this).SetScript(bulletmlInfo.Script(BulletmlLoad.loadEnv()), null);
        }

        public void SetBulletmlInfo(string bulletName, BulletmlInfo bulletmlInfo) 
        {
            this.BulletName = bulletName;
            this.BulletmlInfo = bulletmlInfo;
        }

    }
}
