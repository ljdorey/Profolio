using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace WFInventory.ViewModels
{


    public class ClickSelectTextBox : TextBox
    {
        public ClickSelectTextBox() : base()
        {
    
            var be = BindingOperations.GetBindingExpression(this, TextBox.TextProperty);

            
            AddHandler(PreviewMouseLeftButtonDownEvent,
              new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
            AddHandler(GotKeyboardFocusEvent,
              new RoutedEventHandler(SelectAllText), true);
            AddHandler(MouseDoubleClickEvent,
              new RoutedEventHandler(SelectAllText), true);

            var dp = TextBox.TextProperty;
        }


        private static void SelectivelyIgnoreMouseButton(object sender,
                                                         MouseButtonEventArgs e)
        {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
                parent = VisualTreeHelper.GetParent(parent);
            
            if (parent != null)
            {
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focussed, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = true;
                }
            }

        }

        bool rulesupdated = false;
        private static void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }

        protected override void OnInitialized(EventArgs e)
        {
            var oribinding = BindingOperations.GetBinding(this, TextBox.TextProperty);
            if (oribinding != null)
            {
                Binding newbinding = new Binding(oribinding.Path.Path);
                newbinding.Mode = oribinding.Mode;
                newbinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                foreach (Setter setter in Style.Setters)
                {
                    if (setter.Property.Name == "Text")
                    {
                        if (setter.Value.GetType() == typeof(Binding))
                        {
                            foreach (ValidationRule vr in (setter.Value as Binding).ValidationRules)
                            {
                                if (!newbinding.ValidationRules.Contains(vr))
                                {
                                    newbinding.ValidationRules.Add(vr);
                                }
                            }
                        }
                    }
                }
                var t1 = this.SetBinding(TextBox.TextProperty, newbinding);
                var junk = t1;
            }
        }
    }
}
