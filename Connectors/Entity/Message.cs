using ControllerExChanges.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    public class Message
    {
        public Message(string title = "",
                        string text = "",
                        string account = "",
                        ExchangeType exchangeType = ExchangeType.None,
                        string securityName = "")
        {
            _id++;
            Id = _id;
            Title = title;
            Text = text;
            Account = account;
            ExchangeType = exchangeType;
            SecurityName = securityName;

            DateTime = DateTime.Now;
        }

        public int Id { get; private set; }

        public DateTime DateTime { get; private set; }

        public string Title { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string Account { get; set; } = string.Empty;

        public ExchangeType ExchangeType { get; set; } = ExchangeType.None;

        public string SecurityName { get; set; } = string.Empty;

        static int _id;
    }
}
