
public interface IActorBridge
{
    Task<string> SendMessage(string key, string message);
}
