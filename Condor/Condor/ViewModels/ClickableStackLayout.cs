/*
    This file is part of Condor.
    Condor is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    Condor is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with Condor.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Condor.ViewModels
{
    public class ClickableStackLayout : StackLayout
    {
        private Dictionary<EventHandler, TapGestureRecognizer> m_map;
        private Label m_label;

        public ClickableStackLayout()
        {
            m_map = new Dictionary<EventHandler, TapGestureRecognizer>();
            m_label = new Label() { FontSize=12 };
            this.Enabled = true;
            this.Children.Add(m_label);
        }
        public event EventHandler Clicked
        {
            add
            {
                TapGestureRecognizer rec = new TapGestureRecognizer();
                rec.Tapped += value;
                this.GestureRecognizers.Add(rec);
                m_map.Add(value, rec);
            }
            remove
            {
                this.GestureRecognizers.Remove(m_map[value]);
                m_map.Remove(value);
            }
        }
        public bool Enabled
        {
            get
            {
                return this.IsEnabled;
            }
            set
            {
                this.IsEnabled = value;
                m_label.TextColor = value ? Color.Black : Color.DarkGray;
                this.GestureRecognizers.Clear();
                if (value)
                {
                    foreach (KeyValuePair<EventHandler, TapGestureRecognizer> kvp in m_map)
                    {
                        this.GestureRecognizers.Add(kvp.Value);
                    }
                }
            }
        }

        public string Text
        {
            get
            {
                return m_label.Text;
            }
            set
            {
                m_label.Text = value;
            }
        }
    }
}
