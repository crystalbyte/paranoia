using System.Runtime.CompilerServices;
using dotless.Core.Parser.Tree;
using NLog;

namespace Crystalbyte.Paranoia {
    public static class LoggerExtensions {
        public static void Enter(this Logger logger, [CallerMemberName] string callerName = "") {
            var message = string.Format("Method {0} invoked.", callerName);
            if (logger.IsDebugEnabled) {
                logger.Debug(message);    
            }
        }

        public static void Exit(this Logger logger, [CallerMemberName] string callerName = "") {
            var message = string.Format("Method {0} left.", callerName);
            if (logger.IsDebugEnabled) {
                logger.Debug(message);
            }
        }
    }
}
