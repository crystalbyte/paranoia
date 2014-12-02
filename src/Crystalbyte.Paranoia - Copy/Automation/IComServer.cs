namespace Crystalbyte.Paranoia.Automation {
    public interface IComServer {
        void IncrementObjectCount();
        void DecrementObjectCount();
        void IncrementServerLock();
        void DecrementServerLock();
    }
}
