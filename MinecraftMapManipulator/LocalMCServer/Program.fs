/////////////////////////////

open System.ServiceModel

[<ServiceContract; AllowNullLiteral>]
type IServer =
    [<OperationContract>]
    abstract member Run : arg:string -> string

let ADDRESS = "net.pipe://localhost/SampleServer"

let mutable client = ChannelFactory<IServer>.CreateChannel(new NetNamedPipeBinding(), new EndpointAddress(ADDRESS))
let sendMessage(s) =
    let mutable sent = false
    printfn "trying to send: %s" s
    while not sent do
        try
            if client = null then
                client <- ChannelFactory<IServer>.CreateChannel(new NetNamedPipeBinding(), new EndpointAddress(ADDRESS))
            let r = client.Run(s)
            printfn "success, got response: %s" r
            sent <- true
        with e ->
            printfn "failed, retrying..."
            System.Threading.Thread.Sleep(1000)
            client <- null

/////////////////////////////

open System.Diagnostics 

type InputEvent = 
    | CONSOLE of string     // stuff typed into the keyboard console of this program 
    | MINECRAFT of string   // the stdout of the Minecraft process 
    | COMMAND of string     // a command to run on the MC console
let inputEvents = new System.Collections.Concurrent.BlockingCollection<_>()

let SERVER_DIRECTORY = """C:\Users\Admin1\Desktop\Server""" 
let COMMAND_FILE = System.IO.Path.Combine(SERVER_DIRECTORY,"commands_to_run.txt")

let startServerEventLoop() =
    // SETUP MINECRAFT 
    let minecraftStdin = 
        let psi = new ProcessStartInfo(UseShellExecute=false, RedirectStandardInput=true, RedirectStandardOutput=true) 
        psi.WorkingDirectory <- SERVER_DIRECTORY
        psi.FileName <- "java" 
        psi.Arguments <- "-Xms2048M -Xmx2048M -d64 -jar minecraft_server.1.9.4.jar nogui" 
        let proc = new Process(StartInfo=psi) 
        // START MINECRAFT 
        do
            proc.Start() |> ignore 
            let rec rcvloop() = 
                let data = proc.StandardOutput.ReadLine() 
                if data <> null then 
                    inputEvents.Add(MINECRAFT data) 
                    rcvloop() 
            let t = new System.Threading.Thread(rcvloop) 
            t.Start() 
        proc.StandardInput 
    // SETUP FILE_WATCHER
    do
        let watcherThread() =
            while true do
                System.Threading.Thread.Sleep(1000)
                if System.IO.File.Exists(COMMAND_FILE) then
                    printfn "saw file on disk, processing it..."
                    let cmds = System.IO.File.ReadAllLines(COMMAND_FILE)
                    System.IO.File.Delete(COMMAND_FILE)
                    for cmd in cmds do
                        inputEvents.Add(COMMAND cmd)
        let t = new System.Threading.Thread(watcherThread) 
        t.Start() 
    // SETUP & START CONSOLE 
    do 
        printfn "Minecraft server console and event processing loop started" 
        printfn "press q <enter> to quit" 
        let rec sendloop() = 
            let i = System.Console.ReadLine() 
            if i <> "q" then 
                inputEvents.Add(CONSOLE i) 
                sendloop() 
            else 
                inputEvents.CompleteAdding() 
        let t = new System.Threading.Thread(sendloop) 
        t.Start() 
    // MAIN LOOP 
    for e in inputEvents.GetConsumingEnumerable() do 
        match e with 
        | MINECRAFT data -> 
                try 
                    printfn "MINECRAFT> %s" data 
                    match data.IndexOf("Lorgon111") with 
                    | -1 -> () 
                    | n ->  
                    let data = data.Substring(n+"Lorgon111".Length) 
                    let PROMPT = "> !"
                    let PROMPT = "> "
                    match data.LastIndexOf(PROMPT) with      // may be color reset code between name and text, match separately 
                    | -1 -> () 
                    | n ->  
                        let text = data.Substring(n+PROMPT.Length).ToLowerInvariant()
                        let words = text.Split([|" "|], System.StringSplitOptions.RemoveEmptyEntries) 
                        for w in words do
                            printfn "M: %s" w
                        match words.[0] with
                        | "run" ->
                            let x,y,z = int(words.[1]),int(words.[2]),int(words.[3])
                            //makeRandomCave(map, float(x), float(y), float(z), 5.0, 1.8, 1.0, 300.0, rng)
                            sendMessage(sprintf "%f %f %f %f %f %f %f" (float x) (float y) (float z) 5.0 1.8 1.0 300.0)
                        | _ -> ()
                with e ->  
                    printfn "MINECRAFT FAULT> %s" (e.ToString()) 
                    reraise() 
        | CONSOLE data -> 
            printfn "C: %s" data
            if data.StartsWith("!") then
                inputEvents.Add(COMMAND(data.Substring(1)))
        | COMMAND cmd ->
            minecraftStdin.WriteLine(cmd)
    printfn "DONE!"
    exit(0)

startServerEventLoop()