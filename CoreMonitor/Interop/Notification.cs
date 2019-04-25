using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace CoreMonitor.Interop
{
    class Notification : EventArgs
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public Bitmap Image { get; set; }
        public int DisplayPeriod { get; set; }

        public string Base64PNG
        {
            get
            {
                if (HasImage)
                {
                    MemoryStream ms = new MemoryStream();
                    Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                    return Convert.ToBase64String(ms.ToArray());
                }
                return null;
            }
        }


        public bool HasImage
        {
            get{ return Image != null && Image.Width > 1 && Image.Height > 1;}
        }

        public Notification(string title, string text)
            : this(title,text,new Bitmap(1,1))
        {
        }

        public Notification(string title, string text, int displayPeriod)
            : this(title, text, new Bitmap(1,1), displayPeriod)
        { }

        public Notification(string title, string text, Bitmap image)
            : this(title,text,image,2000)
        {
        }

        public Notification(string title, string text, string base64image)
            : this(title, text, base64image, 2000)
        { }

        public Notification(string title, string text, string base64image, int displayPeriod)
        {
            Title = title;
            Text = text;

            MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64image));
            Bitmap img = (Bitmap)Bitmap.FromStream(ms);
            ms.Close();

            Image = img;
            DisplayPeriod = displayPeriod;
        }

        public Notification(string title, string text,Bitmap image, int displayPeriod)
        {
            Title = title;
            Text = text;
            Image = image;
            DisplayPeriod = displayPeriod;
        }
    }
}
