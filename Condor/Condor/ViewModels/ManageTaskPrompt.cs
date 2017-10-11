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

using Condor.Models;

namespace Condor.ViewModels
{
    class ManageTaskPrompt : Prompt
    {
        public enum BtnStatus {ASSIGN,DEASSIGN};

        public event EventHandler OnAssign;
        public event EventHandler OnDeassign;
        public event EventHandler OnDelete;

        private Entry m_txtTitle;
        private Button m_btnAssDeass;
        private Picker m_mbrPicker;
        private DatePicker m_dp;
        private Entry m_txtDesc;
        private Button m_btnDelete;
        public ManageTaskPrompt(Project p)
        {
            m_txtTitle = new Entry() { IsVisible=false,Placeholder="Name" };
            m_btnAssDeass = new Button() { Text = "Assign Task" };
            m_mbrPicker = new Picker() { ItemDisplayBinding=new Binding("ID"),IsVisible=false,Title="Select a Member" };
            if (p != null)
                m_mbrPicker.ItemsSource = p.Members;
            m_dp = new DatePicker() {};
            m_txtDesc = new Entry() { Placeholder="description" };
            m_btnDelete = new Button() { Text="Delete",BackgroundColor=Color.Red };

            m_btnAssDeass.Clicked += new EventHandler((o, e) =>
            {
                if (Status == BtnStatus.ASSIGN)
                {
                    m_mbrPicker.IsVisible = true;
                }
                else if (Status == BtnStatus.DEASSIGN)
                    this.OnDeassign(o, e);
            });

            m_mbrPicker.SelectedIndexChanged += new EventHandler((o, e) =>
            {
                if (this.OnAssign != null)
                    this.OnAssign(o, e);
            });

            m_btnDelete.Clicked += new EventHandler((o, e) =>
            {
                if (this.OnDelete != null)
                    this.OnDelete(o, e);
            });

            AddView(m_txtTitle);
            AddView(m_btnAssDeass);
            AddView(m_mbrPicker);
            AddView(m_dp);
            AddView(m_txtDesc);
            AddView(m_btnDelete);
        }

        public void HidePicker()
        {
            m_mbrPicker.IsVisible = false;
        }

        public bool CanSetTitle
        {
            get
            {
                return m_txtTitle.IsVisible;
            }
            set
            {
                m_txtTitle.IsVisible = value;
            }
        }

        public bool CanAssign
        {
            get
            {
                return m_btnAssDeass.IsVisible;
            }
            set
            {
                m_btnAssDeass.IsVisible = value;
            }
        }

        public bool CanDelete
        {
            get
            {
                return m_btnDelete.IsVisible;
            }
            set
            {
                m_btnDelete.IsVisible = value;
            }
        }

        public bool CanSetDate
        {
            get
            {
                return m_dp.IsVisible;
            }
            set
            {
                m_dp.IsVisible = value;
            }
        }

        public string TaskTitle
        {
            get
            {
                return m_txtTitle.Text;
            }
            set
            {
                m_txtTitle.Text = value;
            }
        }

        public DateTime Date
        {
            get
            {
                DateTime d = new DateTime(m_dp.Date.Year, m_dp.Date.Month, m_dp.Date.Day, 0, 0, 0, DateTimeKind.Local);
                return d;
            }
            set
            {
                m_dp.Date = new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Local);
            }
        }

        public BtnStatus Status
        {
            get
            {
                return m_btnAssDeass.Text == "Assign Task" ? BtnStatus.ASSIGN : BtnStatus.DEASSIGN;
            }
            set
            {
                m_btnAssDeass.Text = (value == BtnStatus.ASSIGN) ? "Assign Task" : "Deassign Task";
            }
        }

        public Member AssignedMember
        {
            get
            {
                return m_mbrPicker.SelectedItem as Member;
            }
            set
            {
                m_mbrPicker.SelectedItem = value;
            }
        }

        public string Description
        {
            get
            {
                return m_txtDesc.Text;
            }
            set
            {
                m_txtDesc.Text = value;
            }
        }
    }
}
