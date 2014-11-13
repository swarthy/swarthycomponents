using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using SwarthyComponents.UI;
using System.Collections.Generic;

namespace SwarthyComponents.UI
{
    public partial class RegExTextBox : TextBox
    {
        private string regExpression = "*";
        private Regex expression;
        public ValidationPatterns Pattern { get; set; }
        public string RegularExpression
        {
            get
            {
                return regExpression;
            }
            set
            {
                regExpression = value;                
            }
        }
        private string getPatternStr(ValidationPatterns pattern)
        {
            switch (pattern)
            {
                case ValidationPatterns.Float:
                    return @"^-?[0-9]*(?:\,-?[0-9]*)?$";
                case ValidationPatterns.URL:
                    return @"/^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$/";
                case ValidationPatterns.Custom:
                default:
                    return RegularExpression;
            }
        }
        public RegExTextBox()
            : base()
        {
            
        }
        public void InitExpression()
        {
            try
            {
                expression = new Regex(getPatternStr(Pattern));
            }
            catch
            {
                MessageBox.Show("Ошибка инициализации RegExTextBox", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public bool ValidateControl()
        {
            if (expression == null)
                InitExpression();
            return expression.IsMatch(Text);
        }
    }
    public enum ValidationPatterns
    {
        Custom, Float, URL, Integer
    }
}
