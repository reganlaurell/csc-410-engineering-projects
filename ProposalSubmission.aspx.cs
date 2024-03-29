﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;           
using System.Data.SqlClient; 
using System.Configuration;  

public partial class ProposalSubmission : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        needDropDown.DataSource = needDataSource;
        needDropDown.DataBind();
        needDropDown.Items.Insert(0, new ListItem("Select", "%"));

        clientTypeDropDown.DataSource = cleintTypeDataSource;
        clientTypeDropDown.DataBind();
        clientTypeDropDown.Items.Insert(0, new ListItem("Select", "%"));

        orgCategoryDropDown.DataSource = orgCategoryDataSource;
        orgCategoryDropDown.DataBind();
        orgCategoryDropDown.Items.Insert(0, new ListItem("Select", "%"));
    }

    protected void ProposalSubmission_Click(object sender, EventArgs e)
    {
        string projectTitle = replaceSingleQuote(titleText.Text);
        string projectDescription = replaceSingleQuote(proposalDescriptionText.Text);

        string projectNeed = needDropDown.SelectedItem.Text;
        string projectClientType = clientTypeDropDown.SelectedItem.Text;
        string projectCategory = orgCategoryDropDown.SelectedItem.Text;

        if (allFieldsFilledIn(projectTitle, projectDescription, projectNeed, projectClientType, projectCategory) 
            && characterLimitsMet(projectTitle, projectDescription))
        {
            proposalSubmissionDataSource.InsertParameters["Title"].DefaultValue = projectTitle;
            proposalSubmissionDataSource.InsertParameters["Description"].DefaultValue = projectDescription;
            proposalSubmissionDataSource.InsertParameters["TypeOfNeed"].DefaultValue = projectNeed;
            proposalSubmissionDataSource.InsertParameters["ClientType"].DefaultValue = projectClientType;
            proposalSubmissionDataSource.InsertParameters["OrganizationCategory"].DefaultValue = projectCategory;

            try
            {
                proposalSubmissionDataSource.Insert();

                int? projectID = null;
                string projectStatus = "Submitted";
                try
                {
                    SqlConnection conn = new SqlConnection(getConnectionString());
                    conn.Open();

                    SqlCommand command = new SqlCommand("Select ProjectID FROM Projects WHERE Title=@title", conn);
                    command.Parameters.AddWithValue("@title", projectTitle);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            projectID = Convert.ToInt32(reader["ProjectID"]);
                        }
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    statusLabel.Text = ex.Message;
                }

                projectStatusDataSource.InsertParameters["ProjectID"].DefaultValue = Convert.ToString(projectID);
                projectStatusDataSource.InsertParameters["Status"].DefaultValue = projectStatus;
                projectStatusDataSource.InsertParameters["DateUpdated"].DefaultValue = Convert.ToString(DateTime.Now);

                try
                {
                    projectStatusDataSource.Insert();
                }
                catch(Exception ex)
                {
                    statusLabel.Text = ex.Message;
                }

                statusLabel.Text = "Proposal was successfully added";
                resetProposalFields();
                Response.Redirect("~/Default.aspx");
            }
            catch (Exception ex)
            {
                statusLabel.Text = ex.Message;
            }
        }
    }

    private string replaceSingleQuote(string dataField)
    {
        return dataField.Replace("'", "");
    }

    private bool allFieldsFilledIn(string title, string description, string need, string clientType, string category)
    {
        if (!clientType.Equals("Select") && !category.Equals("Select") && !need.Equals("Select")
            && title.Length > 0 && description.Length > 0)
        {
            return true;
        }
        else
        {
            statusLabel.Text = "Please fill in information for all fields";
            return false;
        }
    }

    private bool characterLimitsMet(string title, string description)
    {
        if (title.Length <= 50 && description.Length <= 500)
        {
            titleCharMaxLabel.Text = "";
            descriptionCharMaxLabel.Text = "";
            return true;
        } 
        if(title.Length > 50)
        {
            titleCharMaxLabel.Text = "Proposal Title Max 50 Characters";
        }

        if(description.Length > 500)
        {
            descriptionCharMaxLabel.Text = "Proposal Description Max 500 Characters";
        }

        return false;
    }

    private string getConnectionString()
    {
        return ConfigurationManager.ConnectionStrings["EngineeringProjectsConnectionString"].ConnectionString;
    }

    private void resetProposalFields()
    {
        titleText.Text = "";
        proposalDescriptionText.Text = "";
        needDropDown.SelectedIndex = -1;
        clientTypeDropDown.SelectedIndex = -1;
        orgCategoryDropDown.SelectedIndex = -1;
    }
}