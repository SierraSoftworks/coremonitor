using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using CoreMonitor.Controls;

namespace CoreMonitor.Interop
{
    class MenuDetails : EventArgs
    {
        public string Identifier { get; internal set; }

        public string Text { get; internal set; }

        public APIClient Client { get; internal set; }

        private string _value = "";

        public string Value
        {
            get { return _value; }
            internal set
            {
                _value = value;
                if (DisplayMenu != null)
                {
                    DisplayMenu.Value = value;
                    DisplayMenu.OnValueChanged();
                }
            }
        }
        public string[] ValidValues { get; internal set; }

        [XmlIgnore]
        public bool IsButton
        { get { return ValidValues != null && ValidValues.Length == 0; } }

        MenuItem displayMenu = null;

        public event EventHandler ItemClicked = null;
        public event EventHandler ItemValueChanged = null;

        [XmlIgnore]
        public MenuItem DisplayMenu
        {
            get { return displayMenu; }
            set
            {
                if (displayMenu != value)
                {
                    displayMenu = value;
                    DisplayMenu.ItemClicked += (o, e) =>
                        {
                            if (ItemClicked != null)
                                ItemClicked(o, e);
                        };

                    DisplayMenu.ValueChanged += (o, e) =>
                        {
                            if (ItemValueChanged != null)
                                ItemValueChanged(o, e);
                        };
                }
            }

        }

        public string OnClickEventMessage { get; internal set; }
        public string OnValueChangedEventMessage { get; internal set; }

        

        public MenuDetails(APIClient client, string menuID, string text, string onClickMessage)
        {
            Text = text;
            OnClickEventMessage = onClickMessage;
            Identifier = menuID;
            Client = client;
        }

        public MenuDetails(APIClient client, string menuID, string text, string value, string[] validValues, string onValueChangedMessage)
        {
            Text = text;
            Value = value;
            ValidValues = validValues;
            OnValueChangedEventMessage = onValueChangedMessage;
            Identifier = menuID;
            Client = client;
        }

        //Constructor used for XML Serialization
        internal MenuDetails()
        {

        }
    }
}
