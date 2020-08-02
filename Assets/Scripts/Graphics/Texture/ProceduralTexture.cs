using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ProcedualTexture : MonoBehaviour
{
    [MenuItem("Tools/纹理/生成3D Texture")]
    static void CreateTexture3D()
    {
        // 配置纹理
        int size = 32;
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        // 创建纹理
        Texture3D texture = new Texture3D(size, size, size, format, false);
        texture.wrapMode = wrapMode;

        Color[] colors = new Color[size * size * size];

        float inverseResolution = 1.0f / (size - 1.0f);
        for (int z = 0; z < size; z++)
        {
            int zOffset = z * size * size;
            for (int y = 0; y < size; y++)
            {
                int yOffset = y * size;
                for (int x = 0; x < size; x++)
                {
                    colors[x + yOffset + zOffset] = new Color(x * inverseResolution,
                        y * inverseResolution, z * inverseResolution, 1.0f);
                }
            }
        }

        // Copy the color values to the texture
        texture.SetPixels(colors);

        // Apply the changes to the texture and upload the updated texture to the GPU
        texture.Apply();

        // Save the texture to your Unity Project
        AssetDatabase.CreateAsset(texture, "Assets/Temp/3DTexture.asset");
    }

}
