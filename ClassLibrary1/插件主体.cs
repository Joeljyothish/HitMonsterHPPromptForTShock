using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace TestPlugin
{
    [ApiVersion(2, 1)]//api版本
    public class TestPlugin : TerrariaPlugin
    {
        public override string Author => "GK 阁下";// 插件作者
        public override string Description => "击打高血量怪物时飘动提示怪物血量！";// 插件说明
        public override string Name => "击打提示怪物血量";// 插件名字
        public override Version Version => new Version(1, 0, 0, 1);// 插件版本


        public TestPlugin(Main game) : base(game)// 插件处理
        {
            LPlayers = new LPlayer[256];//应该可以取服务器最大人数
            Order = 1000;//或者顺序一定要在最后
        }

        private LPlayer[] LPlayers { get; set; }//创建一个本地玩家的集合


        public override void Initialize()// 插件启动时，用于初始化各种狗子
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);//钩住游戏初始化时
            //ServerApi.Hooks.NetGetData.Register(this, GetData);
            ServerApi.Hooks.NpcStrike.Register(this, NpcStrike);


            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);//玩家进入服务器
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);//玩家退出服务器
        }
        protected override void Dispose(bool disposing)// 插件关闭时
        {
            if (disposing)
            {
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);//销毁游戏初始化狗子
                //ServerApi.Hooks.NetGetData.Deregister(this, GetData);
                ServerApi.Hooks.NpcStrike.Deregister(this, NpcStrike);



                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);//玩家进入服务器
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);//玩家退出服务器
            }
            base.Dispose(disposing);
        }
        private void OnInitialize(EventArgs args)//游戏初始化
        {
        }
        private void OnGreetPlayer(GreetPlayerEventArgs e)//进入游戏时
        {
            lock (LPlayers)//锁定
                LPlayers[e.Who] = new LPlayer(e.Who);
        }

        public class LPlayer//本地玩家类
        {
            public int Index { get; set; }
            public DateTime LastTiem { get; set; }
            //public int npcid { get; set; }
            //public int life { get; set; }

            public LPlayer(int index)
            {
                //life = 0;
                //npcid = 0;
                Index = index;
                LastTiem = DateTime.UtcNow;
            }
        }

        private void OnLeave(LeaveEventArgs e)
        {
            lock (LPlayers)//锁定
                if (LPlayers[e.Who] != null)
                    LPlayers[e.Who] = null;
        }

        private void NpcStrike(NpcStrikeEventArgs args)//收到数据的狗子程序
        {
            if (args.Handled) return;
            var user = TShock.Players[args.Player.whoAmI];
            if (user == null) return;//若用户不存在直接返回
            if (args.Damage == -1) return;
            if (!args.Npc.active) return;
            if (args.Npc.lifeMax < TShock.Config.Settings.MaxDamage) return;//只针对怪物血量大于伤害最大值的怪
            //int Llife = 0;
            //int Lid = 0;
            lock (LPlayers)//锁定
            {
                if ((DateTime.UtcNow - LPlayers[args.Player.whoAmI].LastTiem).TotalMilliseconds < 900) return;
                LPlayers[args.Player.whoAmI].LastTiem = DateTime.UtcNow;
                //Llife = LPlayers[args.Player.whoAmI].life;
                //Lid = LPlayers[args.Player.whoAmI].npcid;
                //LPlayers[args.Player.whoAmI].life = args.Npc.life;
                //LPlayers[args.Player.whoAmI].npcid = args.Npc.netID;
            }

            Color c = new Color(0, 255, 255);


            //if (args.Npc.netID == Lid && Llife > args.Npc.life && args.Npc.boss);
            //{

            //    int a = args.Npc.life * 100 / args.Npc.lifeMax;
            //    int i = (Llife * 100 / args.Npc.lifeMax) - a;
            //    Console.WriteLine("差距{0}", i);
            //    if (i >= 5)
            //        user.SendMessage(args.Npc.FullName + "剩余"+ a + "%血量:" + args.Npc.life + "", c);
            //}

            //Console.WriteLine("最大血量{0}，血量{1}" , args.Npc.lifeMax, args.Npc.life);

            int a = args.Npc.life * 100 / args.Npc.lifeMax;


            //string text = "["+args.Npc.FullName + "]剩"+ a + "%血:" + args.Npc.life + "";

            string text = args.Npc.FullName + "▕";

            int b = a % 5;
            if (b == 0)
                b = a / 5;
            else
                b = (a / 5) + 1;

            for (int i = 1; i <= 20; i++)
            {
                if (i <= b)
                    text += "▮";
                else
                    text += "▯";
            }
            text += "▏" + a + "%";

            // user.SendData(PacketTypes.CreateCombatTextExtended, text2, (int)c.PackedValue, args.Npc.Center.X, args.Npc.Center.Y);


            user.SendData(PacketTypes.CreateCombatTextExtended, text, (int)c.PackedValue, user.X, user.Y);

        }


    }
}
