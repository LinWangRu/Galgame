using UnityEngine;
using UnityEngine.UI;
public class ScreenShotter : MonoBehaviour
{
    /*主方法，负责捕获当前主摄像机渲染的内容并返回一个缩小后的截图
    返回值：Texture2D，这是 Unity 中存储纹理数据的类*/
    public Texture2D CaptureScreenshot()
    {
        /*步骤 1：获取屏幕尺寸
        获取当前屏幕的宽度和高度（单位是像素）
        Screen.width 和 Screen.height 是 Unity 提供的属性，用来获取屏幕分辨率
        为什么需要？ 截图的纹理大小需要与屏幕尺寸匹配*/
        int width = Screen.width;
        int height = Screen.height;

        /*步骤 2：创建渲染纹理
        创建一个临时的 RenderTexture（渲染纹理）
        参数解析：
        width 和 height：渲染纹理的分辨率，等于屏幕尺寸
        24：深度缓冲区的位数，24 位通常足够
        RenderTexture 是什么？
        它是一种特殊的纹理，用于存储摄像机的渲染输出
        可以简单理解为摄像机的“画布”*/
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 24);

        /*步骤 3：检查主摄像机是否存在
        作用：确保场景中有主摄像机（Camera.main 是 Unity 提供的快速获取主摄像机的属性）
        为什么检查？
        如果没有主摄像机，渲染操作无法完成
        这段代码通过 Debug.LogError 输出错误信息并返回 null，防止程序崩溃*/
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(Constants.CAMERA_NOT_FOUND);
            return null;
        }

        /*步骤 4：设置摄像机渲染目标        
        为什么需要这些操作？
        默认情况下，摄像机会渲染到屏幕上
        我们需要将它的输出重定向到 RenderTexture，以便后续处理*/
        mainCamera.targetTexture = rt; // 告诉主摄像机将渲染结果输出到 rt（渲染纹理）
        RenderTexture.active = rt; // 将渲染纹理设置为当前激活的纹理，后续操作会基于它
        mainCamera.Render(); // 强制主摄像机立即渲染场景并输出到 rt

        /*步骤 5：读取像素数据
        参数解析：
        new Rect(0, 0, width, height)：从左下角(0, 0) 开始，读取整个屏幕区域
        0, 0：将像素数据写入到目标 Texture2D 的起始位置
        为什么需要 Apply()？
        Texture2D 的更改不会立即生效，调用 Apply 后，修改才会实际更新到纹理*/
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false); // 创建一个新的 Texture2D 对象，用于存储屏幕内容
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0); // 使用 ReadPixels 从 RenderTexture 中读取像素数据到 Texture2D
        screenshot.Apply(); // 调用 Apply 应用更改，将数据写入内存

        /*步骤 6：重置渲染目标并释放资源*/
        mainCamera.targetTexture = null; // 将主摄像机的渲染目标重置为默认值（屏幕）
        RenderTexture.active = null; // 清除当前激活的渲染纹理
        RenderTexture.ReleaseTemporary(rt); // 释放临时渲染纹理，避免内存泄漏

        /*步骤 7：缩小截图
        调用 ResizeTexture 方法，将截图缩小到原来的 1 / 6 尺寸
        为什么缩小？
        缩小截图可以减少存储和显示时的内存消耗
        通常用于保存游戏存档缩略图或发送网络请求*/
        Texture2D resizedScreenshot = ResizeTexture(screenshot, width / 6, height / 6);

        /*步骤 8：销毁原始截图，释放内存*/
        Destroy(screenshot);

        return resizedScreenshot;
    }

    /*辅助方法，将输入的 Texture2D 缩小到指定的分辨率
    返回值：缩小后的 Texture2D*/
    private Texture2D ResizeTexture(Texture2D original, int newWidth, int newHeight)
    {
        /*步骤 1：创建渲染纹理
        创建一个与目标分辨率相匹配的渲染纹理，并激活*/
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight, 24);
        RenderTexture.active = rt;

        /*步骤 2：使用 GPU 缩放
        使用 GPU 的 Graphics.Blit 将 original 的像素数据拷贝并缩放到 rt
        为什么使用 GPU？
        GPU 操作比手动逐像素缩放效率更高，适合实时操作*/
        Graphics.Blit(original, rt);

        /*步骤 3：读取缩放后的数据
        读取 RenderTexture 的内容到新的 Texture2D，与截图的逻辑类似*/
        Texture2D resized = new Texture2D(newWidth, newHeight, TextureFormat.RGB24, false);
        resized.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        resized.Apply();

        /*步骤 4：释放资源
        清理临时 RenderTexture，避免内存泄漏*/
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return resized;
    }
    
}