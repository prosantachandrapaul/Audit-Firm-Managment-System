using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WFA_Sample_A;

namespace LoginPage
{
    public partial class Manager_Dashboard : Form
    {
        private DataAccess Da { get; set; }
        private DataSet Ds { get; set; }
        private string Sql { get; set; }

        private string CurrentManagerID;

        public Manager_Dashboard(string loggedInManagerID)
        {
            InitializeComponent();
            this.Da = new DataAccess();
            this.CurrentManagerID = loggedInManagerID;
            LoadManagerOverview();
        }

        private void LogoutBtn_Click(object sender, EventArgs e)
        {
            this.Hide();
            LoginPage login = new LoginPage();
            login.Show();
        }


        // Auditors Panel
        private void AuditorsBtn_Click(object sender, EventArgs e)
        {
            this.PopulateAuditorsGridView();
            this.AuditorsPanel.Visible = true;
            this.ProjectsPanel.Visible = false;
            this.AssignmentsPanel.Visible = false;
            this.ReportsPanel.Visible = false;
            this.HomePanel.Visible = false;
            PopulateAuditorsGridView();
            GenerateAuditorID();
            ClearAll();
        }

        private void PopulateAuditorsGridView()
        {
            string sql = @" SELECT U.UserID, U.UserName,
            CASE WHEN U.Role = 'Auditor' THEN (SELECT COUNT(*) FROM Assignments A 
            WHERE A.AuditorID = U.UserID AND A.Status <> 'Completed') ELSE 0 END AS [Currently Working]
            FROM Users U WHERE U.Role = 'Auditor'; ";
            this.Ds = this.Da.ExecuteQuery(sql);
            this.AuditorsDGV.AutoGenerateColumns = true;
            this.AuditorsDGV.DataSource = this.Ds.Tables[0];
        }

        private void AuditorsDGV_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.AuditorsDGV.ClearSelection();
        }

