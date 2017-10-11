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
using Newtonsoft.Json.Linq;

namespace Condor.Models
{
    public class Member
    {
        private String m_id;
        private Role m_role;
        private List<Task> m_assignedTasks;

        public Member()
        {
            m_assignedTasks = new List<Task>();
        }

        public void Parse(JObject resp)
        {
            m_id = (string)resp.GetValue("username");
            m_role = Role.FromString((string)resp.GetValue("role"));
        }

        public String ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }
        public Role Role
        {
            get
            {
                return m_role;
            }
            set
            {
                m_role = value;
            }
        }

        public int NumAssignedTasks
        {
            get
            {
                return m_assignedTasks.Count;
            }
        }

        public List<Task> Tasks
        {
            get
            {
                return m_assignedTasks;
            }
        }

        public void AssignTask(Task t)
        {
            if (!m_assignedTasks.Contains(t))
            {
                m_assignedTasks.Add(t);
                if (t.Assigned != this)
                    t.Assigned = this;
            }
        }
        public void DeassignTask(Task t)
        {
            if (m_assignedTasks.Contains(t))
            {
                m_assignedTasks.Remove(t);
                if (t.Assigned != this)
                    t.Assigned = null;
            }
        }
    }
}
