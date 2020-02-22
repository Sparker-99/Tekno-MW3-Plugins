using InfinityScript;
using System;

public class Anti_Camp_Moab : BaseScript
{
    private bool _donePrematch = false;

    public Anti_Camp_Moab()
    {
        base.PlayerConnected += onPlayerConnected;
        Log.Debug("AntiCamp After MOAB Loaded By Sparker");
        try
        {
            string text = Call<string>("getDvar", "g_gametype").ToLower();
            for (int i = 0; i < Players.Count; i++)
            {
                Entity entity = Call<Entity>("getEntByNum", i);
                if (entity != null)
                {
                    string field = entity.GetField<string>("targetname");
                }
            }
            OnNotify("prematch_done", (Action)delegate
            {
                _donePrematch = true;
            });
            OnNotify("game_over", (Action)delegate
            {
                _donePrematch = false;
            });
        }
        catch (Exception ex)
        {
            Log.Debug(ex.ToString());
        }
    }

    public void onPlayerConnected(Entity entity)
    {
        try
        {
            int seconds = 0, s2 = 0;
            entity.SetField("IsInf", "0");
            entity.SetField("Moabed", 0);
            entity.SetField("ac_using", 0);
            entity.OnNotify("use_hold", (Action<Entity>)delegate (Entity player)
            {
                player.SetField("ac_using", 1);
            });
            entity.OnNotify("done_using", (Action<Entity>)delegate (Entity player)
            {
                player.SetField("ac_using", 0);
            });
            entity.OnNotify("used_nuke", (Action<Entity>)delegate (Entity player)
            {
                player.SetField("Moabed", 1);
                player.Call("iprintlnbold", $"^3Called moab ^2good ^1! ^3Now leave the spot");
            });
            entity.OnInterval(1000, delegate (Entity player)
            {
                if (!player.IsAlive)
                {
                    return true;
                }
                if (!_donePrematch)
                {
                    return true;
                }
                if (player.GetField<int>("ac_using") != 0)
                {
                    return true;
                }
                if (player.Call<int>("istalking", new Parameter[0]) != 0)
                {
                    Log.Write(LogLevel.Trace, "talking");
                    return true;
                }
                if (player.GetField<string>("IsInf") == "1")
                    return false;
                if (player.HasField("ac_lastPos") && player.GetField<Vector3>("ac_lastPos").DistanceTo2D(player.Origin) > 50f && player.GetField<int>("Moabed") >= 1)
                {
                    seconds = 0;
                    s2 = 0;
                }

                    if (player.HasField("ac_lastPos") && player.GetField<Vector3>("ac_lastPos").DistanceTo2D(player.Origin) < 50f && player.GetField<int>("Moabed") >= 1)
                {
                    if (seconds > 15)
                    {
                        s2++;
                        player.Call("iprintlnbold", $"^3You will be^1 killed ^3if you do not ^1move. ^5[{s2}/15]");
                        player.Call("playlocalsound", "counter_uav_activate");
                    }
                    if (++seconds >= 31)
                    {
                        Vector3 startPosition = new Vector3(player.Origin.X, player.Origin.Y, player.Origin.Z + 50f);
                        Entity rocket = Call<Entity>("magicBullet", "rpg_mp", startPosition, player.Origin);
                        rocket.Call("settargetent", player);
                        Entity rocket2 = Call<Entity>("magicBullet", "rpg_mp", startPosition, player.Origin);
                        rocket2.Call("settargetent", player);
                        //if (player.IsAlive) player.Call("suicide");
                        Utilities.RawSayAll("^5" + player.Name + " ^1has been killed for ^2Camping ^1after moab");
                        seconds = 0;
                        s2 = 0;
                    }
                }
                
                player.SetField("ac_lastPos", player.Origin);
                return true;
            });
        }
        catch (Exception ex)
        {
            Log.Debug(ex.ToString());
        }
        
    }
    public override void OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
    {
        player.SetField("IsInf", "1");
        base.OnPlayerKilled(player, inflictor, attacker, damage, mod, weapon, dir, hitLoc);
    }
}