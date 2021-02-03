#define UNICODE

#include <iostream>
#include <windows.h>
#include <d3d11_1.h>

#pragma comment(lib, "gdi32.lib")

LRESULT CALLBACK WndProc(HWND hwnd, UINT message, WPARAM wparam, LPARAM lparam)
{
    switch (message)
    {
    case WM_CHAR:
        if (wparam == VK_ESCAPE)
        {
            DestroyWindow(hwnd);
        }
    case WM_DESTROY:
        PostQuitMessage(0);
        break;
    default:
        return DefWindowProc(hwnd, message, wparam, lparam);
    }
}

int main()
{
    std::cout << "Hello World" << std::endl;

    WNDCLASS windowClass = {0};
    windowClass.hbrBackground = (HBRUSH)GetStockObject(WHITE_BRUSH);
    windowClass.hCursor = LoadCursor(NULL, IDC_ARROW);
    windowClass.hInstance = NULL;
    windowClass.lpfnWndProc = WndProc;
    windowClass.lpszClassName = L"WindowClass";
    windowClass.style = CS_HREDRAW | CS_VREDRAW;
    if (!RegisterClass(&windowClass))
    {
        MessageBox(NULL, L"Could not register class", L"Error", MB_OK);
    }

    HWND windowHandle = CreateWindow(L"WindowClass",
                                     L"Hello world",
                                     WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX,
                                     GetSystemMetrics(SM_CXSCREEN) / 4, // x coordinate of window start point
                                     GetSystemMetrics(SM_CYSCREEN) / 4, // y start point
                                     GetSystemMetrics(SM_CXSCREEN) / 2, // width of window
                                     GetSystemMetrics(SM_CYSCREEN) / 2, // height of the window
                                     NULL,                              // handles and such, not needed
                                     NULL,
                                     NULL,
                                     NULL);
    ShowWindow(windowHandle, SW_RESTORE);

    MSG messages;
    while (GetMessage(&messages, NULL, 0, 0) > 0)
    {
        TranslateMessage(&messages);
        DispatchMessage(&messages);
    }
    DeleteObject(windowHandle);
    return messages.wParam;
}
