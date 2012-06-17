using System;
using System.Collections.Generic;
using System.Text;
using SierraLib.LCD.Logitech;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace CoreMonitor.LCDControls
{
    class Menu
    {
        private bool visible = true;
        public bool Visible
        {
            get { return visible; }
            private set
            {
                if (value != visible)
                {
                    foreach (var child in DisplayObjects)
                        child.IsVisible = value;
                    visible = value;
                }
            }
        }

        public LcdDevice LCDDevice
        { get; set; }

        public List<LcdGdiObject> DisplayObjects
        { get; set; }

        public List<MenuItem> Items
        { get; set; }

        private void NextItem()
        {
            CurrentItem.Selected = false;
            currentItemIndex ++;
            if (currentItemIndex >= Items.Count)
                currentItemIndex = 0;
            CurrentItem.Selected = true;

            firstItem = Math.Max(0, currentItemIndex - 3);
            lastItem = firstItem + 3;

            CurrentItem.OnItemSelected();
            Update();
        }

        private void PreviousItem()
        {
            CurrentItem.Selected = false;
            currentItemIndex--;
            if (currentItemIndex < 0)
                currentItemIndex = Items.Count - 1;
            CurrentItem.Selected = true;
                       

            firstItem = Math.Max(0, currentItemIndex - 3);
            lastItem = firstItem + 3;

            CurrentItem.OnItemSelected();
            Update();
        }

        private int currentItemIndex = 0;

        private MenuItem CurrentItem
        { get { return Items[currentItemIndex]; } }

        private int firstItem = 0;

        private int lastItem = 0;

        public void Update()
        {
            firstItem = Math.Max(0, currentItemIndex - 3);
            lastItem = firstItem + 3;

            if (Items.Count == 0)
            {
                Visible = false;
                return;
            }

            int menuIndex = 0;
            for (int i = firstItem; i <= lastItem; i++)
            {
                //Populate the items
                int titleIndex = (menuIndex * 3) + 3;
                int valueIndex = titleIndex + 1;
                int selectionIndex = titleIndex - 1;

                if (i < Items.Count && Items[i] != null)
                {
                    DisplayObjects[titleIndex].IsVisible = true;
                    DisplayObjects[valueIndex].IsVisible = true;
                    ((LcdGdiText)DisplayObjects[titleIndex]).Text = Items[i].Text;
                    if (!Items[i].IsButton)
                        ((LcdGdiText)DisplayObjects[valueIndex]).Text = Items[i].Value;
                    else
                        DisplayObjects[valueIndex].IsVisible = false;
                    DisplayObjects[selectionIndex].IsVisible = currentItemIndex == i;
                }
                else
                {
                    DisplayObjects[titleIndex].IsVisible = false;
                    DisplayObjects[valueIndex].IsVisible = false;
                    DisplayObjects[selectionIndex].IsVisible = false;
                }
                menuIndex++;
            }

            if (Items.Count < 4)
                DisplayObjects[14].IsVisible = false;
            else
            {
                DisplayObjects[14].IsVisible = true;
                ((LcdGdiScrollBar)DisplayObjects[14]).TotalItems = Items.Count;
                ((LcdGdiScrollBar)DisplayObjects[14]).VisibleItems = 4;
                ((LcdGdiScrollBar)DisplayObjects[14]).CurrentItem = currentItemIndex;
            }
        }

        public Menu(LcdDevice device)
            :this(device,new List<MenuItem>())
        {

        }

        public Menu(LcdDevice device, List<MenuItem> items)
        {
            LCDDevice = device;
            Items = items;
            DisplayObjects = new List<LcdGdiObject>();

            LinearGradientBrush selectedItemBrush = new LinearGradientBrush(new Rectangle(35, 40, 250, 40), Color.FromArgb(180,180,255), Color.FromArgb(20, 20, 240), LinearGradientMode.Vertical);

            Font headerTextFontMedium = new Font("Arial", 11.0f, FontStyle.Regular, GraphicsUnit.Pixel);
            Font headerTextFontLarge = new Font("Arial", 22.0f, FontStyle.Bold, GraphicsUnit.Pixel);


            //0 - Dim Screen
            DisplayObjects.Add(new LcdGdiRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)), new RectangleF(0, 0, 322, 240)));

            //1 - Outline
            DisplayObjects.Add(new LcdGdiRectangle(new SolidBrush(Color.FromArgb(200, 0, 0, 0)), new RectangleF(35, 40, 255, 160)));

            //2 - Selected Highlight 1
            DisplayObjects.Add(new LcdGdiRectangle(selectedItemBrush,new RectangleF(35,40,250,40)));

            //3 - First menu item name
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = Brushes.White,
                Font = headerTextFontMedium,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Left,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Margin = new MarginF(40,54,120,160),
                Text = "First Menu Item"
            });

            //4 - First menu item value
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = Brushes.LightGray,
                Font = headerTextFontMedium,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Right,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Margin = new MarginF(200, 54, 40, 160),
                Text = "Value"
            });

            //5 - Selected Highlight 2
            DisplayObjects.Add(new LcdGdiRectangle(selectedItemBrush, new RectangleF(35, 80, 250, 40)));
            
            //6 - First menu item name
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = Brushes.White,
                Font = headerTextFontMedium,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Left,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Margin = new MarginF(40, 94, 120, 120),
                Text = "First Menu Item"
            });

            //7 - First menu item value
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = Brushes.LightGray,
                Font = headerTextFontMedium,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Right,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Margin = new MarginF(200, 94, 40, 120),
                Text = "Value"
            });


            //8 - Selected Highlight 1
            DisplayObjects.Add(new LcdGdiRectangle(selectedItemBrush, new RectangleF(35, 120, 250, 40)));

            //9 - First menu item name
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = Brushes.White,
                Font = headerTextFontMedium,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Left,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Margin = new MarginF(40, 134, 120, 80),
                Text = "First Menu Item"
            });

            //10 - First menu item value
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = Brushes.LightGray,
                Font = headerTextFontMedium,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Right,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Margin = new MarginF(200, 134, 40, 80),
                Text = "Value"
            });


            //11 - Selected Highlight 1
            DisplayObjects.Add(new LcdGdiRectangle(selectedItemBrush, new RectangleF(35, 160, 250, 40)));

            //12 - First menu item name
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = Brushes.White,
                Font = headerTextFontMedium,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Left,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Margin = new MarginF(40, 174, 120, 40),
                Text = "First Menu Item"
            });

            //13 - First menu item value
            DisplayObjects.Add(new LcdGdiText()
            {
                Brush = Brushes.LightGray,
                Font = headerTextFontMedium,
                HorizontalAlignment = LcdGdiHorizontalAlignment.Right,
                VerticalAlignment = LcdGdiVerticalAlignment.Top,
                Margin = new MarginF(200, 174, 40, 40),
                Text = "Value"
            });

            //14 - Scroll Bar
            DisplayObjects.Add(new LcdGdiScrollBar()
            {
                Pen = Pens.Transparent,
                ThumbBarBrush = new LinearGradientBrush(new Rectangle(285,40,5,160), Color.FromArgb(180,180,255), Color.FromArgb(20, 20, 240),LinearGradientMode.Horizontal),
                Brush = Brushes.Transparent,
                VisibleItems = 4,
                CurrentItem = 0,
                TotalItems = 4,
                Margin = new MarginF(285,40,30,40),
                Size = new SizeF(5,160)
            });

            Visible = false;

            LCDDevice.SoftButtonsChanged += new EventHandler<LcdSoftButtonsEventArgs>(LCDDevice_SoftButtonsChanged);
        }

        public void ShowMenu()
        {
            Visible = true;
            Update();
        }

        public void HideMenu()
        {
            Visible = false;
        }

        void LCDDevice_SoftButtonsChanged(object sender, LcdSoftButtonsEventArgs e)
        {
            if (Visible)
            {
                if (e.SoftButtons == LcdSoftButtons.Up)
                    PreviousItem();
                else if (e.SoftButtons == LcdSoftButtons.Down)
                    NextItem();
                else if (e.SoftButtons == LcdSoftButtons.Right)
                {
                    if (!CurrentItem.IsButton)
                        CurrentItem.NextValue();
                    Update();
                }
                else if (e.SoftButtons == LcdSoftButtons.Left)
                {
                    if (!CurrentItem.IsButton)
                        CurrentItem.PreviousValue();
                    Update();
                }
                else if (e.SoftButtons == LcdSoftButtons.Cancel)
                    HideMenu();
                else if (e.SoftButtons == LcdSoftButtons.Ok)
                {
                    if (CurrentItem.IsButton)
                    {
                        CurrentItem.OnItemClicked();
                        HideMenu();
                    }

                }
            }
            else if (e.SoftButtons == LcdSoftButtons.Menu)
                ShowMenu();
        }
    }

    class MenuItem
    {
        public MenuItem(string text) :
            this(text, "")
        { }

        public MenuItem(string text, string value) :
            this(text, value, new string[0])
        { }

        public MenuItem(string text, string value, string[] values)
        {
            Text = text;
            Value = value;

            Values = new List<string>();
            if(values != null && values.Length > 0)
                Values.AddRange(values);
        }

        public string Text
        { get; set; }

        public string Value
        { get; set; }

        public bool Selected
        { get; set; }

        public List<string> Values
        { get; set; }

        public bool IsButton
        { get { return Values.Count == 0; } }

        public event EventHandler ItemClicked = null;

        public event EventHandler ItemSelected = null;

        public event EventHandler ValueChanged = null;

        public void OnItemClicked()
        {
            if (ItemClicked != null)
                ItemClicked(this, new EventArgs());
        }

        public void OnItemSelected()
        {
            if (ItemSelected != null)
                ItemSelected(this, new EventArgs());
        }

        public void OnValueChanged()
        {
            if (ValueChanged != null)
                ValueChanged(this, new EventArgs());
        }

        public void NextValue()
        {
            if (Values.Count == 0)
                return;

            int current = Math.Max(Values.IndexOf(Value),0);

            if (current == -1)
            {
                Value = Values[0];
            }

            if (current >= Values.Count - 1)
                Value = Values[0];
            else
                Value = Values[current + 1];
            OnValueChanged();
        }

        public void PreviousValue()
        {
            if (Values.Count == 0)
                return;

            int current = Math.Max(Values.IndexOf(Value), 0);


            if (current == -1)
            {
                Value = Values[0];
            }

            if (current <= 0)
                Value = Values[Values.Count - 1];
            else
                Value = Values[current - 1];
            OnValueChanged();
        }
    }
}
