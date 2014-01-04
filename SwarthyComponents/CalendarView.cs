//Author: Alexander Mochalin
//Copyright (c) 2013-2014 All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace SwarthyComponents.UI
{
    public class CalendarView:DrawPanel
    {
        public EventHandler OnSelectedDateChanged { get; set; }
        List<DateTime> DaysForDraw = new List<DateTime>();
        public Dictionary<DateTime, StyleSettings> FormatedDate = new Dictionary<DateTime, StyleSettings>();        
        int month, year;
        DateTime date = DateTime.MinValue;
        public int Month { get { return month; } set { month = value; date = new DateTime(year, month, 1); GenerateDays(); } }
        public int Year { get { return year; } set { year = value; date = new DateTime(year, month, 1); GenerateDays(); } }
        public void SetDate(int month, int year)
        {
            this.month = month;
            this.year = year;
            date = new DateTime(year, month, 1);
            GenerateDays();
        }
        public Size DaySize { get; set; }
        public int DayPadding { get; set; }
        public int MarginTop { get; set; }
        public int MarginLeft { get; set; }
        public bool DrawTitle { get; set; }
        public DateTime OnMouseDate {get; private set;}
        public DateTime SelectedDate { get; set; }
        public StyleSettings HoverDay { get; set; }
        public StyleSettings SelectedDay { get; set; }
        Point MousePos= new Point(-1, -1);
        StringFormat format = new StringFormat();
        public static StyleSettings DefaultDayStyle { get; set; }
        public CalendarView() : base() {
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            DefaultDayStyle = new StyleSettings(Color.Transparent, Color.Silver, Font, Brushes.Black);
            HoverDay = new StyleSettings(Color.DarkOrange, Color.Red);
            SelectedDay = new StyleSettings(Color.LightBlue, Color.Blue);
            month = DateTime.Today.Month;
            Year = DateTime.Today.Year;            
        }
        public void GenerateDays()
        {
            DaysForDraw.Clear();
            var days = DateTime.DaysInMonth(Year, Month);            
            for (int i = 0; i < days; i++)            
                DaysForDraw.Add(new DateTime(Year, Month, i + 1));
            Refresh();
        }        
        protected override void OnPaint(PaintEventArgs e)
        {
            var graph = e.Graphics;
            int line = 0;
            if (DrawTitle)
                graph.DrawString(date.ToString("MMMM yyyy"), Font, Brushes.Black, new Rectangle(0, 0, DayPadding * 14 + DaySize.Width * 7, (int)Font.Size * 2), format);
            OnMouseDate = DateTime.MinValue;
            DaysForDraw.ForEach(d => {
                var weekPos = (int)(d.DayOfWeek + 6) % 7;//чтобы пнд = 0                      
                var x = weekPos * DaySize.Width + (2 * weekPos + 1) * DayPadding;
                var y = line * DaySize.Height + (2 * line + 1) * DayPadding;
                var r = new Rectangle(x + MarginLeft, y + MarginTop, DaySize.Width, DaySize.Height);
                var style = DefaultDayStyle;
                bool formated = false, selected = false, hover = false;

                if (FormatedDate.ContainsKey(d))//форматирование
                    formated = true;
                if (d == SelectedDate)//выбранная дата
                    selected = true;                    
                if (r.Contains(MousePos))//дата под мышью
                {
                    hover = true;
                    OnMouseDate = d;
                }

                
                if (formated)
                    graph.FillRectangle(FormatedDate[d].BackgroundBrush, r);
                else
                    if (selected)
                        graph.FillRectangle(SelectedDay.BackgroundBrush, r);
                    else
                        if (hover)
                            graph.FillRectangle(HoverDay.BackgroundBrush, r);
                        else
                            graph.DrawRectangle(DefaultDayStyle.BorderPen, r);

                graph.DrawString(d.Day.ToString(), style.Font, style.Brush, r, format);

                if (hover)
                    graph.DrawRectangle(HoverDay.BorderPen, r);
                else
                    if (selected)
                        graph.DrawRectangle(SelectedDay.BorderPen, r);
                    else
                        if (formated)
                            graph.DrawRectangle(FormatedDate[d].BorderPen, r);
                        else
                            graph.DrawRectangle(DefaultDayStyle.BorderPen, r);

                if (weekPos == 6)
                    line++;
            });
            base.OnPaint(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {            
            MousePos = e.Location;            
            base.OnMouseMove(e);
            Refresh();
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (OnMouseDate != DateTime.MinValue)
            {
                SelectedDate = OnMouseDate;
                Refresh();
                if (OnSelectedDateChanged != null)
                    OnSelectedDateChanged(this, EventArgs.Empty);
            }
            base.OnMouseClick(e);
        }
    }
    public class StyleSettings
    {
        public Brush Brush { get; set; }
        public Font Font { get; set; }        
        public Brush BackgroundBrush { get; private set; }        
        public Pen BorderPen { get; private set; }
        public StyleSettings(Color background, Color border, Font font = null, Brush brush = null)
        {            
            BackgroundBrush = new SolidBrush(background);
            BorderPen = new Pen(border);
            Font = font ?? CalendarView.DefaultDayStyle.Font;
            Brush = brush ?? CalendarView.DefaultDayStyle.Brush;
        }
    }
}
