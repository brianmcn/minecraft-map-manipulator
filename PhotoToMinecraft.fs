module PhotoToMinecraft

open System.Drawing
open System.IO
open System.Windows  
open System.Windows.Controls  
open System.Windows.Media  
open System.Windows.Media.Imaging 
open System.Threading 

let blocksDir = """C:\Users\brianmcn\Desktop\MCjustblocks"""
//let targetImage = """C:\Users\brianmcn\Desktop\charliebrown.jpg"""
let targetImage = """C:\Users\brianmcn\Desktop\KurtBrianPic.jpg"""
//let targetImage = """C:\Users\brianmcn\Desktop\TSMS.jpg"""

let blockColors = ResizeArray()
let getFile(file,_,_) = file : string
let getBmp(_,bmp,_) = bmp : System.Drawing.Bitmap 
let getColor(_,_,c) = c

let dist(c1:System.Drawing.Color, c2:System.Drawing.Color) =
    let a = int64 (c1.A - c2.A)
    let r = int64 (int c1.R - int c2.R)
    let g = int64 (int c1.G - int c2.G)
    let b = int64 (int c1.B - int c2.B)
    //a*a+r*r+g*g+b*b
    r*r+g*g+b*b
    //abs(r) + abs(g) + abs(b)

(*
let LA = [| 2L; 3L; 8L; 28L; 38L |]
let doLanczos(bmp:System.Drawing.Bitmap) =
    let mid = new System.Drawing.Bitmap(bmp.Width, bmp.Height)
    let mutable r,g,b,n = 0L, 0L, 0L, 0L
    for x = 0 to mid.Width - 1 do
        for y = 0 to mid.Height - 1 do
            r <- 0L; g <- 0L; b <- 0L; n <- 0L
            if x-4>0 then
                let c = bmp.GetPixel(x-4,y)
                r <- r + (int64 c.R)*LA.[0]; g <- g + (int64 c.G)*LA.[0]; b <- b + (int64 c.B)*LA.[0]; n <- n + LA.[0]
            if x-3>0 then
                let c = bmp.GetPixel(x-3,y)
                r <- r + (int64 c.R)*LA.[1]; g <- g + (int64 c.G)*LA.[1]; b <- b + (int64 c.B)*LA.[1]; n <- n + LA.[1]
            if x-2>0 then
                let c = bmp.GetPixel(x-2,y)
                r <- r + (int64 c.R)*LA.[2]; g <- g + (int64 c.G)*LA.[2]; b <- b + (int64 c.B)*LA.[2]; n <- n + LA.[2]
            if x-1>0 then
                let c = bmp.GetPixel(x-1,y)
                r <- r + (int64 c.R)*LA.[3]; g <- g + (int64 c.G)*LA.[3]; b <- b + (int64 c.B)*LA.[3]; n <- n + LA.[3]
            if true then
                let c = bmp.GetPixel(x,y)
                r <- r + (int64 c.R)*LA.[4]; g <- g + (int64 c.G)*LA.[4]; b <- b + (int64 c.B)*LA.[4]; n <- n + LA.[4]
            if x+1<bmp.Width-1 then
                let c = bmp.GetPixel(x+1,y)
                r <- r + (int64 c.R)*LA.[3]; g <- g + (int64 c.G)*LA.[3]; b <- b + (int64 c.B)*LA.[3]; n <- n + LA.[3]
            if x+2<bmp.Width-1 then
                let c = bmp.GetPixel(x+2,y)
                r <- r + (int64 c.R)*LA.[2]; g <- g + (int64 c.G)*LA.[2]; b <- b + (int64 c.B)*LA.[2]; n <- n + LA.[2]
            if x+3<bmp.Width-1 then
                let c = bmp.GetPixel(x+3,y)
                r <- r + (int64 c.R)*LA.[1]; g <- g + (int64 c.G)*LA.[1]; b <- b + (int64 c.B)*LA.[1]; n <- n + LA.[1]
            if x+4<bmp.Width-1 then
                let c = bmp.GetPixel(x+4,y)
                r <- r + (int64 c.R)*LA.[0]; g <- g + (int64 c.G)*LA.[0]; b <- b + (int64 c.B)*LA.[0]; n <- n + LA.[0]
            r <- r/n; g <- g/n; b <- b/n
            mid.SetPixel(x,y,System.Drawing.Color.FromArgb(255, int r, int g, int b))
    mid
*)
let thumbnailify(bmp:System.Drawing.Bitmap) =
    // downscale, then upscale
    //let thumb = new System.Drawing.Bitmap(bmp, bmp.Width/16, bmp.Height/16)
    //new System.Drawing.Bitmap(thumb, bmp.Width, bmp.Height)
    let temp = new System.Drawing.Bitmap(bmp.Width/16, bmp.Height/16)
    use g = System.Drawing.Graphics.FromImage(temp)
    g.InterpolationMode <- System.Drawing.Drawing2D.InterpolationMode.Bicubic 
    g.DrawImage(bmp, 0, 0, bmp.Width/16, bmp.Height/16)
    new System.Drawing.Bitmap(temp, bmp.Width, bmp.Height)

