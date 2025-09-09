open System
open MainWindow
open Gtk

[<EntryPoint;STAThread>]
let main argv = 
    //(new Application()).Run(new MyForm())
    Application.Init()
    let win = new MainWindow()
    win.ShowAll()

    //sample ()
    Application.Run()
    0