using swed32;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;


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
            var health = 0x100;
            var team = 0xF4;
            var clientState = 0x58CFC4;
            var maxPlayers = 0x388;
            var forceJump = 0x527C38C;
            var forceLeft = 0x3202928;
            var forceRight = 0x3202934;
            var radarBase = 0x52071D4;
            var setClanTag = 0x8A320;
            var forceAttack = 0x3202970;
            var crosshairId = 0x11838;


            // engine
            var viewAngles = 0x4D90; // or 0x31E8


            // offsets
            var weaponTable = 0x52244F8;
            var weaponTableIndex = 0x326C;
            var c4Owner = 0x3C8890;
            var spawnTime = 0x103C0;
            var vecOrigin = 0x138; //XYZ Player Position
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


                    //Console.WriteLine("Triggerbot ON!");


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

            #endregion
        }
    }
}
