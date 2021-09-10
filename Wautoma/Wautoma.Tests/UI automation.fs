module Tests.``UI automation``

open Wautoma.UIAutomation.AutomationExamples
open Xunit

//[<Fact>]
let ``moving to GMail`` () = moveToGmail (fun x -> printf $"%s{x}")
