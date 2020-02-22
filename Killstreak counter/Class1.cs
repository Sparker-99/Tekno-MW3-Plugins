using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfinityScript;

namespace KillStreak_Counter
{
    public class KillStreak_Counter : BaseScript
    {
        private static HudElem[] KSHuds = new HudElem[18];
        private static HudElem[] NoKillsHuds = new HudElem[18];

        public KillStreak_Counter()
        {
            PlayerConnected += Ks_PlayerConnected;
        }

        public override void OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            if (player.HasField("KStreak") && attacker.HasField("KStreak"))
            {
                if (player != attacker)
                    attacker.SetField("KStreak", attacker.GetField<int>("KStreak") + 1);
                player.SetField("KStreak", 0);
                HudElem elem = NoKillsHuds[attacker.Call<int>("getentitynumber")];
                if (elem == null)
                    throw new Exception("AttackerNoKills is null. Attacker: " + attacker.Name);
                elem.SetText("^3" + attacker.GetField<int>("KStreak").ToString());
                NoKillsHuds[attacker.Call<int>("getentitynumber")] = elem;
                HudElem elem2 = NoKillsHuds[player.Call<int>("getentitynumber")];
                if (elem2 == null)
                    throw new Exception("VictimNoKills is null. Victim: " + player.Name);
                elem2.SetText("0");
                NoKillsHuds[player.Call<int>("getentitynumber")] = elem2;
            }
        }

        public void Ks_PlayerConnected(Entity player)
        {
            player.SetField("KStreak", 0);
            CreateHudElem(player);
        }

        private void CreateHudElem(Entity player)
        {
            Entity entity = Entity.GetEntity(player.EntRef);
            HudElem elem = HudElem.CreateFontString(entity,"hudsmall", 0.8f);
            elem.SetPoint("TOP", "TOP", -9, 2);
            elem.SetText("^5KILLSTREAK: ");
            HudElem elem2 = HudElem.CreateFontString(entity,"hudsmall", 0.8f);
            elem2.SetPoint("TOP", "TOP", 39, 2);
            elem2.SetText("^30");
            KSHuds[entity.Call<int>("getentitynumber")] = elem;
            NoKillsHuds[entity.Call<int>("getentitynumber")] = elem2;
        }
    }
}
