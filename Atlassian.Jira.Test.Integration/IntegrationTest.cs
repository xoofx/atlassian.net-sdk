using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;

namespace Atlassian.Jira.Test.Integration
{
    public class IntegrationTest
    {
        private readonly Jira _jira;
        private readonly Random _random;

        public IntegrationTest()
        {
            _jira = new Jira("http://localhost:2990/jira", "admin", "admin");
            _jira.Debug = true;
            _random = new Random();
        }

        [Fact]
        public void QueryWithZeroResults()
        {
            var issues = from i in _jira.Issues
                         where i.Created == new DateTime(2010,1,1)
                         select i;

            Assert.Equal(0, issues.Count());
        }

        [Fact]
        public void CreateAndQueryForIssueWithMinimumFieldsSet()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);

            var issue = new Issue()
            {
                Project = "TST",
                Type = "1",
                Summary = summaryValue
            };

            issue = _jira.CreateIssue(issue);


            var issues = (from i in _jira.Issues
                                where i.Key == issue.Key
                                select i).ToArray();

            Assert.Equal(1, issues.Count());

            Assert.Equal(summaryValue, issues[0].Summary);
            Assert.Equal("TST", issues[0].Project);
            Assert.Equal("1", issues[0].Type);
        }


        [Fact]
        public void CreateAndQueryIssueWithAllFieldsSet()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);

            var issue = new Issue()
            {
                Assignee = "admin",
                Description = "Test Description",
                DueDate = new DateTime(2011, 12, 12),
                Environment = "Test Environment",
                Project = "TST",
                Reporter = "admin",
                Type = "1",
                Summary = summaryValue
            };

            issue = _jira.CreateIssue(issue);


            var queriedIssues = (from i in _jira.Issues
                          where i.Key == issue.Key
                          select i).ToArray();

            Assert.Equal(summaryValue, queriedIssues[0].Summary);
        }

        [Fact]
        public void UpdateWithAllFieldsSet()
        {
            // arrange, create an issue to test.
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue()
            {
                Assignee = "admin",
                Description = "Test Description",
                DueDate = new DateTime(2011, 12, 12),
                Environment = "Test Environment",
                Project = "TST",
                Reporter = "admin",
                Type = "1",
                Summary = summaryValue
            };
            issue = _jira.CreateIssue(issue);


            // act, get an issue and update it
            var serverIssue = (from i in _jira.Issues
                                 where i.Key == issue.Key
                                 select i).ToArray().First();

            serverIssue.Description = "Updated Description";
            serverIssue.DueDate = new DateTime(2011, 10, 10);
            serverIssue.Environment = "Updated Environment";
            serverIssue.Summary = "Updated " + summaryValue;
            _jira.UpdateIssue(serverIssue);

            // assert, get the issue again and verify
            var newServerIssue = (from i in _jira.Issues
                               where i.Key == issue.Key
                               select i).ToArray().First();

            Assert.Equal("Updated " + summaryValue, newServerIssue.Summary);
            Assert.Equal("Updated Description", newServerIssue.Description);
            Assert.Equal("Updated Environment", newServerIssue.Environment);

            // Note: Dates returned from JIRA are UTC
            Assert.Equal(new DateTime(2011, 10, 10).ToUniversalTime(), newServerIssue.DueDate);
        }

        [Fact]
        public void UploadAndDownloadOfAttachments()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue()
            {
                Project = "TST",
                Type = "1",
                Summary = summaryValue
            };

            // create an issue, verify no attachments
            issue = _jira.CreateIssue(issue);
            Assert.Equal(0, issue.GetAttachments().Count);

            // upload an attachment
            File.WriteAllText("testfile.txt", "Test File Content");
            issue.AddAttachment("testfile.txt");

            var attachments = issue.GetAttachments();
            Assert.Equal(1, attachments.Count);
            Assert.Equal("testfile.txt", attachments[0].FileName);

            // download an attachment
            var tempFile = Path.GetTempFileName();
            attachments[0].Download(tempFile);
            Assert.Equal("Test File Content", File.ReadAllText(tempFile));
        }

        [Fact]
        public void AddingAndRetrievingComments()
        {
            var summaryValue = "Test Summary " + _random.Next(int.MaxValue);
            var issue = new Issue()
            {
                Project = "TST",
                Type = "1",
                Summary = summaryValue
            };

            // create an issue, verify no comments
            issue = _jira.CreateIssue(issue);
            Assert.Equal(0, issue.GetComments().Count);

            // Add a comment
            issue.AddComment("new comment");

            var comments = issue.GetComments();
            Assert.Equal(1, comments.Count);
            Assert.Equal("new comment", comments[0].Body);

        }

        [Fact]
        public void MaximumNumberOfIssuesPerRequest()
        {
            // create 2 issues with same summary
            var randomNumber = _random.Next(int.MaxValue);
            _jira.CreateIssue(new Issue() { Project = "TST", Type = "1", Summary = "Test Summary " + randomNumber });
            _jira.CreateIssue(new Issue() { Project = "TST", Type = "1", Summary = "Test Summary " + randomNumber }); 

            //set maximum issues and query
            _jira.MaxIssuesPerRequest = 1;
            var issues = from i in _jira.Issues
                         where i.Summary == randomNumber.ToString()
                         select i;

            Assert.Equal(1, issues.Count());

        }

        [Fact]
        public void QueryIssuesWithTakeExpression()
        {
            // create 2 issues with same summary
            var randomNumber = _random.Next(int.MaxValue);
            _jira.CreateIssue(new Issue() { Project = "TST", Type = "1", Summary = "Test Summary " + randomNumber });
            _jira.CreateIssue(new Issue() { Project = "TST", Type = "1", Summary = "Test Summary " + randomNumber });

            // query with take method to only return 1
            var issues = (from i in _jira.Issues
                         where i.Summary == randomNumber.ToString()
                         select i).Take(1);

            Assert.Equal(1, issues.Count());
        }

        [Fact]
        public void RetrieveIssueTypesForProject()
        {
            var issueTypes = _jira.GetIssueTypes("TST");

            Assert.Equal(4, issueTypes.Count());
            Assert.True(issueTypes.Any(i => i.Name == "Bug"));
        }

        [Fact]
        public void RetrievesIssuePriorities()
        {
            var priorities = _jira.GetIssuePriorities();

            Assert.True(priorities.Any(i => i.Name == "Blocker"));
        }

        [Fact]
        public void RetrievesIssueResolutions()
        {
            var resolutions = _jira.GetIssueResolutions();

            Assert.True(resolutions.Any(i => i.Name == "Fixed"));
        }

        [Fact]
        public void RetrievesIssueStatuses()
        {
            var statuses = _jira.GetIssueStatuses();

            Assert.True(statuses.Any(i => i.Name == "Open"));
        }

        /// <summary>
        /// https://bitbucket.org/farmas/atlassian.net-sdk/issue/3/serialization-error-when-querying-some
        /// </summary>
        [Fact]
        public void HandleRetrievalOfMessagesWithLargeContentStrings()
        {
            var issue = new Issue()
            {
                Project = "TST",
                Type = "1",
                Summary = "Serialization nastiness"
            };

            issue.Description = @"
{code}
[Fatal Error] :1:12: The entity ""nbsp"" was referenced, but not declared.
2011-07-28 10:11:03,102 WARN [StreamsCompletionService::thread-19] [streams.confluence.renderer.ContentEntityRendererFactory] stripGadgetMacros Unable to parse xml: &nbsp;
 -- url: /streams/plugins/servlet/streams | userName: anonymous
org.xml.sax.SAXParseException: The entity ""nbsp"" was referenced, but not declared.
        at org.apache.xerces.parsers.DOMParser.parse(Unknown Source)
        at org.apache.xerces.jaxp.DocumentBuilderImpl.parse(Unknown Source)
        at javax.xml.parsers.DocumentBuilder.parse(DocumentBuilder.java:124)
        at com.atlassian.streams.confluence.renderer.ContentEntityRendererFactory$ContentEntityRenderer.stripGadgetMacros(ContentEntityRendererFactory.java:249)
        at com.atlassian.streams.confluence.renderer.ContentEntityRendererFactory$ContentEntityRenderer.content(ContentEntityRendererFactory.java:222)
        at com.atlassian.streams.confluence.renderer.ContentEntityRendererFactory$ContentEntityRenderer.access$400(ContentEntityRendererFactory.java:95)
        at com.atlassian.streams.confluence.renderer.ContentEntityRendererFactory$ContentEntityRenderer$3.get(ContentEntityRendererFactory.java:196)
        at com.atlassian.streams.confluence.renderer.ContentEntityRendererFactory$ContentEntityRenderer$3.get(ContentEntityRendererFactory.java:192)
        at com.atlassian.streams.confluence.renderer.ContentEntityRendererFactory$ContentEntityRenderer$2.apply(ContentEntityRendererFactory.java:161)
        at com.atlassian.streams.confluence.renderer.ContentEntityRendererFactory$ContentEntityRenderer$2.apply(ContentEntityRendererFactory.java:158)
        at com.atlassian.streams.confluence.renderer.ContentEntityRendererFactory$ContentEntityRenderer.renderContentAsHtml(ContentEntityRendererFactory.java:108)
        at com.atlassian.streams.api.StreamsEntry$3.get(StreamsEntry.java:291)
        at com.atlassian.streams.api.StreamsEntry$3.get(StreamsEntry.java:288)
        at com.google.common.base.Suppliers$MemoizingSupplier.get(Suppliers.java:93)
        at com.atlassian.streams.api.StreamsEntry.renderContentAsHtml(StreamsEntry.java:280)
        at com.atlassian.streams.api.StreamsEntry.toStaticEntry(StreamsEntry.java:108)
        at com.atlassian.streams.internal.feed.FeedEntry.fromStreamsEntry(FeedEntry.java:70)
        at com.atlassian.streams.internal.feed.FeedEntry$1.apply(FeedEntry.java:77)
        at com.atlassian.streams.internal.feed.FeedEntry$1.apply(FeedEntry.java:73)
        at com.google.common.collect.Iterators$8.next(Iterators.java:697)
        at com.google.common.collect.Iterators$5.next(Iterators.java:514)
        at com.google.common.collect.Lists.newArrayList(Lists.java:132)
        at com.google.common.collect.Lists.newArrayList(Lists.java:113)
        at com.google.common.collect.ImmutableList.copyOf(ImmutableList.java:229)
        at com.atlassian.streams.internal.feed.FeedModel.<init>(FeedModel.java:66)
        at com.atlassian.streams.internal.feed.FeedModel.<init>(FeedModel.java:37)
        at com.atlassian.streams.internal.feed.FeedModel$Builder.build(FeedModel.java:231)
        at com.atlassian.streams.internal.LocalActivityProvider$FeedFetcher.doInTransaction(LocalActivityProvider.java:219)
        at com.atlassian.streams.internal.LocalActivityProvider$FeedFetcher.doInTransaction(LocalActivityProvider.java:192)
        at com.atlassian.sal.core.transaction.HostContextTransactionTemplate$1.doInTransaction(HostContextTransactionTemplate.java:25)
        at com.atlassian.sal.spring.component.SpringHostContextAccessor$1.doInTransaction(SpringHostContextAccessor.java:88)
        at org.springframework.transaction.support.TransactionTemplate.execute(TransactionTemplate.java:128)
        at com.atlassian.sal.spring.component.SpringHostContextAccessor.doInTransaction(SpringHostContextAccessor.java:82)
        at sun.reflect.GeneratedMethodAccessor99.invoke(Unknown Source)
        at sun.reflect.DelegatingMethodAccessorImpl.invoke(DelegatingMethodAccessorImpl.java:25)
        at java.lang.reflect.Method.invoke(Method.java:597)
        at com.atlassian.plugin.osgi.hostcomponents.impl.DefaultComponentRegistrar$ContextClassLoaderSettingInvocationHandler.invoke(DefaultComponentRegistrar.java:129)
        at $Proxy224.doInTransaction(Unknown Source)
        at sun.reflect.GeneratedMethodAccessor99.invoke(Unknown Source)
        at sun.reflect.DelegatingMethodAccessorImpl.invoke(DelegatingMethodAccessorImpl.java:25)
        at java.lang.reflect.Method.invoke(Method.java:597)
        at com.atlassian.plugin.osgi.bridge.external.HostComponentFactoryBean$DynamicServiceInvocationHandler.invoke(HostComponentFactoryBean.java:154)
        at $Proxy224.doInTransaction(Unknown Source)
        at com.atlassian.sal.core.transaction.HostContextTransactionTemplate.execute(HostContextTransactionTemplate.java:21)
        at sun.reflect.GeneratedMethodAccessor531.invoke(Unknown Source)
        at sun.reflect.DelegatingMethodAccessorImpl.invoke(DelegatingMethodAccessorImpl.java:25)
        at java.lang.reflect.Method.invoke(Method.java:597)
        at org.springframework.aop.support.AopUtils.invokeJoinpointUsingReflection(AopUtils.java:307)
        at org.springframework.osgi.service.importer.support.internal.aop.ServiceInvoker.doInvoke(ServiceInvoker.java:58)
        at org.springframework.osgi.service.importer.support.internal.aop.ServiceInvoker.invoke(ServiceInvoker.java:62)
        at org.springframework.aop.framework.ReflectiveMethodInvocation.proceed(ReflectiveMethodInvocation.java:171)
        at org.springframework.aop.support.DelegatingIntroductionInterceptor.doProceed(DelegatingIntroductionInterceptor.java:131)
        at org.springframework.aop.support.DelegatingIntroductionInterceptor.invoke(DelegatingIntroductionInterceptor.java:119)
        at org.springframework.aop.framework.ReflectiveMethodInvocation.proceed(ReflectiveMethodInvocation.java:171)
        at org.springframework.osgi.service.util.internal.aop.ServiceTCCLInterceptor.invokeUnprivileged(ServiceTCCLInterceptor.java:56)
        at org.springframework.osgi.service.util.internal.aop.ServiceTCCLInterceptor.invoke(ServiceTCCLInterceptor.java:39)
        at org.springframework.aop.framework.ReflectiveMethodInvocation.proceed(ReflectiveMethodInvocation.java:171)
        at org.springframework.osgi.service.importer.support.LocalBundleContextAdvice.invoke(LocalBundleContextAdvice.java:59)
        at org.springframework.aop.framework.ReflectiveMethodInvocation.proceed(ReflectiveMethodInvocation.java:171)
        at org.springframework.aop.support.DelegatingIntroductionInterceptor.doProceed(DelegatingIntroductionInterceptor.java:131)
        at org.springframework.aop.support.DelegatingIntroductionInterceptor.invoke(DelegatingIntroductionInterceptor.java:119)
        at org.springframework.aop.framework.ReflectiveMethodInvocation.proceed(ReflectiveMethodInvocation.java:171)
        at org.springframework.aop.framework.JdkDynamicAopProxy.invoke(JdkDynamicAopProxy.java:204)
        at $Proxy729.execute(Unknown Source)
        at com.atlassian.streams.internal.LocalActivityProvider$1$1.get(LocalActivityProvider.java:126)
        at com.atlassian.streams.internal.LocalActivityProvider$1$1.get(LocalActivityProvider.java:123)
        at com.atlassian.streams.internal.PassThruSessionManager.withSession(PassThruSessionManager.java:11)
        at com.atlassian.streams.internal.SwitchingSessionManager.withSession(SwitchingSessionManager.java:22)
        at com.atlassian.streams.internal.LocalActivityProvider$1.call(LocalActivityProvider.java:122)
        at com.atlassian.streams.internal.LocalActivityProvider$1.call(LocalActivityProvider.java:118)
        at com.atlassian.streams.internal.FeedBuilder$ToFeedCallable$1.call(FeedBuilder.java:112)
        at com.atlassian.streams.internal.FeedBuilder$ToFeedCallable$1.call(FeedBuilder.java:107)
        at java.util.concurrent.FutureTask$Sync.innerRun(FutureTask.java:303)
        at java.util.concurrent.FutureTask.run(FutureTask.java:138)
        at java.util.concurrent.Executors$RunnableAdapter.call(Executors.java:441)
        at java.util.concurrent.FutureTask$Sync.innerRun(FutureTask.java:303)
        at java.util.concurrent.FutureTask.run(FutureTask.java:138)
        at com.atlassian.util.concurrent.LimitedExecutor$Runner.run(LimitedExecutor.java:96)
        at com.atlassian.sal.core.executor.ThreadLocalDelegateRunnable.run(ThreadLocalDelegateRunnable.java:34)
        at java.util.concurrent.ThreadPoolExecutor$Worker.runTask(ThreadPoolExecutor.java:886)
        at java.util.concurrent.ThreadPoolExecutor$Worker.run(ThreadPoolExecutor.java:908)
        at java.lang.Thread.run(Thread.java:662)
{code}";

            var serverIssue = _jira.CreateIssue(issue);
        }
    }
}
