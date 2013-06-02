  /****************************\
    D3D9 Model Recognition Logger
    Last Edited By : PheonX
    Credits: DrUnKeN ChEeTaH
    \****************************/

    #pragma comment (lib, "d3dx9.lib")
    #pragma comment (lib, "d3d9.lib")

    #include <Windows.h>
    #include <d3d9.h>
    #include <d3dx9.h>
    #include <iostream>
    #include <fstream>
    #include <detours.h>

    using namespace std;

    #define HOOK(func,addy) o##func = (t##func)DetourFunction((PBYTE)addy,(PBYTE)hk##func) // Quick Hook with Detours

    #define STRIDE 0
    #define NUMVERTS 1
    #define PRIMCOUNT 2
    #define STARTINDEX 3
    #define LOGVALUES 4

    #define ES 0
    #define DIP 1
    #define SSS 2

    void Log(char* fmt, ...);
    void ModelRecLoggerMenu();

    struct ModelRecLogger_t
    {
		char* type;
		int value;
		bool isLogging;
    };

    ModelRecLogger_t model[5] = {
    {"Stride :", 0, false},
    {"NumVert :", 0, false},
    {"PrimCount :", 0, false},
    {"StartIndex :", 0, false},
    {"Log All Value", 0, false}
    };

    unsigned int g_uiTimer = NULL;
    unsigned int g_uiStride = NULL;

    LPD3DXFONT g_pFont = NULL;
    LPD3DXLINE g_pLine = NULL;
    D3DVIEWPORT9 g_ViewPort;

    LPDIRECT3DTEXTURE9 g_pTexRed = NULL;
    LPDIRECT3DTEXTURE9 g_pTexOrange = NULL;

    const BYTE red[ 58 ] = {
    0x42, 0x4D, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
    0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00
    };

    const BYTE orange[ 58 ] = {
    0x42, 0x4D, 0x3A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
    0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA5, 0xFF, 0x00
    };

    //---------------------------------------------------------------------------------------------------------------------------------

    typedef HRESULT (WINAPI* tSetStreamSource)(LPDIRECT3DDEVICE9 pDevice,UINT StreamNumber,IDirect3DVertexBuffer9* pStreamData,UINT OffsetInBytes,UINT Stride);
    tSetStreamSource oSetStreamSource;

    typedef HRESULT (WINAPI* tEndScene)(LPDIRECT3DDEVICE9 pDevice);
    tEndScene oEndScene;

    typedef HRESULT (WINAPI* tDrawIndexedPrimitive)(LPDIRECT3DDEVICE9 pDevice, D3DPRIMITIVETYPE PrimType,INT BaseVertexIndex,UINT MinVertexIndex,UINT NumVertices,UINT startIndex,UINT primCount);
    tDrawIndexedPrimitive oDrawIndexedPrimitive;

    //---DrawString--------------------------------------------------------------------------------------------------------------------

    void DrawString(int x, int y, DWORD color, const char *fmt, ...)
    {
		RECT FontPos = { x, y, x + 120, y + 16 };
		char buf[1024] = {'\0'};
		va_list va_alist;

		va_start(va_alist, fmt);
		vsprintf(buf, fmt, va_alist);
		va_end(va_alist);

		g_pFont->DrawText(NULL, buf, -1, &FontPos, DT_NOCLIP, color);
    }

    //---DrawRectangle-----------------------------------------------------------------------------------------------------------------

    void DrawRectangle(float x, float y, float w, int h)
    {

		D3DXVECTOR2 vLine1[2];
		D3DXVECTOR2 vLine2[2];
		D3DXVECTOR2 vLine3[2];
		D3DXVECTOR2 vLine4[2];

		vLine1[0].x = x;
		vLine1[0].y = y;
		vLine1[1].x = x;
		vLine1[1].y = y+h;

		vLine2[0].x = x+w;
		vLine2[0].y = y;
		vLine2[1].x = x+w;
		vLine2[1].y = y+h;

		vLine3[0].x = x;
		vLine3[0].y = y;
		vLine3[1].x = x+w;
		vLine3[1].y = y;

		vLine4[0].x = x;
		vLine4[0].y = y+h;
		vLine4[1].x = x+w;
		vLine4[1].y = y+h;

		g_pLine->SetWidth(2);
		g_pLine->SetAntialias(false);
		g_pLine->SetGLLines(false);
		g_pLine->Begin();
		g_pLine->Draw(vLine1, 2, 0xFF0000FF);
		g_pLine->Draw(vLine2, 2, 0xFF0000FF);
		g_pLine->Draw(vLine3, 2, 0xFF0000FF);
		g_pLine->Draw(vLine4, 2, 0xFF0000FF);
		g_pLine->End();

    }

    //---Hooked DirectX Functions-------------------------------------------------------------------------------------------------------

    HRESULT WINAPI hkEndScene(LPDIRECT3DDEVICE9 pDevice)
    {
		if(g_pTexRed == NULL) D3DXCreateTextureFromFileInMemory(pDevice, (LPCVOID)&red, sizeof(red), &g_pTexRed);
		if(g_pTexOrange == NULL) D3DXCreateTextureFromFileInMemory(pDevice, (LPCVOID)&orange, sizeof(orange), &g_pTexOrange);

		if(g_pFont == NULL) D3DXCreateFont(pDevice, 15, 0, FW_BOLD, 1, 0, DEFAULT_CHARSET, OUT_DEFAULT_PRECIS, ANTIALIASED_QUALITY, DEFAULT_PITCH | FF_DONTCARE, "Verdana", &g_pFont);
		if(g_pLine == NULL) D3DXCreateLine(pDevice, &g_pLine);

		pDevice->GetViewport(&g_ViewPort);

		if(g_pFont != NULL && g_pLine != NULL)
		{
			ModelRecLoggerMenu();
		
			if(g_uiTimer > GetTickCount())
			{
				DrawString(g_ViewPort.Width/2,g_ViewPort.Height/2, 0xFF00FF00, "Values Saved");
			}
		}

		return oEndScene(pDevice);
    }


	//
	// DrawIndexedPrimitive Hook - colors models based on numVert and primCount
	//
    HRESULT WINAPI hkDrawIndexedPrimitive(LPDIRECT3DDEVICE9 pDevice, D3DPRIMITIVETYPE PrimType,INT BaseVertexIndex,UINT MinVertexIndex,UINT NumVertices,UINT startIndex,UINT primCount)
    {
		__asm nop

		HRESULT hRet = oDrawIndexedPrimitive(pDevice, PrimType, BaseVertexIndex, MinVertexIndex, NumVertices, startIndex, primCount);
		bool bIsLogging = false;

		for(int i = 0; i < 4; i++)
		{
			if(model[i].isLogging == true)
			{
				bIsLogging = true;
				break;
			}
		}

		if(bIsLogging)
		{
			TEST::
			model[STRIDE] == g_uiStride && model[NumVertices] == NumVertices 
			&& model[PRIMCOUNT] == primCount && model[STARTINDEX] == startIndex

			if((model[STRIDE].isLogging == true ? model[STRIDE].value : g_uiStride) == g_uiStride &&
			(model[NUMVERTS].isLogging == true ? model[NUMVERTS].value : NumVertices) == NumVertices &&
			(model[PRIMCOUNT].isLogging == true ? model[PRIMCOUNT].value : primCount) == primCount &&
			(model[STARTINDEX].isLogging == true ? model[STARTINDEX].value : startIndex) == startIndex)
			{
				pDevice->SetRenderState( D3DRS_ZENABLE,false );
				pDevice->SetRenderState( D3DRS_FILLMODE,D3DFILL_SOLID );
				pDevice->SetTexture( 0, g_pTexOrange );
				oDrawIndexedPrimitive(pDevice, PrimType, BaseVertexIndex, MinVertexIndex, NumVertices, startIndex, primCount);
				pDevice->SetRenderState( D3DRS_ZENABLE, true );
				pDevice->SetRenderState( D3DRS_FILLMODE,D3DFILL_SOLID );
				pDevice->SetTexture( 0, g_pTexRed );
			}
		}

		return hRet;
    }

	//
	//SetStreamSource Hook to get the Stride
	//
    HRESULT WINAPI hkSetStreamSource(LPDIRECT3DDEVICE9 pDevice,UINT StreamNumber,IDirect3DVertexBuffer9* pStreamData,UINT OffsetInBytes,UINT Stride)
    {
		__asm nop

		if(StreamNumber == 0)
		{
			g_uiStride = Stride;
		}

		return oSetStreamSource(pDevice, StreamNumber, pStreamData, OffsetInBytes, Stride);
    }

    //-----------------------------------------------------------------------------------------------------------------------------------


	//
	// Just logs to a file
	//
    void Log(char* fmt, ...)
    {
		char buf[1024] = {0};
		va_list va_alist;
		ofstream output;

		va_start(va_alist, fmt);
		vsnprintf(buf, sizeof(buf), fmt, va_alist);
		va_end(va_alist);

		output.open("D3D9 Model Logger.txt", ios::app);
		if(output.fail()) return;
		output << buf << endl;
		output.close();
    }

    LRESULT CALLBACK MsgProc(HWND hwnd,UINT uMsg,WPARAM wParam,LPARAM lParam)
	{
		return DefWindowProc(hwnd, uMsg, wParam, lParam);
	}
    

	//
	// No purpose other than to get vtable
	//
	void DX_Init(DWORD* table)
    {
		WNDCLASSEX wc = {sizeof(WNDCLASSEX),CS_CLASSDC,MsgProc,0L,0L,GetModuleHandle(NULL),NULL,NULL,NULL,NULL,"DX",NULL};
		RegisterClassEx(&wc);
		HWND hWnd = CreateWindow("DX",NULL,WS_OVERLAPPEDWINDOW,100,100,300,300,GetDesktopWindow(),NULL,wc.hInstance,NULL);
		LPDIRECT3D9 pD3D = Direct3DCreate9( D3D_SDK_VERSION );
		D3DPRESENT_PARAMETERS d3dpp;
		ZeroMemory( &d3dpp, sizeof(d3dpp) );
		d3dpp.Windowed = TRUE;
		d3dpp.SwapEffect = D3DSWAPEFFECT_DISCARD;
		d3dpp.BackBufferFormat = D3DFMT_UNKNOWN;
		LPDIRECT3DDEVICE9 pd3dDevice;
		pD3D->CreateDevice(D3DADAPTER_DEFAULT,D3DDEVTYPE_HAL,hWnd,D3DCREATE_SOFTWARE_VERTEXPROCESSING,&d3dpp,&pd3dDevice);
		DWORD* pVTable = (DWORD*)pd3dDevice;
		pVTable = (DWORD*)pVTable[0];

		table[ES] = pVTable[42];
		table[DIP] = pVTable[82];
		table[SSS] = pVTable[100];

		DestroyWindow(hWnd);
    }
    //------------------------------------------------------------------------------------------------------------------------------------

    void ModelRecLoggerMenu()
    {
		static int menuIndex = 0;
		static int incrementBy = 1;

		DrawString(10, 10, 0xFFFF00FF, "( Stride Logger )........Last Edited By : PheonX", incrementBy);

		for(int i = 0; i < 5; i++)
		{
			if(i == menuIndex)
			{
			DrawRectangle(8, 23+(i*15), 140, 18);
			}
			DrawString(10, 25+(i*15), model[i].isLogging==true?0xFF00FF00:0xFFFF0000, "%s", model[i].type);
			if(i != LOGVALUES)
			{
				DrawString(100, 25+(i*15), model[i].isLogging==true?0xFF00FF00:0xFFFF0000, "%i", model[i].value);
			}
		}

		if(GetAsyncKeyState(VK_UP)&1) menuIndex--;
		if(GetAsyncKeyState(VK_DOWN)&1) menuIndex++;
		if(GetAsyncKeyState(VK_LEFT)&1) model[menuIndex].value-=incrementBy;
		if(GetAsyncKeyState(VK_RIGHT)&1) model[menuIndex].value+=incrementBy;

		if(GetAsyncKeyState(VK_DELETE)&1)model[menuIndex].isLogging = !model[menuIndex].isLogging;

		if(GetAsyncKeyState(VK_INSERT)&1)model[menuIndex].value = 0;
		if(GetAsyncKeyState(VK_NEXT)&1)
		{
			incrementBy *= 10;
			if(incrementBy > 1000)
			{
				incrementBy = 1;
			}
		}

		if(GetAsyncKeyState(VK_END)&1)
		{
			for(int i = 0; i < 4; i++)
			{
				model[i].isLogging = false;
			}
		}

		if(model[LOGVALUES].isLogging == true)
		{
			model[LOGVALUES].isLogging = false;
			g_uiTimer = GetTickCount() + 2000;

			Log("Stride : %i | NumVerts : %i | PrimCount : %i | StartIndex : %i",
			model[STRIDE].isLogging == true ? model[STRIDE].value : -1,
			model[NUMVERTS].isLogging == true ? model[NUMVERTS].value : -1,
			model[PRIMCOUNT].isLogging == true ? model[PRIMCOUNT].value : -1,
			model[STARTINDEX].isLogging == true ? model[STARTINDEX].value : -1);
		}

		if(menuIndex > 4) menuIndex = 0;
		if(menuIndex < 0) menuIndex = 4;

		if(model[menuIndex].value < 0) model[menuIndex].value = 0;
    }

    //------------------------------------------------------------------------------------------------------------------------------------

    DWORD WINAPI MyThread(LPVOID)
    {
		DWORD VTable[3] = {0};

		while(GetModuleHandle("d3d9.dll")==NULL)
		{ Sleep(250); }

		DX_Init(VTable);

		HOOK(EndScene,VTable[ES]);
		HOOK(DrawIndexedPrimitive,VTable[DIP]);
		HOOK(SetStreamSource,VTable[SSS]);

		return 0;
    }

    BOOL WINAPI DllMain(HMODULE hModule, DWORD dwReason, LPVOID lpvReserved)
    {
		if(dwReason == DLL_PROCESS_ATTACH){
		CreateThread(0,0,MyThread,0,0,0);
    }

    return TRUE;
}