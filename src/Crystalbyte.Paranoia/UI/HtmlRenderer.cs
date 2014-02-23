using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;
using Crystalbyte.Paranoia.Html;

namespace Crystalbyte.Paranoia.UI {

    [ContentProperty("Source")]
    [TemplatePart(Name = XamlHostName, Type = typeof(ContentControl))]
    public sealed class HtmlRenderer : Control {
        public const string XamlHostName = "PART_XamlHost";

        static HtmlRenderer() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlRenderer), new FrameworkPropertyMetadata(typeof(HtmlRenderer)));
        }
        
        public string Source {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(HtmlRenderer), new PropertyMetadata(string.Empty, OnSourceChanged));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var renderer = (HtmlRenderer)d;
            // Check if template is loaded.
            if (renderer.XamlHost == null) {
                return;
            }

            var source = renderer.Source;
            if (string.IsNullOrWhiteSpace(source)) {
                renderer.XamlHost.Blocks.Clear();
                return;
            }

            var xaml = HtmlToXamlConverter.ConvertHtmlToXaml(source, false);
            using (var reader = new StringReader(xaml)) {
                using (var xml = new XmlTextReader(reader)) {
                    var section = (Section) XamlReader.Load(xml);
                    renderer.XamlHost.Blocks.Clear();
                    renderer.XamlHost.Blocks.Add(section);
                }
            }
        }

        internal FlowDocument XamlHost { get; private set; }

        public override void OnApplyTemplate() {
            XamlHost = (FlowDocument)Template.FindName(XamlHostName, this);
            base.OnApplyTemplate();
        }
    }
}
