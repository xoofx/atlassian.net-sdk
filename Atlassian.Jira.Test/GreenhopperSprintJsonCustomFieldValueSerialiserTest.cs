using Atlassian.Jira.Remote;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Atlassian.Jira.Test
{
    public class GreenhopperSprintJsonCustomFieldValueSerialiserTest
    {
        [Fact]
        public void Test_FromJson()
        {
            var serialiser = new GreenhopperSprintJsonCustomFieldValueSerialiser();

            var actual = serialiser.FromJson(@"[
{
    'id': 1,
    'name': 'Sprint1',
    'state': 'active',
    'boardId': 1,
    'goal': '',
    'startDate': '2010-11-07T13:51:21.353Z',
    'endDate': '2011-07-17T12:51:00.000Z'
},{
    'id': 2,
    'name': 'Sprint2',
    'state': 'active',
    'boardId': 2,
    'goal': '',
    'startDate': '2019-11-07T13:51:21.353Z',
    'endDate': '2020-07-17T12:51:00.000Z'
}
            ]".Replace('\'', '\"'));

            var expected = new[] {"Sprint2", "Sprint1"};
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Test_ToJson()
        {
            var serialiser = new GreenhopperSprintJsonCustomFieldValueSerialiser();

            var actual = serialiser.ToJson(new[]
            {
                "Sprint1",
                "Sprint2",
            });

            var expected = (JToken) "Sprint1";
            Assert.Equal(expected.ToString(), actual.ToString());
        }
    }
}
