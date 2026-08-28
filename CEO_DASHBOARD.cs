using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WFA_Sample_A;

namespace LoginPage
{
    public partial class CEO_Dashboard : Form
    {
        private DataAccess Da { get; set; }
        private DataSet Ds { get; set; }
        private string Sql { get; set; }

        public CEO_Dashboard()
        {
            InitializeComponent();
            this.Da = new DataAccess();
            this.LoadCEOOverview();
            this.RoleCmb.SelectedIndexChanged += RoleComboBox_SelectedIndexChanged;
        }

        private void LogoutBtn_Click(object sender, EventArgs e)
        {
            this.Hide();
            LoginPage login = new LoginPage();
            login.Show();
        }

        // Users Panel

        private void UsersBtn_Click(object sender, EventArgs e)
        {
            this.UsersControlPanel.Visible = true;
            this.ProjectsControlPanel.Visible = false;
            this.AssignmentsControlPanel.Visible = false;
            this.ReportsControlPanel.Visible = false;
            this.HomePanel.Visible = false;
            PopulateUsersGridView();
        }

        private void UsersClearBtn_Click(object sender, EventArgs e)
        {
            ClearAll();
            PopulateUsersGridView();
            GenerateUserID();
        }

        private void UsersDGV_DoubleClick(object sender, EventArgs e)
        {

            if (UsersDGV.CurrentRow != null)
            {
                try
                {
                    this.UseridTxt.Text = UsersDGV.CurrentRow.Cells["UserID"].Value?.ToString();
                    this.NameTxt.Text = UsersDGV.CurrentRow.Cells["UserName"].Value?.ToString();
                    this.RoleCmb.Text = UsersDGV.CurrentRow.Cells["Role"].Value?.ToString();
                    this.PasswordTxt.Text = UsersDGV.CurrentRow.Cells["Password"].Value?.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading Users details: " + ex.Message);
                }
            }
        }

        private void UsersSaveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(this.NameTxt.Text, @"^[a-zA-Z]+$"))
                {
                    MessageBox.Show("User Name must contain only letters.");
                    return;
                }

                this.Sql = "select * from Users where UserID = '" + this.UseridTxt.Text + "'";
                this.Ds = this.Da.ExecuteQuery(this.Sql);

