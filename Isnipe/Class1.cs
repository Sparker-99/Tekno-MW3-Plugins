using InfinityScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class ISnipe : BaseScript
{
    private IDictionary<string, string> Config = new Dictionary<string, string>();

    private string ISnipeConfig = "scripts\\ISnipeConfig.txt";

    private bool AntiHS = false;

    private bool AntiNS = false;

    private bool AntiDS = false;

    private bool AntiFDMG = false;

    private bool AntiPlant = false;

    private bool AntiKnife = false;

    private bool HudAlive = false;

    private bool AliveCounterWorks = false;

    private int ProcessID = Process.GetCurrentProcess().Id;

    private int DefaultKnifeAddress;

    private unsafe int* KnifeRange;

    private unsafe int* ZeroAddress;

    internal bool antiknife;

    public ISnipe()
    {
        Log.Info("Trying to load ISnipe Script...");
        try
        {
            SetupFiles();
            SetupKnife();
            SetupISnipe();
            Log.Info("ISnipe Script by Astro fixed by Sparker and Parsival");
        }
        catch (Exception e)
        {
            Log.Error("Something wrong with ISnipe Script...");
            Log.Error(e);
        }
    }

    private void SetupISnipe()
    {
        if (AntiHS)
        {
            Log.Info("Anti Hardscope loaded.");
        }
        if (AntiDS)
        {
            Log.Info("Anti Prone loaded.");
        }
        if (AntiNS)
        {
            Log.Info("Anti NoScope loaded.");
        }
        if (AntiFDMG)
        {
            Log.Info("Anti FallDamage loaded.");
        }
        if (AntiPlant)
        {
            Log.Info("Anti Plant loaded.");
        }
        if (HudAlive)
        {
            if (base.Call<string>("getDvar", "g_gametype") == "snd" || base.Call<string>("getDvar", "g_gametype") == "sd")
            {
                Log.Info("Alive Counter Loaded.");
                AliveCounterWorks = true;
            }
            else
                Log.Info("GameType is not search and destroy , AliveCounter will not work!");
        }
        if (AntiKnife)
        {
            Log.Info("Anti Knife loaded.");
            DisableKnife();
        }
        else
        {
            EnableKnife();
        }
        base.PlayerConnected += ISnipe_PlayerConnected;
    }

    private void ISnipe_PlayerConnected(Entity obj)
    {
        if (HudAlive && AliveCounterWorks)
            aliveCounter(obj);
        if (AntiDS)
        {
            obj.Call("notifyOnPlayerCommand", "antids", "toggleprone");
            obj.OnNotify("antids", (Action<Entity>)delegate (Entity ent)
            {
                ent.Call("setstance", "crouch");
                ent.Call("iprintlnbold", Config["antipronemsg"]);
            });
        }
        if (AntiHS)
        {
            int adsTime = 0;
            obj.OnInterval(100, delegate (Entity player)
            {
                if (!player.IsAlive)
                {
                    return true;
                }
                if (player.Call<float>("playerads", new Parameter[0]) >= 1f)
                {
                    adsTime++;
                }
                else
                {
                    adsTime = 0;
                }
                if ((double)adsTime >= 3.5)
                {
                    adsTime = 0;
                    string currentWeapon = player.CurrentWeapon;
                    if (currentWeapon.Contains("iw5_l96a1") || currentWeapon.Contains("iw5_msr"))
                    {
                        player.Call("allowads", false);
                        OnInterval(50, delegate
                        {
                            if (player.Call<int>("adsbuttonpressed", new Parameter[0]) > 0)
                            {
                                return true;
                            }
                            player.Call("allowads", true);
                            player.Call("iprintlnbold", Config["antihsmsg"]);
                            return false;
                        });
                    }
                }
                return true;
            });
        }
        if (AntiPlant)
        {
            obj.OnNotify("weapon_change", (Action<Entity, Parameter>)delegate (Entity ent, Parameter weapon)
            {
                if (weapon.As<string>() == "briefcase_bomb_mp")
                {
                    ent.TakeWeapon("briefcase_bomb_mp");
                    ent.Call("playLocalSound", "mp_ingame_summary");
                    ent.Call("iprintlnbold", Config["antiplantmsg"]);
                }
            });
        }
        if (AntiFDMG)
        {
            obj.OnNotify("weapon_fired", (Action<Entity, Parameter>)delegate (Entity player, Parameter weapon)
            {
                if (obj.Call<float>("playerads", new Parameter[0]) == 0f && IsSniper(player.CurrentWeapon))
                {
                    player.Call("iprintlnbold", Config["antinsmsg"]);
                }
            });
        }
    }

    public override void OnPlayerDamage(Entity player, Entity inflictor, Entity attacker, int damage, int dFlags, string mod, string weapon, Vector3 point, Vector3 dir, string hitLoc)
    {
        if (AntiFDMG && mod == "MOD_FALLING")
        {
            player.Health += damage;
        }
        else
        {
            base.OnPlayerDamage(player, inflictor, attacker, damage, dFlags, mod, weapon, point, dir, hitLoc);
        }
    }

    private bool IsSniper(string currentWeapon)
    {
        return currentWeapon.Contains("l96a1") || currentWeapon.Contains("msr") || currentWeapon.Contains("dragunov") || currentWeapon.Contains("rsass") || currentWeapon.Contains("barret");
    }

    private void SetupFiles()
    {
        if (!File.Exists(ISnipeConfig))
        {
            string[] contents = new string[12]
            {
                "[CONFIG]",
                "[Anti_Hardscope]=true",
                "[Anti_Hardscope_Message]=^1Hardscoping ^7is not allowed",
                "[Anti_Plant]=true",
                "[Anti_Plant_Message]=^1Planting ^7is not allowed",
                "[Anti_Prone]=true",
                "[Anti_Prone_Message]=^1Dropshoting ^7is not allowed",
                "[Anti_NoScope]=true",
                "[Anti_NoScope_Message]=^1NoScoping ^7is not allowed",
                "[Anti_FallDamage]=true",
                "[Anti_Knife]=true",
                "[HUD_Alive_Counter]=true"
            };
            File.WriteAllLines(ISnipeConfig, contents);
        }
        string[] array = File.ReadAllLines(ISnipeConfig);
        SetValue("Anti_Hardscope", "antihs");
        SetValue("Anti_Hardscope_Message", "antihsmsg");
        SetValue("Anti_Plant", "antiplant");
        SetValue("Anti_Plant_Message", "antiplantmsg");
        SetValue("Anti_Prone", "antiprone");
        SetValue("Anti_Prone_Message", "antipronemsg");
        SetValue("Anti_NoScope", "antins");
        SetValue("Anti_NoScope_Message", "antinsmsg");
        SetValue("Anti_FallDamage", "antifalldmg");
        SetValue("Anti_Knife", "antiknife");
        SetValue("HUD_Alive_Counter", "hudalive");
        if (Config["antihs"].Equals("true"))
        {
            AntiHS = true;
        }
        if (Config["antins"].Equals("true"))
        {
            AntiNS = true;
        }
        if (Config["antiprone"].Equals("true"))
        {
            AntiDS = true;
        }
        if (Config["antiplant"].Equals("true"))
        {
            AntiPlant = true;
        }
        if (Config["antifalldmg"].Equals("true"))
        {
            AntiFDMG = true;
        }
        if (Config["antiknife"].Equals("true"))
        {
            AntiKnife = true;
        }
        if (Config["hudalive"] == ("true"))
			HudAlive = true;
    }

    private void aliveCounter(Entity player)
    {
        HudElem fontString1 = HudElem.CreateFontString(player, "hudbig", 0.6f);
        fontString1.SetPoint("DOWNRIGHT", "DOWNRIGHT", -19, 60);
        fontString1.SetText("^5Allies^7:");
        fontString1.HideWhenInMenu = true;
        HudElem fontString2 = HudElem.CreateFontString(player, "hudbig", 0.6f);
        fontString2.SetPoint("DOWNRIGHT", "DOWNRIGHT", -19, 80);
        fontString2.SetText("^1Enemy^7:");
        fontString2.HideWhenInMenu = true;
        HudElem hudElem2 = HudElem.CreateFontString(player, "hudbig", 0.6f);
        hudElem2.SetPoint("DOWNRIGHT", "DOWNRIGHT", -8, 60);
        hudElem2.HideWhenInMenu = true;
        HudElem hudElem3 = HudElem.CreateFontString(player, "hudbig", 0.6f);
        hudElem3.SetPoint("DOWNRIGHT", "DOWNRIGHT", -8, 80);
        hudElem3.HideWhenInMenu = true;
        this.OnInterval(50, (Func<bool>)(() =>
        {
            string str1 = (string)player.GetField<string>("sessionteam");
            string str2 = ((int)this.Call<int>("getteamplayersalive", "axis")).ToString();
            string str3 = ((int)this.Call<int>("getteamplayersalive", "allies")).ToString();
            hudElem2.SetText(str1.Equals("allies") ? str3 : str2);
            hudElem3.SetText(str1.Equals("allies") ? str2 : str3);
            return true;
        }));
    }

    private void SetValue(string linestart, string key)
    {
        string[] array = File.ReadAllLines(ISnipeConfig);
        foreach (string text in array)
        {
            if (text.StartsWith("[" + linestart + "]="))
            {
                char[] separator = new char[1]
                {
                    '='
                };
                string value = text.Split(separator)[1];
                Config.Add(key, value);
            }
        }
    }

    private unsafe void SetupKnife()
    {
        if (!Directory.Exists("scripts\\Knife"))
        {
            Directory.CreateDirectory("scripts\\Knife");
        }
        try
        {
            byte?[] array = new byte?[23]
            {
                139,
                null,
                null,
                null,
                131,
                null,
                4,
                null,
                131,
                null,
                12,
                217,
                null,
                null,
                null,
                139,
                null,
                217,
                null,
                null,
                null,
                217,
                5
            };
            KnifeRange = (int*)(FindMem(array, 1, 4194304, 5242880) + array.Length);
            if ((int)KnifeRange == array.Length)
            {
                byte?[] array2 = new byte?[25]
                {
                    139,
                    null,
                    null,
                    null,
                    131,
                    null,
                    24,
                    null,
                    131,
                    null,
                    12,
                    217,
                    null,
                    null,
                    null,
                    141,
                    null,
                    null,
                    null,
                    217,
                    null,
                    null,
                    null,
                    217,
                    5
                };
                KnifeRange = (int*)(FindMem(array2, 1, 4194304, 5242880) + array2.Length);
                if ((int)KnifeRange == array2.Length)
                {
                    KnifeRange = null;
                }
            }
            DefaultKnifeAddress = *KnifeRange;
            byte?[] array3 = new byte?[24]
            {
                217,
                92,
                null,
                null,
                216,
                null,
                null,
                216,
                null,
                null,
                217,
                92,
                null,
                null,
                131,
                null,
                1,
                15,
                134,
                null,
                0,
                0,
                0,
                217
            };
            ZeroAddress = (int*)(FindMem(array3, 1, 4194304, 5242880) + array3.Length + 2);
            if (KnifeRange == null || DefaultKnifeAddress == 0 || ZeroAddress == null)
            {
                Log.Error("Error finding address: NoKnife Plugin will not work");
            }
        }
        catch (Exception ex)
        {
            Log.Error("Error in NoKnife Plugin. Plugin will not work.");
            Log.Error(ex.ToString());
        }
        if (DefaultKnifeAddress == (int)ZeroAddress)
        {
            if (!File.Exists("scripts\\Knife\\addr_" + ProcessID))
            {
                Log.Error("Error: NoKnife will not work.");
            }
            else
            {
                DefaultKnifeAddress = int.Parse(File.ReadAllText("scripts\\Knife\\addr_" + ProcessID));
            }
        }
        else
        {
            File.WriteAllText("scripts\\Knife\\addr_" + ProcessID, DefaultKnifeAddress.ToString());
        }
    }

    internal unsafe void DisableKnife()
    {
        antiknife = false;
        *KnifeRange = (int)ZeroAddress;
    }

    internal unsafe void EnableKnife()
    {
        antiknife = true;
        *KnifeRange = DefaultKnifeAddress;
    }

    private unsafe int FindMem(byte?[] search, int num = 1, int start = 16777216, int end = 63963136)
    {
        try
        {
            int num2 = 0;
            for (int i = start; i < end; i++)
            {
                byte* ptr = (byte*)i;
                bool flag = false;
                for (int j = 0; j < search.Length; j++)
                {
                    if (search[j].HasValue)
                    {
                        int num3 = *ptr;
                        byte? b = search[j];
                        if (num3 != b.GetValueOrDefault() || ((!b.HasValue) ? true : false))
                        {
                            break;
                        }
                    }
                    if (j == search.Length - 1)
                    {
                        if (num == 1)
                        {
                            flag = true;
                        }
                        else
                        {
                            num2++;
                            if (num2 == num)
                            {
                                flag = true;
                            }
                        }
                    }
                    else
                    {
                        ptr++;
                    }
                }
                if (flag)
                {
                    return i;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("Error in FindMem:" + ex.Message);
        }
        return 0;
    }
    public override EventEat OnSay3(Entity player, ChatType type, string name, ref string message)
    {
        if (message.ToLower() == "!fx")
        {
            AfterDelay(10, delegate
            {
                player.Call("iprintlnbold", "^2Usage ^1!Fx <on/off>");
            });
            return EventEat.EatGame;
        }
        if (message.ToLower() == "!fx on")
        {
            AfterDelay(10, delegate
            {
                player.SetClientDvar("fx_draw", "1");
                player.SetClientDvar("r_fog", "1");
                Utilities.SayTo(player, "^1Fx ^7is now turned on");
            });
            return EventEat.EatGame;
        }
        if (message.ToLower() == "!fx off")
        {
            AfterDelay(10, delegate
            {
                player.SetClientDvar("fx_draw", "0");
                player.SetClientDvar("r_fog", "0");
                Utilities.SayTo(player, "^1Fx ^7is now turned off");
            });
            return EventEat.EatGame;
        }
        return EventEat.EatNone;
    }
}


