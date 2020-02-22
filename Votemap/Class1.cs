using System.Linq;
using System.Text;
using System.IO;
using InfinityScript;
using System;

namespace voteMap
{
    public class voteMap : BaseScript
    {
        private bool voteInProgress = false;
        private HudElem voteBG;
        private HudElem voteText;
        private HudElem voteControls;
        private HudElem voteTimer;
        private string vote = "";
        private string voteBGMaterial = "gradient";
        private int yesVotes = 0;
        private int noVotes = 0;
        private string nextMap = null;
        public voteMap()
        {
            Log.Debug("[Votemap by SPARKER loaded]");

            Call("precacheShader", voteBGMaterial);

            Call("setDvarifUninitialized", "sv_voting", 1);
            Call("setDvarIfUninitialized", "sv_votingTime", 30);
            Call("setDvarIfUninitialized", "sv_votingRatio", 0.6f);
            Call("setDvarifUninitialized", "sv_votingWaitsForRound", 1);
            Call("setDvarIfUninitialized", "sv_votingHudOffsetY", 0);

            PlayerConnected += onPlayerConnected;

            OnInterval(45000, (Func<bool>)delegate
            {
                base.Call("iprintln", "^1Votemap by ^3$^1P^2A^1R^2K^1E^2R");
                return true;
            });
        }

        private void onPlayerConnected(Entity player)
        {
            player.Call("notifyOnPlayerCommand", "voteYes", "vote yes");
            player.Call("notifyOnPlayerCommand", "voteNo", "vote no");
            player.SetField("votedForYes", -1);

            player.OnNotify("voteYes", p => playerVote(player, true));
            player.OnNotify("voteNo", p => playerVote(player, false));
        }


        public override EventEat OnSay2(Entity player, string name, string message)
        {
            if (Call<int>("getDvarInt", "sv_voting") != 1) return EventEat.EatNone;
            if (Call<int>("getDvarInt", "sv_votingTime") < 1)//Don't start votes with invalid timer value
            {
                Log.Error("Unable to start a vote! sv_votingTime is too low!");
                return EventEat.EatNone;
            }

            message = message.ToLower();
            if (message.StartsWith("!votemap "))
            {
                if (voteInProgress)
                {
                    Utilities.SayTo(player, "^1There is already a vote in progress! Wait for the current vote to finish.");
                    return EventEat.EatGame;
                }
                if (message.Split(' ').Length < 2)
                {
                    Utilities.SayTo(player, "^1You must specify a map to vote for! ^7Usage: !votemap <mapName>");
                    return EventEat.EatGame;
                }
                string map = message.Split(' ')[1];

                if (map == "")
                {
                    Utilities.SayTo(player, "^1The specified map was not found!");
                    return EventEat.EatGame;
                }

                initVote(player, map);
                return EventEat.EatGame;
            }
            return EventEat.EatNone;
        }

