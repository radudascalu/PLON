module Plon.Serialization.Tests.Serializer

open System
open Xunit
open Plon.Serialization.Value.Serializer
open ComplexObjects
open Plon.Serialization.Value
open Plon.Serialization.Common
open System.Collections.Generic

[<Fact>]
let ``Serialize string`` () =
    let serialized = serialize "abc"
    Assert.Equal("\"abc\"", serialized)
    
[<Fact>]
let ``Serialize bool`` () =
    let serialized = serialize true
    Assert.Equal("true", serialized)
    
[<Fact>]
let ``Serialize float`` () =
    let serialized = serialize (float 23.567)
    Assert.Equal("23.567", serialized)
    
[<Fact>]
let ``Serialize int`` () =
    let serialized = serialize -456
    Assert.Equal("-456", serialized)
    
[<Fact>]
let ``Serialize array`` () =
    let arr = [|2; 6; 8; 9|]
    let serialized = serialize arr
    Assert.Equal("[2,6,8,9]", serialized)
    
[<Fact>]
let ``Serialize list`` () =
    let arr = [2; 6; 8; 9]
    let serialized = serialize arr
    Assert.Equal("[2,6,8,9]", serialized)
    
[<Fact>]
let ``Serialize complex object`` () =
    let birthday = new DateTime(1990, 12, 1)
    let passenger1 = Passenger("John", birthday)
    let engine = Engine("V8", 249)
    let car = Car("BMW", engine, [|passenger1|])
    let serialized = serialize car
    let expected = "{\"BMW\",{\"V8\",249},[{\"John\",\"" + birthday.ToString() + "\"}]}"
    Assert.Equal(expected, serialized)
    
[<Fact>]
let ``Deserialize string`` () =
    let serialized = PlonString "abc"
    let metadata = { Types = []; RootType = MetadataString }
    let deserialized = Deserializer.deserialize<string> serialized metadata
    Assert.Equal("abc", deserialized)
    
[<Fact>]
let ``Deserialize bool`` () =
    let serialized = PlonBoolean true
    let metadata = { Types = []; RootType = MetadataBool }
    let deserialized = Deserializer.deserialize<bool> serialized metadata
    Assert.Equal(true, deserialized)
    
[<Fact>]
let ``Deserialize float`` () =
    let serialized = PlonNumber 232.5m
    let metadata = { Types = []; RootType = MetadataNumber }
    let deserialized = Deserializer.deserialize<float> serialized metadata
    Assert.Equal(232.5, deserialized)

[<Fact>]
let ``Deserialize int`` () =
    let serialized = PlonNumber 23m
    let metadata = { Types = []; RootType = MetadataNumber }
    let deserialized = Deserializer.deserialize<int> serialized metadata
    Assert.Equal(23, deserialized)
    
[<Fact>]
let ``Deserialize IEnumerable`` () =
    let serialized = PlonArray [PlonString "abc"; PlonNull; PlonString "bca"]
    let metadata = { Types = []; RootType = MetadataArray MetadataString }
    let deserialized = Deserializer.deserialize<List<string>> serialized metadata
    let expected = ["abc"; null; "bca"]
    deserialized 
    |> Seq.toList
    |> List.fold2 (fun s e d -> Assert.Equal(e, d)) () expected 
    
[<Fact>]
let ``Deserialize list`` () =
    let serialized = PlonArray [PlonString "abc"; PlonNull; PlonString "bca"]
    let metadata = { Types = []; RootType = MetadataArray MetadataString }
    let deserialized = Deserializer.deserialize<string list> serialized metadata
    let expected = ["abc"; null; "bca"]
    List.fold2 (fun s e d -> Assert.Equal(e, d)) () expected deserialized
    
[<Fact>]
let ``Deserialize custom object`` () =
    let serialized = PlonObject [PlonString "V8"; PlonNumber 249m]
    let metadata = 
        { Types = 
            [{ Name = "Engine"
               Properties =
                [{ Name = "Model"
                   Type = MetadataString }
                 { Name = "HorsePower"
                   Type = MetadataNumber }] }] 
          RootType = MetadataObject "Engine" }
    let deserialized = Deserializer.deserialize<Engine> serialized metadata
    let expected = Engine("V8", 249)
    Assert.NotNull(deserialized)
    Assert.Equal(expected.Model, deserialized.Model)
    Assert.Equal(expected.HorsePower, deserialized.HorsePower)

    
