module Tests.``UI automation``

open Wautoma.MyHotkeys
open Xunit

//[<Fact>]
let ``moving to GMail`` () =
    goToChromeTab "gmail" (fun x -> printf $"%s{x}")
