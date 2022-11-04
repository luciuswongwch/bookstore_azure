using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Utility
{
    public class EmailSenderSettings
    {
        public string SMTPServer { get; set; }
        public int SMTPPort { get; set; }
        public string EmailAddress { get; set; }
        public string EmailPassword { get; set; }
    }
}
