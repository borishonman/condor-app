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
    public class Task
    {
        private String m_name;
        private Member m_assignedMember;
        private TaskStatus m_status;
        private DateTime m_due;
        private String m_desc;

        public Task()
        {
            m_assignedMember = null;
            m_status = TaskStatus.NO_WORK_DONE;
            /*m_name = name;
            m_assignedMember = assigned;
            if (m_assignedMember != null)
                m_assignedMember.AssignTask(this);
            m_status = status;
            m_due = due;
            m_desc = desc;*/
        }

        public void Parse(JObject resp, List<Member> members)
        {
            m_name = (string)resp.GetValue("name");
            m_desc = (string)resp.GetValue("description");
            m_due = DateTime.Parse((string)resp.GetValue("due"));
            m_status = TaskStatus.FromString((string)resp.GetValue("status"));

            string assignedName = (string)resp.GetValue("assigned");
            foreach (Member m in members)
            {
                if (m.ID.Equals(assignedName))
                    this.Assigned = m;
            }
        }

        public String Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }
        public TaskStatus Status
        {
            get
            {
                return m_status;
            }
            set
            {
                m_status = value;
            }
        }
        public DateTime Due
        {
            get
            {
                return m_due;
            }
            set
            {
                m_due = value;
            }
        }

        public string DueString
        {
            get
            {
                return m_due.ToShortDateString();
            }
        }
        public String Description
        {
            get
            {
                return m_desc;
            }
            set
            {
                m_desc = value;
            }
        }
        public Member Assigned
        {
            get
            {
                return m_assignedMember;
            }
            set
            {
                if (value == null)
                {
                    m_assignedMember.DeassignTask(this);
                    m_assignedMember = value;
                }
                else
                {
                    m_assignedMember = value;
                    bool found = false;
                    foreach (Task t in m_assignedMember.Tasks)
                    {
                        if (t == this)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        m_assignedMember.AssignTask(this);
                }
            }
        }

        public string AssignedID
        {
            get
            {
                if (m_assignedMember == null)
                    return "NOT ASSIGNED";
                else
                    return m_assignedMember.ID;
            }
        }
    }
}
