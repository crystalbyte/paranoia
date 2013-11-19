using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Crystalbyte.Paranoia.UI {
    public sealed class StageGroup : ItemsControl {

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is StageItem;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new StageItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
        }
    }
}
