using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crystalbyte.Paranoia {
    public class NavigationInterceptor : INavigationInterceptor {
        public System.ComponentModel.CollectionChangeAction AddRule(NavigationFilterRule filterRule) {
            throw new NotImplementedException();
        }

        public System.ComponentModel.CollectionChangeAction AddRule(string pattern, NavigationRule rule) {
            throw new NotImplementedException();
        }

        public int AddRules(params NavigationFilterRule[] rules) {
            throw new NotImplementedException();
        }

        public event BeginLoadingFrameEventHandler BeginLoadingFrame;

        public event BeginNavigationEventHandler BeginNavigation;

        public string[] Blacklist {
            get { throw new NotImplementedException(); }
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(string pattern) {
            throw new NotImplementedException();
        }

        public NavigationRule GetRule(string url) {
            throw new NotImplementedException();
        }

        public NavigationRule ImplicitRule {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public int RemoveRules(string pattern, NavigationRule rule) {
            throw new NotImplementedException();
        }

        public int RemoveRules(string pattern) {
            throw new NotImplementedException();
        }

        public string[] Whitelist {
            get { throw new NotImplementedException(); }
        }

        public event System.Collections.Specialized.NotifyCollectionChangedEventHandler CollectionChanged;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public IEnumerator<NavigationFilterRule> GetEnumerator() {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
    }
}
