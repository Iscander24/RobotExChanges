using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace BaseRobot.ViewModels
//{
//    public static class ComboBoxBehaviors
//    {
//        public static readonly DependencyProperty KeepDropDownOpenOnTextChangedProperty =
//            DependencyProperty.RegisterAttached(
//                "KeepDropDownOpenOnTextChanged",
//                typeof(bool),
//                typeof(ComboBoxBehaviors),
//                new PropertyMetadata(false, OnKeepDropDownOpenOnTextChangedChanged));

//        public static bool GetKeepDropDownOpenOnTextChanged(DependencyObject obj)
//            => (bool)obj.GetValue(KeepDropDownOpenOnTextChangedProperty);

//        public static void SetKeepDropDownOpenOnTextChanged(DependencyObject obj, bool value)
//            => obj.SetValue(KeepDropDownOpenOnTextChangedProperty, value);

//        private static void OnKeepDropDownOpenOnTextChangedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            if (d is not ComboBox comboBox)
//                return;

//            if ((bool)e.NewValue)
//            {
//                comboBox.Loaded += (_, _) =>
//                {
//                    if (comboBox.Template.FindName("PART_EditableTextBox", comboBox) is TextBox textBox)
//                    {
//                        textBox.TextChanged += (_, _) =>
//                        {
//                            if (!comboBox.IsDropDownOpen)
//                                comboBox.IsDropDownOpen = true;
//                        };

//                        // управление стрелками при выпадающем списке (ArrowKeys и Enter)
//                        textBox.PreviewKeyDown += (sender, args) =>
//                        {
//                            switch (args.Key)
//                            {
//                                case Key.Down:
//                                    comboBox.IsDropDownOpen = true;
//                                    comboBox.Focus();
//                                    comboBox.SelectedIndex =
//                                        (comboBox.SelectedIndex + 1) % comboBox.Items.Count;
//                                    args.Handled = true;
//                                    break;

//                                case Key.Up:
//                                    comboBox.IsDropDownOpen = true;
//                                    comboBox.Focus();
//                                    comboBox.SelectedIndex =
//                                        comboBox.SelectedIndex > 0
//                                            ? comboBox.SelectedIndex - 1
//                                            : comboBox.Items.Count - 1;
//                                    args.Handled = true;
//                                    break;

//                                case Key.Enter:
//                                    if (comboBox.SelectedItem != null)
//                                    {
//                                        comboBox.Text = comboBox.SelectedItem.ToString();
//                                        comboBox.IsDropDownOpen = false;
//                                        Keyboard.ClearFocus();
//                                    }
//                                    args.Handled = true;
//                                    break;

//                                case Key.Tab:                   // добавляем еще и Tab в качестве выбора
//                                    if (comboBox.SelectedItem != null)
//                                    {
//                                        comboBox.Text = comboBox.SelectedItem.ToString();
//                                        comboBox.IsDropDownOpen = false;
//                                    }
//                                    break;
//                            }
//                        };
//                    }
//                };
//            }
//        }
//    }
//}
{
    public static class ComboBoxBehaviors
    {
        public static readonly DependencyProperty KeepDropDownOpenOnTextChangedProperty =
            DependencyProperty.RegisterAttached(
                "KeepDropDownOpenOnTextChanged",
                typeof(bool),
                typeof(ComboBoxBehaviors),
                new PropertyMetadata(false, OnKeepDropDownOpenOnTextChangedChanged));

        public static bool GetKeepDropDownOpenOnTextChanged(DependencyObject obj)
            => (bool)obj.GetValue(KeepDropDownOpenOnTextChangedProperty);

        public static void SetKeepDropDownOpenOnTextChanged(DependencyObject obj, bool value)
            => obj.SetValue(KeepDropDownOpenOnTextChangedProperty, value);

        private static void OnKeepDropDownOpenOnTextChangedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ComboBox comboBox)
                return;

            if ((bool)e.NewValue)
            {
                comboBox.Loaded += (_, _) =>
                {
                    var textBox = FindEditableTextBox(comboBox);
                    if (textBox == null)
                        return;

                    textBox.TextChanged += (_, _) =>
                    {
                        if (!comboBox.IsDropDownOpen)
                            comboBox.IsDropDownOpen = true;
                    };

                    textBox.PreviewKeyDown += (_, args) =>
                    {
                        if (!comboBox.IsDropDownOpen)
                            comboBox.IsDropDownOpen = true;

                        if (comboBox.Items.Count == 0)
                            return;

                        switch (args.Key)
                        {
                            case Key.Down:
                                comboBox.SelectedIndex =
                                    comboBox.SelectedIndex < comboBox.Items.Count - 1
                                        ? comboBox.SelectedIndex + 1
                                        : 0;
                                ScrollIntoView(comboBox, comboBox.SelectedItem);
                                args.Handled = true;
                                break;

                            case Key.Up:
                                comboBox.SelectedIndex =
                                    comboBox.SelectedIndex > 0
                                        ? comboBox.SelectedIndex - 1
                                        : comboBox.Items.Count - 1;
                                ScrollIntoView(comboBox, comboBox.SelectedItem);
                                args.Handled = true;
                                break;

                            case Key.Enter:
                                if (comboBox.SelectedItem != null)
                                {
                                    comboBox.Text = comboBox.SelectedItem.ToString();
                                    comboBox.IsDropDownOpen = false;
                                    Keyboard.ClearFocus();
                                }
                                args.Handled = true;
                                break;
                        }
                    };
                };
            }
        }

        // универсальный поиск TextBox внутри ComboBox шаблона
        private static TextBox? FindEditableTextBox(ComboBox combo)
        {
            combo.ApplyTemplate();
            return combo.Template.FindName("PART_EditableTextBox", combo) as TextBox
                   ?? combo.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
        }

        // Находим внутренний ListBox и вызываем ScrollIntoView
        private static void ScrollIntoView(ComboBox combo, object? item)
        {
            if (item == null) return;

            combo.ApplyTemplate();
            if (combo.Template.FindName("PART_Popup", combo) is Popup popup)
            {
                if (popup.Child is FrameworkElement popupContent)
                {
                    var listBox = popupContent.GetVisualDescendants().OfType<ListBox>().FirstOrDefault();
                    listBox?.ScrollIntoView(item);
                }
            }
        }

        // Вспомогательный обход визуального дерева
        private static System.Collections.Generic.IEnumerable<DependencyObject> GetVisualDescendants(this DependencyObject root)
        {
            if (root == null) yield break;
            var queue = new System.Collections.Generic.Queue<DependencyObject>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    yield return child;
                    queue.Enqueue(child);
                }
            }
        }
    }
}