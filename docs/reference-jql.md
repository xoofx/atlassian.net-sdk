# JQL Support

Using LINQ to query for JIRA issues works by translating the query expression into [JQL](http://confluence.atlassian.com/display/JIRA/Advanced+Searching).

## Supported Operators ##
* Equals
* NotEquals
* Contains
* NotContains
* GreaterThan
* LessThan
* GreaterThanOrEquals
* LessThanOrEquals
* Is
* IsNot

## Supported Keywords ##
* And
* Or
* Empty
* Null
* OrderBy

## Supported Fields ##
* Summary
* Description
* Environment
* Assignee
* Key
* Priority
* Project
* Reporter
* Resolution
* Status
* Type
* Vote
* Created
* DueDate
* Updated
* Components
* Fix Version
* Affected Version
* Custom Fields
