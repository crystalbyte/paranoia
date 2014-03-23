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
    /// Interaction logic for ApplicationMenuItem.xaml
    /// </summary>
    public class ApplicationMenuItem : TabItem {

        #region Construction

        static ApplicationMenuItem() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ApplicationMenuItem),
                new FrameworkPropertyMetadata(typeof(ApplicationMenuItem)));
        }

        #endregion
    }
}