module MainWindow

open Gtk
open POParser
open Karambolo.PO

type MainWindow() as this =
    inherit Window("PO Editor")

    let vbox = new VBox()
    let menubar = new MenuBar()
    let fileMenu = new Menu()
    let file = new MenuItem("File")
    let openItem = new MenuItem("Open")
    let saveItem = new MenuItem("Save")

    let toolbar = new Toolbar()
    let togglePanelButton = new ToggleToolButton(Gtk.Stock.GoDown)
    let scroll = new ScrolledWindow()
    let treeView = new TreeView()
    let listStore = new ListStore(typeof<string>, typeof<string>)

    // Bottom panel
    let bottomPanel = new EventBox()
    let bottomPanelHeight = 100

    // Split panel and text boxes
    let hpaned = new HPaned()
    let sourceTextView = new TextView()
    let translationTextView = new TextView()

    do
        this.SetDefaultSize(800, 600)
        this.DeleteEvent.Add(fun _ -> Application.Quit(); ())
        this.Add(vbox)

        // Menu
        file.Submenu <- fileMenu
        fileMenu.Append(openItem)
        fileMenu.Append(saveItem)
        menubar.Append(file)
        vbox.PackStart(menubar, false, false, 0u)

        // Toolbar
        toolbar.ToolbarStyle <- ToolbarStyle.Icons
        
        toolbar.Insert(togglePanelButton, -1)
        vbox.PackStart(toolbar, false, false, 0u)

        // TreeView
        treeView.Model <- listStore

        let appendColumn title colIndex editable resiziable =
            let renderer = new CellRendererText()
            renderer.Height <- 24
            renderer.Ellipsize <- Pango.EllipsizeMode.End
            let col = new TreeViewColumn(title, renderer)
            col.Resizable <- resiziable
            if editable then
                renderer.Editable <- true
                renderer.Edited.Add(fun args ->
                    let iter = ref Unchecked.defaultof<TreeIter>
                    if listStore.GetIterFromString(iter, args.Path) then
                        listStore.SetValue(!iter, colIndex, args.NewText)
                )
            col.PackStart(renderer, true)
            col.AddAttribute(renderer, "text", colIndex)
            if colIndex = 0 then
                col.MaxWidth <- 400
            treeView.AppendColumn(col) |> ignore

        appendColumn "Source (msgid)" 0 false true
        appendColumn "Translation (msgstr)" 1 false false

        scroll.Add(treeView)
        vbox.PackStart(scroll, true, true, 0u)

        // Bottom panel setup (now with split and text boxes)
        hpaned.Pack1(sourceTextView, true, true)
        hpaned.Pack2(translationTextView, true, false)
        bottomPanel.Add(hpaned)
        bottomPanel.Visible <- false
        vbox.PackStart(bottomPanel, true, true, uint32 bottomPanelHeight)

        // Toggle button setup
        togglePanelButton.Active <- true
        togglePanelButton.Toggled.Add(fun _ ->
            bottomPanel.Visible <- togglePanelButton.Active
            if togglePanelButton.Active then
                togglePanelButton.Label <- "Hide Panel"
            else
                togglePanelButton.Label <- "Show Panel"
        )

        // Events
        openItem.Activated.Add(fun _ ->
            let dialog = new FileChooserDialog("Open PO File", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept)
            if dialog.Run() = int ResponseType.Accept then
                let data = parseCatalog dialog.Filename
                listStore.Clear()
                for item in data.Catalog do
                    match item with
                    | :? POSingularEntry as pse ->
                        listStore.AppendValues(item.Key.Id, item[0]) |> ignore
                    | _ -> 
                        printfn "Only singular entries are supported. Key %s skipped." item.Key.Id
                        ()
                    
            dialog.Destroy()
        )

        saveItem.Activated.Add(fun _ ->
            let dialog = new FileChooserDialog("Save PO File", this, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Save", ResponseType.Accept)
            if dialog.Run() = int ResponseType.Accept then
                let rows =
                    listStore
                    |> Seq.cast<TreeIter>
                    |> Seq.map (fun iter ->
                        let msgid = string (listStore.GetValue(iter, 0))
                        let msgstr = string (listStore.GetValue(iter, 1))
                        msgid, msgstr
                    )
                    |> List.ofSeq
                let data = parseCatalog dialog.Filename
                for row in rows do
                    let entry = data.Catalog |> Seq.tryFind (fun e -> e.Key.Id = fst row) |> Option.defaultValue (POSingularEntry(POKey(fst row)))
                    match entry with
                    | :? POSingularEntry as pse ->
                        pse.Translation <- snd row
                        ()
                    | _ -> ()
                writePOCatalog dialog.Filename data.Catalog
            dialog.Destroy()
        )

