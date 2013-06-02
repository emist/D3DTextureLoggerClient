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

    #define ES 0
    #define DIP 1
    #define SSS 2

    void Log(char* fmt, ...);
    void ModelRecLoggerMenu();

    unsigned int g_uiTimer = NULL;

    //---------------------------------------------------------------------------------------------------------------------------------

    typedef HRESULT (WINAPI* tDrawIndexedPrimitive)(LPDIRECT3DDEVICE9 pDevice, D3DPRIMITIVETYPE PrimType,INT BaseVertexIndex,UINT MinVertexIndex,UINT NumVertices,UINT startIndex,UINT primCount);
    tDrawIndexedPrimitive oDrawIndexedPrimitive;


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
    




}