module Tests.``UI automation``

open Wautoma.MyHotkeys

//[<Fact>]
let ``moving to GMail`` () = openGmail (fun x -> printf $"%s{x}")
