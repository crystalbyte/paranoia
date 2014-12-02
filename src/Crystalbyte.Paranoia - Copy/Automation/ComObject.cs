namespace Crystalbyte.Paranoia.Automation {
    public abstract class ComObject {
        private readonly IComServer _server;

        protected ComObject() {
            // COM demands parameterless constructor.    
        }

        protected ComObject(IComServer server) {
            _server = server;
            _server.IncrementObjectCount();
        }

        ~ComObject() {
            _server.DecrementObjectCount();
        }
    }
}
