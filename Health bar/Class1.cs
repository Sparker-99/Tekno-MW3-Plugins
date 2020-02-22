using InfinityScript;

public class hpHud : BaseScript
{
    public hpHud()
    {
        Call("precacheShader", "black");
        base.PlayerConnected += delegate (Entity player)
        {
            HudElem bar = HudElem.NewClientHudElem(player);
            HudElem barBack = HudElem.NewClientHudElem(player);
            barBack.X = 395f;
            barBack.Y = -100f;
            bar.X = 395f;
            bar.Y = -102f;
            barBack.AlignX = "center";
            barBack.AlignY = "bottom";
            barBack.HorzAlign = "center";
            barBack.VertAlign = "bottom";
            barBack.Alpha = 0.5f;
            barBack.HideWhenInMenu = true;
            bar.AlignX = "center";
            bar.AlignY = "bottom";
            bar.HorzAlign = "center";
            bar.VertAlign = "bottom";
            bar.Alpha = 1;
            bar.Color = new Vector3(0f, 0f, 0f);
            bar.HideWhenInMenu = true;
            HudElem hp = HudElem.CreateFontString(player, "default", 1f);
            hp.HideWhenInMenu = true;
            hp.SetPoint("RIGHT", "RIGHT", -25, 111);
            OnInterval(10, delegate
            {
                if (player.Health >= 100)
                {
                    bar.Color = new Vector3(0f, 5f, 0f);
                    hp.SetText("^2" + player.Health);
                }
                else if (player.Health < 100 && player.Health > 50)
                {
                    bar.Color = new Vector3(6f, 6f, 0f);
                    hp.SetText("^3" + player.Health);
                }
                else
                {
                    bar.Color = new Vector3(5f, 0f, 0f);
                    hp.SetText("^1" + player.Health);
                }
                barBack.SetShader("black", 13, (int)((float)player.Health * 1.1f + 5f));
                bar.SetShader("white", 7, (int)((float)player.Health * 1.1f));
                if (player.Health == 0)
                {
                    hp.SetText("");
                }
                return true;
            });
        };
    }
}
