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
    public class Project
    {
        private List<Member> m_members;
        private List<Task> m_tasks;
        private String m_id;
        private String m_title;
        private String m_desc;
        private Member m_creator;

        public Project(String id)
        {
            m_members = new List<Member>();
            m_tasks = new List<Task>();
            m_id = id;
        }

        public void Parse(JObject resp)
        {
            this.Title = (string)resp.GetValue("title");
            this.Description = (string)resp.GetValue("description");
            
            JObject members = (JObject)resp.GetValue("members");
            JArray tasks = (JArray)resp.GetValue("tasks");

            foreach (KeyValuePair<string,JToken> kvp in members)
            {
                JObject m = (JObject)kvp.Value;
                Member newMember = new Member();
                newMember.Parse(m);
                m_members.Add(newMember);
                if (newMember.ID.Equals((string)resp.GetValue("creator")))
                    m_creator = newMember;
            }

            foreach (JToken v in tasks)
            {
                JObject t = (JObject)v;
                Task newTask = new Task();
                newTask.Parse(t, m_members);
                m_tasks.Add(newTask);
            }
        }

        public String ID
        {
            get
            {
                return m_id;
            }
        }
        public String Title
        {
            get
            {
                return m_title;
            }
            set
            {
                m_title = value;
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

        public List<Member> Members
        {
            get
            {
                return m_members;
            }
        }

        public List<Task> Tasks
        {
            get
            {
                return m_tasks;
            }
        }

        public Member GetMemberByIndex(int index)
        {
            if (index + 1 >= m_members.Count)
                return null;

            return m_members[index];
        }
        public Member GetMemberByName(String name)
        {
            foreach (Member m in m_members)
            {
                if (m.ID.Equals(name))
                    return m;
            }
            return null;
        }
    }
}
