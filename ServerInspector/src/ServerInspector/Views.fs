module ServerInspector.Views

open ServerInspector.Models
open Giraffe
open Giraffe.GiraffeViewEngine
open Microsoft.AspNetCore.Http

let layout (content: XmlNode list) =
    html [] [
        head [] [
            title []  [ str "Giraffe" ]
        ]
        body [] content
    ]
    
let renderHelloWorld: HttpHandler =
    let model = { Text = "world" }
    [
        div [] [ sprintf "%s" model.Text |> str ]
    ]
    |> layout
    |> htmlView

let renderHelloX (name: string): HttpHandler =
    [
        h1 [] [ str "Lines" ]
        div [] [ sprintf "%s" name |> str ]
    ]
    |> layout
    |> htmlView

let renderLines(lines) : HttpHandler =
    lines
    |> List.mapi (fun i l -> div [] [sprintf "Line #%d: %s" i l |> str])
    |> layout
    |> htmlView
   