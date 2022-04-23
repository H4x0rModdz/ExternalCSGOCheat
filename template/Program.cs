using swed32;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using template;
using System.Collections.Generic;
using System.Linq;

namespace CSGO
{
    internal class Program
    {
        [DllImport("user32.dll")]

        static extern short GetAsyncKeyState(Keys vKey);

        static void Main(string[] args)
        {
            #region Variables In Hex
            // client
            var localPlayer = 0xDB65DC;
            var entityList = 0x4DD245C;
            //var maxPlayers = 0x388;
            //var forceJump = 0x527C38C;
            //var forceLeft = 0x3202928;
            //var forceRight = 0x3202934;
            //var radarBase = 0x52071D4;
            //var setClanTag = 0x8A320;
            var forceAttack = 0x3202970;
            var crosshairId = 0x11838;
            //var weaponTable = 0x52244F8;
            //var weaponTableIndex = 0x326C;
            //var spawnTime = 0x103C0;
            //var c4Owner = 0x3C8890;
            //var timeStramp = 1648625815;


            // engine
            var clientState = 0x58CFC4;
            var viewAngles = 0x4D90; 


            // offsets
            var team = 0xF4;
            var health = 0x100;
            var xyz = 0x138; //XYZ Player Position
            var dormant = 0xED;

            #endregion

            swed swed = new swed();

            swed.GetProcess("csgo");

            // module base address 

            var client = swed.GetModuleBase("client.dll");
            var engine = swed.GetModuleBase("engine.dll");

            #region Test Codes
            // player health 

            //int dwLocalPlayer = 0xDB65DC;
            //int m_iHealth = 0x100;

            //var buffer = swed.ReadPointer(client, dwLocalPlayer);


            //while (true)
            //{
            //var hp = BitConverter.ToInt32(swed.ReadBytes(buffer, m_iHealth, 4), 0);

            //Console.WriteLine("Player health ---> " + hp);

            //}
            #endregion

            #region TriggerBot
            while (true)
            {
                if (GetAsyncKeyState(Keys.LShiftKey) < 0)
                {
                    var buffer = swed.ReadPointer(client, localPlayer);
                    var crosshair = BitConverter.ToInt32(swed.ReadBytes(buffer, crosshairId, 4), 0);
                    var ourTeam = BitConverter.ToInt32(swed.ReadBytes(buffer, team, 4), 0);

                    var enemy = swed.ReadPointer(client, entityList + (crosshair - 1) * 0x10);
                    var enemyTeam = BitConverter.ToInt32(swed.ReadBytes(enemy, team, 4), 0);
                    var enemyHealth = BitConverter.ToInt32(swed.ReadBytes(enemy, health, 4), 0);


                    //Console.WriteLine("Triggerbot ON!"); <-- IGNORE THIS


                    if (ourTeam != enemyTeam && enemyHealth > 1)
                    {
                        swed.WriteBytes(client, forceAttack, BitConverter.GetBytes(5));
                        Thread.Sleep(1);
                        swed.WriteBytes(client, forceAttack, BitConverter.GetBytes(4));
                    }

                }
                Thread.Sleep(1);

            }
            #endregion

            #region Aimbot

            Entity player = new Entity();
            List<Entity> entities = new List<Entity>();


            while (true)
            {
                if (GetAsyncKeyState(Keys.CapsLock) < 0)
                {
                    updateLocal();
                    updateEntities();

                    entities = entities.OrderBy(o => o.mag).ToList();

                    if (entities.Count > 0)
                        aim(entities[0]);
                }

                Thread.Sleep(1);
            }


            float calcmag(Entity e)
            {
                return (float)Math.Sqrt(Math.Pow(e.x - player.x, 2) + Math.Pow(e.y - player.y, 2) + Math.Pow(e.z - player.z, 2));
            }

            void updateLocal()
            {
                var buffer = swed.ReadPointer(client, localPlayer);

                var coords = swed.ReadBytes(buffer, xyz, 12);

                player.x = BitConverter.ToSingle(coords, 0);
                player.y = BitConverter.ToSingle(coords, 4);
                player.z = BitConverter.ToSingle(coords, 8);

                player.team = BitConverter.ToInt32(swed.ReadBytes(buffer, team, 4), 0);
            }

            void updateEntities()
            {
                entities.Clear();

                for (int i = 1; i < 32; i++)
                {

                    var buffer = swed.ReadPointer(client, entityList + i * 0x10);
                    var teammate = BitConverter.ToInt32(swed.ReadBytes(buffer, team, 4), 0);

                    var dorm = BitConverter.ToInt32(swed.ReadBytes(buffer, dormant, 4), 0);

                    var hp = BitConverter.ToInt32(swed.ReadBytes(buffer, health, 4), 0);


                    if (hp < 2 || dorm != 0 || teammate == player.team)
                        continue;


                    var coords = swed.ReadBytes(buffer, xyz, 12);

                    var ent = new Entity();

                    ent.x = BitConverter.ToSingle(coords, 0);
                    ent.y = BitConverter.ToSingle(coords, 4);
                    ent.z = BitConverter.ToSingle(coords, 8);
                    ent.team = teammate;
                    ent.health = hp;

                    ent.mag = calcmag(ent);
                    entities.Add(ent);
                    
                }
            }

            void aim(Entity ent)
            {

                // X

                float deltaX = ent.x - player.x;
                float deltaY = ent.y - player.y;

                float X = (float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI);

                // Y

                float deltaZ = ent.z - player.z;

                double distance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));

                float Y = -(float)(Math.Atan2(deltaZ, distance) * 180 / Math.PI);



                var buffer = swed.ReadPointer(engine, clientState);
                swed.WriteBytes(buffer, viewAngles, BitConverter.GetBytes(Y));
                swed.WriteBytes(buffer, viewAngles + 0x4, BitConverter.GetBytes(X));


            }


            #endregion
        }
    }
}
