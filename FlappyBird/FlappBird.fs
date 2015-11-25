#if INTERACTIVE
//#I @"../packages/MonoGame.Framework.WindowsDX.3.4.0.459/lib/net40/"
#I @"../packages/MonoGame.Framework.WindowsGL.3.4.0.459/lib/net40/"
#r "MonoGame.Framework.dll"
#r "System.Xml.dll"
#load "FontRendering.fs"
#endif

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
 
/// Generates the level's tube positions
let generateLevel n =
   let rand = System.Random()
   [for i in 1..n -> 50+(i*150), 32+rand.Next(160)]

open System.IO
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

let localPath file = Path.Combine(__SOURCE_DIRECTORY__, file)

let loadImage (device:GraphicsDevice) file =   
   use stream = File.OpenRead(localPath file)
   let texture = Texture2D.FromStream(device, stream)
   let textureData = Array.create<Color> (texture.Width * texture.Height) Color.Transparent
   texture.GetData(textureData)
   texture

type FlappyBird() as this =
   inherit Game()
   do this.Window.Title <- "Flap me"
   let graphics = new GraphicsDeviceManager(this)
   do graphics.PreferredBackBufferWidth <- 288
   do graphics.PreferredBackBufferHeight <- 440
   let mutable spriteBatch : SpriteBatch = null
   let mutable bg : Texture2D = null
   let mutable ground : Texture2D = null
   let mutable tube1 : Texture2D = null
   let mutable tube2 : Texture2D = null
   let mutable bird_sing : Texture2D = null
   let mutable fontRenderer = Unchecked.defaultof<FontRendering.FontRenderer>
   let mutable lastKeyState = KeyboardState()
   let mutable lastMouseState = MouseState()
   let level = generateLevel 10
   let mutable flappy = { X = 30.0; Y = 150.0; VY = 0.0; IsAlive=true }
   let flapMe () = if flappy.IsAlive then flappy <- flap flappy
   let mutable scroll = 0

   override this.LoadContent() =
      spriteBatch <- new SpriteBatch(this.GraphicsDevice)
      let load = loadImage this.GraphicsDevice
      bg <- load "bg.png"
      ground <- load "ground.png"
      tube1 <- load "tube1.png"
      tube2 <- load "tube2.png"
      bird_sing <- load "bird_sing.png"
      use fontTextureStream = System.IO.File.OpenRead(localPath "Impact_0.png")
      let fontTexture = Texture2D.FromStream(this.GraphicsDevice, fontTextureStream)
      let fontFile = FontRendering.FontLoader.Load(localPath "Impact.fnt")
      fontRenderer <- FontRendering.FontRenderer(fontFile, fontTexture)

   override this.Update(gameTime) =
      scroll <- scroll - 1
      let currentKeyState = Keyboard.GetState()
      let currentMouseState = Mouse.GetState()
      let isKeyPressedSinceLastFrame key =
         currentKeyState.IsKeyDown(key) && lastKeyState.IsKeyUp(key)
      let isMouseClicked () =
         currentMouseState.LeftButton = ButtonState.Pressed &&
         lastMouseState.LeftButton = ButtonState.Released
      if isKeyPressedSinceLastFrame Keys.Space || isMouseClicked () 
      then flapMe ()
      flappy <- update flappy
      lastKeyState <- currentKeyState
      lastMouseState <- currentMouseState     
   
   override this.Draw(gameTime) =
      this.GraphicsDevice.Clear Color.White
      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied)
      let draw (texture:Texture2D) (x,y) =
         spriteBatch.Draw(texture, Rectangle(x,y,texture.Width,texture.Height), Color.White)      
      draw bg (0,0)
      draw bird_sing (int flappy.X,int flappy.Y)
      for (x,y) in level do
         let x = x+scroll        
         draw tube1 (x,-320+y)
         draw tube2 (x,y+100)
      draw ground (0,360)
      if not flappy.IsAlive then
         fontRenderer.DrawText(spriteBatch, 50, 50, "GAME OVER")
      spriteBatch.End()

do
   use game = new FlappyBird()
   game.Run()
