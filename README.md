NLog.StructuredLogging.Json
=======================

[![Join the chat at https://gitter.im/justeat/NLog.StructuredLogging.Json](https://badges.gitter.im/justeat/NLog.StructuredLogging.Json.svg)](https://gitter.im/justeat/NLog.StructuredLogging.Json?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

[![Build status](https://ci.appveyor.com/api/projects/status/kvpbuiiljr1hdhv8?svg=true)](https://ci.appveyor.com/project/justeattech/nlog-structuredlogging-json)

[Get the package on NuGet](https://www.nuget.org/packages/NLog.StructuredLogging.Json/)

For use with [NLog](http://nlog-project.org/), [NXLog](http://nxlog.org/) and [Kibana](https://www.elastic.co/products/kibana).

Render one JSON object per line and parameters as properties for each `LogEventInfo` message.

##What problems does this solve?

### Structured logging

When logging without StructuredLogging.Json, the "Message" field is used to hold unstructured data, e.g.:

```
@LogType: nlog
Level: Warn
Message: Order 1234 resent to Partner 4567
```

When we want to query Kibana for all occurrences of this log message, we have to do partial string matching as the message is slightly different each time.
When we want to query kibana for all messages related to this order, we also have to do partial string matching on the message as the orderId is embedded in the message.

When logging with StructuredLogging.Json, the data can be structured:

```
@LogType: nlog
Level: Warn
Message: Order resent to Partner
OrderId: 1234
PartnerId: 4567
```

This makes it much easier to search Kibana for this exact message text and see all the times that this log statement was fired, across time.
We can also very easily search for all the different log messages related to a particular orderId, partnerId, or any other fields that can be logged.

### Simpler, more flexible logging configuration

No need for a custom nxlog configuration file, and no need to specify all the columns used.


## How to get it


1. Update the dependencies as below
2. Install the `NLog.StructuredLogging.Json` package from NuGet
3. Update your NLog config so you write out JSON with properties
4. Add additional properties when you log

1. Update the dependencies
------------------------------------------

- Ensure you have version of NLog >= 4.3.0 (assembly version 4.0.0.0 - remember to update any redirects)
- Ensure you have version of Newtonsoft.Json >= 9.0.1


```
Update-Package NLog
Update-Package Newtonsoft.Json
```

2. Install the NLog.StructuredLogging.Json renderer from NuGet
----------------------------------------
Make sure the DLL is copied to your output folder

```
Install-Package NLog.StructuredLogging.Json
```

3. Update your NLog config so you write out JSON with properties
----------------------------------------------------------------
NLog needs to write to JSON using the `structuredlogging.json` layout renderer.<br />
The `structuredlogging.json` layout renderer is declared in this project.<br />
Any DLLs that start with NLog. are automatically loaded by NLog at runtime in your app.<br />
* [Copy and replace your nlog.config with this example nlog.config in your solution](Examples/nlog.config)

4. Write additional properties to the NLog.LogEvent object when logging
-----------------------------------------------------------------------

## Usage

Use the log properties to add extra fields to the JSON. You can add any contextual data values here:

```c#
using NLog.StructuredLogging.Json;

...

logger.ExtendedInfo("Sending order", new { OrderId = 1234, RestaurantId = 4567 } );


logger.ExtendedWarn("Order resent", new { OrderId = 1234, CustomerId = 4567 } );

logger.ExtendedError("Could not contact customer", new { CustomerId = 1234, HttpStatusCode = 404 } );

logger.ExtendedException(ex, "Error sending order to Restaurant", new { OrderId = 1234, RestaurantId = 4567 } );

```

The last parameter is an anonymous tuple, and is used as a bag of named values.  The property names and values on this tuple become field names and corresponding values.


### Logging data from exceptions

If exceptions are logged with `ExtendedException` then the name-value pairs in [the exception's data collection](https://msdn.microsoft.com/en-us/library/system.exception.data.aspx) are recorded.

e.g. where we do:

```csharp
var restaurant = _restaurantService.GetRestaurant(restaurantId);
if (restaurant == null)
{
	throw new RestaurantNotFoundException();
}
```

We can improve on this with:

```csharp
var restaurant = _restaurantService.GetRestaurant(restaurantId);
if (restaurant == null)
{
	var ex = RestaurantNotFoundException();
	ex.Data.Add("RestaurantId", restaurantId);
	throw ex;
}
```
This is useful where the exception is caught and logged by a global "catch-all" exception handler which will have no knowledge of the context in which the exception was thrown.

Use the exception's `Data` collection rather than adding properties to exception types to store values.

The best practices and pitfalls below also apply to exception data, as these values are serialised in the same way to the same destination.

### Logging inner exceptions 

You do not need to explicitly log inner exceptions, or exceptions contained in an `AggregateException`. They are automatically logged in both cases. 
Each inner exception is logged as a separate log entry, so that the inner exceptions can be searched for all the usual fields such as `ExceptionMessage` or `ExceptionType`.

When an exception has one or more inner exceptions, some extra fields are logged: `ExceptionIndex`, `ExceptionCount` and `ExceptionTag`.

 * ExceptionCount: Tells you have many exceptions were logged together.
 * ExceptionIndex: This exception's index in the grouping.
 * ExceptionTag: a unique guid identifier that is generated and applied to the exceptions in the group. Searching for this guid should show you all the grouped exceptions and nothing else.
 
 e.g. logging an exception with 2 inner exceptions might produce these log entries:
 
 ````
ExceptionMessage: "Outer message"
ExceptionType: "ArgumentException"
ExceptionIndex: 1
ExceptionCount: 3
ExceptionTag: "6fc5d910-3335-4eba-89fd-f9229e4a29b3"

ExceptionMessage: "Mid message"
ExceptionType: "ApplicationException"
ExceptionIndex: 2
ExceptionCount: 3
ExceptionTag: "6fc5d910-3335-4eba-89fd-f9229e4a29b3"

ExceptionMessage: "inner message"
ExceptionType: "NotImplementedException"
ExceptionIndex: 3
ExceptionCount: 3
ExceptionTag: "6fc5d910-3335-4eba-89fd-f9229e4a29b3"
````


### Best practices

- The message logged **should be the same every time**. It should be a constant string, not a string formatted to contain data values such as ids or quantities. Then it is easy to search for.
- The message logged **should be distinct** i.e. not the same as the message produced by an unrelated log statement. Then searching for it does not match unrelated things as well.
- The message **should be a reasonable length** i.e. longer than a word, but shorter than an essay.
- The data **should be simple values**. Use field values of types e.g. `string`, `int`, `decimal`, `DateTimeOffset`, or `enum` types. StructuredLogging.Json does not log hierarchical data, just a flat list of key-value pairs. The values are serialised to string with some simple rules:
  - Nulls are serialised as empty strings.
  - `DateTime` values (and `DateTime?`, `DateTimeOffset` and `DateTimeOffset?`) are serialised to string in [ISO8601 date and time format](https://en.wikipedia.org/wiki/ISO_8601).
  - Everything else is just serialised with `.ToString()`. This won't do anything useful for your own types unless you override `.ToString()`. See the "Code Pitfalls" below.
- The data fields **should have consistent names and values**. Much of the utility of Kibana is from collecting logs from multiple systems and searching across them.  e.g. if one system logs request failures with a data field `StatusCode: 404` and another system with `HttpStatusCode: NotFound` then it will be much harder to search and aggregate logging data across these systems.

### Code pitfalls

#### Reserved field names

The enforced set of attributes is hard-coded. Supplemental data (from logProperties or the exception data bag), to avoid colliding with this, will be emitted with `data_` or `ex_` prefixes.

#### No format strings

Don't do this:

```csharp
_logger.ExtendedWarn("Order {0} resent", new { OrderId = 1234 } );`
```

As there's no format string, the `{0}` is not filled in.

#### No simple data values

Don't do

```csharp
int orderId = 1234
_logger.ExtendedWarn("Order resent", orderId);
```

as the last parameter needs to be an object with named properties on it.

#### No nested data values

Don't serialise complex objects such as domain objects or DTOs as values, e.g.:

```csharp
var orderDetails = new OrderDetails
  {
     OrderId = 123,
	 Time = DateTimeOffset.UtcNow.AddMinutes(45)
  };

// let's log the OrderDetails
_logger.ExtendedInfo("Order saved", new { OrderDetails = orderDetails });
```

The `orderDetails` object will be serialised with `ToString()`. Unless this method is overridden in the OrderDetails type declaration, it will not produce any useful output. And if it is overridden, we only get one key-value pair, when instead the various values such as `OrderId` are better logged in separate fields.


#### No debug log level


There is no `logger.ExtendedDebug()` method. It could be added if need be, but there's not much point: use `logger.ExtendedInfo` instead. When all messages of every level get sent to kibana for later filtering, there's no need for fine-grained log levels.

## Contributors

Started for JustEat Technology by Alexander Williamson in 2015.


And then battle-tested in production with code and fixes from: 
Jaimal Chohan, Jeremy Clayden, Andy Garner, Kenny Hung, Henry Keen, Payman Labbaf, João Lebre, Peter Mounce, Simon Ness, Mykola Shestopal, Anthony Steele.

