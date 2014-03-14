﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Crystalbyte.Paranoia.UI {
    /// <summary>
    /// Interaction logic for RibbonGroup.xaml
    /// </summary>
    public class RibbonGroup : HeaderedItemsControl {

        #region Construction

        static RibbonGroup() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonGroup),
               new FrameworkPropertyMetadata(typeof(RibbonGroup)));
        }

        #endregion

        #region Dependency Properties

        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(RibbonGroup), new PropertyMetadata(null));

        public object CommandParameters {
            get { return GetValue(CommandParametersProperty); }
            set { SetValue(CommandParametersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParameters.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParametersProperty =
            DependencyProperty.Register("CommandParameters", typeof(object), typeof(RibbonGroup), new PropertyMetadata(null));

        #endregion
    }
}
