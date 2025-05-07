module Shared

open Karambolo.PO

let writePOCatalog (fileName: string) (catalog: POCatalog) =
    let generator = POGenerator()
    if catalog.Encoding = null then
        catalog.Encoding <- "utf-8"
    use writer = new System.IO.StreamWriter(fileName)
    generator.Generate(writer, catalog)

