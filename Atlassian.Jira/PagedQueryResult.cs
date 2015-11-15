using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atlassian.Jira
{
    /// <summary>
    /// PagedQueryResult that can be deserialized from default JIRA paging response.
    /// </summary>
    internal class PagedQueryResult<T> : IPagedQueryResult<T>
    {
        private readonly IEnumerable<T> _enumerable;
        private readonly int _startAt;
        private readonly int _itemsPerPage;
        private readonly int _totalItems;

        public PagedQueryResult(IEnumerable<T> enumerable, int startAt, int itemsPerPage, int totalItems)
        {
            _enumerable = enumerable;
            _startAt = startAt;
            _itemsPerPage = itemsPerPage;
            _totalItems = totalItems;
        }

        public static PagedQueryResult<T> FromJson(JObject pagedJson, IEnumerable<T> items)
        {
            return new PagedQueryResult<T>(
                items,
                GetPropertyOrDefault<int>(pagedJson, "startAt"),
                GetPropertyOrDefault<int>(pagedJson, "maxResults"),
                GetPropertyOrDefault<int>(pagedJson, "total"));
        }

        public int StartAt
        {
            get { return _startAt; }
        }

        public int ItemsPerPage
        {
            get { return _itemsPerPage; }
        }

        public int TotalItems
        {
            get { return _totalItems; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        private static TValue GetPropertyOrDefault<TValue>(JObject json, string property)
        {
            var val = json[property];

            if (val == null || val.Type == JTokenType.Null)
            {
                return default(TValue);
            }
            else
            {
                return val.Value<TValue>();
            }
        }
    }
}