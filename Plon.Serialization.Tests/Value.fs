module Plon.Serialization.Tests.Serializer

open Xunit
open ComplexObjects

[<Fact>]
let ``Serialize and deserialize string`` () =
    let obj = "test"
    let serialized = Plon.Serialization.Serializer.serialize obj
    let deserialized = Plon.Serialization.Deserializer.deserialize<string> serialized
    Assert.Equal(obj, deserialized)

[<Fact>]
let ``Serialize and deserialize bool`` () =
    let obj = true
    let serialized = Plon.Serialization.Serializer.serialize obj
    let deserialized = Plon.Serialization.Deserializer.deserialize<bool> serialized
    Assert.Equal(obj, deserialized)

[<Fact>]
let ``Serialize and deserialize int`` () =
    let obj = 42
    let serialized = Plon.Serialization.Serializer.serialize obj
    let deserialized = Plon.Serialization.Deserializer.deserialize<int> serialized
    Assert.Equal(obj, deserialized)
    
[<Fact>]
let ``Serialize and deserialize custom object`` () =
    let obj = Engine("V8", 249)
    let serialized = Plon.Serialization.Serializer.serialize obj
    let deserialized = Plon.Serialization.Deserializer.deserialize<Engine> serialized
    Assert.Equal(obj.Model, deserialized.Model)
    Assert.Equal(obj.HorsePower, deserialized.HorsePower)
    