using ControlzEx.Theming;
using MahApps.Metro.Controls;
using BaseRobot.Commands;
using BaseRobot.RobotEntity;
using BaseRobot.RobotEnums;
using BaseRobot.ServicesRobot;
using BaseRobot.Views;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using UpdateType = Telegram.Bot.Types.Enums.UpdateType;
using ControllerExChanges.Entity;
using ControllerExChanges.Interfaces;
using ControllerExChanges.Enums;
using ControllerExChanges.Controller;

namespace BaseRobot.ViewModels
{
    public class MyBotVM : BaseVM
    {
        public MyBotVM(MyBot metroWindow, ILogger logger, Controller controller, ConnectionsVM connectionsVM)
        {
            _metroWindow = metroWindow;

            _logger = logger.ForContext<MyBot>();

            _controller = controller;

            ConnectionsVM = connectionsVM;

            Init();
        }

        #region =========================== Fields =========================================

        Messenger _messenger;

        List<IConnector> _servers = new List<IConnector>();

        IConnector _connector;

        List<Security> _securities = new List<Security>();

        private Security _security;

        private MetroWindow _metroWindow;

        Config _config;

        ILogger _logger;

        private TelegramBotClient _telegramBot;

        private string _token = "8531579091:AAH8ARd67VDYe0b-BoRHNycNq2eruuYhEKY";

        private Controller _controller;

        #endregion


        #region =========================== Properties =========================================
        /// <summary>
        /// список тем из mahapps
        /// </summary>
        public ReadOnlyObservableCollection<Theme> Themes { get; set; } = ThemeManager.Current.Themes;

        /// <summary>
        /// Выбранная тема
        /// </summary>
        public Theme SelectedTheme
        {
            get => _selectedTheme;

            set
            {
                _selectedTheme = value;
                OnPropertyChanged(nameof(SelectedTheme));

                if (SelectedTheme != null)
                {
                    ThemeManager.Current.ChangeTheme(_metroWindow, SelectedTheme);

                    _config.SaveConfig(_metroWindow, SelectedTheme);
                }
            }
        }
        private Theme _selectedTheme = ThemeManager.Current.Themes[0];

        /// <summary>
        /// список всех ботов для отображения в Tab
        /// </summary>
        public ObservableCollection<Robot> Robots
        {
            get => _robot;

            set
            {
                _robot = value;
                OnPropertyChanged(nameof(Robots));
            }
        }
        private ObservableCollection<Robot> _robot = new ObservableCollection<Robot>();

        public Robot SelectedRobot
        {
            get => _selectedRobot;

            set
            {
                _selectedRobot = value;
                OnPropertyChanged(nameof(SelectedRobot));
            }
        }
        private Robot _selectedRobot;

        /// <summary>
        /// свойство для переключения открытия/закрытия flyout
        /// </summary>
        public bool IsOpenConnections
        {
            get => _isOpenConnections;

            set
            {
                _isOpenConnections = value;
                OnPropertyChanged(nameof(IsOpenConnections));
            }
        }
        private bool _isOpenConnections = false;

        public ConnectionsVM ConnectionsVM
        {
            get => _connectionsVM;

            set
            {
                _connectionsVM = value;
                OnPropertyChanged(nameof(ConnectionsVM));
            }
        }
        private ConnectionsVM _connectionsVM;

        /// <summary>
        /// Список торговых бумаг
        /// </summary>
        //public ObservableCollection<string> ListSecurities { get; set; } = new ObservableCollection<string>();


        //public ObservableCollection<string> FilteredItems
        //{
        //    get => _filteredItems;
        //    set
        //    {
        //        _filteredItems = value; 
        //        OnPropertyChanged(); 
        //    }
        //}
        //private ObservableCollection<string> _filteredItems;


        //public string SearchText
        //{
        //    get => _searchText;
        //    set
        //    {
        //        _searchText = value;
        //        OnPropertyChanged();
        //        UpdateFilteredItems();
        //    }
        //}
        //private string _searchText;

        /// <summary>
        /// Выбранная бумага
        /// </summary>
        //public string SelectedSecurity
        //{
        //    get => _selectedSecurity;

        //    set
        //    {
        //        _selectedSecurity = value;
        //        OnPropertyChanged(nameof(SelectedSecurity));

        //        _security = GetSecurityForName(_selectedSecurity);

        //        StartSecurity(_security);
        //    }
        //}
        //private string _selectedSecurity = "";

        #endregion

        #region =========================== Commands =========================================

        private DelegateCommand _commandServersToConnect;

        public DelegateCommand CommandServersToConnect
        {
            get
            {
                if (_commandServersToConnect == null)
                {
                    _commandServersToConnect = new DelegateCommand(ServersToConnect);
                }
                return _commandServersToConnect;
            }
        }

        private DelegateCommand _commandAddStrategy;

        public DelegateCommand CommandAddStrategy
        {
            get
            {
                if (_commandAddStrategy == null)
                {
                    _commandAddStrategy = new DelegateCommand(AddStrategy);
                }
                return _commandAddStrategy;
            }
        }

