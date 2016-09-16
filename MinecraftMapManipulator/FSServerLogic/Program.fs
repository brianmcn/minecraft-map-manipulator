
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
    for x in [1.0 .. 6.0] do
        let r = 606.75 * x * x * x * x - 6274.0 *x * x * x + 22685.75 *x * x - 33255.5 * x + 16269.0
        printfn "%f %f" x r

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

(*
    1.000000 32.000000
    2.000000 17.000000
    3.000000 423.000000
    4.000000 11.000000
    5.000000 12104.000000
    6.000000 64587.000000
*)

