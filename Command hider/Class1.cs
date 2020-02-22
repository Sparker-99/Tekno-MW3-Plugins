using InfinityScript;

public class Commandhider : BaseScript
{
    public Commandhider()
    {
        Log.Debug("Command Hider loaded");
    }

    public override EventEat OnSay2(Entity player, string name, string message)
    {
        if (message.StartsWith("!"))

            return EventEat.EatGame;

        return EventEat.EatNone;

    }
}