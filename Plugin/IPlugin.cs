namespace Plugin
{
    public interface IPlugin
    {
        void Initialize();
        void OnMessageRecieved();
    }
}
