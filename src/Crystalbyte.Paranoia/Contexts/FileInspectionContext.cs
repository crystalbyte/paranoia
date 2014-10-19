using System.IO;
using System.Threading.Tasks;
using Crystalbyte.Paranoia.Mail;

namespace Crystalbyte.Paranoia {
    public sealed class FileInspectionContext : InspectionContext {
        private readonly FileSystemInfo _file;

        public FileInspectionContext(FileSystemInfo file) {
            _file = file;
        }

        protected async override Task<MailMessageReader> GetMailMessageReaderAsync() {
            var bytes = await Task.Run(() => File.ReadAllBytes(_file.FullName));
            return new MailMessageReader(bytes);
        }
    }
}
