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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Condor.Helpers;

using Condor.Models;
using System.Collections.ObjectModel;

using Condor.ViewModels;

using Condor.Models.API.CommandModels;
using Condor.Models.API.CommandModels.Project.Commands;
using Condor.Models.API.CommandModels.Member.Commands;
using Condor.Models.API.CommandModels.Permission.Commands;

namespace Condor.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ProjectPage : ContentPage
	{
        private MasterDetailPage m_mdp;

        public Project m_proj;
        private Member m_me;

        private Label m_txtDesc;
        private ListView m_lstMembers;
        private ListView m_lstMyTasks;
        private ListView m_lstAllTasks;
        private ToolbarItem m_mnuDelProj;
        private ToolbarItem m_mnuAddMember;
        private ToolbarItem m_mnuCreateTask;

        //permissions
        private bool m_permManageMembers;
        private bool m_permManageTasks;

		public ProjectPage (MasterDetailPage mdp, Project p, string my_username)
		{
			InitializeComponent ();

            m_mdp = mdp;
            m_proj = p;

            m_txtDesc = this.FindByName<Label>("txt_project_description");
            m_lstMembers = this.FindByName<ListView>("lay_members");
            m_lstMyTasks = this.FindByName<ListView>("lay_my_tasks");
            m_lstAllTasks = this.FindByName<ListView>("lay_all_tasks");
            m_mnuDelProj = this.ToolbarItems[0];
            m_mnuAddMember = this.ToolbarItems[1];
            m_mnuCreateTask = this.ToolbarItems[2];

            //set the event handler for toolbar menu items clicked
            m_mnuDelProj.Clicked += ToolbarClicked;
            m_mnuAddMember.Clicked += ToolbarClicked;
            m_mnuCreateTask.Clicked += ToolbarClicked;

            //set up the members listview event listener
            m_lstMembers.ItemSelected += ListItemSelected;

            //set up the my tasks listview event listener
            m_lstMyTasks.ItemSelected += ListItemSelected;

            //set up the all tasks listview event listener
            m_lstAllTasks.ItemSelected += ListItemSelected;

            //empty the toolbar menu so we can re-add the things we can use later
            this.ToolbarItems.Clear();

            //check and save project-based permissions
            //only check task, we are checking member further down so just set the local var there
            APICommand cmd = new CanManageTasksCommand() { Project = p.ID };
            cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
            {
                m_permManageTasks = (bool)response.GetValue("haspermission");
            });
            CondorAPI.getInstance().Execute(cmd);

            //check permissions to determine what to show in context menu
            //delete project permission
            cmd = new CanDeleteProjectCommand() { Project = p.ID };
            cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
            {
                if ((bool)response.GetValue("haspermission"))
                {
                    this.ToolbarItems.Insert(0, m_mnuDelProj);
                }
            });
            CondorAPI.getInstance().Execute(cmd);
            //add member/create task permission (project manager)
            cmd = new CanManageMembersCommand() { Project = p.ID };
            cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
            {
                if ((bool)response.GetValue("haspermission"))
                {
                    this.ToolbarItems.Insert(1, m_mnuAddMember);
                    this.ToolbarItems.Insert(2, m_mnuCreateTask);
                }
                m_permManageMembers = (bool)response.GetValue("haspermission");
            });
            CondorAPI.getInstance().Execute(cmd);

            //set the description
            m_txtDesc.Text = p.Description;

            //get myself in the project
            m_me = p.GetMemberByName(my_username);

            ReloadProjectDisplay();
        }

        private void ReloadProjectDisplay()
        {
            //build the list of members
            m_lstMembers.ItemsSource = new ObservableCollection<Member>(m_proj.Members);

            //build the list of my tasks
            List<Condor.Models.Task> myTasks = new List<Models.Task>();
            foreach (Condor.Models.Task t in m_proj.Tasks)
            {
                if (t.Assigned != null && t.Assigned.Equals(m_me))
                    myTasks.Add(t);
            }
            m_lstMyTasks.ItemsSource = new ObservableCollection<Models.Task>(myTasks);

            //build the list of tasks
            m_lstAllTasks.ItemsSource = new ObservableCollection<Models.Task>(m_proj.Tasks);
        }

        private void ListItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ListView senderLV = (ListView)sender;
            object tappedObj = e.SelectedItem;

            //don't allow items to be selected
            senderLV.SelectedItem = null;

            //this will be called again when we clear the SelectedItem, so this is here to skip that call
            if (tappedObj == null)
                return;

            if (senderLV == m_lstMembers && m_permManageMembers)
            { //member list triggered this event
                Member tappedMember = tappedObj as Member;
                //can't manage myself
                if (tappedMember == m_me)
                    return;
                ManageMemberPrompt p = new ManageMemberPrompt();
                if (tappedMember.Role == Role.MEMBER)
                    p.State = ManageMemberPrompt.BtnState.PROMOTE;
                else if (tappedMember.Role == Role.MANAGER)
                    p.State = ManageMemberPrompt.BtnState.DEMOTE;
                p.OnPromote += new EventHandler((o, e2) =>
                {
                    PromoteMemberCommand cmd = new PromoteMemberCommand() { Project = m_proj.ID, Member = tappedMember.ID };
                    cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                    {
                        if ((string)response.GetValue("result") != "success")
                        {
                            DependencyService.Get<INotification>().ShortAlert("ERROR: "+(string)response.GetValue("msg"));
                            return;
                        }
                        tappedMember.Role = Role.MANAGER;
                        p.State = ManageMemberPrompt.BtnState.DEMOTE;
                        ReloadProjectDisplay();
                    });
                    CondorAPI.getInstance().Execute(cmd);
                });
                p.OnDemote += new EventHandler((o, e2) =>
                {
                    DemoteMemberCommand cmd = new DemoteMemberCommand() { Project = m_proj.ID, Member = tappedMember.ID };
                    cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                    {
                        if ((string)response.GetValue("result") != "success")
                        {
                            DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                            return;
                        }
                        tappedMember.Role = Role.MEMBER;
                        p.State = ManageMemberPrompt.BtnState.PROMOTE;
                        ReloadProjectDisplay();
                    });
                    CondorAPI.getInstance().Execute(cmd);
                });
                p.OnDelete += new EventHandler((o, e2) =>
                {
                    Prompt pc = new Prompt() { PromptTitle="Are you sure?",PositiveButtonText="Yes",NegativeButtonText="No" };
                    pc.OnPromptSaved += new Prompt.PromptClosedEventListener(() =>
                    {
                        DeleteMemberCommand cmd = new DeleteMemberCommand() { Project = m_proj.ID, Member = tappedMember.ID };
                        cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                        {
                            if ((string)response.GetValue("result") != "success")
                            {
                                DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                                return;
                            }
                            m_proj.Members.Remove(tappedMember);
                            ReloadProjectDisplay();
                            pc.Dismiss();
                            p.Dismiss();
                        });
                        CondorAPI.getInstance().Execute(cmd);
                    });
                    pc.Show(m_mdp);
                });
                p.Show(m_mdp);
            }
            else if (senderLV == m_lstMyTasks)
            { //my tasks list triggered this event
                Models.Task tappedTask = tappedObj as Models.Task;
                ManageMyTaskPrompt p = new ManageMyTaskPrompt(tappedTask);
                p.OnPromptSaved += new Prompt.PromptClosedEventListener(() =>
                {
                    //do nothing if nothing was changed
                    if (p.SelectedStatus == tappedTask.Status)
                        return;
                    //execute the command to set the task status
                    SetTaskStatusCommand cmd = new SetTaskStatusCommand() { Project = m_proj.ID, Task = tappedTask.Name, Status = p.SelectedStatus.str };
                    cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                    {
                        if ((string)response.GetValue("result") != "success")
                        {
                            DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                            return;
                        }
                        //update the status and reload the display
                        tappedTask.Status = p.SelectedStatus;
                        ReloadProjectDisplay();
                    });
                    CondorAPI.getInstance().Execute(cmd);
                });
                p.Show(m_mdp);
            }
            else if (senderLV == m_lstAllTasks && m_permManageTasks)
            { //all tasks list triggered this event
                Models.Task tappedTask = tappedObj as Models.Task;
                ManageTaskPrompt p = new ManageTaskPrompt(m_proj);
                p.Date = tappedTask.Due;
                p.Description = tappedTask.Description;
                p.Status = (tappedTask.Assigned == null) ? ManageTaskPrompt.BtnStatus.ASSIGN : ManageTaskPrompt.BtnStatus.DEASSIGN;
                p.AssignedMember = tappedTask.Assigned;
                p.OnAssign += new EventHandler((o, e2) =>
                {
                    AssignTaskCommand cmd = new AssignTaskCommand() { Project=m_proj.ID,Task=tappedTask.Name,Member=p.AssignedMember.ID };
                    cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                    {
                        if ((string)response.GetValue("result") != "success")
                        {
                            DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                            return;
                        }
                        //update the dialog
                        p.HidePicker();
                        p.Status = ManageTaskPrompt.BtnStatus.DEASSIGN;
                        //update the task and reload the display
                        tappedTask.Assigned = p.AssignedMember;
                        ReloadProjectDisplay();
                    });
                    CondorAPI.getInstance().Execute(cmd);
                });
                p.OnDeassign += new EventHandler((o, e2) =>
                {
                    DeassignTaskCommand cmd = new DeassignTaskCommand() { Project=m_proj.ID,Task=tappedTask.Name };
                    cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                    {
                        if ((string)response.GetValue("result") != "success")
                        {
                            DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                            return;
                        }
                        //update the dialog
                        p.Status = ManageTaskPrompt.BtnStatus.ASSIGN;
                        //update the task and reload the display
                        tappedTask.Assigned = null;
                        ReloadProjectDisplay();
                    });
                    CondorAPI.getInstance().Execute(cmd);
                });
                p.OnDelete += new EventHandler((o, e2) =>
                {
                    DeleteTaskCommand cmd = new DeleteTaskCommand() { Project=m_proj.ID,Title=tappedTask.Name };
                    cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                    {
                        if ((string)response.GetValue("result") != "success")
                        {
                            DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                            return;
                        }
                        //remove the task from the list
                        m_proj.Tasks.Remove(tappedTask);
                        //reload the display
                        ReloadProjectDisplay();
                        //close the prompt
                        p.Dismiss();
                    });
                    CondorAPI.getInstance().Execute(cmd);
                });
                p.OnPromptSaved += new Prompt.PromptClosedEventListener(() =>
                {
                    //update the due date if changed
                    if (p.Date.ToFileTimeUtc() != tappedTask.Due.ToFileTimeUtc())
                    {
                        SetTaskDueCommand cmd = new SetTaskDueCommand() { Project = m_proj.ID, Task = tappedTask.Name, Due = p.Date };
                        cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                        {
                            if ((string)response.GetValue("result") != "success")
                            {
                                DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                                return;
                            }
                            //update the due date
                            tappedTask.Due = p.Date;
                            //reload the display
                            ReloadProjectDisplay();
                        });
                        CondorAPI.getInstance().Execute(cmd);
                    }
                    //update the description if changed
                    if (p.Description != tappedTask.Description)
                    {
                        SetTaskDescCommand cmd = new SetTaskDescCommand() { Project = m_proj.ID, Task = tappedTask.Name, Description = p.Description };
                        cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                        {
                            if ((string)response.GetValue("result") != "success")
                            {
                                DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                                return;
                            }
                            //update the description
                            tappedTask.Description = p.Description;
                            //reload the display
                            ReloadProjectDisplay();
                        });
                        CondorAPI.getInstance().Execute(cmd);
                    }
                });
                p.Show(m_mdp);
            }
        }

        private void ToolbarClicked(object sender, EventArgs e)
        {
            ToolbarItem tappedItem = sender as ToolbarItem;

            if (tappedItem == m_mnuDelProj)
            {
                Prompt p = new Prompt() { PromptTitle="Are you sure?",PositiveButtonText="Yes",NegativeButtonText="No" };
                p.OnPromptSaved += new Prompt.PromptClosedEventListener(() =>
                {
                    DeleteCommand cmd = new DeleteCommand() { Title=m_proj.ID };
                    cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                    {
                        if ((string)response.GetValue("result") != "success")
                        {
                            DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                            return;
                        }
                        //update the project list
                        ((MainPage)m_mdp).DeleteProject(m_proj);
                        //go back to the home page
                        ((MainPage)m_mdp).DisplayHome();
                    });
                    CondorAPI.getInstance().Execute(cmd);
                });
                p.Show(m_mdp);
            }
            else if (tappedItem == m_mnuAddMember)
            {
                TextPrompt p = new TextPrompt() { PromptTitle="Enter a Mattermost User ID",Hint="user_id",PositiveButtonText="Add",NegativeButtonText="Cancel",Keyboard=Keyboard.Plain };
                p.OnPromptSaved += new Prompt.PromptClosedEventListener(() =>
                {
                    AddMemberCommand cmd = new AddMemberCommand() { Project=m_proj.ID,Member=p.Text };
                    cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                    {
                        if ((string)response.GetValue("result") != "success")
                        {
                            DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                            return;
                        }
                        //update the task with the new member
                        Member newMember = new Member() { Role=Role.MEMBER,ID=p.Text };
                        m_proj.Members.Add(newMember);
                        //reload the project display
                        ReloadProjectDisplay();
                    });
                    CondorAPI.getInstance().Execute(cmd);
                });
                p.Show(m_mdp);
            }
            else if (tappedItem == m_mnuCreateTask)
            {
                ManageTaskPrompt p = new ManageTaskPrompt(m_proj) { CanAssign=false,CanSetTitle=true,CanDelete=false,PositiveButtonText="Create",NegativeButtonText="Cancel" };
                p.OnPromptSaved += new Prompt.PromptClosedEventListener(() =>
                {
                    CreateTaskCommand cmd = new CreateTaskCommand() { Project=m_proj.ID,Title=p.TaskTitle,Due=p.Date,Description=p.Description };
                    cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                    {
                        if ((string)response.GetValue("result") != "success")
                        {
                            DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                            return;
                        }
                        //update the project with the new Task
                        Models.Task t = new Models.Task() { Name=p.TaskTitle,Due=p.Date,Description=p.Description };
                        m_proj.Tasks.Add(t);
                        //reload the project display
                        ReloadProjectDisplay();
                    });
                    CondorAPI.getInstance().Execute(cmd);
                });
                p.Show(m_mdp);
            }
        }
	}
}