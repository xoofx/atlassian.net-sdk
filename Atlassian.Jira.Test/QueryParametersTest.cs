using System.Linq;
using Xunit;

namespace Atlassian.Jira.Test
{
    public class QueryParametersTest
    {
        [Fact]
        public void GetQueryParametersFromPath()
        {
            // Arrange
            var url = "?field1=9&field2=Test";

            // Act
            var parameters =  QueryParametersHelper.GetQueryParametersFromPath(url);

            // Assert
            Assert.NotNull(parameters);
            Assert.Equal(parameters.Count(), 2);

            Assert.Equal(parameters.First().Name, "field1");
            Assert.Equal(parameters.First().Value, "9");

            Assert.Equal(parameters.ElementAt(1).Name, "field2");
            Assert.Equal(parameters.ElementAt(1).Value, "Test");
        }

        [Fact]
        public void GetQueryParametersFromPathNoEqual()
        {
            // Arrange
            var url = "?field1";

            // Act
            var parameters = QueryParametersHelper.GetQueryParametersFromPath(url);

            // Assert
            Assert.NotNull(parameters);
            Assert.Equal(parameters.Count(), 1);

            Assert.Equal(parameters.First().Name, "field1");
            Assert.Equal(parameters.First().Value, "");
        }
        
        [Fact]
        public void GetQueryParametersFromPathMultipleEquals()
        {
            // Arrange
            var url = "?field1=value=string==";

            // Act
            var parameters = QueryParametersHelper.GetQueryParametersFromPath(url);

            // Assert
            Assert.NotNull(parameters);
            Assert.Equal(parameters.Count(), 1);

            Assert.Equal(parameters.First().Name, "field1");
            Assert.Equal(parameters.First().Value, "value=string==");
        }
    }
}