        private DelegateCommand _commandRemoveTab;

        public DelegateCommand CommandRemoveTab
        {
            get
            {
                if (_commandRemoveTab == null)
                {
                    _commandRemoveTab = new DelegateCommand(RemoveTab);
                }
                return _commandRemoveTab;
            }
        }

        #endregion

        #region =========================== Methods =========================================

        private void RemoveTab(object? obj)
        {
            if (obj is string header
                && header != "")
            {
                Robot? robot = GetRobot(header);

                if (robot != null)
                {
                    MessageBoxResult result = MessageBox.Show($"Удалить вкладку {robot.Header}?", "", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        robot.Server.RemoveSecurityFromSubscription(robot.Security);

                        Robots.Remove(robot);
                    }
                }
            }
        }

        private Robot? GetRobot(string header)
        {
            foreach (var robot in Robots)
            {
                if (robot.Header == header)
                {
                    return robot;
                }
            }
            return null;
        }


        private void Init()
        {
            _messenger = Messenger.Instance;
            _messenger.Message += _messenger_Message;

            _config = Config.LoadConfig();

            Recovery(_config);

            _metroWindow.Closing += _metroWindow_Closing;

            _telegramBot = new TelegramBotClient(_token);

            _telegramBot.StartReceiving(updateHandler : HandleUpdateAsync, errorHandler : HandleErrorAsync);

            _logger.Information("Method{@Method}, State TelegramBot {@State}", nameof(Init), _telegramBot.BotId);

        }

