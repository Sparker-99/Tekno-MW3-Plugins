using InfinityScript;
using System;

public class AntiCamp : BaseScript
{
    private bool _donePrematch = false;

    public AntiCamp()
    {
        base.PlayerConnected += onPlayerConnected;
        Log.Debug("AntiCamp Loaded");
        try
        {
            string text = Call<string>("getDvar", new Parameter[1]
            {
                "g_gametype"
            }).ToLower();
            for (int i = 0; i < base.Players.Count; i++)
            {
                Entity entity = Call<Entity>("getEntByNum", new Parameter[1]
                {
                    i
                });
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
            int seconds = 0;
            int s2 = 0;
            entity.SetField("ac_using", 0);
            entity.OnNotify("use_hold", (Action<Entity>)delegate (Entity player)
            {
                player.SetField("ac_using", 1);
            });
            entity.OnNotify("done_using", (Action<Entity>)delegate (Entity player)
            {
                player.SetField("ac_using", 0);
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
                if (player.HasField("ac_lastPos") && player.GetField<Vector3>("ac_lastPos").DistanceTo2D(player.Origin) > 50f)
                {
                    seconds = 0;
                    s2 = 0;
                }
                    if (player.HasField("ac_lastPos") && player.GetField<Vector3>("ac_lastPos").DistanceTo2D(player.Origin) < 50f)
                    {
                      if (seconds > 20)
                      {
                        s2++;
                        player.Call("iprintlnbold", $"^3You will be^1 killed ^3if you do not ^1move. ^5[{s2}/10]");
                      }
                      if (++seconds >= 30)
                      {
                        player.Call("suicide");
                        Utilities.RawSayAll("^5" + player.Name + " ^1has been killed for ^2Camping");
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
}
