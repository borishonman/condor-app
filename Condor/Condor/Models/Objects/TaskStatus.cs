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

namespace Condor.Models
{
    public class TaskStatus
    {
        private String m_status;

        public static TaskStatus NO_WORK_DONE = new TaskStatus("No Work Done");
        public static TaskStatus IN_PROGRESS = new TaskStatus("In Progress");
        public static TaskStatus DONE = new TaskStatus("Done");

        private static TaskStatus[] m_stats =
        {
            NO_WORK_DONE, IN_PROGRESS, DONE
        };

        private TaskStatus(String stat)
        {
            this.m_status = stat;
        }
        public static TaskStatus FromString(string str)
        {
            foreach (TaskStatus r in m_stats)
            {
                if (r.str.Equals(str))
                    return r;
            }
            return null;
        }

        public static List<TaskStatus> Stats
        {
            get
            {
                return new List<TaskStatus>(m_stats);
            }
        }

        public String str
        {
            get
            {
                return m_status;
            }
        }
        public static bool operator ==(TaskStatus ts1, TaskStatus ts2) => (ts1.str.Equals(ts2.str));
        public static bool operator !=(TaskStatus ts1, TaskStatus ts2) => (!ts1.str.Equals(ts2.str));

        public override bool Equals(Object ts)
        {
            TaskStatus ts2 = (TaskStatus)ts;
            return this.str.Equals(ts2.str);
        }

        public override int GetHashCode()
        {
            return this.str.GetHashCode();
        }
    }
}
