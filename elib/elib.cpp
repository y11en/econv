// elib.cpp: 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include <Windows.h>
#include "elib.h"
#include <stdio.h>

char * WINAPI GetECmd(const char * lib, int id)
{
	HMODULE module = LoadLibrary(lib);
	if (!module) return NULL;

	PFN_GET_LIB_INFO get_info = (PFN_GET_LIB_INFO)GetProcAddress(module, FUNCNAME_GET_LIB_INFO);
	PLIB_INFO info = get_info();

	return id >= info->m_nCmdCount ? NULL : info->m_pBeginCmdInfo[id].m_szName;
}