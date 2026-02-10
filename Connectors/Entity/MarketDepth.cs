using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControllerExChanges.Entity
{
    public class MarketDepth
    {
        /// <summary>
        /// time to create a glass
        /// время создания стакана
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// levels of sales. best with index 0
        /// уровни продаж. лучшая с индексом 0
        /// </summary>
        public List<MarketDepthLevel> Asks { get; set; } = new List<MarketDepthLevel>();

        /// <summary>
        /// purchase levels. best with index 0
        /// уровни покупок. лучшая с индексом 0
        /// </summary>
        public List<MarketDepthLevel> Bids { get; set; } = new List<MarketDepthLevel>();


        public MarketDepthLevel Ask()
        {
            if (Asks.Count > 0) return Asks[0];

            return new MarketDepthLevel();
        }

        public MarketDepthLevel Bid()
        {
            if (Bids.Count > 0) return Bids[0];

            return new MarketDepthLevel();
        }


        /// <summary>
        /// security that owns to glass
        /// бумага, которой принадлежит стакан
        /// </summary>
        public string IsinId { get; set; } = string.Empty;


    }
}
