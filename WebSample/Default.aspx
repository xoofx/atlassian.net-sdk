<%@ Page Language="C#" %>
<%@ Import Namespace="Atlassian.Jira" %>
<%@ Register Namespace="Atlassian.Jira.Web" TagPrefix="jira" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        var jira = new Jira(
           "http://localhost:8080",
           "admin",
           "admin");

        var issues = from i in jira.IssueSearch()
                     where i.Assignee == "admin"
                     select i;

        issues1.DataSource = issues;
        issues1.DataBind();
    }
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <jira:IssuesGridView runat="server" id="issues1"></jira:IssuesGridView>

        
    </div>
    </form>
</body>
</html>
