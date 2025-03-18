open Karambolo.PO
open System
open System.Diagnostics
open Eto.Forms
open Eto.Drawing

type MyObject() =
    let mutable textProperty = ""
    member this.TextProperty
      with get () = textProperty
      and set value = 
        textProperty <- value
        Console.WriteLine(sprintf "Set TextProperty to %s" value)

type Translation() =
    member val Source : string = "" with get,set
    member val Translation : string = "" with get,set

let parseCatalog fileName =
    let parser = POParser();
    use stream = new System.IO.FileStream(fileName, System.IO.FileMode.Open)
    let result = parser.Parse(stream)
    result

type MyForm() as this =
    inherit Form()

    do
        this.ClientSize <- Size(600, 400)
        this.Title <- "Robo Translator"

        let grid = new GridView<Translation>()
        grid.ShowHeader <- true
        let createSimpleColumn header =
            let c = new GridColumn()
            c.HeaderText <- header
            c.DataCell <- new TextBoxCell(header)
            c.Editable <- false
            c.Resizable <- true
            c.Expand <- true
            c
        grid.Columns.Add(createSimpleColumn "Source")
        grid.Columns.Add(createSimpleColumn "Translation")
        this.Content <- grid
        
        // Set data context so it propegates to all child controls
        this.DataContext <- MyObject(TextProperty = "Initial Value 1")

        let quitItemCommand = 
            new Command(
                (fun sender e -> Application.Instance.Quit()),
                MenuText = "Quit",
                Shortcut = (Application.Instance.CommonModifier ||| Keys.Q)
            )
        let quitMenuItem = quitItemCommand.CreateMenuItem()
        let openFileItemCommand = 
            new Command(
                (fun sender e -> 
                    let dialog = new Eto.Forms.OpenFileDialog()
                    if dialog.ShowDialog this = DialogResult.Ok then
                        let parseResults = parseCatalog dialog.FileName
                        if parseResults.Success then
                            let catalog = parseResults.Catalog
                            grid.DataStore <- catalog |> Seq.map (fun item -> Translation(Source = item.Key.Id, Translation = item[0])) |> Array.ofSeq
                    else
                        ()
                ),
                MenuText = "&Open",
                Shortcut = (Application.Instance.CommonModifier ||| Keys.O)
            )

        let fileMenu = new SubMenuItem(Text = "&File")
        fileMenu.Items.Add(openFileItemCommand.CreateMenuItem())
        //fileMenu.Items.Add(quitMenuItem)
        //this.Menu.Items <- new MenuItemsCollection()
        let menu = new MenuBar()
        menu.Items.Add(fileMenu)
        menu.QuitItem <- quitMenuItem
        this.Menu <- menu

[<EntryPoint;STAThread>]
let main argv = 
    (new Application()).Run(new MyForm())
    0