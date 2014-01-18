﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia.Messaging {
    public static class SmtpCommands {
        public static string Ehlo = "EHLO";
        public static string Helo = "HELO";
        public static string Mail = "MAIL";
        public static string Rcpt = "RCPT";
        public static string Data = "DATA";
        public static string Quit = "QUIT";
        public static string StartTls = "STARTTLS";
    }
}
