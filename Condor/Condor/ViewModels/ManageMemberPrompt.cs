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
    class ManageMemberPrompt : Prompt
    {
        private Button m_btnPromDem;
        private Button m_del;

        public event EventHandler OnPromote;
        public event EventHandler OnDemote;
        public event EventHandler OnDelete;

        public enum BtnState { PROMOTE, DEMOTE };

        public ManageMemberPrompt()
        {
            m_btnPromDem = new Button() { Text = "Promote to Project Manager" };
            m_del = new Button() { Text="Delete",BackgroundColor=Color.Red };

            m_btnPromDem.Clicked += new EventHandler((o, e) =>
            {
                if (this.State == BtnState.PROMOTE)
                    this.OnPromote(o, e);
                else if (this.State == BtnState.DEMOTE)
                    this.OnDemote(o, e);
            });
            m_del.Clicked += new EventHandler((o, e) =>
            {
                this.OnDelete(o, e);
            });

            PositiveButtonText = "Close";
            NegativeButtonVisible = false;

            this.AddView(m_btnPromDem);
            this.AddView(m_del);
        }

        public BtnState State
        {
            get
            {
                return (m_btnPromDem.Text == "Promote to Project Manager") ? BtnState.PROMOTE : BtnState.DEMOTE;
            }
            set
            {
                if (value == BtnState.PROMOTE)
                {
                    m_btnPromDem.Text = "Promote to Project Manager";
                }
                else if (value == BtnState.DEMOTE)
                {
                    m_btnPromDem.Text = "Demote to Memebr";
                }
            }
        }
    }
}