        private void initVote(Entity owner, string subject)
        {
            subject = getMapName(subject);
            string mapAlias = getMapName(subject);
            nextMap = subject;

            if (subject == "[MapNotFound]" || mapAlias == "[MapNotFound]")
            {
                Utilities.SayTo(owner, "^1Map was not found or its DLC.");
                return;
            }
            if (subject == Call<string>("getdvar", "mapname"))
            {
                Utilities.SayTo(owner, "^1You are currently playing this map.");
                return;
            }

            voteInProgress = true;
            vote = subject;

            int hudOffset = Call<int>("getDvarInt", "sv_votingHudOffsetY");

            //Voting hud
            voteBG = HudElem.CreateServerIcon(voteBGMaterial, 1, 64);
            voteBG.AlignX = "left";
            voteBG.AlignY = "middle";
            voteBG.HorzAlign = "left_adjustable";
            voteBG.VertAlign = "middle";
            voteBG.X = 0;
            voteBG.Y = 5 + hudOffset;
            voteBG.Alpha = 0;
            voteBG.Sort = 10;
            voteBG.Foreground = false;
            voteBG.HideWhenInMenu = false;
            voteBG.Call("fadeovertime", 1);
            voteBG.Alpha = 1;
            voteBG.Call("scaleovertime", 1, 228, 64);

            voteText = HudElem.CreateServerFontString("objective", 1);
            voteText.AlignX = "left";
            voteText.AlignY = "middle";
            voteText.HorzAlign = "left_adjustable";
            voteText.VertAlign = "middle";
            voteText.X = -100;
            voteText.Y = -15 + hudOffset;
            voteText.Alpha = 0;
            voteText.Sort = 20;
            voteText.Foreground = true;
            voteText.HideWhenInMenu = false;
            voteText.Call("fadeovertime", 1);
            voteText.Alpha = 1;
            voteText.Call("moveovertime", 1);
            voteText.X = 5;
            string type = "Map: ^5";
            voteText.SetText("Voting for " + type + mapAlias + "\n^7Called by " + owner.Name);

            voteControls = HudElem.CreateServerFontString("objective", 1);
            voteControls.AlignX = "left";
            voteControls.AlignY = "middle";
            voteControls.HorzAlign = "left_adjustable";
            voteControls.VertAlign = "middle";
            voteControls.X = -100;
            voteControls.Y = 25 + hudOffset;
            voteControls.Alpha = 0;
            voteControls.Sort = 20;
            voteControls.Foreground = true;
            voteControls.HideWhenInMenu = false;
            voteControls.Call("fadeovertime", 1);
            voteControls.Alpha = 1;
            voteControls.Call("moveovertime", 1);
            voteControls.X = 5;
            voteControls.SetText("^3[{vote yes}] ^7Yes(^20^7)    |    ^3[{vote no}] ^7No(^20^7)");

            voteTimer = HudElem.CreateServerFontString("objective", 0.75f);
            voteTimer.AlignX = "left";
            voteTimer.AlignY = "middle";
            voteTimer.HorzAlign = "left_adjustable";
            voteTimer.VertAlign = "middle";
            voteTimer.X = -100;
            voteTimer.Y = 10 + hudOffset;
            voteTimer.Alpha = 0;
            voteTimer.Sort = 20;
            voteTimer.Foreground = true;
            voteTimer.HideWhenInMenu = false;
            voteTimer.Call("fadeovertime", 1);
            voteTimer.Alpha = 1;
            voteTimer.Call("moveovertime", 1);
            voteTimer.X = 5;
            voteTimer.Call("settimer", Call<int>("getDvarInt", "sv_votingTime"));

            AfterDelay(Call<int>("getDvarInt", "sv_votingTime") * 1000, checkVoteResults);

            bool shouldPlaySounds = true;
            AfterDelay(Call<int>("getDvarInt", "sv_votingTime") * 1000, () => shouldPlaySounds = false);

            OnInterval(1000, () =>
            {
                foreach (Entity player in Players)
                {
                    if (player.GetField<string>("classname") == "player")
                        player.Call("playLocalSound", "trophy_detect_projectile");
                }
                if (shouldPlaySounds) return true;
                return false;
            });
        }

        private void playerVote(Entity player, bool yes)
        {
            if (!voteInProgress) return;

            bool playerVote = player.GetField<int>("votedForYes") == 1;
            bool playerVoteIsNull = player.GetField<int>("votedForYes") == -1;

            if (yes && (!playerVote || playerVoteIsNull))
            {
                if (player.GetField<int>("votedForYes") != -1) noVotes--;
                yesVotes++;
                player.SetField("votedForYes", 1);
            }
            else if (!yes && (playerVote || playerVoteIsNull))
            {
                if (player.GetField<int>("votedForYes") != -1) yesVotes--;
                noVotes++;
                player.SetField("votedForYes", 0);
            }
            voteControls.SetText("^3[{vote yes}] ^7Yes(^2" + yesVotes + "^7)    |    ^3[{vote no}] ^7No(^2" + noVotes + "^7)");
        }