        /// <summary>
        /// Метод для обработки обновлений по ТГ-боту
        /// </summary>
        /// <param name="telegramBotClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task HandleUpdateAsync(ITelegramBotClient telegramBotClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message != null)
            {
                ReadMessage(update.Message, telegramBotClient);
            }

            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                GetTelegramBot(update, telegramBotClient);
            }
        }

        private async void GetTelegramBot(Update update, ITelegramBotClient telegramBotClient)
        {
            foreach (Robot robot in Robots)
            {
                if (robot.Header == update.CallbackQuery.Data)
                {
                    await telegramBotClient.SendMessage(update.CallbackQuery.From.Id, 
                        $"Информация по {robot.Header}: \nОбъем позиции: {robot.position?.Volume} \nЦена открытия: {robot.OpenPrice}",
                        replyMarkup: GetBasicButton());
                    
                    return;
                }
            }
            await telegramBotClient.SendMessage(update.CallbackQuery.From.Id, $"Ошибка бота", replyMarkup: GetBasicButton());
        }

        /// <summary>
        /// Метод для чтения входящих текстовых сообщений в ТГ-боте
        /// </summary>
        /// <param name="message"></param>
        /// <param name="telegramBotClient"></param>
        private void ReadMessage(Telegram.Bot.Types.Message message, ITelegramBotClient telegramBotClient)
        {
            switch (message.Text)
            {
                case "Start":
                    StartBotAsync(message, telegramBotClient);
                    break;
                case "All robots":
                    AllRobotsAsync(message, telegramBotClient);
                    break;
            }
        }

        private async void AllRobotsAsync (Telegram.Bot.Types.Message message, ITelegramBotClient telegramBotClient)
        {
            await telegramBotClient.SendMessage(message.Chat.Id, $"Всего роботов : {Robots.Count}", replyMarkup: GetInlineKeyboardButtonAllRobots());
        }

        /// <summary>
        /// метод возвращающий инлайн кнопки в ТГ-боте соответсвующие роботам приложения
        /// </summary>
        /// <returns></returns>
        private InlineKeyboardMarkup GetInlineKeyboardButtonAllRobots()
        {
            List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>>();

            foreach (var robot in Robots)
            {
                InlineKeyboardButton button = InlineKeyboardButton.WithCallbackData(robot.Header);
                
                buttons.Add(new List<InlineKeyboardButton>() {button});
            }

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(buttons);

            return inlineKeyboard;
        }

        /// <summary>
        /// Метод для отправки сообщения в ответ на соответсвующее текстовое сообщение
        /// </summary>
        /// <param name="message"></param>
        /// <param name="telegramBotClient"></param>
        private async void StartBotAsync(Telegram.Bot.Types.Message message, ITelegramBotClient telegramBotClient)
        {
            await telegramBotClient.SendMessage(message.Chat.Id, $"Привет, {message.From.FirstName}", replyMarkup: GetBasicButton());
        }

        /// <summary>
        /// Метод ждя вывода и содержания надстрочных кнопок в ТГ-боте
        /// </summary>
        /// <returns></returns>
        private ReplyKeyboardMarkup GetBasicButton()
        {
            var keyboard = new List<List<KeyboardButton>>();

            keyboard.Add(new List<KeyboardButton> { new KeyboardButton("Start"), new KeyboardButton("All robots") });

            ReplyKeyboardMarkup markup = new ReplyKeyboardMarkup(keyboard);

            markup.ResizeKeyboard = true;

            return markup;
        }

        private async Task HandleErrorAsync(ITelegramBotClient telegramBotClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception != null)
            {
                _logger.Information("Method{@Method} Exception{@Exception}", nameof(HandleErrorAsync), exception.Message);
            }
        }

        /// <summary>
        /// Метод срабатывающий при закрытии окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _metroWindow_Closing(object sender, CancelEventArgs e)
        {
            if (sender is MetroWindow window)
            {
                ReadConfigs();

                _config.SaveConfig(window, SelectedTheme);

                //_logger.Information("Method{@Method}, New Order {@Order}", nameof(_metroWindow_Closing), sender, e);
            }
        }

        private void Recovery(Config config)
        {
            _metroWindow.Left = config.Left;
            _metroWindow.Top = config.Top;
            _metroWindow.Height = config.Height;
            _metroWindow.Width = config.Width;

            if (!string.IsNullOrEmpty(config.Theme))        //тема оформления
            {
                SelectedTheme = GetThemeFromString(config.Theme);
            }

            if (config.ConfigRobots.Count > 0)          //
            {
                foreach (ConfigRobot configRobot in config.ConfigRobots)
                {
                    AddStrategy(configRobot);
                }
            }
        }

        /// <summary>
        /// Метод для получения темы оформления окна из строки
        /// </summary>
        /// <param name="theme"></param>
        /// <returns></returns>
        private Theme? GetThemeFromString(string theme)
        {
            foreach (Theme item in ThemeManager.Current.Themes)
            {
                if (item.Name == theme) return item;
            }
            return null;
        }

        private void _messenger_Message(MessageType type, object message)
        {
            switch (type)
            {
                case MessageType.SaveParaments:
                    ReadConfigs();
                    _config.SaveConfig(_metroWindow, SelectedTheme);
                    break;

                case MessageType.ChangeSecurity:                            // если отправлен robotом, то запускаем метод ChangeSecurity 
                    if (message is Robot robot) ChangeSecurity(robot); 
                    break;
            }
        }
        
        /// <summary>
        /// Метод открывающий окно смены бумаги с серверами, классами бумаг, бумагами
        /// </summary>
        /// <param name="robot"></param>
        private void ChangeSecurity(Robot robot)
        {
            ChangeSecurityWindow changeSecurityWindow = new ChangeSecurityWindow(robot, _controller);

            ThemeManager.Current.ChangeTheme(changeSecurityWindow, SelectedTheme);

            changeSecurityWindow.ShowDialog();
        }

        private void ReadConfigs()
        {
            if (Robots.Count == 0) return;

            List<ConfigRobot> list = new List<ConfigRobot> ();

            foreach (Robot robot in Robots)
            {
                ConfigRobot configRobot = new ConfigRobot()
                {
                    Header = robot.Header,
                    SecurityName = robot.Security?.Name ?? "",
                    SecurityClass = robot.Security?.ClassCode ?? "",
                    PortfolioNumber = robot.SelectedPortfolio?.Name ?? "",
                    SecurityIsinId = robot.Security?.IsinId ?? "",                      //

                    ExchangeType = robot.Server?.ExchangeType ?? ExchangeType.None,
                };

                if (robot.position != null)
                {
                    configRobot.Orders = robot.position.Orders;
                    configRobot.MyTrades = robot.position.MyTrades;
                }

                list.Add(configRobot);
            }
            _config.ConfigRobots = list;
        }

        /// <summary>
        /// Добавить новую стратегию. 
        /// Применяется для команды. Когда добавляем новую стратегию, а не загружаем старую
        /// </summary>
        /// <param name="obj"></param>
        private void AddStrategy(object? obj)  
        {
            AddStrategy(null);

            Log.Logger.Information("Method{@Method}", nameof(AddStrategy));
        }
        
        private void AddStrategy(ConfigRobot? configRobot)
        {
            Robot? robot = RobotFactory.CreateRobot();

            robot.theme = SelectedTheme;
            
            if (configRobot != null)
            {
                robot.Header = configRobot.Header;
                robot.ConfigRobot = configRobot;        // добавить метод возвращающий security из списка бумаг по isin или SecurityName + SecurityClass
            }
            else
            {
                robot.Header = "Tab" + (Robots.Count + 1).ToString();
            }                

            Robots.Add(robot);

            SelectedRobot = robot;
        }
        void ServersToConnect(object o)
        {
            IsOpenConnections = !IsOpenConnections;
        }

        //private void UpdateFilteredItems()
        //{
        //    if (string.IsNullOrEmpty(SearchText))
        //        FilteredItems = new ObservableCollection<string>(ListSecurities);
        //    else
        //        FilteredItems = new ObservableCollection<string>(
        //            ListSecurities.Where(i => i.IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) >= 0));
        //}



        //private Security GetSecurityForName(string name)
        //{
        //    for (int i = 0; i < _securities.Count; i++)
        //    {
        //        if (_securities[i].Name == name) return _securities[i];
        //    }
        //    return null;
        //}

        #endregion
    }
}
