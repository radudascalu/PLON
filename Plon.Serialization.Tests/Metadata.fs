module Plon.Serialization.Tests.Metadata

open System
open Xunit
open Plon.Serialization.Serializer
open ComplexObjects
open Plon.Serialization

[<Fact>]
let ``Metadata of complex object`` () =
    let metadata = Metadata.getPlonMetadata typeof<Car>
    let expected = "{\"definedTypes\":[{\"name\":\"Plon.Serialization.Tests.ComplexObjects+Car\",\"properties\":[{\"name\":\"Brand\",\"type\":\"string\"},{\"name\":\"Engine\",\"type\":\"Plon.Serialization.Tests.ComplexObjects+Engine\"},{\"name\":\"Passengers\",\"type\":\"Plon.Serialization.Tests.ComplexObjects+Passenger[]\"}]},{\"name\":\"Plon.Serialization.Tests.ComplexObjects+Engine\",\"properties\":[{\"name\":\"Model\",\"type\":\"string\"},{\"name\":\"HorsePower\",\"type\":\"number\"}]},{\"name\":\"Plon.Serialization.Tests.ComplexObjects+Passenger\",\"properties\":[{\"name\":\"Name\",\"type\":\"string\"},{\"name\":\"Birthday\",\"type\":\"string\"}]}],\"_plonRootType\":\"Plon.Serialization.Tests.ComplexObjects+Car\"}"
    Assert.Equal(expected, metadata)
