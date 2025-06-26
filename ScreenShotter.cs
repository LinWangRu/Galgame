using UnityEngine;
using UnityEngine.UI;
public class ScreenShotter : MonoBehaviour
{
    /*�����������𲶻�ǰ���������Ⱦ�����ݲ�����һ����С��Ľ�ͼ
    ����ֵ��Texture2D������ Unity �д洢�������ݵ���*/
    public Texture2D CaptureScreenshot()
    {
        /*���� 1����ȡ��Ļ�ߴ�
        ��ȡ��ǰ��Ļ�Ŀ�Ⱥ͸߶ȣ���λ�����أ�
        Screen.width �� Screen.height �� Unity �ṩ�����ԣ�������ȡ��Ļ�ֱ���
        Ϊʲô��Ҫ�� ��ͼ�������С��Ҫ����Ļ�ߴ�ƥ��*/
        int width = Screen.width;
        int height = Screen.height;

        /*���� 2��������Ⱦ����
        ����һ����ʱ�� RenderTexture����Ⱦ����
        ����������
        width �� height����Ⱦ����ķֱ��ʣ�������Ļ�ߴ�
        24����Ȼ�������λ����24 λͨ���㹻
        RenderTexture ��ʲô��
        ����һ��������������ڴ洢���������Ⱦ���
        ���Լ����Ϊ������ġ�������*/
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 24);

        /*���� 3�������������Ƿ����
        ���ã�ȷ�������������������Camera.main �� Unity �ṩ�Ŀ��ٻ�ȡ������������ԣ�
        Ϊʲô��飿
        ���û�������������Ⱦ�����޷����
        ��δ���ͨ�� Debug.LogError ���������Ϣ������ null����ֹ�������*/
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError(Constants.CAMERA_NOT_FOUND);
            return null;
        }

        /*���� 4�������������ȾĿ��        
        Ϊʲô��Ҫ��Щ������
        Ĭ������£����������Ⱦ����Ļ��
        ������Ҫ����������ض��� RenderTexture���Ա��������*/
        mainCamera.targetTexture = rt; // ���������������Ⱦ�������� rt����Ⱦ����
        RenderTexture.active = rt; // ����Ⱦ��������Ϊ��ǰ������������������������
        mainCamera.Render(); // ǿ���������������Ⱦ����������� rt

        /*���� 5����ȡ��������
        ����������
        new Rect(0, 0, width, height)�������½�(0, 0) ��ʼ����ȡ������Ļ����
        0, 0������������д�뵽Ŀ�� Texture2D ����ʼλ��
        Ϊʲô��Ҫ Apply()��
        Texture2D �ĸ��Ĳ���������Ч������ Apply ���޸ĲŻ�ʵ�ʸ��µ�����*/
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false); // ����һ���µ� Texture2D �������ڴ洢��Ļ����
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0); // ʹ�� ReadPixels �� RenderTexture �ж�ȡ�������ݵ� Texture2D
        screenshot.Apply(); // ���� Apply Ӧ�ø��ģ�������д���ڴ�

        /*���� 6��������ȾĿ�겢�ͷ���Դ*/
        mainCamera.targetTexture = null; // �������������ȾĿ������ΪĬ��ֵ����Ļ��
        RenderTexture.active = null; // �����ǰ�������Ⱦ����
        RenderTexture.ReleaseTemporary(rt); // �ͷ���ʱ��Ⱦ���������ڴ�й©

        /*���� 7����С��ͼ
        ���� ResizeTexture ����������ͼ��С��ԭ���� 1 / 6 �ߴ�
        Ϊʲô��С��
        ��С��ͼ���Լ��ٴ洢����ʾʱ���ڴ�����
        ͨ�����ڱ�����Ϸ�浵����ͼ������������*/
        Texture2D resizedScreenshot = ResizeTexture(screenshot, width / 6, height / 6);

        /*���� 8������ԭʼ��ͼ���ͷ��ڴ�*/
        Destroy(screenshot);

        return resizedScreenshot;
    }

    /*����������������� Texture2D ��С��ָ���ķֱ���
    ����ֵ����С��� Texture2D*/
    private Texture2D ResizeTexture(Texture2D original, int newWidth, int newHeight)
    {
        /*���� 1��������Ⱦ����
        ����һ����Ŀ��ֱ�����ƥ�����Ⱦ����������*/
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight, 24);
        RenderTexture.active = rt;

        /*���� 2��ʹ�� GPU ����
        ʹ�� GPU �� Graphics.Blit �� original ���������ݿ��������ŵ� rt
        Ϊʲôʹ�� GPU��
        GPU �������ֶ�����������Ч�ʸ��ߣ��ʺ�ʵʱ����*/
        Graphics.Blit(original, rt);

        /*���� 3����ȡ���ź������
        ��ȡ RenderTexture �����ݵ��µ� Texture2D�����ͼ���߼�����*/
        Texture2D resized = new Texture2D(newWidth, newHeight, TextureFormat.RGB24, false);
        resized.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        resized.Apply();

        /*���� 4���ͷ���Դ
        ������ʱ RenderTexture�������ڴ�й©*/
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return resized;
    }
    
}