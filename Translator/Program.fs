open Karambolo.PO

let parser = POParser();
let result = parser.Parse("sample.po");
if result.Success then
    let catalog = result.Catalog
    printfn "Parsed %d entries" catalog.Count
else
    let diagnostics = result.Diagnostics
    for diagnostic in diagnostics do
        printfn "Error: %s, Severity: %O" diagnostic.Code diagnostic.Severity

printfn "Hello from F#"
