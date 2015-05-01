using System;

namespace Crystalbyte.Paranoia.Data.SQLite {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class VirtualAttribute : Attribute {
        public VirtualAttribute(ModuleType module) {
            Module = module;
        }

        public ModuleType Module { get; set; }

        public string GetModuleName() {
            switch (Module) {
                case ModuleType.Fts3:
                    return "fts3";
                default:
                    throw new IndexOutOfRangeException("Module");
            }
        }

    }
}
