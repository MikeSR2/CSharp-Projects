using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace WhiteBoard
{
    class KinectCursor
    {
        
        public Point Location { get; set; }

        public Size Size { get; set; }

        public Cursor Cursor { get; set; }

        public KinectCursor()
        { 
        }
        public void MoveCursor(int x, int y)
        {
            Cursor = new Cursor(Cursor.Current.Handle);
            Cursor.Position = new Point(x, y);
            Cursor.Clip = new Rectangle(this.Location, this.Size);
        }
    }
}
