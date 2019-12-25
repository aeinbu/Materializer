# Materializer
Classes can only be combined by inheriting, while interfaces can be combined.

With `Materializer` you can define your DTOs as interfaces without the need to create classes for each of the combinations you'll want to use.

Given the following interfaces:
```csharp
inferface IOne
{
	int First { get; set; }
	string Second { get; set; }
}

inferface ITwo
{
	DateTime Third { get; set; }
	Double Fourth { get; set; }
}

inferface ICombined : IOne, ITwo
{
}
```
Without `Materializer` you would need to write a class for each of the ones you want to use as DTO, producing a lot of overhead code not really worth anything.
With `Materializer` you can instantiate each interface directly.

```csharp
var materializer = new Materializer();

// create an object representing the simple IOne interface
var obj1 = materializer.New<IOne>();
obj1.First = 5;
obj1.Second = "Mary Poppins";

// create an object representing the combination interface, ICombined
var obj2 = materializer.New<ICombined>();
obj2.First = 10;
obj2.Second = "Peter Pan";
obj2.Third = DateTime.Now;
obj2.Fourth = 1.0;
```

## Serializable support

### NewtonSoft.Json
The created objects are serializable with `NewtonSoft.Json` without setting any options. (See the tests for sample code.)

### .NET serialization
To be serializable with .NET serialization, a class must have the `[Serializable]` attribute. You can turn this on with the `forSerializable` option, like this:
```csharp
var materializer = new Materializer(forSerializable: true);
```
  When you set the `forSerializable` option on a serializer, all classes made with that serializer will have the `[Serializable]` attribute:

