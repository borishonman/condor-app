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
    class TextPrompt : Prompt
    {
        private Entry m_field;
        
        public TextPrompt()
        {
            m_field = new Entry();
            this.AddView(m_field); 
        }

        public Keyboard Keyboard
        {
            get
            {
                return m_field.Keyboard;
            }
            set
            {
                m_field.Keyboard = value;
            }
        }
        public string Text
        {
            get
            {
                return m_field.Text;
            }
            set
            {
                m_field.Text = value;
            }
        }
        public string Hint
        {
            get
            {
                return m_field.Placeholder;
            }
            set
            {
                m_field.Placeholder = value;
            }
        }
    }
}
