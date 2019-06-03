# SimpleCacheForTests
A simple solution for persitsting objects from .net Entity Framework Queries, so the tests can run direct to the database, but the query results are cached to a file and the tests get the same input each testrun.

## Example:
Assuming the basic call is `MyContext.GetFromDatabase(int id);` amd the assemblyname amd namespace is `MyAssembly.MyNamespace`.


```
using SimpleCacheForTests;

using (ShimsContext.Create())
{
    // the shim call from Microsoft.QualityTools.Testing.Fakes.dll 
    MyAssembly.MyNamespace.Fakes.MyContext.AllInstances.GetFromDatabase = (x, id)
         =>
     {
         return ObjectCache.FromQueryCache(() => { 
            // base call without shims (or there would be a loop...
            return ShimsContext.ExecuteWithoutShims(() => x.GetFromDatabase(id));  
         }, "test.json");
     };

    var myTestObject = new MyTestClass();
    // the database call is nested inside MethodThatCallsInternallyGetFromDatabase, 123 is the id 
    // retrunValue and expected are assumed values
    var returnValue = myTestObject.MethodThatCallsInternallyGetFromDatabase(123);
   
    var expected = expected;
    Assert.IsTrue(returnValue == expected);
};  
``` 
