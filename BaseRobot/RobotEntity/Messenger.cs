using BaseRobot.RobotEnums;

namespace BaseRobot.RobotEntity
{
    public class Messenger
    {
        private static readonly Messenger _instance = new Messenger();

        public static Messenger Instance
        {
            get => _instance;
        }

        public void SendMessage(MessageType type, object? message = null)
        {
            Message?.Invoke(type, message);
        }

        public delegate void MessageDelegate(MessageType type, object? message);
        public event MessageDelegate? Message;
    }
}