        private void checkVoteResults()
        {
            if (!voteInProgress) return;

            voteTimer.Call("fadeovertime", 1);
            voteTimer.Alpha = 0;
            AfterDelay(1000, () => voteTimer.Call("destroy"));
            int totalVotes = yesVotes + noVotes;

            if (yesVotes == noVotes)
            {
                yesVotes = 0;
                noVotes = 0;
                votingFail();
                return;
            }
            if (noVotes > yesVotes)
            {
                yesVotes = 0;
                noVotes = 0;
                votingFail();
                return;
            }
            if (yesVotes > noVotes)
                votingPass();

            yesVotes = 0;
            noVotes = 0;

            //if (ratio >= 0.6f)//Call<float>("getDvarFloat", "sv_votingRatio"))
            //if(yesVotes > noVotes)
            //votingPass();
        }
        private void votingPass()
        {
            if (nextMap == null) nextMap = "mp_dome";
            bool waitForRound = true;//Call<int>("getDvarInt", "sv_votingWaitsForRound") == 1 ? true : false;

            foreach (Entity player in Players)
            {
                if (player.GetField<string>("classname") == "player")
                    player.Call("playLocalSound", "mp_bonus_start");
            }
            voteBG.Call("scaleOverTime", 1, 228, 24);
            voteText.Call("fadeOverTime", 1);
            voteText.Alpha = 0;
            voteControls.Call("fadeOverTime", 1);
            voteControls.Alpha = 0;
            AfterDelay(1000, () =>
            {
                if (!waitForRound)
                {
                    foreach (Entity player in Players)
                    {
                        if (player.GetField<string>("classname") == "player")
                            player.SetClientDvar("g_hardcore", "1");
                    }
                }
                voteControls.Call("destroy");
                if (!waitForRound)
                {
                    voteText.SetText("^2Vote Passed!");
                    voteText.FontScale = 1.25f;
                }
                else voteText.SetText("^2Vote Passed! Waiting to end");
                voteText.Y += 20;
                voteText.Call("fadeOverTime", 1);
                voteText.Alpha = 1;
            });

            if (!waitForRound)
            {
                AfterDelay(6000, () =>
                {
                    Utilities.ExecuteCommand("map " + nextMap);
                });
            }
            else
            {
                OnNotify("game_over", () => AfterDelay(7000, () =>
                {
                    Utilities.ExecuteCommand("map " + nextMap);
                }));
            }
        }
        private void votingFail()
        {
            foreach (Entity player in Players)
            {
                if (player.GetField<string>("classname") == "player")
                    player.Call("playLocalSound", "counter_uav_deactivate");
            }
            voteBG.Call("scaleOverTime", 1, 228, 24);
            voteText.Call("fadeOverTime", 1);
            voteText.Alpha = 0;
            voteControls.Call("fadeOverTime", 1);
            voteControls.Alpha = 0;
            AfterDelay(1000, () =>
            {
                voteControls.Call("destroy");
                voteText.SetText("^1Vote Failed!");
                voteText.FontScale = 1.25f;
                voteText.Y += 20;
                voteText.Call("fadeOverTime", 1);
                voteText.Alpha = 1;
            });
            AfterDelay(7000, () =>
            {
                voteText.Call("fadeovertime", 1);
                voteText.Alpha = 0;
                voteBG.Call("fadeovertime", 1);
                voteBG.Alpha = 0;
            });
            AfterDelay(8000, () =>
            {
                voteText.Call("destroy");
                voteBG.Call("destroy");
                voteInProgress = false;
                foreach (Entity player in Players)
                {
                    if (player.GetField<string>("classname") == "player")
                        player.SetField("votedForYes", -1);
                }
            });
        }

