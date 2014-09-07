namespace Crystalbyte.Paranoia {
    public abstract class HierarchyContext : SelectionObject {
        public virtual bool IsListed {
            get { return true; }
        }
    }
}
