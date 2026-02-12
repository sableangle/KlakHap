# KlakHap iOS 設定指南

這份指南說明如何在 Unity iOS 專案中設定 KlakHap 插件。

## 插件設定

### 1. 原生庫設定

確保 iOS 插件庫已正確放置：
- 檔案位置：`Packages/jp.keijiro.klak.hap/Plugin/iOS/libKlakHap.a`
- 確認檔案是 universal binary，包含 ARM64 和 x86_64 架構

### 2. Unity 平台設定

在 Unity Editor 中設定 iOS 原生庫：

1. 選擇 `libKlakHap.a` 檔案
2. 在 Inspector 中配置：
   - **Settings for iOS**:
     - ✅ iOS
     - Platform settings:
       - SDK: Any SDK
       - CPU: ARM64 (可選加上 x86_64 for simulator)
       - PlaceholderPath: 留空
       - AddToEmbeddedBinaries: ✅ (勾選)

### 3. Xcode 專案設定

當 Unity 建置 Xcode 專案後，請確認：

1. **Linked Frameworks**：
   - `libKlakHap.a` 應該出現在 "Link Binary With Libraries" 中

2. **Header Search Paths**：
   - 如果需要，可能需要添加標頭檔案路徑

3. **Other Linker Flags**：
   - 一般不需要額外設定，但如果遇到連結問題可以嘗試添加 `-ObjC`

## 條件編譯

KlakHap 使用條件編譯來處理不同平台的原生庫載入：

```csharp
internal static class NativeLibrary
{
#if UNITY_IOS && !UNITY_EDITOR
    internal const string Name = "__Internal";  // iOS 靜態連結
#else
    internal const string Name = "KlakHap";     // 其他平台動態連結
#endif
}
```

## 常見問題

### 1. DllNotFoundException
**錯誤**：`Unable to load DLL 'KlakHap'`

**解決方案**：
- 確認 `libKlakHap.a` 已正確設定為 iOS 平台庫
- 確認勾選了 "AddToEmbeddedBinaries"
- 重新建置 Xcode 專案

### 2. 連結錯誤
**錯誤**：`Undefined symbol: _KlakHap_xxxx`

**解決方案**：
- 確認 `libKlakHap.a` 包含正確的架構
- 使用 `lipo -info libKlakHap.a` 檢查架構
- 重新建置原生庫

### 3. 模擬器問題
**錯誤**：在 iOS 模擬器上運行失敗

**解決方案**：
- 確認 universal library 包含 x86_64 架構
- 在 Unity 的平台設定中包含 x86_64

## 驗證設定

在 Unity Editor 中驗證設定：

1. 切換到 iOS 平台
2. 檢查 Console 是否有任何原生庫載入錯誤
3. 建置並部署到設備/模擬器測試

## 架構資訊

生成的 universal library 支援：
- **ARM64**：iOS 設備（iPhone/iPad）
- **x86_64**：iOS 模擬器

```bash
# 檢查庫架構
file Packages/jp.keijiro.klak.hap/Plugin/iOS/libKlakHap.a
lipo -info Packages/jp.keijiro.klak.hap/Plugin/iOS/libKlakHap.a
```

這應該顯示：
```
Architectures in the fat file: libKlakHap.a are: x86_64 arm64
```