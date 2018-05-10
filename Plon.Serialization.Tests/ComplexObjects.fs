module Plon.Serialization.Tests.ComplexObjects

open System

type Passenger(name, birthday) =
    member this.Name : string = name
    member this.Birthday : DateTime = birthday

type Engine(model, horsePower) =
    let mutable model : string = model
    let mutable horsePower = horsePower

    member this.Model
        with get() = model
        and set(value) = model <- value

    member this.HorsePower
        with get() = horsePower
        and set(value) = horsePower <- value

    new() = Engine(null, 0)

type Car(brand, engine, passengers) =
    member this.Brand : string = brand
    member this.Engine : Engine = engine
    member this.Passengers : Passenger[] = passengers