module Plon.Serialization.Tests.Serializer

open System
open Xunit
open Plon.Serialization.Serializer
open ComplexObjects

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
