using BaseRobot.ViewModels;
using ControllerExChanges.Enums;

namespace BaseRobot.RobotEntity
{
    public class LimitOrder : BaseVM
    {
        public string SecurityName
        {
            get => _securityName;

            set
            {
                _securityName = value;
                OnPropertyChanged(nameof(SecurityName));
            }
        }
        private string _securityName;

        public Operation Direction
        {
            get => _direction;

            set
            {
                _direction = value;
                OnPropertyChanged(nameof(Direction));
            }
        }
        private Operation _direction;

        public decimal PriceOrder
        {
            get => _priceOrder;

            set
            {
                _priceOrder = value;
                OnPropertyChanged(nameof(PriceOrder));
            }
        }
        private decimal _priceOrder;

        public decimal Volume
        {
            get => _volume;

            set
            {
                _volume = value;
                OnPropertyChanged(nameof(Volume));
            }
        }
        private decimal _volume;

        public string Comment
        {
            get => _comment;

            set
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }
        private string _comment;

        public OrderStatus Status
        {
            get => _status;

            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        private OrderStatus _status;
    }
}
