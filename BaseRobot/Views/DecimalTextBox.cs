using System.Windows.Controls;

namespace BaseRobot.Views
{
    public class DecimalTextBox : TextBox
    {
        public DecimalTextBox()
        {
            this.PreviewTextInput += IntTextBlock_PreviewTextInput;

            this.TextChanged += IntTextBox_TextChanged;
        }

        private void IntTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb) tb.Select(tb.Text.Length, 0);
        }

        private void IntTextBlock_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (Char.IsDigit(e.Text, 0)
                || (this.Text.Length == 0 && e.Text == "-")
                || (e.Text == "." && this.Text.IndexOf(".") == -1))
            {
                e.Handled = false;
                return;
            }

            e.Handled = true;
        }
    }
}