        private string getMapName(string map)
        {
            switch (map)
            {
                case "dome":
                    return "mp_dome";
                case "lockdown":
                    return "mp_alpha";
                case "mission":
                    return "mp_bravo";
                case "seatown":
                    return "mp_seatown";
                case "bootleg":
                    return "mp_bootleg";
                case "carbon":
                    return "mp_carbon";
                case "downturn":
                    return "mp_exchange";
                case "hardhat":
                    return "mp_hardhat";
                case "fallen":
                    return "mp_lambeth";
                case "interchange":
                    return "mp_interchange";
                case "bakaara":
                    return "mp_mogadishu";
                case "resistance":
                    return "mp_paris";
                case "arkaden":
                    return "mp_plaza2";
                case "outpost":
                    return "mp_radar";
                case "underground":
                    return "mp_underground";
                case "village":
                    return "mp_village";
                case "intersection":
                    return "mp_crosswalk_ss";
                case "aground":
                    return "mp_aground_ss";
                case "boardwalk":
                    return "mp_boardwalk";
                case "u-turn":
                    return "mp_burn_ss";
                case "foundation":
                    return "mp_cement";
                case "erosion":
                    return "mp_courtyard_ss";
                case "getaway":
                    return "mp_hillside_ss";
                case "piazza":
                    return "mp_italy";
                case "sanctuary":
                    return "mp_meteora";
                case "gulch":
                    return "mp_moab";
                case "black box":
                    return "mp_morningwood";
                case "parish":
                    return "mp_nola";
                case "overwatch":
                    return "mp_overwatch";
                case "liberation":
                    return "mp_park";
                case "oasis":
                    return "mp_qadeem";
                case "lookout":
                    return "mp_restrepo_ss";
                case "off shore":
                    return "mp_roughneck";
                case "decommission":
                    return "mp_shipbreaker";
                case "vortex":
                    return "mp_six_ss";
                case "terminal":
                    return "mp_terminal_cls";
                /////////////////////////////////////// 
                case "mp_dome":
                    return "Dome";
                case "mp_alpha":
                    return "Lockdown";
                case "mp_bravo":
                    return "Mission";
                case "mp_seatown":
                    return "Seatown";
                case "mp_bootleg":
                    return "Bootleg";
                case "mp_carbon":
                    return "Carbon";
                case "mp_exchange":
                    return "Downturn";
                case "mp_hardhat":
                    return "Hardhat";
                case "mp_lambeth":
                    return "Fallen";
                case "mp_interchange":
                    return "Interchange";
                case "mp_mogadishu":
                    return "Bakaara";
                case "mp_paris":
                    return "Resistance";
                case "mp_plaza2":
                    return "Arkaden";
                case "mp_radar":
                    return "Outpost";
                case "mp_underground":
                    return "Underground";
                case "mp_village":
                    return "Village";
                case "mp_crosswalk_ss":
                    return "Intersection";
                case "mp_aground_ss":
                    return "Aground";
                case "mp_boardwalk":
                    return "Boardwalk";
                case "mp_burn_ss":
                    return "U-turn";
                case "mp_cement":
                    return "Foundation";
                case "mp_courtyard_ss":
                    return "Erosion";
                case "mp_hillside_ssgetaway":
                    return "Getaway";
                case "mp_italy":
                    return "Piazza";
                case "mp_meteora":
                    return "Sanctuary";
                case "mp_moab":
                    return "Gulch";
                case "mp_morningwood":
                    return "Black box";
                case "mp_nola":
                    return "Parish";
                case "mp_overwatch":
                    return "Overwatch";
                case "mp_park":
                    return "Liberation";
                case "mp_qadeem":
                    return "Oasis";
                case "mp_restrepo_ss":
                    return "Lookout";
                case "mp_roughneck":
                    return "Off Shore";
                case "mp_shipbreaker":
                    return "Decommission";
                case "mp_six_ss":
                    return "Vortex";
                case "mp_terminal_cls":
                    return "Terminal";
                default:
                    return "[MapNotFound]";
            }
        }
    }
}
