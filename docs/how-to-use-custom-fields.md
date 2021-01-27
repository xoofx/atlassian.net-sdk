# Use Custom Field 'Name' instead of 'Id' #
The API exposed to clients uses the 'name' of the custom field instead of the 'id'. The 'name' of a custom field can be viewed directly on the web UI so it is easier to discover.

Internally, the SDK looks up the 'id' of the custom field by querying the metadata of custom fields from JIRA.

# Reading custom fields #

```csharp
// Get a custom field with single value.
var singleValue = issue["My Field Name"]; 

//  Get a custom field with multiple values.
var multiValue = issue.CustomFields["My Field Name"].Values; 

// Get a cascading select custom field.
var cascadingSelect = issue.CustomFields.GetCascadingSelectField("My Field Name");

// Deserialize the custom field value to a type (ie. user picker)
 var user = issue.CustomFields.GetAs<JiraUser>("User Field");
 var users = issue.CustomFields.GetAs<JiraUser[]>("Users Field");

```

# Writing to a custom field

```csharp
// Set a single value custom field
issue["My CustomField"] = "Updated Field";

// Set a multi-value custom field
issue.CustomFields.AddArray("Custom Labels Field", "label1", "label2");

// Set a cascading-select custom field.
issue.CustomFields.AddCascadingSelectField("Custom Cascading Select Field", "Option3");

// Save all your changes to the server.
await issue.SaveChangesAsync();

```

# Custom Serialization #
The SDK has serializers for the built in custom field types available by JIRA Core. Users can register their own serializers to handle other custom field types:

### 1. Implement ICustomFieldValueSerializer ###
This class will handle serializing between JSON (used by JIRA) and a string[] (used internally by the SDK).

```csharp
public class MyCustomFieldValueSerializer : ICustomFieldValueSerializer
{
    public string[] FromJson(JToken json)
    {
        // your code
    }

    public JToken ToJson(string[] values)
    {
        // your code
    }
}
```

### 2. Discover the custom field type ###
In order to register a custom field serializer the custom field type must be known. One way to do this is to use the SDK to call Jira.Fields.GetCustomFieldsAsync(). Make sure to turn on request tracing as described [here](https://bitbucket.org/farmas/atlassian.net-sdk/wiki/How%20to%20Debug%20Problems) which will print the response into trace and you will be able to see the custom field metadata from JIRA. You can see the custom field type in the returned JSON

```csharp
[GET] Response for Url: rest/api/2/field
[
...
  {
    "clauseNames": [
      "cf[10311]",
      "Custom Url Field"
    ],
    "custom": true,
    "id": "customfield_10311",
    "key": "customfield_10311",
    "name": "Custom Url Field",
    "navigable": true,
    "orderable": true,
    "schema": {
      "custom": "com.atlassian.jira.plugin.system.customfieldtypes:url",  <-- CUSTOM FIELD TYPE
      "customId": 10311,
      "type": "string"
    },
    "searchable": true
  }
...
]

```

### 3. Register the new serializer ###

```csharp
var settings = new JiraRestClientSettings();
settings.CustomFieldSerializers.Add("com.atlassian.jira.plugin.system.customfieldtypes:url", new MyCustomFieldValueSerializer());
return Jira.CreateRestClient(<url>, <user>, <pass>, settings);
```


# Built-in Custom Serialization #

## Sprint serializer ##

The format of the sprint changed in Jira, depending on the Jira version that you target you may need to switch to the new serializer:

```
var settings = new JiraRestClientSettings();
settings.CustomFieldSerializers["com.pyxis.greenhopper.jira:gh-sprint"] = new new GreenhopperSprintJsonCustomFieldValueSerialiser();
var jira = new Jira("url", "user", "password", settings);
```