let computeAverageColorOfSection(x1, x2, y1, y2, targetBmp:System.Drawing.Bitmap) =
    let mutable ra, ga, ba, count = 0L,0L,0L,0L
    for xx = x1 to x2 do
        for yy = y1 to y2 do
            let c = targetBmp.GetPixel(xx,yy)
            ra <- ra + int64 c.R; ga <- ga + int64 c.G; ba <- ba + int64 c.B
            count <- count + 1L
    // normal average is in ra/ga/ba, now sharpen by emphaszing points far from the average
    let THRESHOLD = 48L
    let AMOUNT = 6
    let mutable r, g, b = 0L,0L,0L
    for xx = x1 to x2 do
        for yy = y1 to y2 do
            let c = targetBmp.GetPixel(xx,yy)
            r <- r + int64 c.R; g <- g + int64 c.G; b <- b + int64 c.B
            count <- count + 1L
            if abs(int64 c.R - ra) + abs(int64 c.G - ga) + abs(int64 c.B - ba) > THRESHOLD then
                for z = 0 to AMOUNT do
                    r <- r + int64 c.R; g <- g + int64 c.G; b <- b + int64 c.B
                    count <- count + 1L
    let avgColor = System.Drawing.Color.FromArgb(255,int(r/count),int(g/count),int(b/count))
    avgColor
(*
    let mutable r, g, b, count = 0L,0L,0L,0L
    for xx = x1 to x2 do
        for yy = y1 to y2 do
            let c = targetBmp.GetPixel(xx,yy)
            r <- r + int64 c.R; g <- g + int64 c.G; b <- b + int64 c.B
            count <- count + 1L
            if xx-x1 > (x2-x1/4) && xx-x1 < 3*(x2-x1)/4 && yy-y1 > (y2-y1/4) && yy-y1 < 3*(y2-y1)/4 then
                // weight center more
                r <- r + int64 c.R; g <- g + int64 c.G; b <- b + int64 c.B
                count <- count + 1L
            if xx-x1 > (x2-x1/3) && xx-x1 < 2*(x2-x1)/3 && yy-y1 > (y2-y1/3) && yy-y1 < 2*(y2-y1)/3 then
                // weight center more
                r <- r + int64 c.R; g <- g + int64 c.G; b <- b + int64 c.B
                count <- count + 1L
    let avgColor = System.Drawing.Color.FromArgb(255,int(r/count),int(g/count),int(b/count))
    avgColor
    *)
(*
    targetBmp.GetPixel(x1+(x2-x1)/2,y1+(y2-y1)/2)
    *)

let SHOWMIDDLE = true
let SHOWRIGHT = true

let mutable pictureBlockFilenames : string[,] = null

let computeMinecraft(w,h) =
    pictureBlockFilenames <- Array2D.zeroCreate w h
    for (_,_,c:System.Drawing.Color) in blockColors do
        printfn "%3d %3d %3d" c.R c.G c.B
    let targetBmp = new Bitmap(targetImage)
    (*
    let targetBmp =
        let temp = new System.Drawing.Bitmap(16*w, 16*h)
        use g = System.Drawing.Graphics.FromImage(temp)
        g.InterpolationMode <- System.Drawing.Drawing2D.InterpolationMode.Bicubic 
        g.DrawImage(targetBmp, 0, 0, 16*w, 16*h)
        temp
        *)
    //let targetBmp = new System.Drawing.Bitmap(targetBmp, 16*w, 16*h)
    //let targetBmp = thumbnailify(targetBmp)
    //let targetBmp = doLanczos(targetBmp)
    let result = new Bitmap(((if SHOWMIDDLE then 1 else 0)+(if SHOWRIGHT then 1 else 0)+1)*16*w,16*h)
    let xRatio = float targetBmp.Width / float w
    let yRatio = float targetBmp.Height / float h
    for x = 0 to w-1 do
        for y = 0 to h-1 do
            let avgColor = computeAverageColorOfSection(int (float x * xRatio), int (float (x+1) * xRatio)-1, 
                                                        int (float y * yRatio), int (float (y+1) * yRatio)-1, targetBmp)
            let mutable minDist = dist(avgColor, getColor(blockColors.[0]))
            let mutable best = 0
            for i = 1 to blockColors.Count-1 do
                if dist(avgColor, getColor(blockColors.[i])) < minDist then
                    best <- i
                    minDist <- dist(avgColor, getColor(blockColors.[i]))
            let blockBmp = getBmp(blockColors.[best])
            pictureBlockFilenames.[x,y] <- getFile(blockColors.[best])
            for dx=0 to 15 do
                for dy=0 to 15 do
                    result.SetPixel(16*x+dx, 16*y+dy, blockBmp.GetPixel(dx,dy))      
            if SHOWMIDDLE then
                for dx=0 to 15 do
                    for dy=0 to 15 do
                        result.SetPixel(16*w+16*x+dx, 16*y+dy, avgColor)      
    if SHOWRIGHT then
        let targetBmp = new System.Drawing.Bitmap(targetBmp, 16*w, 16*h)
        for x=0 to targetBmp.Width-1 do
            for y=0 to targetBmp.Height-1 do
                result.SetPixel((if SHOWMIDDLE then 2 else 1)*targetBmp.Width+x, y, targetBmp.GetPixel(x,y))
    result


