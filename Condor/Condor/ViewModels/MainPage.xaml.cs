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

using Condor.Models;
using Condor.Helpers;
using Condor.Models.API.CommandModels;
using Condor.Models.API.CommandModels.Auth.Commands;
using Condor.Models.API.CommandModels.Project.Commands;
using Condor.Models.API.CommandModels.Permission.Commands;
using Condor.ViewModels;

using System.Collections.ObjectModel;

namespace Condor.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage
    {
        private Label m_lblCurTeam;
        private Label m_lblCurUserId;

        private ClickableStackLayout m_cellMemberPerms;
        private ClickableStackLayout m_cellCreateProject;
        private StackLayout m_mnuAdminTitle;

        private ClickableStackLayout m_cellLogin;
        private ClickableStackLayout m_cellAddr;

        IConfig m_config;
        private Projects m_projects;
        private string m_myusername;
        private string m_team;

        public MainPage()
        {
            InitializeComponent();
            MasterPage.ListView.ItemSelected += ListView_ItemSelected;

            m_lblCurTeam = MasterPage.RootView.FindByName<Label>("txt_cur_team");
            m_lblCurUserId = MasterPage.RootView.FindByName<Label>("txt_cur_userid");

            m_mnuAdminTitle = MasterPage.RootView.FindByName<StackLayout>("mnu_admin_title");
            m_cellMemberPerms = MasterPage.RootView.FindByName<ClickableStackLayout>("mnu_perms_table");
            m_cellCreateProject = MasterPage.RootView.FindByName<ClickableStackLayout>("mnu_create_project");

            m_cellLogin = MasterPage.RootView.FindByName<ClickableStackLayout>("mnu_login");
            m_cellAddr = MasterPage.RootView.FindByName<ClickableStackLayout>("mnu_addr");

            //get the config class (will be different based on the platform we built)
            m_config = ConfigFactory.GetInstance();

            //set up the login clicked event listener
            m_cellLogin.Clicked += (o, e) =>
            {
                this.IsPresented = false; //hide the sidebar thing

                if (m_cellLogin.Text == "Logout")
                { //if logout button, do logout and skip the login actions
                    LogoutCommand cmd = new LogoutCommand();
                    cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                    {
                        if (((string)response.GetValue("msg") != "" && response.GetValue("msg") != null) || !(bool)response.GetValue("result"))
                        {
                            DependencyService.Get<INotification>().ShortAlert("ERROR: "+(string)response.GetValue("msg"));
                            return;
                        }
                        CompleteLogout();
                    });
                    CondorAPI.getInstance().Execute(cmd);
                    return;
                }
                //execute the login command
                CredsPrompt p = new CredsPrompt() { PromptTitle="Login" };
                p.OnPromptSaved += new Prompt.PromptClosedEventListener(() =>
                {
                    LoginCommand cmd = new LoginCommand() { Username=p.Username,Password=p.Password };
                    cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                    {
                        if ((string)response.GetValue("msg") != "" && response.GetValue("msg") != null)
                        { //check if an error was thrown when contacting the server
                            DependencyService.Get<INotification>().ShortAlert("ERROR: "+(string)response.GetValue("msg"));
                            return;
                        }
                        if (response.GetValue("token") == null)
                        { //if token is null then assume the login failed
                            DependencyService.Get<INotification>().ShortAlert("ERROR: Invalid Credentials");
                            return;
                        }
                        m_config.Token = (string)response.GetValue("token");
                        CondorAPI.getInstance().Token = m_config.Token;
                        m_myusername = (string)response.GetValue("username");
                        CompleteLogin();
                    });
                    CondorAPI.getInstance().Execute(cmd);
                });
                p.Show(this);
            };

            //set up the server address menu event listener
            m_cellAddr.Clicked += new EventHandler((o, e) =>
            {
                this.IsPresented = false;
                AskServerAddress();
            });

            //set up the member permissions menu event listener
            m_cellMemberPerms.Clicked += new EventHandler((o, e) =>
            {
                ((NavigationPage)Detail).PushAsync(new MemberPermsPage(this));
                //hide the sidebar
                this.IsPresented = false;
            });

            //set up the create project menu event listener
            m_cellCreateProject.Clicked += new EventHandler((o, e) =>
            {
                ManageTaskPrompt p = new ManageTaskPrompt(null) { PromptTitle="Create Project",CanAssign=false,CanDelete=false,CanSetTitle=true,CanSetDate=false,PositiveButtonText="Create" };
                p.OnPromptSaved += new Prompt.PromptClosedEventListener(() =>
                {
                    CreateCommand cmd = new CreateCommand() { Creator=m_myusername,Title=p.TaskTitle,Description=p.Description };
                    cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
                    {
                        if ((string)response.GetValue("result") != "success")
                        {
                            DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                            return;
                        }
                        //add the new project
                        Project newProject = new Project(p.PromptTitle.Replace(' ', '_').ToLower());
                        newProject.Title = p.PromptTitle;
                        newProject.Description = p.Description;
                        //add the creator as a member of the project
                        Member newProjectCreator = new Member();
                        newProjectCreator.ID = m_myusername;
                        newProjectCreator.Role = Role.MANAGER;
                        newProject.Members.Add(newProjectCreator);
                        //add the project to the project list
                        m_projects.AppendProject(newProject);
                        //reload the projects list in the sidebar
                        FillProjectsList();
                    });
                    CondorAPI.getInstance().Execute(cmd);
                });
                p.Show(this);
                //hide the sidebar
                this.IsPresented = false;
            });

            if (m_config.Address != null && !m_config.Address.Equals(""))
            { //if a server address is specified, initialize the app
                Initialize();
            }
            else
            { //otherwise ask for a server address
                AskServerAddress();
            }
        }

        public void DeleteProject(Project proj)
        {
            m_projects.DeleteProject(proj);
        }

        public void DisplayHome()
        {
            //replace the Detail page with the homepage
            Detail = new NavigationPage(new HomePage()) { BarBackgroundColor = (Color)Application.Current.Resources["Primary"], BarTextColor = Color.White };

            //set the window title
            this.Title = "Condor";

            //try to fill the list of projects in the sidebar
            FillProjectsList();
        }

        private void AskServerAddress()
        {
            TextPrompt p = new TextPrompt() { PromptTitle = "Server Address", Hint = "https://condor.example.com", Text = m_config.Address, Keyboard=Keyboard.Url };
            p.OnPromptSaved += new Prompt.PromptClosedEventListener(() =>
            {
                if (p.Text != "")
                {
                    //append a '/'
                    if (!p.Text.EndsWith("/"))
                        p.Text += "/";
                    //append scheme
                    if (!p.Text.StartsWith("http://") && !p.Text.StartsWith("https://"))
                        p.Text = "http://" + p.Text;
                    //set the address
                    m_config.Address = p.Text;
                    
                    //initialize the app
                    Initialize();
                    //display toast
                    DependencyService.Get<INotification>().ShortAlert("Server address saved, you may now log in");
                }
            });
            p.Show(this);
        }

        private void Initialize()
        {
            //enable login menu option
            m_cellLogin.IsEnabled = true;

            //initialize the condor helper
            CondorAPI.getInstance().Initialize(m_config.Address);
            CondorAPI.getInstance().Token = m_config.Token;

            //display the home page
            DisplayHome();
            //try restoring the login, only if a token is set
            if (m_config.Token == "")
                return;
            APICommand cmd = new RestoreLoginCommand() { Token = m_config.Token };
            cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
            {
                if (response.GetValue("username") == null)
                { //token was invalid
                    DependencyService.Get<INotification>().ShortAlert("Session timed out or server unavailable, please log in again");
                    CondorAPI.getInstance().Token = "";
                    m_config.Token = "";
                    return;
                }
                //set my username and complete the login
                m_myusername = (string)response.GetValue("username");
                CompleteLogin();
            });
            CondorAPI.getInstance().Execute(cmd);
        }

        private void SetNavMenu(bool loggedIn)
        {
            if (loggedIn)
            {
                m_cellLogin.Text = "Logout";
                m_cellAddr.Enabled = false;
                m_cellLogin.Enabled = true;
            }
            else
            {
                m_cellLogin.Text = "Login";
                m_cellAddr.Enabled = true;
                m_cellLogin.Enabled = true;
                m_mnuAdminTitle.IsVisible = false;
                m_cellCreateProject.IsVisible = false;
                m_cellMemberPerms.IsVisible = false;
            }
        }

        private void CompleteLogin()
        {
            //make toast
            DependencyService.Get<INotification>().ShortAlert("Logged in as "+m_myusername);

            //set the API caller's token
            CondorAPI.getInstance().Token = m_config.Token;
            SetNavMenu(true);

            //determine whether to show or hide the member permissions menu item
            APICommand cmd = new CanModifyPermissionsCommand();
            cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
            {
                if ((string)response.GetValue("result") != "success")
                {
                    DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                    return;
                }
                m_cellMemberPerms.IsVisible = (bool)response.GetValue("haspermission");
                if (m_cellMemberPerms.IsVisible)
                    m_mnuAdminTitle.IsVisible = true;
            });
            CondorAPI.getInstance().Execute(cmd);

            //determine whether to show or hide the create project member item
            cmd = new CanCreateProjectsCommand();
            cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
            {
                if ((string)response.GetValue("result") != "success")
                {
                    DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                    return;
                }
                m_cellCreateProject.IsVisible = (bool)response.GetValue("haspermission");
                if (m_cellCreateProject.IsVisible)
                    m_mnuAdminTitle.IsVisible = true;
            });
            CondorAPI.getInstance().Execute(cmd);

            //set the user ID
            m_lblCurUserId.Text = m_myusername;

            //set the team
            cmd = new GetTeamCommand();
            cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
            {
                m_team = (string)response.GetValue("team");
                m_lblCurTeam.Text = m_team;
            });
            CondorAPI.getInstance().Execute(cmd);

            //fill the projects list
            FillProjectsList();
        }

        private void CompleteLogout()
        {
            //display toast
            DependencyService.Get<INotification>().ShortAlert("Logged out");
            //clear the API's token
            CondorAPI.getInstance().Token = "";
            //clear the saved token
            m_config.Token = "";
            //nullify my username
            m_myusername = null;
            SetNavMenu(false);
            //reset the labels to default
            m_lblCurTeam.Text = "Condor";
            m_lblCurUserId.Text = "Task Management Engine";

            //go back to the home page
            DisplayHome();
        }

        private void FillProjectsList()
        {
            //get the projects' ListView object
            ListView sidebarList = ((MainPageMaster)this.Master).ListView.FindByName<ListView>("lst_main_menu");

            //do nothing if no username is set
            if (m_myusername == null || m_myusername == "")
            {
                sidebarList.ItemsSource = null;
                return;
            }

            //get the projects list
            APICommand cmd = new GetAllProjectsCommand() { Token=m_config.Token };
            cmd.OnResponseReceived += new APICommand.ResponseReceivedHandler((response) =>
            {
                if (!((string)response.GetValue("result")).Equals("success"))
                { //failed to get the projects list
                    DependencyService.Get<INotification>().ShortAlert("Error getting projects list: "+(string)response.GetValue("msg"));
                    return;
                }
                //parse the projects data
                m_projects = new Projects();
                m_projects.Parse(response);

                //set the listview source
                sidebarList.ItemsSource = m_projects.ProjectList;
            });
            CondorAPI.getInstance().Execute(cmd);
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            Project item = e.SelectedItem as Project;
            if (item == null)
                return;

            //set the detail page to the project (also set page title)
            Detail = new NavigationPage(new ProjectPage(this, item, m_myusername) { Title=m_team+" | "+item.Title });

            //and close the sidebar thing
            IsPresented = false;
        }
    }
}