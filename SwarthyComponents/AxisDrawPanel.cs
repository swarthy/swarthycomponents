//Author: Alexander Mochalin
//Copyright (c) 2013-2014 All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SwarthyComponents.UI
{
    public class AxisDrawPanel : DrawPanel
    {
        public Point Center = Point.Empty;
        public SmoothingMode smoothingMode = SmoothingMode.Default;
        public List<DPObject> objects = new List<DPObject>();
        float scaleX = 1, scaleY = 1, dx = 0, dy = 0, angle = 0;
        bool _movable = true;
        internal RectangleF visible_area = new RectangleF();
        public DPAxisX axisX;
        public DPAxisY axisY;
        public float PixelsInUnit = 10;
        Matrix view = new Matrix(), view_inv;

        public bool ShowPosition = true;
        protected Point mousePosition;
        public PointF MouseLocation = PointF.Empty;

        #region Properties
        public float ScaleX
        {
            get
            { return scaleX; }
            set
            {
                scaleX = value;
                UpdateViewMatrix();
            }
        }
        public float ScaleY
        {
            get
            { return scaleY; }
            set
            {
                scaleY = value;
                UpdateViewMatrix();
            }
        }
        new public float Scale
        {
            get
            {
                return (scaleX + scaleY) / 2;
            }
        }
        public float DX
        {
            get
            { return dx; }
            set
            {
                dx = value;
                UpdateViewMatrix();
            }
        }
        public float DY
        {
            get
            { return dy; }
            set
            {
                dy = value;
                UpdateViewMatrix();
            }
        }
        public float Angle
        {
            get
            { return angle; }
            set
            {
                angle = value;
                UpdateViewMatrix();
            }
        }

        public bool Movable
        {
            get
            {
                return _movable;
            }
            set
            {
                _movable = value;
            }
        }
        #endregion

        public AxisDrawPanel()
            : base()
        {
            BackColor = Color.White; DPObject.font = Font;
            axisX = new DPAxisX(this);
            axisY = new DPAxisY(this);
            objects.Add(axisX);
            objects.Add(axisY);
            UpdateViewMatrix();
        }
        protected override void OnCreateControl()
        {
            FindForm().MouseWheel += (s, e) =>
            {
                if (e.Delta != 0 && _movable)
                {
                    var oldZero = GetPosition(new PointF(e.X, e.Y));
                    scaleX += (scaleX * e.Delta) / 800;
                    scaleY += (scaleY * e.Delta) / 800;
                    if (scaleX <= 0.1F)
                        scaleX = 0.1F;
                    if (scaleY <= 0.1F)
                        scaleY = 0.1F;
                    UpdateViewMatrix();
                    var newZero = GetPosition(new PointF(e.X, e.Y));
                    dx += newZero.X - oldZero.X;
                    dy += newZero.Y - oldZero.Y;
                    UpdateViewMatrix();
                    Refresh();
                }
            };
            base.OnCreateControl();
        }
        void UpdateViewMatrix()
        {
            view = new Matrix(1, 0, 0, -1, 0, 0);
            view.Scale(scaleX, scaleY);
            view.Translate(dx, dy);
            view.Rotate(angle);
            view_inv = view.Clone();
            view_inv.Invert();
            var lt = GetPosition(new PointF(0, 0));
            var rb = GetPosition(new PointF(Width, Height));
            visible_area = new RectangleF(lt, new SizeF(rb.X - lt.X, rb.Y - lt.Y));
        }
        public void SetScale(float scaleX, float scaleY)
        {
            this.scaleX = scaleX;
            this.scaleY = scaleY;
            UpdateViewMatrix();
        }
        public void SetScale(float value)
        {
            SetScale(value, value);
        }
        public void SetOffset(float dx, float dy)
        {
            this.dx = dx;
            this.dy = dy;
            UpdateViewMatrix();
        }
        public void SetOffset(float value)
        {
            SetOffset(value, value);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Transform = view;
            e.Graphics.SmoothingMode = smoothingMode;
            foreach (var o in objects)
            {
                o.Draw(e);
            }
            if (ShowPosition)
            {
                e.Graphics.Transform = new Matrix(1, 0, 0, 1, 0, 0);
                MouseLocation = GetPosition(mousePosition);
                string info = string.Format("x: {0}, y: {1}\r\nscaleX: {2}, scaleY: {3}\r\nl:{4} r:{5} t:{6} b:{7}", MouseLocation.X, MouseLocation.Y, scaleX, scaleY, visible_area.Left, visible_area.Right, visible_area.Top, visible_area.Bottom);
                var off = e.Graphics.MeasureString(info, Font);
                e.Graphics.DrawString(info, Font, Brushes.Black, new PointF(2, Height - off.Height - 2));
            }
            base.OnPaint(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool needUpdate = false;
            if (e.Button == System.Windows.Forms.MouseButtons.Right && _movable)
            {
                dx += (e.Location.X - mousePosition.X) / scaleX;
                dy += (mousePosition.Y - e.Location.Y) / scaleY;
                needUpdate = true;
            }
            mousePosition = e.Location;
            if (needUpdate)
                UpdateViewMatrix();
            Refresh();
        }
        public PointF GetPosition(PointF p, bool toUnits = false)
        {
            var arr = new PointF[] { p };
            view_inv.TransformPoints(arr);
            return toUnits ? new PointF(arr[0].X / PixelsInUnit, arr[0].Y / PixelsInUnit) : arr[0];
        }
        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UpdateViewMatrix();
            Refresh();
        }
    }
    public class DPObject
    {
        protected AxisDrawPanel Panel;
        public static Font font;
        protected Color _color = Color.Black;
        protected SolidBrush _brush;
        protected Pen _pen;
        public bool Visible = true;
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _brush = new SolidBrush(value);
                _pen = new Pen(value);
                _color = value;
            }
        }
        public DPObject(AxisDrawPanel panel)
        {
            Panel = panel;
            Color = Color.Black;
        }
        public virtual void Draw(PaintEventArgs e)
        {
            _pen.Width = Panel.Scale < 1 ? 1 : 1 / Panel.Scale;
            if (!Visible)
                return;
        }
    }
    public class DPArea : DPObject
    {
        public bool Fill = true;
        public PointF[] points;
        public bool ShowCenter = false;
        private bool _centered = false;
        private PointF _centeroid;
        public PointF Centeroid
        {
            get
            {
                return _centered ? _centeroid : findCenteroid();
            }
        }
        private PointF findCenteroid()
        {
            float area = 0;
            const float k_inv3 = 1f / 3f;
            for (int i = 0; i < points.Length; i++)
            {
                var p1 = points[i];
                var p2 = points[(i + 1) % points.Length];
                var D = p1.Cross(p2);
                var tArea = 0.5f * D;
                area += tArea;
                _centeroid.X += tArea * k_inv3 * (p1.X + p2.X);
                _centeroid.Y += tArea * k_inv3 * (p1.Y + p2.Y);
            }
            _centeroid.X *= 1f / area;
            _centeroid.Y *= 1f / area;
            _centered = true;
            return _centeroid;
        }
        public DPArea(AxisDrawPanel panel) : base(panel) { }
        public override void Draw(PaintEventArgs e)
        {
            if (!Visible)
                return;
            base.Draw(e);
                        
            if (Fill)
                e.Graphics.FillPolygon(_brush, points);
            else
                e.Graphics.DrawPolygon(_pen, points);
            if (ShowCenter)
            {
                e.Graphics.FillRectangle(Brushes.Red, Centeroid.X, Centeroid.Y, 0.1f, 0.1f);
            }
        }
    }
    public class DPCross : DPObject
    {
        public float Delta = 5;
        public PointF Position;
        public DPCross(AxisDrawPanel panel) : base(panel) { }
        public override void Draw(PaintEventArgs e)
        {
            if (!Visible)
                return;
            base.Draw(e);
            e.Graphics.DrawLine(_pen, Position.X - Delta, Position.Y - Delta, Position.X + Delta, Position.Y + Delta);
            e.Graphics.DrawLine(_pen, Position.X - Delta, Position.Y + Delta, Position.X + Delta, Position.Y - Delta);
        }
    }
    public class DPLine : DPObject
    {
        public PointF Point1, Point2;
        public DPLine(AxisDrawPanel panel) : base(panel) { }
        public override void Draw(PaintEventArgs e)
        {
            if (!Visible)
                return;
            base.Draw(e);
            e.Graphics.DrawLine(_pen, Point1, Point2);
        }
    }
    public class DPPoint : DPObject
    {
        public PointF Position;
        public DPPoint(AxisDrawPanel panel) : base(panel) { }
        public override void Draw(PaintEventArgs e)
        {
            if (!Visible)
                return;
            e.Graphics.FillRectangle(_brush, Position.X, Position.Y, 0.1f, 0.1f);
            base.Draw(e);
        }
        public override string ToString()
        {
            return Position.ToString();
        }
    }
    public class DPAxisX : DPObject
    {
        public bool ShowAxisName = false;
        public DPAxisX(AxisDrawPanel panel) : base(panel) { }
        public override void Draw(PaintEventArgs e)
        {
            if (!Visible)
                return;
            base.Draw(e);
            var copy = Panel.visible_area;
            e.Graphics.DrawLine(_pen, new PointF(copy.Left, 0), new PointF(copy.Right, 0));
            for (float i = copy.Left - (copy.Left % Panel.PixelsInUnit); i <= copy.Right; i += Panel.PixelsInUnit)
                e.Graphics.DrawLine(_pen, new PointF(i, Panel.PixelsInUnit / 4), new PointF(i, -Panel.PixelsInUnit / 4));
            if (ShowAxisName)
                e.Graphics.DrawString("x", font, _brush, new PointF(copy.Right - 15, -20));
        }
    }
    public class DPAxisY : DPObject
    {
        public bool ShowAxisName = false;
        public DPAxisY(AxisDrawPanel panel) : base(panel) { }
        public override void Draw(PaintEventArgs e)
        {
            if (!Visible)
                return;
            base.Draw(e);
            var copy = Panel.visible_area;
            e.Graphics.DrawLine(_pen, new PointF(0, copy.Top), new PointF(0, copy.Bottom));
            for (float i = copy.Bottom - (copy.Bottom % Panel.PixelsInUnit); i <= copy.Top; i += Panel.PixelsInUnit)
                e.Graphics.DrawLine(_pen, new PointF(-Panel.PixelsInUnit / 4, i), new PointF(Panel.PixelsInUnit / 4, i));
            if (ShowAxisName)
                e.Graphics.DrawString("λ", font, _brush, new PointF(10, copy.Top - 20));
        }
    }
    public class DPPath : DPObject
    {
        public List<PointF> Points = new List<PointF>();
        public DPPath(AxisDrawPanel panel) : base(panel) { }
        public override void Draw(PaintEventArgs e)
        {
            if (!Visible)
                return;
            base.Draw(e);
            PointF[] pts = Points.ToArray();
            if (pts.Length < 2)
                return;
            e.Graphics.DrawLines(_pen, pts);
        }
    }
    public class DPFunction : DPObject
    {
        public List<FunctionInfo> PartsOfFunc = new List<FunctionInfo>();
        public DPFunction(AxisDrawPanel panel) : base(panel) { }
        public override void Draw(PaintEventArgs e)
        {
            if (!Visible)
                return;
            base.Draw(e);
            List<PointF> Points = new List<PointF>();
            var copy = Panel.visible_area;
            float steps = 10;
            for (float i = copy.Left / Panel.PixelsInUnit; i <= copy.Right / Panel.PixelsInUnit; i += 1 / (steps * Panel.Scale))
                foreach (var p in PartsOfFunc)
                {
                    if (p.Constr(i))
                    {
                        Points.Add(new PointF(i * Panel.PixelsInUnit, p.y(i) * Panel.PixelsInUnit));
                    }
                }
            if (Points.Count > 1)
                e.Graphics.DrawLines(_pen, Points.ToArray());
        }
    }
    public delegate bool constraint(float x);
    public delegate float fx(float x);
    public struct FunctionInfo
    {
        public constraint Constr;
        public fx y;
        public FunctionInfo(constraint constr, fx f)
        {
            Constr = constr;
            y = f;
        }
    }
    public static class SwarthyExtentions
    {
        public static float Cross(this PointF self, PointF v)
        {            
            return self.X * v.Y - self.Y * v.X;
        }
    }
}
