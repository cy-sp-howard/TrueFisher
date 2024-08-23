// pch.h: 此為先行編譯的標頭檔。
// 以下所列檔案只會編譯一次，可改善之後組建的組建效能。
// 這也會影響 IntelliSense 效能，包括程式碼完成以及許多程式碼瀏覽功能。
// 但此處所列的檔案，如果其中任一在組建之間進行了更新，即會重新編譯所有檔案。
// 請勿於此處新增會經常更新的檔案，如此將會對於效能優勢產生負面的影響。
#pragma once
#ifndef PCH_H
#define PCH_H

// 請於此新增您要先行編譯的標頭

#define WIN32_LEAN_AND_MEAN             // 從 Windows 標頭排除不常使用的項目
// Windows 標頭檔
#include <windows.h>


#endif //PCH_H
