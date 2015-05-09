using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitterProxy.Common.Models;

namespace TwitterProxy.WebServer.Models
{
    public class MypageModel
    {
        public string ScreenName { get; set; }
        public IList<Consumer> Consumers { get; set; }
        public IList<AccessTokenViewModel> AccessTokens { get; set; }
    }
}
