module Plon.Serialization.Tests.ComplexObjects

type Passenger(name, birthday) =
    member this.Name = name
    member this.Birthday = birthday

type Engine(model, horsePower) =
    member this.Model = model
    member this.HorsePower = horsePower

type Car(brand, engine, passengers) =
    member this.Brand = brand
    member this.Engine = engine
    member this.Passengers = passengers