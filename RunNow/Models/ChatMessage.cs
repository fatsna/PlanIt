using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunNow.Models
{
    public class ChatMessage
    {
        public string Sender { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string ProfileImage { get; set; } = string.Empty;

        public bool IsUserMessage => Sender == "User";
    }
}