                if (this.Ds.Tables[0].Rows.Count == 1)
                {
                    this.Sql = @"update Users 
                        set UserName = '" + this.NameTxt.Text +
                                @"', Role = '" + this.RoleCmb.Text +
                                @"', Password = '" + this.PasswordTxt.Text +
                                @"' where UserID = '" + this.UseridTxt.Text + "';";

                    int count = this.Da.ExecuteUpdateQuery(this.Sql);
                    if (count == 1)
                    {
                        MessageBox.Show(this.NameTxt.Text + " has been updated properly");
                    }
                    else
                    {
                        MessageBox.Show("User data updation failed");
                    }
                }
                else
                {
                    this.Sql = @"insert into Users (UserID, UserName, Role, Password) 
                         values ('" + this.UseridTxt.Text + "', '" + this.NameTxt.Text +
                                "', '" + this.RoleCmb.Text + "', '" + this.PasswordTxt.Text + "'); ";

                    int count = this.Da.ExecuteUpdateQuery(this.Sql);
                    if (count == 1)
                    {
                        MessageBox.Show(this.NameTxt.Text + " has been added properly");
                        this.GenerateUserID();
                    }
                    else
                    {
                        MessageBox.Show("User data insertion failed");
                    }
                }

                this.PopulateUsersGridView();
                this.ClearAll();
            }
            catch (Exception exc)
            {
                MessageBox.Show("An error has occured during saving the User data\n\n" + exc.Message);
            }
        }
        private void UsersDeletelBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string id = this.UsersDGV.CurrentRow.Cells["UserID"].Value.ToString();
                string name = this.UsersDGV.CurrentRow.Cells["UserName"].Value.ToString();
                string role = this.UsersDGV.CurrentRow.Cells["Role"].Value.ToString();

                this.Sql = @"delete from Users where UserID = '" + id + "';";
                int count = this.Da.ExecuteUpdateQuery(this.Sql);

                if (count == 1)
                {
                    MessageBox.Show(name + " has been deleted");
                }
                else
                {
                    MessageBox.Show("User data deletion failed");
                }

                this.PopulateUsersGridView();
                this.ClearAll();
            }
            catch (Exception exc)
            {
                MessageBox.Show("An error has occured during deletion\n" + exc.Message);
            }
        }

        private void RoleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            GenerateUserID();
        }

        private void PopulateUsersGridView
        (string sql = @" SELECT U.UserID, U.UserName, U.Password, U.Role,
        CASE 
            WHEN U.Role = 'Manager' THEN (SELECT COUNT(*) FROM Projects P WHERE P.ManagerID = U.UserID AND P.Status <> 'Completed')
            WHEN U.Role = 'Auditor' THEN (SELECT COUNT(*) FROM Assignments A WHERE A.AuditorID = U.UserID AND A.Status <> 'Completed')
            ELSE 0 END AS CurrentlyWorking
            FROM Users U WHERE U.Role <> 'CEO'; ")
        {
            try
            {
                this.Ds = this.Da.ExecuteQuery(sql);
                this.UsersDGV.AutoGenerateColumns = false;
                this.UsersDGV.DataSource = this.Ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading users: " + ex.Message);
            }
        }

        private void GenerateUserID()
        {
            string role = this.RoleCmb.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(role))
            {
                this.UseridTxt.Text = "";
                return;
            }

            string prefix = "";
            switch (role)
            {
                case "Manager":
                    prefix = "M-";
                    break;
                case "Auditor":
                    prefix = "CA-";
                    break;
            }

            string sql = $"SELECT ISNULL(MAX(CAST(SUBSTRING(UserID, {prefix.Length + 1}, LEN(UserID) - {prefix.Length}) AS INT)), 0) FROM Users WHERE UserID LIKE '{prefix}%';";
            this.Ds = this.Da.ExecuteQuery(sql);

            int maxId = int.Parse(this.Ds.Tables[0].Rows[0][0].ToString());
            this.UseridTxt.Text = prefix + (maxId + 1).ToString("D3");

        }

        private void ClearAll()
        {
            this.UseridTxt.Clear();
            this.UseridTxt.ReadOnly = true;
            this.NameTxt.Clear();
            this.RoleCmb.SelectedIndex = -1;
            this.PasswordTxt.Clear();
        }



        // Projects Panel
        private void ProjectsBtn_Click(object sender, EventArgs e)
        {
            this.ProjectsControlPanel.Visible = true;
            this.UsersControlPanel.Visible = false;
            this.AssignmentsControlPanel.Visible = false;
            this.ReportsControlPanel.Visible = false;
            this.HomePanel.Visible = false;
            ClearAllProjects();
            PopulateProjectsGridView();
            PopulateManagerCombo();
            GenerateProjectID();
        }

        private void PopulateProjectsGridView(string sql = "SELECT * FROM Projects;")
        {
            this.Ds = this.Da.ExecuteQuery(sql);
            this.ProjectsDGV.AutoGenerateColumns = false;
            this.ProjectsDGV.DataSource = this.Ds.Tables[0];
        }

        private void ProjectsClearBtn_Click(object sender, EventArgs e)
        {
            ClearAllProjects();
            PopulateProjectsGridView();
            PopulateManagerCombo();
            GenerateProjectID();
        }

        private void ProjectsDGV_DoubleClick(object sender, EventArgs e)
        {
            if (ProjectsDGV.CurrentRow != null)
            {
                try
                {
                    this.ProjectidTxt.Text = ProjectsDGV.CurrentRow.Cells["ProjectID"].Value?.ToString();
                    this.ProjectnameTxt.Text = ProjectsDGV.CurrentRow.Cells["ProjectName"].Value?.ToString();
                    this.ProjectsManagerCmb.SelectedValue = ProjectsDGV.CurrentRow.Cells["ManagerID"].Value?.ToString();
                    this.ClientnameTxt.Text = ProjectsDGV.CurrentRow.Cells["ClientName"].Value?.ToString();
                    this.StatusCmb.Text = ProjectsDGV.CurrentRow.Cells["Status"].Value?.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading Project details: " + ex.Message);
                }
            }
        }

        private void ProjectsSaveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                this.Sql = "SELECT * FROM Projects WHERE ProjectID = '" + this.ProjectidTxt.Text + "'";
                this.Ds = this.Da.ExecuteQuery(this.Sql);

                string managerID = this.ProjectsManagerCmb.SelectedValue?.ToString() ?? "";

                if (this.Ds.Tables[0].Rows.Count == 1)
                {
                    this.Sql = @"UPDATE Projects
                                 SET ProjectName = '" + this.ProjectnameTxt.Text +
                                 "', ClientName = '" + this.ClientnameTxt.Text +
                                 "', ManagerID = '" + managerID +
                                 "', Status = '" + this.StatusCmb.Text +
                                 "' WHERE ProjectID = '" + this.ProjectidTxt.Text + "';";

                    int count = this.Da.ExecuteUpdateQuery(this.Sql);
                    if (count == 1)
                        MessageBox.Show(this.ProjectnameTxt.Text + " has been updated properly");
                    else
                        MessageBox.Show("Project data updation failed");
                }
                else
                {
                    this.Sql = @"INSERT INTO Projects (ProjectID, ProjectName, ClientName, ManagerID, Status)
                                 VALUES ('" + this.ProjectidTxt.Text +
                                 "', '" + this.ProjectnameTxt.Text +
                                 "', '" + this.ClientnameTxt.Text +
                                 "', '" + managerID +
                                 "', '" + this.StatusCmb.Text + "');";

                    int count = this.Da.ExecuteUpdateQuery(this.Sql);
                    if (count == 1)
                    {
                        MessageBox.Show(this.ProjectnameTxt.Text + " has been added properly");
                        GenerateProjectID();
                    }
                    else
                        MessageBox.Show("Project data insertion failed");
                }

            }
            catch (Exception exc)
            {
                MessageBox.Show("An error has occured during saving the Project data\n\n" + exc.Message);
            }

            ClearAllProjects();
            PopulateProjectsGridView();
            PopulateManagerCombo();
            GenerateProjectID();
        }

        private void ProjectsDeleteBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.ProjectsDGV.CurrentRow == null)
                {
                    MessageBox.Show("Please select a project to delete");
                    return;
                }

                string id = this.ProjectsDGV.CurrentRow.Cells["ProjectID"].Value.ToString();
                string name = this.ProjectsDGV.CurrentRow.Cells["ProjectName"].Value.ToString();

                this.Sql = @"DELETE FROM Projects WHERE ProjectID = '" + id + "';";
                int count = this.Da.ExecuteUpdateQuery(this.Sql);

                if (count == 1)
                    MessageBox.Show(name + " has been deleted");
                else
                    MessageBox.Show("Project data deletion failed");

            }
            catch (Exception exc)
            {
                MessageBox.Show("An error has occured during deletion\n" + exc.Message);
            }
            ClearAllProjects();
            PopulateProjectsGridView();
            PopulateManagerCombo();
            GenerateProjectID();
        }

        private void PopulateManagerCombo()
        {
            try
            {
                string sql = @" SELECT U.UserID, (U.UserID + ' (' + CAST(ISNULL(S.AssignCount, 0) AS VARCHAR(10)) + ')') AS DisplayText FROM Users U
                LEFT JOIN ( SELECT ManagerID, COUNT(*) AS AssignCount FROM Projects
                WHERE Status <> 'Completed'  
                GROUP BY ManagerID ) 
                S ON U.UserID = S.ManagerID WHERE U.Role = 'Manager'
                ORDER BY U.UserID; ";

                DataSet ds = this.Da.ExecuteQuery(sql);

                this.ProjectsManagerCmb.DataSource = null;
                this.ProjectsManagerCmb.Items.Clear();

                this.ProjectsManagerCmb.DisplayMember = "DisplayText";
                this.ProjectsManagerCmb.ValueMember = "UserID";
                this.ProjectsManagerCmb.DataSource = ds.Tables[0];

                this.ProjectsManagerCmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading managers: " + ex.Message);
            }
        }

        public void GenerateProjectID()
        {
            string sql = "SELECT ISNULL(MAX(CAST(SUBSTRING(ProjectID, 3, LEN(ProjectID)-2) AS INT)), 0) FROM Projects;";
            this.Ds = this.Da.ExecuteQuery(sql);

            int maxId = int.Parse(this.Ds.Tables[0].Rows[0][0].ToString());
            this.ProjectidTxt.Text = "PR" + (maxId + 1).ToString("D3");
        }

        private void ClearAllProjects()
        {
            this.ProjectidTxt.Clear();
            this.ProjectnameTxt.Clear();
            this.ClientnameTxt.Clear();
            this.ProjectsManagerCmb.SelectedIndex = -1;
            this.StatusCmb.SelectedIndex = -1;
        }


        //Asignments Panel

        private void AssignmentsBtn_Click(object sender, EventArgs e)
        {
            this.UsersControlPanel.Visible = false;
            this.ProjectsControlPanel.Visible = false;
            this.ReportsControlPanel.Visible = false;
            this.HomePanel.Visible = false;
            this.AssignmentsControlPanel.Visible = true;
            PopulateAssignmentsGridView();
            PopulateProjectCombo();
            PopulateAuditorCombo();
            GenerateAssignmentID();
        }

        private void PopulateAssignmentsGridView(string sql = @" SELECT AssignmentID, ProjectID, AuditorID, Status FROM Assignments; ")
        {
            this.Ds = this.Da.ExecuteQuery(sql);
            this.AssignmentsDGV.AutoGenerateColumns = false;
            this.AssignmentsDGV.DataSource = this.Ds.Tables[0];
        }

        private void AssignmentsClearBtn_Click(object sender, EventArgs e)
        {
            ClearAllAssignments();
            PopulateAssignmentsGridView();
            PopulateProjectCombo();
            PopulateAuditorCombo();
            GenerateAssignmentID();
        }

        private void AssignmentsDGV_DoubleClick(object sender, EventArgs e)
        {
            if (AssignmentsDGV.CurrentRow != null)
            {
                try
                {
                    string assignmentId = AssignmentsDGV.CurrentRow.Cells["AssignmentID"].Value?.ToString();

                    if (!string.IsNullOrEmpty(assignmentId))
                    {
                        string sql = $"SELECT * FROM Assignments WHERE AssignmentID = '{assignmentId}';";
                        DataSet dsAssignment = this.Da.ExecuteQuery(sql);

                        if (dsAssignment.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = dsAssignment.Tables[0].Rows[0];

                            this.AssignmentidTxt.Text = dr["AssignmentID"].ToString();
                            this.TaskDescriptionTxt.Text = dr["TaskDescription"].ToString();
                            this.ProjectidCmb.SelectedValue = dr["ProjectID"].ToString();
                            this.AuditoridCmb.SelectedValue = dr["AuditorID"].ToString();
                            this.AssignmentStatusCmb.Text = dr["Status"].ToString();
                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading Assignment details: " + ex.Message);
                }
            }
        }

        private void AssignmentSaveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                this.Sql = "SELECT * FROM Assignments WHERE AssignmentID = '" + this.AssignmentidTxt.Text + "'";
                this.Ds = this.Da.ExecuteQuery(this.Sql);

                string projectID = this.ProjectidCmb.SelectedValue?.ToString() ?? "";
                string auditorID = this.AuditoridCmb.SelectedValue?.ToString() ?? "";

                if (this.Ds.Tables[0].Rows.Count == 1)
                {
                    this.Sql = @"UPDATE Assignments
                                 SET TaskDescription = '" + this.TaskDescriptionTxt.Text +

                                 "', ProjectID = '" + projectID +
                                 "', AuditorID = '" + auditorID +
                                 "', Status = '" + this.AssignmentStatusCmb.Text +
                                 "' WHERE AssignmentID = '" + this.AssignmentidTxt.Text + "';";

                    int count = this.Da.ExecuteUpdateQuery(this.Sql);
                    if (count == 1)
                        MessageBox.Show("Assignment has been updated properly");
                    else
                        MessageBox.Show("Assignment data updation failed");
                }
                else
                {
                    this.Sql = @"INSERT INTO Assignments (AssignmentID, TaskDescription, ProjectID, AuditorID, Status)
                                 VALUES ('" + this.AssignmentidTxt.Text +
                                 "', '" + this.TaskDescriptionTxt.Text +
                                 "', '" + projectID +
                                 "', '" + auditorID +
                                 "', '" + this.AssignmentStatusCmb.Text + "');";

                    int count = this.Da.ExecuteUpdateQuery(this.Sql);
                    if (count == 1)
                    {
                        MessageBox.Show("Assignment has been added properly");
                        GenerateAssignmentID();
                    }
                    else
                        MessageBox.Show("Assignment data insertion failed");
                }

            }
            catch (Exception exc)
            {
                MessageBox.Show("An error has occured during saving the Assignment data\n\n" + exc.Message);
            }
            ClearAllAssignments();
            PopulateAssignmentsGridView();
            PopulateProjectCombo();
            PopulateAuditorCombo();
            GenerateAssignmentID();
        }

        private void AssignmentDeleteBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.AssignmentsDGV.CurrentRow == null)
                {
                    MessageBox.Show("Please select a Assignment to delete");
                    return;
                }

                string id = this.AssignmentsDGV.CurrentRow.Cells["AssignmentID"].Value.ToString();

                this.Sql = @"DELETE FROM Assignments WHERE AssignmentID = '" + id + "';";
                int count = this.Da.ExecuteUpdateQuery(this.Sql);

                if (count == 1)
                    MessageBox.Show("Selected Assignment (ID: " + id + ") has been deleted");
                else
                    MessageBox.Show("Assignment data deletion failed");

            }
            catch (Exception exc)
            {
                MessageBox.Show("An error has occured during deletion\n" + exc.Message);
            }

            ClearAllAssignments();
            PopulateAssignmentsGridView();
            PopulateProjectCombo();
            PopulateAuditorCombo();
            GenerateAssignmentID();
        }

        private void PopulateAuditorCombo()
        {
            try
            {
                string sql = @" SELECT U.UserID, (U.UserID + ' (' + CAST(ISNULL(S.AssignCount, 0) AS VARCHAR(10)) + ')') AS DisplayText FROM Users U
                LEFT JOIN ( SELECT AuditorID, COUNT(*) AS AssignCount FROM Assignments
                WHERE Status <> 'Completed' 
                GROUP BY AuditorID )
                S ON U.UserID = S.AuditorID WHERE U.Role = 'Auditor'
                ORDER BY U.UserID; ";
                DataSet ds = this.Da.ExecuteQuery(sql);
                this.AuditoridCmb.DataSource = null;
                this.AuditoridCmb.Items.Clear();
                this.AuditoridCmb.DisplayMember = "DisplayText";
                this.AuditoridCmb.ValueMember = "UserID";
                this.AuditoridCmb.DataSource = ds.Tables[0];
                this.AuditoridCmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading auditors: " + ex.Message);
            }
        }

        private void PopulateProjectCombo()
        {
            try
            {
                string sql = "SELECT ProjectID FROM Projects;";
                DataSet ds = this.Da.ExecuteQuery(sql);
                this.ProjectidCmb.DataSource = ds.Tables[0];
                this.ProjectidCmb.DisplayMember = "ProjectID";
                this.ProjectidCmb.ValueMember = "ProjectID";
                this.ProjectidCmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading managers: " + ex.Message);
            }
        }

        private void GenerateAssignmentID()
        {
            string sql = "SELECT ISNULL(MAX(CAST(SUBSTRING(AssignmentID, 3, LEN(AssignmentID)-2) AS INT)), 0) FROM Assignments;";
            this.Ds = this.Da.ExecuteQuery(sql);
            int maxId = int.Parse(this.Ds.Tables[0].Rows[0][0].ToString());
            this.AssignmentidTxt.Text = "AS" + (maxId + 1).ToString("D3");

        }

        private void ClearAllAssignments()
        {
            this.AssignmentidTxt.Clear();
            this.AssignmentidTxt.ReadOnly = true;
            this.TaskDescriptionTxt.Clear();
            this.ProjectidCmb.SelectedIndex = -1;
            this.AuditoridCmb.SelectedIndex = -1;
            this.AssignmentStatusCmb.SelectedIndex = -1;
        }

        private void AssignmentsDGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    return;

                string columnName = AssignmentsDGV.Columns[e.ColumnIndex].Name;
                string assignmentId = AssignmentsDGV.Rows[e.RowIndex].Cells["AssignmentID"].Value?.ToString();

                if (string.IsNullOrEmpty(assignmentId))
                {
                    MessageBox.Show("Please select a valid assignment.");
                    return;
                }

                if (columnName == "A_Delete")
                {
                    DialogResult result = MessageBox.Show(
                        "Are you sure you want to delete this assignment (ID: " + assignmentId + ")?",
                        "Confirm Deletion",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (result == DialogResult.Yes)
                    {
                        this.Sql = $"DELETE FROM Assignments WHERE AssignmentID = '{assignmentId}';";
                        int count = this.Da.ExecuteUpdateQuery(this.Sql);

                        if (count == 1)
                            MessageBox.Show("Selected Assignment (ID: " + assignmentId + ") has been deleted");
                        else
                            MessageBox.Show("Assignment data deletion failed");

                        ClearAllAssignments();
                        PopulateAssignmentsGridView();
                        PopulateAuditorCombo();
                        GenerateAssignmentID();
                    }
                }

                else if (columnName == "View")
                {
                    string sql = $"SELECT TaskDescription FROM Assignments WHERE AssignmentID = '{assignmentId}';";
                    DataSet ds = this.Da.ExecuteQuery(sql);

                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        string taskDescription = ds.Tables[0].Rows[0]["TaskDescription"].ToString();
                        TaskDescrLbl.Text = taskDescription;
                        TaskDescriptionPanel.Visible = true;
                        TaskDescriptionPanel.BringToFront();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void AssignmentsDGV_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.AssignmentsDGV.ClearSelection();
        }
        private void CloseTaskDescriptionBtn_Click(object sender, EventArgs e)
        {
            TaskDescriptionPanel.Visible = false;
        }
        private void AssignmentsDGV_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (this.AssignmentsDGV.Columns[e.ColumnIndex].Name == "A_Status" && e.Value != null)
            {
                string status = e.Value.ToString();

                if (status == "Pending")
                {
                    e.CellStyle.ForeColor = Color.Red;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status == "Ongoing")
                {
                    e.CellStyle.ForeColor = Color.Orange;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status == "Completed")
                {
                    e.CellStyle.ForeColor = Color.Green;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
            }
        }

        // Reports Panel

        private void ReportsBtn_Click(object sender, EventArgs e)
        {
            this.UsersControlPanel.Visible = false;
            this.ProjectsControlPanel.Visible = false;
            this.AssignmentsControlPanel.Visible = false;
            this.HomePanel.Visible = false;
            this.ReportsControlPanel.Visible = true;
            PopulateReportsGridView();
            PopulateAssignmentCombo();
            GenerateReportID();
        }
        private void PopulateReportsGridView(string sql = "SELECT * FROM Reports;")
        {
            this.Ds = this.Da.ExecuteQuery(sql);
            this.ReportsDGV.AutoGenerateColumns = false;
            this.ReportsDGV.DataSource = this.Ds.Tables[0];
        }
        private void PopulateAssignmentCombo()
        {
            try
            {
                string sql = "SELECT AssignmentID FROM Assignments;";
                DataSet ds = this.Da.ExecuteQuery(sql);
                this.AssignmentidCmb.DataSource = ds.Tables[0];
                this.AssignmentidCmb.DisplayMember = "AssignmentID";
                this.AssignmentidCmb.ValueMember = "AssignmentID";
                this.AssignmentidCmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading managers: " + ex.Message);
            }
        }
        private void GenerateReportID()
        {
            string sql = "SELECT ISNULL(MAX(CAST(SUBSTRING(ReportID, 3, LEN(ReportID)-2) AS INT)), 0) FROM Reports;";
            this.Ds = this.Da.ExecuteQuery(sql);
            int maxId = int.Parse(this.Ds.Tables[0].Rows[0][0].ToString());
            this.ReportidTxt.Text = "RE" + (maxId + 1).ToString("D3");

        }

        private void ReportsClearBtn_Click(object sender, EventArgs e)
        {
            ClearAllReports();
            PopulateReportsGridView();
            PopulateAssignmentCombo();
            GenerateReportID();
        }


        private void ReportsDGV_DoubleClick(object sender, EventArgs e)
        {
            if (ReportsDGV.CurrentRow != null)
            {
                try
                {
                    string reportId = ReportsDGV.CurrentRow.Cells["ReportID"].Value?.ToString();

                    if (!string.IsNullOrEmpty(reportId))
                    {
                        string sql = $"SELECT ReportID, AssignmentID, ReportDetails,SubmittedDate from Reports WHERE ReportID = '{reportId}';";
                        DataSet dsReport = this.Da.ExecuteQuery(sql);

                        if (dsReport.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = dsReport.Tables[0].Rows[0];

                            this.ReportidTxt.Text = dr["ReportID"].ToString();
                            this.ReportDetailsTxt.Text = dr["ReportDetails"].ToString();
                            this.AssignmentidCmb.SelectedValue = dr["AssignmentID"].ToString();
                            this.SubmittedDate.Value = DateTime.Parse(dr["SubmittedDate"].ToString());

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading Assignment details: " + ex.Message);
                }
            }
        }
        private void ReportsDGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                try
                {
                    string columnName = ReportsDGV.Columns[e.ColumnIndex].Name;

                    if (columnName == "ReportDetails")
                    {
                        DataGridViewRow row = ReportsDGV.Rows[e.RowIndex];

                        ReportsDGV.ClearSelection();
                        row.Selected = true;

                        this.ReportidTxt.Text = row.Cells["ReportID"].Value?.ToString();

                        var dataRowView = row.DataBoundItem as DataRowView;
                        if (dataRowView != null)
                        {

                            string reportDetails = dataRowView["ReportDetails"].ToString();
                            this.ReportDetailsShowLbl.Text = reportDetails;
                            string feedback = dataRowView["Feedback"].ToString();
                            this.FeedbackTxt.Text = feedback;
                        }

                        ReportDetailsPanel.Visible = true;
                        ReportDetailsPanel.BringToFront();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }

        }

        private void ReportSaveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                this.Sql = "SELECT * FROM Reports WHERE ReportID = '" + this.ReportidTxt.Text + "'";
                this.Ds = this.Da.ExecuteQuery(this.Sql);

                string assignmentID = this.AssignmentidCmb.SelectedValue?.ToString() ?? "";
                string submittedDate = this.SubmittedDate.Value.ToString("yyyy-MM-dd HH:mm:ss");

                if (this.Ds.Tables[0].Rows.Count == 1)
                {
                    this.Sql = @"UPDATE Reports
                         SET ReportDetails = '" + this.ReportDetailsTxt.Text +
                                 "', AssignmentID = '" + assignmentID +
                                 "', SubmittedDate = '" + submittedDate +
                                 "' WHERE ReportID = '" + this.ReportidTxt.Text + "';";
                    int count = this.Da.ExecuteUpdateQuery(this.Sql);
                    if (count == 1)
                        MessageBox.Show("Report has been updated properly");
                    else
                        MessageBox.Show("Report data updation failed");
                }
                else
                {
                    this.Sql = @"INSERT INTO Reports (ReportID, ReportDetails, AssignmentID, SubmittedDate)
                         VALUES ('" + this.ReportidTxt.Text +
                                 "', '" + this.ReportDetailsTxt.Text +
                                 "', '" + assignmentID +
                                 "', '" + submittedDate + "');";
                    int count = this.Da.ExecuteUpdateQuery(this.Sql);
                    if (count == 1)
                    {
                        MessageBox.Show("Report has been added properly");
                        GenerateReportID();
                    }
                    else
                        MessageBox.Show("Report data insertion failed");
                }

                ClearAllReports();
                PopulateReportsGridView();
                PopulateAssignmentCombo();
                GenerateReportID();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while saving the report\n\n" + ex.Message);
            }
        }

        private void ReportDeleteBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.ReportsDGV.CurrentRow == null)
                {
                    MessageBox.Show("Please select a Report to delete");
                    return;
                }
                string id = this.ReportsDGV.CurrentRow.Cells["ReportID"].Value.ToString();
                this.Sql = @"DELETE FROM Reports WHERE ReportID = '" + id + "';";
                int count = this.Da.ExecuteUpdateQuery(this.Sql);
                if (count == 1)
                    MessageBox.Show("Selected Report (ID: " + id + ") has been deleted");
                else
                    MessageBox.Show("Report data deletion failed");
                PopulateReportsGridView();
                ClearAllReports();
                PopulateAssignmentCombo();
            }
            catch (Exception exc)
            {
                MessageBox.Show("An error has occured during deletion\n" + exc.Message);
            }
            ClearAllReports();
            PopulateReportsGridView();
            PopulateAssignmentCombo();
            GenerateReportID();
        }
        private void ClearAllReports()
        {
            this.ReportidTxt.Clear();
            this.ReportidTxt.ReadOnly = true;
            this.ReportDetailsTxt.Clear();
            this.AssignmentidCmb.SelectedIndex = -1;
            this.SubmittedDate.Value = DateTime.Now;

        }

        private void CrossLbl_Click(object sender, EventArgs e)
        {
            this.ReportDetailsPanel.Visible = false;
        }

        private void ApproveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(this.ReportidTxt.Text))
                {
                    MessageBox.Show("Please select a valid report before approving.");
                    return;
                }

                string reportId = this.ReportidTxt.Text;
                string feedback = this.FeedbackTxt.Text.Trim();
                string checkSql = $"SELECT ApprovalStatus FROM Reports WHERE ReportID = '{reportId}';";
                var ds = this.Da.ExecuteQuery(checkSql);

                if (ds.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("Report not found.");
                    return;
                }

                string currentStatus = ds.Tables[0].Rows[0]["ApprovalStatus"].ToString();

                if (currentStatus == "Approved")
                {
                    MessageBox.Show("The report is already approved!");
                    return;
                }

                string updateSql = $@" UPDATE Reports
                SET Feedback = '{feedback}', ApprovalStatus = 'Approved'
                WHERE ReportID = '{reportId}';";

                int updated = this.Da.ExecuteUpdateQuery(updateSql);

                if (updated > 0)
                {
                    MessageBox.Show("Report approved successfully!");

                    this.PopulateReportsGridView();
                }
                else
                {
                    MessageBox.Show("Approval failed. Please check the data.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void RejectBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(this.ReportidTxt.Text))
                {
                    MessageBox.Show("Please select a valid report before rejecting.");
                    return;
                }

                string reportId = this.ReportidTxt.Text;
                string feedback = this.FeedbackTxt.Text.Trim();
                string checkSql = $"SELECT ApprovalStatus FROM Reports WHERE ReportID = '{reportId}';";
                var ds = this.Da.ExecuteQuery(checkSql);

                if (ds.Tables[0].Rows.Count == 0)
                {
                    MessageBox.Show("Report not found.");
                    return;
                }

                string currentStatus = ds.Tables[0].Rows[0]["ApprovalStatus"].ToString();

                if (currentStatus == "Rejected")
                {
                    MessageBox.Show("The report is already rejected!");
                    return;
                }

                string updateSql = $@" UPDATE Reports
                SET Feedback = '{feedback}', ApprovalStatus = 'Rejected'
                WHERE ReportID = '{reportId}';";

                int updated = this.Da.ExecuteUpdateQuery(updateSql);

                if (updated > 0)
                {
                    MessageBox.Show("Report rejected successfully!");

                    this.PopulateReportsGridView();
                }
                else
                {
                    MessageBox.Show("Rejection failed. Please check the data.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void ReportsDGV_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.ReportsDGV.ClearSelection();
        }

        private void ReportsDGV_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (this.ReportsDGV.Columns[e.ColumnIndex].Name == "R_Status" && e.Value != null)
            {
                string status = e.Value.ToString();

                if (status == "Pending")
                {
                    e.CellStyle.ForeColor = Color.Orange;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status == "Rejected")
                {
                    e.CellStyle.ForeColor = Color.Red;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
                else if (status == "Approved")
                {
                    e.CellStyle.ForeColor = Color.Green;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
            }
        }

        // Home Panel

        private void Homebtn_Click(object sender, EventArgs e)
        {
            this.UsersControlPanel.Visible = false;
            this.ProjectsControlPanel.Visible = false;
            this.AssignmentsControlPanel.Visible = false;
            this.ReportsControlPanel.Visible = false;
            this.LoadCEOOverview();
        }
        private void LoadCEOOverview()
        {
            try
            {
                string pendingSql = "SELECT COUNT(*) FROM Projects WHERE Status = 'Pending'";
                string activeSql = "SELECT COUNT(*) FROM Projects WHERE Status = 'Ongoing'";
                string submittedSql = "SELECT COUNT(*) FROM Projects WHERE Status = 'Submitted'";

                this.PendingLbl.Text = this.Da.ExecuteQueryTable(pendingSql).Rows[0][0].ToString();
                this.ActiveLbl.Text = this.Da.ExecuteQueryTable(activeSql).Rows[0][0].ToString();
                this.FinishedLbl.Text = this.Da.ExecuteQueryTable(submittedSql).Rows[0][0].ToString();

                string resubmissionSql = "SELECT COUNT(*) FROM Reports WHERE ApprovalStatus = 'Rejected'";
                string rsubmittedSql = "SELECT COUNT(*) FROM Reports WHERE ApprovalStatus = 'Approved'";
                string R_PendingSql = "SELECT COUNT(*) FROM Reports WHERE ApprovalStatus = 'Pending'";

                this.ResubmissiontextLbl.Text = this.Da.ExecuteQueryTable(resubmissionSql).Rows[0][0].ToString();
                this.R_SubmittedtextLbl.Text = this.Da.ExecuteQueryTable(rsubmittedSql).Rows[0][0].ToString();
                this.ApprovalPendingLbl.Text = this.Da.ExecuteQueryTable(R_PendingSql).Rows[0][0].ToString();

                string managersSql = "SELECT COUNT(*) FROM Users WHERE ROLE = 'Manager'";
                string auditorsSql = "SELECT COUNT(*) FROM Users WHERE ROLE = 'Auditor'";

                this.ManagersCountLbl.Text = this.Da.ExecuteQueryTable(managersSql).Rows[0][0].ToString();
                this.AuditorscountLbl.Text = this.Da.ExecuteQueryTable(auditorsSql).Rows[0][0].ToString();

                this.HomePanel.Visible = true;
                this.SubPanel1.Visible = true;
                this.SubPanel2.Visible = true;
                this.SubPanel4.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading CEO Overview: " + ex.Message);
            }
        }

    }
}