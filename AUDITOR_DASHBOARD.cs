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
    public partial class Auditor_Dashboard : Form
    {
        private DataAccess Da { get; set; }
        private DataSet Ds { get; set; }
        private string Sql { get; set; }

        private string CurrentAuditorID;


        public Auditor_Dashboard(string loggedInAuditorID)
        {
            InitializeComponent();
            this.Da = new DataAccess();
            this.CurrentAuditorID = loggedInAuditorID;
            LoadAuditorOverview();

        }

        private void LogoutBtn_Click(object sender, EventArgs e)
        {
            this.Hide();
            LoginPage login = new LoginPage();
            login.Show();
        }

        //Project Panel
        private void ProjectsBtn_Click(object sender, EventArgs e)
        {
            this.ProjectsPanel.Visible = true;
            this.AssignmentsPanel.Visible = false;
            this.ReportsPanel.Visible = false;
            this.HomePanel.Visible = false;
            PopulateProjectsGridView();
        }

        private void ProjectsDGV_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.ProjectsDGV.ClearSelection();
        }

        private void PopulateProjectsGridView(string sql = "SELECT * FROM Projects;")
        {
            this.Ds = this.Da.ExecuteQuery(sql);
            this.ProjectsDGV.AutoGenerateColumns = false;
            this.ProjectsDGV.DataSource = this.Ds.Tables[0];
        }


        //Assignments Panel

        private void AssignmentsBtn_Click(object sender, EventArgs e)
        {
            this.ProjectsPanel.Visible = false;
            this.ReportsPanel.Visible = false;
            this.HomePanel.Visible = false;
            this.AssignmentsPanel.Visible = true;
            PopulateAssignmentsGridView();
            PopulateProjectCmb();
            GenerateAssignmentID();
        }

        private void PopulateAssignmentsGridView(string sql = null)
        {
            if (string.IsNullOrEmpty(sql))
            { 
                sql = @" SELECT AssignmentID,ProjectID,Status FROM Assignments WHERE AuditorID = '" + this.CurrentAuditorID + "';";
            }

            this.Ds = this.Da.ExecuteQuery(sql);
            this.AssignmentsDGV.AutoGenerateColumns = false; 
            this.AssignmentsDGV.DataSource = this.Ds.Tables[0];
        }

        private void AssignmentsClearBtn_Click(object sender, EventArgs e)
        {
            ClearAllAssignments();
            PopulateAssignmentsGridView();
            PopulateProjectCmb();
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

                if (this.Ds.Tables[0].Rows.Count == 1)
                {
                    this.Sql = @"UPDATE Assignments
                         SET TaskDescription = '" + this.TaskDescriptionTxt.Text +
                                 "', ProjectID = '" + projectID +
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
                                 "', '" + this.CurrentAuditorID +
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
                MessageBox.Show("An error has occurred during saving the Assignment data\n\n" + exc.Message);
            }

            ClearAllAssignments();
            PopulateAssignmentsGridView();
            PopulateProjectCmb();
            GenerateAssignmentID();
        }

        private void PopulateProjectCmb()
        {
            try
            {
                string sql = $@" SELECT DISTINCT p.ProjectID FROM Projects p
                INNER JOIN Assignments a ON p.ProjectID = a.ProjectID
                WHERE a.AuditorID = '{this.CurrentAuditorID}';";

                DataSet ds = this.Da.ExecuteQuery(sql);
                this.ProjectidCmb.DataSource = ds.Tables[0];
                this.ProjectidCmb.DisplayMember = "ProjectID";
                this.ProjectidCmb.ValueMember = "ProjectID";
                this.ProjectidCmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading projects for auditor: " + ex.Message);
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
            this.AssignmentStatusCmb.SelectedIndex = -1;
        }
       
        private void AssignmentsDGV_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.AssignmentsDGV.ClearSelection();
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
                        PopulateProjectCmb();
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
        private void CloseTaskDescriptionBtn_Click(object sender, EventArgs e)
        {
            this.TaskDescriptionPanel.Visible = false;
        }

        // Reports Panel
        private void ReportsBtn_Click(object sender, EventArgs e)
        {
           
            this.ProjectsPanel.Visible = false;
            this.AssignmentsPanel.Visible = false;
            this.HomePanel.Visible = false;
            this.ReportsPanel.Visible = true;
            PopulateReportsGridView();
            PopulateAssignmentCombo(this.CurrentAuditorID);
            GenerateReportID();
        }
        private void PopulateReportsGridView(string sql = "SELECT * FROM Reports;")
        {
            this.Ds = this.Da.ExecuteQuery(sql);
            this.ReportsDGV.AutoGenerateColumns = false;
            this.ReportsDGV.DataSource = this.Ds.Tables[0];
        }


        private void PopulateAssignmentCombo(string currentAuditorId)
        {
            try
            {
                string sql = $"SELECT AssignmentID FROM Assignments WHERE AuditorID = '{currentAuditorId}';";
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
            PopulateReportsGridView();
            PopulateAssignmentCombo(this.CurrentAuditorID);
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
                    string currentStatus = this.Ds.Tables[0].Rows[0]["ApprovalStatus"].ToString();

                    if (currentStatus == "Approved" || currentStatus == "Rejected")
                    {
                        currentStatus = "Pending";
                    }
                    else
                    {
                        currentStatus = this.Ds.Tables[0].Rows[0]["ApprovalStatus"].ToString(); // Keep existing status if Pending
                    }

                    this.Sql = @"UPDATE Reports
                               SET ReportDetails = '" + this.ReportDetailsTxt.Text +
                                  "', AssignmentID = '" + assignmentID +
                                  "', SubmittedDate = '" + submittedDate +
                                  "', ApprovalStatus = '" + currentStatus + "'" +
                                  " WHERE ReportID = '" + this.ReportidTxt.Text + "';";

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
                PopulateAssignmentCombo(this.CurrentAuditorID);
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
                PopulateAssignmentCombo(this.CurrentAuditorID);
            }
            catch (Exception exc)
            {
                MessageBox.Show("An error has occured during deletion\n" + exc.Message);
            }
            ClearAllReports();
            PopulateReportsGridView();
            PopulateAssignmentCombo(this.CurrentAuditorID);
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

        private void ReportsDGV_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.ReportsDGV.ClearSelection();   
        }

        // Home Panel
        private void Homebtn_Click(object sender, EventArgs e)
        { 
            this.ProjectsPanel.Visible = false;
            this.ReportsPanel.Visible = false;
            this.AssignmentsPanel.Visible = false;
            LoadAuditorOverview();
        }

        private void LoadAuditorOverview()
        {
            try
            {
                string P_pendingSql = $@"SELECT COUNT(DISTINCT P.ProjectID) FROM Projects P INNER JOIN Assignments A ON P.ProjectID = A.ProjectID
                                      WHERE A.AuditorID = '{CurrentAuditorID}' AND P.Status = 'Pending'";
                string P_activeSql = $@"SELECT COUNT(DISTINCT P.ProjectID) FROM Projects P INNER JOIN Assignments A ON P.ProjectID = A.ProjectID
                                      WHERE A.AuditorID = '{CurrentAuditorID}' AND P.Status = 'Ongoing'";
                string P_finishedSql = $@"SELECT COUNT(DISTINCT P.ProjectID) FROM Projects P INNER JOIN Assignments A ON P.ProjectID = A.ProjectID
                                      WHERE A.AuditorID = '{CurrentAuditorID}' AND P.Status = 'Completed'";
                string A_pendingSql = $@"SELECT COUNT(*) FROM Assignments 
                                      WHERE Status = 'Pending' AND AuditorID = '{CurrentAuditorID}'";
                string A_activeSql = $@"SELECT COUNT(*) FROM Assignments 
                                      WHERE Status = 'Ongoing' AND AuditorID = '{CurrentAuditorID}'";
                string A_finishedSql = $@"SELECT COUNT(*) FROM Assignments 
                                      WHERE Status = 'Completed' AND AuditorID = '{CurrentAuditorID}'";
                string R_pendingSql = $@"SELECT COUNT(*) FROM Reports R INNER JOIN Assignments A ON R.AssignmentID = A.AssignmentID
                                      WHERE R.ApprovalStatus = 'Pending' AND A.AuditorID = '{CurrentAuditorID}'";
                string R_approvedSql = $@"SELECT COUNT(*) FROM Reports R INNER JOIN Assignments A ON R.AssignmentID = A.AssignmentID
                                      WHERE R.ApprovalStatus = 'Approved' AND A.AuditorID = '{CurrentAuditorID}'";
                string R_rejectedSql = $@"SELECT COUNT(*) FROM Reports R INNER JOIN Assignments A ON R.AssignmentID = A.AssignmentID
                                      WHERE R.ApprovalStatus = 'Rejected' AND A.AuditorID = '{CurrentAuditorID}'";

                this.P_PendingTextLbl.Text = this.Da.ExecuteQueryTable(P_pendingSql).Rows[0][0].ToString();
                this.P_ActiveTextLbl.Text = this.Da.ExecuteQueryTable(P_activeSql).Rows[0][0].ToString();
                this.P_FinishedTextLbl.Text = this.Da.ExecuteQueryTable(P_finishedSql).Rows[0][0].ToString();

                this.A_PendingTextLbl.Text = this.Da.ExecuteQueryTable(A_pendingSql).Rows[0][0].ToString();
                this.A_ActiveTextLbl.Text = this.Da.ExecuteQueryTable(A_activeSql).Rows[0][0].ToString();
                this.A_FinishedTextLbl.Text = this.Da.ExecuteQueryTable(A_finishedSql).Rows[0][0].ToString();

                this.R_PendingTextLbl.Text = this.Da.ExecuteQueryTable(R_pendingSql).Rows[0][0].ToString();
                this.R_ApprovedTextLbl.Text = this.Da.ExecuteQueryTable(R_approvedSql).Rows[0][0].ToString();
                this.R_RejectedTextLbl.Text = this.Da.ExecuteQueryTable(R_rejectedSql).Rows[0][0].ToString();

                this.HomePanel.Visible = true;
                this.SubPanel2.Visible = true;
                this.SubPanel3.Visible = true;
                this.SubPanel4.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Auditor Overview: " + ex.Message);
            }
        }


    }
}