let bmpToImage(bmp : Bitmap, scale) =
    let image = new Image(Width=float(bmp.Width)*scale, Height=float(bmp.Height)*scale) 
    let ms = new MemoryStream()
    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png)
    ms.Position <- 0L
    let bi = new BitmapImage()
    bi.BeginInit()
    bi.StreamSource <- ms
    bi.EndInit()
    image.Source <- bi
    image

let isBlacklistedBlocks(s:string) =
    s = "ice" ||
    s.StartsWith("furnace_front") ||
    s.StartsWith("tnt") ||
    s.StartsWith("mycel") ||
(*
    s.StartsWith("quartz_ore") ||
    *)
    false

let isBlacklistedHeads(s:string) =
    s.EndsWith("_top") ||  // TODO pose head for rotating logs?
    s.EndsWith("furnace_side") ||   // TODO
    s.EndsWith("furnace_front_on") ||
    s.EndsWith("redstone_lamp_on") ||
    s.StartsWith("dispenser_front") ||
    s.StartsWith("dropper_front") ||
    s.StartsWith("stone_slab_side") ||
    false

let isBlacklisted(s:string) = isBlacklistedHeads(s)


let grid = // do essential computations, even as a library
        let grid = new Grid()
        let add(thing,row,col) =
            grid.Children.Add(thing) |> ignore
            Grid.SetRow(thing,row)
            Grid.SetColumn(thing,col)
        grid.HorizontalAlignment <- HorizontalAlignment.Center 
        grid.VerticalAlignment <- VerticalAlignment.Center 
        grid.ShowGridLines <- true
        for i = 1 to 3 do grid.ColumnDefinitions.Add(new ColumnDefinition())
        let mutable i = 0
        for file in Directory.EnumerateFiles(blocksDir) do
            printfn "%s" (Path.GetFileNameWithoutExtension file)
            if Path.GetExtension(file).ToLower() = ".png" && not(isBlacklisted(Path.GetFileNameWithoutExtension file)) then
                let name = Path.GetFileNameWithoutExtension file
                let bmp = new Bitmap(file)
                grid.RowDefinitions.Add(new RowDefinition())
                let txt = new TextBlock()
                txt.Text <- name
                txt.FontSize <- 12.0
                txt.Margin <- new Thickness(4.0)
                add(txt,i,0)
                let img = bmpToImage(bmp, 1.0)
                img.Margin <- new Thickness(4.0)
                add(img,i,1)
                let mutable a, r, g, b = 0,0,0,0
                for x in 0..bmp.Width-1 do
                    for y in 0..bmp.Height-1 do
                        let c = bmp.GetPixel(x,y)
                        a <- a + int c.A
                        r <- r + int c.R
                        g <- g + int c.G
                        b <- b + int c.B
                let colorAvgBmp = new Bitmap(1,1)
                let n = bmp.Width * bmp.Height 
                let c = System.Drawing.Color.FromArgb(a/n,r/n,g/n,b/n)
                blockColors.Add(file,bmp,c)
                colorAvgBmp.SetPixel(0,0,c)
                let img = bmpToImage(colorAvgBmp, 16.0)
                img.Margin <- new Thickness(4.0)
                add(img,i,2)
                i <- i + 1
        printfn "%d" i
        grid

//let finalBmp = computeMinecraft(86,48)
let finalBmp = computeMinecraft(55,30)

type MyWPFWindow() as this =  
    inherit Window()    
    do 
        let sv = new ScrollViewer()
        sv.VerticalScrollBarVisibility <- ScrollBarVisibility.Visible 
        sv.Content <- grid
        let showBlocks = false
        if showBlocks then
            this.SizeToContent <- SizeToContent.Width 
            this.Height <- 500.0
            this.Content <- sv
        else
            let SCALE = 1.5
            let finalBmp = new System.Drawing.Bitmap(finalBmp, System.Drawing.Size(int(float finalBmp.Width*SCALE), int(float finalBmp.Height*SCALE)))
            this.SizeToContent <- SizeToContent.WidthAndHeight 
            this.Content <- bmpToImage(finalBmp, 0.5)

#if IS_MAIN_PROGRAM

[<System.STAThread()>]  
do   
    let app =  new Application()  
    app.Run(new MyWPFWindow()) |> ignore 

#endif
