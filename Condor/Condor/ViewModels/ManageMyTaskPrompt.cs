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
    class ManageMyTaskPrompt : Prompt
    {
        private Picker m_statPikr;
        public ManageMyTaskPrompt(Task t)
        {
            m_statPikr = new Picker() { ItemsSource = TaskStatus.Stats };
            m_statPikr.ItemDisplayBinding = new Binding("str");
            m_statPikr.SelectedItem = t.Status;
            AddView(m_statPikr);
        }

        public TaskStatus SelectedStatus
        {
            get
            {
                return (TaskStatus)m_statPikr.SelectedItem;
            }
        }
    }
}
