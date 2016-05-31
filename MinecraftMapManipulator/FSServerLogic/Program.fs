
open System.ServiceModel

[<ServiceContract>]
type IServer =
    [<OperationContract>]
    abstract member Run : arg:string -> string

let ADDRESS = "net.pipe://localhost/SampleServer"

[<ServiceBehavior(InstanceContextMode=InstanceContextMode.Single)>]
type Server()  =
    interface IServer with
        member this.Run(s) =
            printfn "CGLS received: %s" s
            let nums = s.Split [|' '|] 
            let [|xs; ys; zs; rs; initPhi; initTheta; desiredLength|] = nums |> Array.map float
            let rng = new System.Random()
            CaveGeneration.makeRandomCave(xs, ys, zs, rs, initPhi, initTheta, desiredLength, rng)
            ""

[<EntryPoint>]
let main _argv = 
    let host = new ServiceHost(new Server())
    host.AddServiceEndpoint(typeof<IServer>, new NetNamedPipeBinding(), ADDRESS) |> ignore
    host.Open()
    printfn "cave generation logic service (CGLS) is running, type 'q' <enter> to quit"
    let mutable ok = false
    while not ok do
        let i = System.Console.ReadLine()
        if i = "q" then
            ok <- true
    0