        private void AuditorsDGV_DoubleClick(object sender, EventArgs e)
        {
            if (AuditorsDGV.CurrentRow != null)
            {
                try
                {
                    string auditorId = AuditorsDGV.CurrentRow.Cells["UserID"].Value?.ToString();

                    if (!string.IsNullOrEmpty(auditorId))
                    {
                        string sql = $"SELECT * FROM Users WHERE UserID = '{auditorId}';";
                        DataSet dsAuditor = this.Da.ExecuteQuery(sql);

                        if (dsAuditor.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = dsAuditor.Tables[0].Rows[0];

                            this.AuditoridTxt.Text = dr["UserID"].ToString();
                            this.AuditorNameTxt.Text = dr["UserName"].ToString();
                            this.PasswordTxt.Text = dr["Password"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading Auditor details: " + ex.Message);
                }
            }
        }


        public void GenerateAuditorID()
        {
            string sql = @" SELECT ISNULL(MAX(CAST(SUBSTRING(UserID, 4, LEN(UserID)-3) AS INT)), 0) FROM Users 
                         WHERE Role = 'Auditor'; ";
            this.Ds = this.Da.ExecuteQuery(sql);
            int maxId = int.Parse(this.Ds.Tables[0].Rows[0][0].ToString());
            this.AuditoridTxt.Text = "CA-" + (maxId + 1).ToString("D3");
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            ClearAll();
        }

        private void ClearAll()
        {
            this.AuditoridTxt.Clear();
            this.AuditorNameTxt.Clear();
            this.PasswordTxt.Clear();
            this.AuditorsDGV.ClearSelection();
            GenerateAuditorID();
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.AuditoridTxt.Text) ||
                    string.IsNullOrWhiteSpace(this.AuditorNameTxt.Text) ||
                    string.IsNullOrWhiteSpace(this.PasswordTxt.Text))
                {
                    MessageBox.Show("Please fill all fields before saving.");
                    return;
                }

                string sqlCheck = $"SELECT COUNT(*) FROM Users WHERE UserID = '{this.AuditoridTxt.Text}';";
                DataSet dsCheck = this.Da.ExecuteQuery(sqlCheck);
                int count = int.Parse(dsCheck.Tables[0].Rows[0][0].ToString());
                string sql;
                if (count > 0)
                {
                    sql = $@" UPDATE Users
                          SET UserName = '{this.AuditorNameTxt.Text}',
                          Password = '{this.PasswordTxt.Text}'
                          WHERE UserID = '{this.AuditoridTxt.Text}'; ";
                }
                else
                {

                    sql = $@" INSERT INTO Users (UserID, UserName, Password, Role)
                    VALUES ('{this.AuditoridTxt.Text}', '{this.AuditorNameTxt.Text}', '{this.PasswordTxt.Text}', 'Auditor'); ";
                }

                int result = this.Da.ExecuteUpdateQuery(sql);

                if (result > 0)
                {
                    MessageBox.Show("Auditor saved successfully.");
                    this.PopulateAuditorsGridView();
                    ClearAll();
                }
                else
                {
                    MessageBox.Show("Failed to save auditor.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving auditor: " + ex.Message);
            }
        }


        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.AuditoridTxt.Text))
                {
                    MessageBox.Show("Please select an Auditor to delete.");
                    return;
                }

                DialogResult result = MessageBox.Show("Are you sure you want to delete this Auditor?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    string sql = $"DELETE FROM Users WHERE UserID = '{this.AuditoridTxt.Text}' AND Role = 'Auditor';";
                    int rowsAffected = this.Da.ExecuteUpdateQuery(sql);

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Auditor deleted successfully.");
                        this.PopulateAuditorsGridView();
                        ClearAll();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete auditor.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting auditor: " + ex.Message);
            }
        }

        // Project Panel
        private void ProjectsBtn_Click(object sender, EventArgs e)
        {
            this.ProjectsPanel.Visible = true;
            this.AuditorsPanel.Visible = false;
            this.ReportsPanel.Visible = false;
            this.AssignmentsPanel.Visible = false;
            this.HomePanel.Visible = false;
            ClearAllProjects();
            GenerateProjectID();
            PopulateProjectsGridView();
            PopulateOthersProjectGridView();

        }

        private void PopulateOthersProjectGridView()
        {
            string sql = $"SELECT * FROM Projects WHERE ManagerID <> '{this.CurrentManagerID}';";

            this.Ds = this.Da.ExecuteQuery(sql);
            this.OthersProjectDGV.AutoGenerateColumns = true;
            this.OthersProjectDGV.DataSource = this.Ds.Tables[0];

        }

        private void PopulateProjectsGridView()
        {
            string sql = $"SELECT * FROM Projects WHERE ManagerID = '{this.CurrentManagerID}';";

            this.Ds = this.Da.ExecuteQuery(sql);
            this.ProjectsDGV.AutoGenerateColumns = true;
            this.ProjectsDGV.DataSource = this.Ds.Tables[0];

            if (this.ProjectsDGV.Columns.Contains("ManagerID"))
            {
                this.ProjectsDGV.Columns["ManagerID"].Visible = false;
            }
        }

        private void ProjectsDGV_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (this.ProjectsDGV.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
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
        private void OthersProjectDGV_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (this.OthersProjectDGV.Columns[e.ColumnIndex].Name == "Others_Status" && e.Value != null)
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

        private void ProjectsDGV_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.ProjectsDGV.ClearSelection();
        }
        private void OthersProjectDGV_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.OthersProjectDGV.ClearSelection();
        }

        private void ProjectsDGV_DoubleClick(object sender, EventArgs e)
        {
            if (ProjectsDGV.CurrentRow != null)
            {
                try
                {
                    this.ProjectidTxt.Text = ProjectsDGV.CurrentRow.Cells["ProjectID"].Value?.ToString();
                    this.ProjectnameTxt.Text = ProjectsDGV.CurrentRow.Cells["ProjectName"].Value?.ToString();
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

                if (this.Ds.Tables[0].Rows.Count == 1)
                {
                    this.Sql = @"UPDATE Projects
                         SET ProjectName = '" + this.ProjectnameTxt.Text +
                                 "', ClientName = '" + this.ClientnameTxt.Text +
                                 "', ManagerID = '" + this.CurrentManagerID +
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
                                 "', '" + this.CurrentManagerID +
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
            GenerateProjectID();
        }

        public void GenerateProjectID()
        {
            string sql = "SELECT ISNULL(MAX(CAST(SUBSTRING(ProjectID, 3, LEN(ProjectID)-2) AS INT)), 0) FROM Projects;";
            this.Ds = this.Da.ExecuteQuery(sql);
            int maxId = int.Parse(this.Ds.Tables[0].Rows[0][0].ToString());
            this.ProjectidTxt.Text = "PR" + (maxId + 1).ToString("D3");

        }

        private void ProjectsClearBtn_Click(object sender, EventArgs e)
        {
            ClearAllProjects();
            PopulateProjectsGridView();
            GenerateProjectID();
        }
        private void ProjectsDGV_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                    return;

                if (ProjectsDGV.Columns[e.ColumnIndex].Name != "Delete")
                    return;

                if (this.ProjectsDGV.CurrentRow == null)
                {
                    MessageBox.Show("Please select a project to delete");
                    return;
                }

                string id = this.ProjectsDGV.CurrentRow.Cells["ProjectID"].Value.ToString();
                string name = this.ProjectsDGV.CurrentRow.Cells["ProjectName"].Value.ToString();

                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete this project?",
                    "Confirm Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.No)
                    return;

                this.Sql = @"DELETE FROM Projects WHERE ProjectID = '" + id + "';";
                int count = this.Da.ExecuteUpdateQuery(this.Sql);

                if (count == 1)
                    MessageBox.Show(name + " has been deleted");
                else
                    MessageBox.Show("Project data deletion failed");
            }
            catch (Exception exc)
            {
                MessageBox.Show("An error has occurred during deletion\n" + exc.Message);
            }

            ClearAllProjects();
            PopulateProjectsGridView();
            GenerateProjectID();
        }

        private void ClearAllProjects()
        {
            this.ProjectidTxt.Clear();
            this.ProjectnameTxt.Clear();
            this.ClientnameTxt.Clear();
            this.StatusCmb.SelectedIndex = -1;
        }

        // Assignments Panel
        private void AssignmentsBtn_Click(object sender, EventArgs e)
        {
            this.AuditorsPanel.Visible = false;
            this.ProjectsPanel.Visible = false;
            this.ReportsPanel.Visible = false;
            this.HomePanel.Visible = false;
            this.AssignmentsPanel.Visible = true;
            PopulateAssignmentsGridView();
            PopulateProjectCombo(this.CurrentManagerID);
            PopulateAuditorCombo();
            GenerateAssignmentID();
        }

        private void PopulateAssignmentsGridView()
        {
            try
            {
                string managerId = this.CurrentManagerID;

                string sql = $@" SELECT A.* FROM Assignments A INNER JOIN Projects P ON A.ProjectID = P.ProjectID
                WHERE P.ManagerID = '{managerId}'";
                this.Ds = this.Da.ExecuteQuery(sql);
                this.AssignmentsDGV.AutoGenerateColumns = false;
                this.AssignmentsDGV.DataSource = this.Ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading assignments: " + ex.Message);
            }
        }


        private void AssignmentClearBtn_Click(object sender, EventArgs e)
        {
            ClearAllAssignments();
            PopulateAssignmentsGridView();
            PopulateProjectCombo(this.CurrentManagerID);
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
            PopulateProjectCombo(this.CurrentManagerID);
            PopulateAuditorCombo();
            GenerateAssignmentID();
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
                        PopulateProjectCombo(this.CurrentManagerID);
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

        private void PopulateProjectCombo(string currentManagerId)
        {
            try
            {
                string sql = $"SELECT ProjectID FROM Projects WHERE ManagerID = '{currentManagerId}';";
                DataSet ds = this.Da.ExecuteQuery(sql);

                this.ProjectidCmb.DataSource = ds.Tables[0];
                this.ProjectidCmb.DisplayMember = "ProjectID";
                this.ProjectidCmb.ValueMember = "ProjectID";
                this.ProjectidCmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading projects: " + ex.Message);
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

        private void AssignmentsDGV_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.AssignmentsDGV.ClearSelection();
        }
        private void CloseTaskDescriptionBtn_Click(object sender, EventArgs e)
        {
            this.TaskDescriptionPanel.Visible = false;
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
            this.AuditorsPanel.Visible = false;
            this.ProjectsPanel.Visible = false;
            this.AssignmentsPanel.Visible = false;
            this.HomePanel.Visible = false;
            this.ReportsPanel.Visible = true;
            PopulateReportsGridView(this.CurrentManagerID);
            PopulateAssignmentCombo(this.CurrentManagerID);
            GenerateReportID();
        }
        private void PopulateReportsGridView(string currentManagerId)
        {
            try
            {
                string sql = $@" SELECT R.* FROM Reports R INNER JOIN Assignments A ON R.AssignmentID = A.AssignmentID INNER JOIN Projects P ON A.ProjectID = P.ProjectID
                             WHERE P.ManagerID = '{currentManagerId}'";
                this.Ds = this.Da.ExecuteQuery(sql);
                this.ReportsDGV.AutoGenerateColumns = false;
                this.ReportsDGV.DataSource = this.Ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading reports: " + ex.Message);
            }
        }

        private void PopulateAssignmentCombo(string currentManagerId)
        {
            try
            {
                string sql = $@"SELECT A.AssignmentID FROM Assignments A INNER JOIN Projects P ON A.ProjectID = P.ProjectID
                             WHERE P.ManagerID = '{currentManagerId}'";

                DataSet ds = this.Da.ExecuteQuery(sql);
                this.AssignmentidCmb.DataSource = ds.Tables[0];
                this.AssignmentidCmb.DisplayMember = "AssignmentID";
                this.AssignmentidCmb.ValueMember = "AssignmentID";
                this.AssignmentidCmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading assignments: " + ex.Message);
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
            PopulateReportsGridView(this.CurrentManagerID);
            PopulateAssignmentCombo(this.CurrentManagerID);
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
                PopulateReportsGridView(this.CurrentManagerID);
                PopulateAssignmentCombo(this.CurrentManagerID);
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
                PopulateReportsGridView(this.CurrentManagerID);
                ClearAllReports();
                PopulateAssignmentCombo(this.CurrentManagerID);
            }
            catch (Exception exc)
            {
                MessageBox.Show("An error has occured during deletion\n" + exc.Message);
            }
            ClearAllReports();
            PopulateReportsGridView(this.CurrentManagerID);
            PopulateAssignmentCombo(this.CurrentManagerID);
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
                    this.PopulateReportsGridView(this.CurrentManagerID);
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

                    this.PopulateReportsGridView(this.CurrentManagerID);
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
            this.AuditorsPanel.Visible = false;
            this.ProjectsPanel.Visible = false;
            this.AssignmentsPanel.Visible = false;
            this.ReportsPanel.Visible = false;
            LoadManagerOverview();
           
        }

        private void LoadManagerOverview()
        {
            try
            {
                string currentlyfreeSql = @" SELECT COUNT(*) FROM Users U WHERE U.Role = 'Auditor'
                                          AND NOT EXISTS ( SELECT 1  FROM Assignments A WHERE A.AuditorID = U.UserID AND A.Status <> 'Completed' ); ";
                string currentlyworkingSql = "SELECT COUNT(AuditorID) FROM Assignments WHERE Status <> 'Completed' ";

                string P_pendingSql = $"SELECT COUNT(*) FROM Projects WHERE Status = 'Pending' AND ManagerID = '{CurrentManagerID}'";
                string P_activeSql = $"SELECT COUNT(*) FROM Projects WHERE Status = 'Ongoing' AND ManagerID = '{CurrentManagerID}'";
                string P_finishedSql = $"SELECT COUNT(*) FROM Projects WHERE Status = 'Completed' AND ManagerID = '{CurrentManagerID}'";

                string A_pendingSql = $@" SELECT COUNT(*) FROM Assignments A INNER JOIN Projects P ON A.ProjectID = P.ProjectID
                                      WHERE A.Status = 'Pending' AND P.ManagerID = '{CurrentManagerID}'";
                string A_activeSql = $@" SELECT COUNT(*) FROM Assignments A INNER JOIN Projects P ON A.ProjectID = P.ProjectID
                                     WHERE A.Status = 'Ongoing' AND P.ManagerID = '{CurrentManagerID}'";
                string A_finishedSql = $@" SELECT COUNT(*) FROM Assignments A INNER JOIN Projects P ON A.ProjectID = P.ProjectID
                                     WHERE A.Status = 'Completed' AND P.ManagerID = '{CurrentManagerID}'";

                this.P_PendingTextLbl.Text = this.Da.ExecuteQueryTable(P_pendingSql).Rows[0][0].ToString();
                this.P_ActiveTextLbl.Text = this.Da.ExecuteQueryTable(P_activeSql).Rows[0][0].ToString();
                this.P_FinishedTextLbl.Text = this.Da.ExecuteQueryTable(P_finishedSql).Rows[0][0].ToString();

                string rejectedSql = $@" SELECT COUNT(*) FROM Reports R INNER JOIN Assignments A ON R.AssignmentID = A.AssignmentID INNER JOIN Projects P ON A.ProjectID = P.ProjectID
                                     WHERE R.ApprovalStatus = 'Rejected' AND P.ManagerID = '{CurrentManagerID}'";
                string approvedSql = $@" SELECT COUNT(*) FROM Reports R INNER JOIN Assignments A ON R.AssignmentID = A.AssignmentID INNER JOIN Projects P ON A.ProjectID = P.ProjectID
                                     WHERE R.ApprovalStatus = 'Approved' AND P.ManagerID = '{CurrentManagerID}'";
                string R_pendingSql = $@" SELECT COUNT(*) FROM Reports R INNER JOIN Assignments A ON R.AssignmentID = A.AssignmentID INNER JOIN Projects P ON A.ProjectID = P.ProjectID
                                     WHERE R.ApprovalStatus = 'Pending' AND P.ManagerID = '{CurrentManagerID}'";

                this.R_RejectedTextLbl.Text = this.Da.ExecuteQueryTable(rejectedSql).Rows[0][0].ToString();
                this.R_ApprovedTextLbl.Text = this.Da.ExecuteQueryTable(approvedSql).Rows[0][0].ToString();
                this.R_PendingTextLbl.Text = this.Da.ExecuteQueryTable(R_pendingSql).Rows[0][0].ToString();

                this.A_PendingTextLbl.Text = this.Da.ExecuteQueryTable(A_pendingSql).Rows[0][0].ToString();
                this.A_ActiveTextLbl.Text = this.Da.ExecuteQueryTable(A_activeSql).Rows[0][0].ToString();
                this.A_FinishedTextLbl.Text = this.Da.ExecuteQueryTable(A_finishedSql).Rows[0][0].ToString();

                this.CurrentlyFreeTextLbl.Text = this.Da.ExecuteQueryTable(currentlyfreeSql).Rows[0][0].ToString();
                this.CurrentlyWorkingTextLbl.Text = this.Da.ExecuteQueryTable(currentlyworkingSql).Rows[0][0].ToString();

                this.HomePanel.Visible = true;
                this.SubPanel1.Visible = true;
                this.SubPanel2.Visible = true;
                this.SubPanel3.Visible = true;
                this.SubPanel4.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Manager Overview: " + ex.Message);
            }
        }

    }
}
