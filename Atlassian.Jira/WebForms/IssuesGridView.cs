using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Drawing;

namespace Atlassian.Jira.WebForms
{
    public class IssuesGridView: GridView
    {
        public IssuesGridView()
        {
            this.EnableViewState = false;
            this.Width = Unit.Percentage(100);

            PrepareColumns();
            PrepareStyles();
        }

        private void PrepareColumns()
        {
            this.AutoGenerateColumns = false;
            this.Columns.Add(new BoundField() { HeaderText = "Project", DataField = "Project" });
            this.Columns.Add(new BoundField() { HeaderText = "Key", DataField = "Key" });
            this.Columns.Add(new BoundField() { HeaderText = "Summary", DataField = "Summary" });
            this.Columns.Add(new BoundField() { HeaderText = "Description", DataField = "Description" });
            this.Columns.Add(new BoundField() { HeaderText = "Environment", DataField = "Environment" });
            this.Columns.Add(new BoundField() { HeaderText = "Assignee", DataField = "Assignee" });
            this.Columns.Add(new BoundField() { HeaderText = "Votes", DataField = "Votes" });
            this.Columns.Add(new BoundField() { HeaderText = "Status", DataField = "Status" });
            this.Columns.Add(new BoundField() { HeaderText = "Type", DataField = "Type" });
        }

        private void PrepareStyles()
        {
            this.CellPadding = 4;
            this.ForeColor = ColorTranslator.FromHtml("#333333");
            this.GridLines = System.Web.UI.WebControls.GridLines.None;

            this.AlternatingRowStyle.BackColor = Color.White;
            this.AlternatingRowStyle.ForeColor = ColorTranslator.FromHtml("#284775");

            this.HeaderStyle.BackColor = ColorTranslator.FromHtml("#5D7B9D");
            this.HeaderStyle.Font.Bold = true;
            this.HeaderStyle.ForeColor = Color.White;

            this.RowStyle.BackColor = ColorTranslator.FromHtml("#F7F6F3");
            this.RowStyle.ForeColor = ColorTranslator.FromHtml("#333333");

            this.RowStyle.HorizontalAlign = System.Web.UI.WebControls.HorizontalAlign.Center;

        }
    }
}
