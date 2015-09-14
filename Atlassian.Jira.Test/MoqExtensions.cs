using Moq.Language.Flow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira.Test
{
    public static class MoqExtensions
    {
        public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup,
           params TResult[] results) where T : class
        {
            setup.Returns(new Queue<TResult>(results).Dequeue);
        }

        public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup,
          params object[] results) where T : class
        {
            var queue = new Queue(results);
            setup.Returns(() =>
            {
                var result = queue.Dequeue();
                if (result is Exception)
                {
                    throw result as Exception;
                }
                return (TResult)result;
            });
        }
    }
}
