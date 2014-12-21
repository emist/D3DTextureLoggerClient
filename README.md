<h1>D3DTextureLoggerClient</h1>

This program allows you to easily go through the list of primitives in memory at any given time.  It eliminates the need to manually increase prims and verts until you hit the texture you want.  Clicking the "Save Prim" number saves a screenshot of the currently selected prim textured in red and saves it to the Output folder in the executable's directory. 

<h2>Get the source</h2>

Download the latest source code with:

`git clone git@github.com:emist/D3DTextureLoggerClient.git`

Keep your source up to date with:

`git pull origin master`

<h2>Dependencies</h2>

[.NET Framework](http://www.microsoft.com/net/download.aspx)

[SlimdDX](http://slimdx.org/)

[Easyhook](http://easyhook.codeplex.com/)

[Windows Debug Help Library](http://msdn.microsoft.com/en-us/library/windows/desktop/ms679309.aspx)

<h2>Compiling</h2>

Load the solution into visual studio and press F6

<h2>Documentation</h2>

[Docs](http://eryanbot.com/jtp/2013/06/02/d3dtextureloggerclient/)

<h2>Troubleshooting</h2>

If when you run the program you do not get a message on the command prompt telling you where EndScene, SetStreamSource, DrawIndexedPrimitives, BeginScene, and Reset are in memory then you have a problem with the debug server/symbols.  Make sure that dbghelp.dll, symsrv.dll and srcrv.dll are in your path.  If all else fails, dropping them inside the same directory as your D3DTextureLoggerClient shouldwork. 

If you get errors when you click "Inject" then something is wrong with your Easyhook libs.  Make sure that you have EasyHook.dll, EasyHook32.dll, EasyHook64.dll, EasyHookSvc, EasyHook32Svc, and EasyHook64Svc in your path.  If all else fails, dropping them into your executable folder will do the trick. 