﻿// Flappy bird prototype using Windows Forms
// Note: no collision detection

#if INTERACTIVE
#r "System.Drawing.dll"
#r "System.Windows.Forms.dll"
#endif
open System.IO
open System.Drawing
open System.Windows.Forms
open System.Net

/// Double-buffered form
type CompositedForm () =
   inherit Form()
   override this.CreateParams = 
      let cp = base.CreateParams
      cp.ExStyle <- cp.ExStyle ||| 0x02000000
      cp

/// Loads an image from a file or url
let load (file:string) (url:string) =
   let path = Path.Combine(__SOURCE_DIRECTORY__, file)
   if File.Exists path then Image.FromFile path
   else
      let request = HttpWebRequest.Create(url)
      use response = request.GetResponse()
      use stream = response.GetResponseStream()
      Image.FromStream(stream)

let bg = load "bg.png" "http://flappycreator.com/default/bg.png"
let ground = load "ground.png" "http://flappycreator.com/default/ground.png"
let tube1 = load "tube1.png" "http://flappycreator.com/default/tube1.png"
let tube2 = load "tube2.png" "http://flappycreator.com/default/tube2.png"
let bird_sing = load "bird_sing.png" "http://flappycreator.com/default/bird_sing.png"

/// Bird type
type Bird = { X:float; Y:float; VY:float; IsAlive:bool }
/// Respond to flap command
let flap (bird:Bird) = { bird with VY = - System.Math.PI }
/// Applies gravity to bird
let gravity (bird:Bird) = { bird with VY = bird.VY + 0.1 }
/// Applies physics to bird
let physics (bird:Bird) = { bird with Y = bird.Y + bird.VY }
/// Updates bird with gravity & physics
let update = gravity >> physics

/// Paints the game scene
let paint (graphics:Graphics) scroll level (flappy:Bird) =
   let draw (image:Image) (x,y) =
      graphics.DrawImage(image,x,y,image.Width,image.Height)
   draw bg (0,0)
   draw bird_sing (int flappy.X, int flappy.Y)
   let drawTube (x,y) =      
      draw tube1 (x-scroll,-320+y)
      draw tube2 (x-scroll,y+100)
   for (x,y) in level do drawTube (x,y)
   draw ground (0,340)
    
/// Generates the level's tube positions
let generateLevel n =
   let rand = System.Random()
   [for i in 1..n -> 50+(i*150), 32+rand.Next(160)]

let level = generateLevel 10
let scroll = ref 0
let flappy = ref { X = 30.0; Y = 150.0; VY = 0.0; IsAlive=true }

let form = new CompositedForm(Text="Flap me",Width=288,Height=440)
form.Paint.Add(fun args ->
   flappy := update !flappy
   paint args.Graphics !scroll level !flappy; 
   incr scroll)

let flapme () = if (!flappy).IsAlive then flappy := flap !flappy
// Respond to mouse clicks
form.Click.Add(fun args -> flapme())
// Respond to space key
form.KeyDown.Add(fun args -> if args.KeyCode = Keys.Space then flapme())
// Show form
form.Show()
// Update form asynchronously every 18ms
async { 
   while true do
      do! Async.Sleep(18)
      form.Invalidate() 
} |> Async.StartImmediate