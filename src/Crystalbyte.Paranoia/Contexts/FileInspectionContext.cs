using System.IO;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Mail;

namespace Crystalbyte.Paranoia {
    public sealed class FileInspectionContext : InspectionContext {
        private readonly FileSystemInfo _file;

        public FileInspectionContext(FileSystemInfo file) {
            _file = file;
        }

        #region Implementation of InspectionContext

        internal override Task ReplyAsync() {
            throw new System.NotImplementedException();
        }

        internal override Task ReplyAll() {
            throw new System.NotImplementedException();
        }

        internal override Task Forward() {
            throw new System.NotImplementedException();
        }

        protected internal async override Task<MailMessageReader> GetMailMessageReaderAsync() {
            var bytes = await Task.Run(() => File.ReadAllBytes(_file.FullName));
            return new MailMessageReader(bytes);
        }

        #endregion
    }
}
