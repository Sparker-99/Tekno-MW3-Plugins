using InfinityScript;
using System;

public class inf : BaseScript
{
    private string primary = null;

    private string secondary = null;

    public inf()
    {
        Random random = new Random();
        int sayi = random.Next(0, 15);
        askerleriHazirla(sayi);
        base.PlayerConnected += delegate (Entity player)
        {
            inf inf = this;
            spawn(player);
            player.SpawnedPlayer += delegate
            {
                inf.spawn(player);
            };
        };
    }

    private void spawn(Entity ent)
    {
        if (ent.GetField<string>("sessionteam") == "allies")
        {
            AfterDelay(500, delegate
            {
                ent.TakeAllWeapons();
                AfterDelay(300, delegate
                {
                    ent.GiveWeapon(primary);
                    ent.GiveWeapon(secondary);
                    ent.SwitchToWeaponImmediate(primary);
                    ent.Call("givemaxammo", primary);
                    ent.Call("givemaxammo", secondary);
                    ent.Call("SetOffhandPrimaryClass", "bouncingbetty");
                    ent.GiveWeapon("bouncingbetty_mp");
                    ent.GiveWeapon("concussion_grenade_mp");
                    ent.Call("setweaponammoclip", "bouncingbetty_mp", 1);
                });
            });
        }
        else if (ent.GetField<string>("sessionteam") == "axis")
        {
            ent.SetField("maxhealth", 110);
            ent.Health = 110;
            OnInterval(100, delegate
            {
                ent.Call("setmovespeedscale", 1.1f);
                if (!ent.IsAlive)
                {
                    return false;
                }
                return true;
            });
            ent.SetPerk("specialty_fastreload", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_longersprint", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_grenadepulldeath", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_blindeye", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_paint", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_hardline", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_coldblooded", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_quickdraw", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_assists", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_detectexplosive", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_autospot", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_quieter", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_stalker", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_fastoffhand", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_lightweight", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_commando", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_falldamage", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_fasterlockon", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_fastermelee", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_fastmantle", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_fastmeleerecovery", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_holdbreath", codePerk: true, useSlot: false);
            ent.SetPerk("specialty_light_armor", codePerk: true, useSlot: false);
        }
    }

    private void askerleriHazirla(int sayi)
    {
        switch (sayi)
        {
            case 0:
                primary = "iw5_spas12_mp_xmags_reflex_scope2_camo06";
                secondary = "iw5_g18_mp_xmags_reflexsmg_grip";
                break;
            case 1:
                primary = "iw5_spas12_mp_eotech_scope3_camo05";
                secondary = "iw5_g18_mp_xmags_reflexsmg_grip";
                break;
            case 2:
                primary = "iw5_spas12_mp_grip_camo08";
                secondary = "iw5_g18_mp_xmags_reflexsmg_grip";
                break;
            case 3:
                primary = "iw5_striker_mp_xmags_camo01";
                secondary = "iw5_p99_mp_xmags_grip_silencer02";
                break;
            case 4:
                primary = "iw5_aa12_mp_reflex_scope2_camo02";
                secondary = "iw5_deserteagle_mp_xmags_grip_silencer02";
                break;
            case 5:
                primary = "iw5_aa12_mp_xmags_grip_eotech_scope4_camo07";
                secondary = "iw5_deserteagle_mp_xmags_grip_silencer02";
                break;
            case 7:
                primary = "iw5_striker_mp_camo08";
                secondary = "iw5_fmg9_mp_grip_reflexsmg";
                break;
            case 8:
                primary = "iw5_1887_mp_camo09";
                secondary = "iw5_usp45_mp_xmags_grip";
                break;
            case 9:
                primary = "iw5_ksg_mp_grip_camo08";
                secondary = "iw5_usp45_mp_xmags_grip";
                break;
            case 10:
                primary = "iw5_ksg_mp_xmags_eotech_camo07";
                secondary = "iw5_44magnum_mp_xmags_grip";
                break;
            case 11:
                primary = "iw5_striker_mp_grip_camo06";
                secondary = "iw5_44magnum_mp_xmags_grip";
                break;
            case 12:
                primary = "iw5_striker_mp_eotech_camo06";
                secondary = "iw5_mp412_mp_xmags_grip";
                break;
            case 13:
                primary = "iw5_usas12_mp_eotech_camo06";
                secondary = "iw5_deserteagle_mp_xmags_tactical_silencer02";
                break;
            case 14:
                primary = "iw5_striker_mp_xmags_grip_eotech_scope5_camo09";
                secondary = "iw5_p99_mp_xmags_grip_silencer02";
                break;
            default:
                primary = "iw5_usas12_mp_xmags_grip_eotech_scope5_camo09";
                secondary = "iw5_44magnum_mp_xmags_grip";
                break;
        }
    }
}