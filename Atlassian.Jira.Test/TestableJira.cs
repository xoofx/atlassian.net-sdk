using System.Globalization;
using System.Linq.Expressions;
using System.Threading;
using Atlassian.Jira.Linq;
using Atlassian.Jira.Remote;
using Moq;

namespace Atlassian.Jira.Test
{
    public class TestableJira : Jira
    {
        public Mock<IJqlExpressionVisitor> Translator;
        public Mock<IJiraRestClient> RestService;
        public Mock<IFileSystem> FileSystem;
        public Mock<IIssueTypeService> IssueTypeService;
        public Mock<IIssueFieldService> IssueFieldService;
        public Mock<IIssueFilterService> IssueFilterService;
        public Mock<IIssueService> IssueService;
        public Mock<IIssuePriorityService> IssuePriorityService;
        public Mock<IIssueResolutionService> IssueResolutionService;

        internal static readonly CultureInfo TestCulture = CultureInfo.InvariantCulture;

        private TestableJira()
            : base(new ServiceLocator())
        {
            RestService = new Mock<IJiraRestClient>();
            FileSystem = new Mock<IFileSystem>();
            Translator = new Mock<IJqlExpressionVisitor>();
            IssueTypeService = new Mock<IIssueTypeService>();
            IssueFieldService = new Mock<IIssueFieldService>();
            IssueFilterService = new Mock<IIssueFilterService>();
            IssueService = new Mock<IIssueService>();
            IssuePriorityService = new Mock<IIssuePriorityService>();
            IssueResolutionService = new Mock<IIssueResolutionService>();

            Services.Register<IIssueTypeService>(() => IssueTypeService.Object);
            Services.Register<IIssueFieldService>(() => IssueFieldService.Object);
            Services.Register<IIssueFilterService>(() => IssueFilterService.Object);
            Services.Register<IIssueService>(() => IssueService.Object);
            Services.Register<IJqlExpressionVisitor>(() => Translator.Object);
            Services.Register<IFileSystem>(() => FileSystem.Object);
            Services.Register<IJiraRestClient>(() => RestService.Object);
            Services.Register<IIssuePriorityService>(() => IssuePriorityService.Object);
            Services.Register<IIssueResolutionService>(() => IssueResolutionService.Object);

            Translator.Setup(t => t.Process(It.IsAny<Expression>())).Returns(new JqlData() { Expression = "dummy expression" });
        }

        public static TestableJira Create()
        {
            // switch thread locale to avoid "2016/01/01 does not equal 2016.01.01" errors
            Thread.CurrentThread.CurrentCulture = TestCulture;
            Thread.CurrentThread.CurrentUICulture = TestCulture;

            return new TestableJira();
        }

        public void SetupIssues(params RemoteIssue[] remoteIssues)
        {
            this.IssueService.SetupIssues(this, remoteIssues);
        }
    }
}
