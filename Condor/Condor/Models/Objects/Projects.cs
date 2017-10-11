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
    public class Projects
    {
        private List<Project> m_projects;

        public Projects()
        {
            m_projects = new List<Project>();
        }
        public void Parse(JObject resp)
        {
            JObject projs = (JObject)resp.GetValue("projs");
            foreach (KeyValuePair<string,JToken> kvp in projs)
            {
                string id = kvp.Key;
                JObject proj = (JObject)kvp.Value;
                Project newProj = new Project(id);
                newProj.Parse(proj);
                m_projects.Add(newProj);
            }
        }
        public Project GetProjectById(String id)
        {
            foreach (Project p in m_projects)
            {
                if (p.ID.Equals(id))
                    return p;
            }
            return null;
        }
        public void AppendProject(Project p)
        {
            m_projects.Add(p);
        }
        public void DeleteProject(Project p)
        {
            m_projects.Remove(p);
        }

        public List<Project> ProjectList
        {
            get
            {
                return m_projects;
            }
        }
    }
}
