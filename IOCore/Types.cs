using IOCore.Libs;

namespace IOCore.Types
{
    public class Record
    {
        public object Value;
        public string Text;

        public Record(object value, string text)
        {
            Value = value;
            Text = text;
        }
    }

    public class SwitchRecord
    {
        public object Value;
        public string Text;
        public bool IsOn;

        public SwitchRecord(object value, string text, bool isOn)
        {
            Value = value;
            Text = text;
            IsOn = isOn;
        }
    }

    public class Option : IOItem
    {
        public bool AutoRaise = false;

        private object _tag;
        public object Tag { get => _tag; set { _tag = value; if (AutoRaise) Notify(nameof(Tag)); } }

        private string _text;
        public string Text { get => _text; set { _text = value; if (AutoRaise) Notify(nameof(Text)); } }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; if (AutoRaise) Notify(nameof(IsSelected)); } }

        private bool _isEnabled;
        public bool IsEnabled { get => _isEnabled; set { _isEnabled = value; if (AutoRaise) Notify(nameof(IsEnabled)); } }

        public Option(bool autoRaise)
        {
            AutoRaise = autoRaise;
        }
    }
}