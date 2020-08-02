using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class SdfGeneratorWindow : EditorWindow
{
    [MenuItem("Window/SDF Texture Generator")]
    public static void OpenSdfGeneratorWindow()
    {
        EditorWindow.GetWindow(typeof(SdfGeneratorWindow));
    }
    private Texture2D m_source_texture;
    private Texture2D m_dst_texture;
    private int m_dis_texture_width = 256;
    private int m_dis_texture_height = 256;

    private void OnGUI()
    {
        m_source_texture = EditorGUILayout.ObjectField("Source Texture ", m_source_texture, typeof(Texture2D), true) as Texture2D;

        m_dis_texture_width = EditorGUILayout.IntField("Dst Texture Width: ", m_dis_texture_width);
        m_dis_texture_height = EditorGUILayout.IntField("Dst Texture Height: ", m_dis_texture_height);

        if (GUILayout.Button("Generate SDF Texture and Save PNG"))
        {
            if (m_source_texture == null)
            {
                Debug.Log("缺少 Source Texture");
            }
            else
            {
                if (m_dst_texture == null)
                {
                    m_dst_texture = new Texture2D(m_dis_texture_width, m_dis_texture_height);
                }
                GenerateSDF(m_source_texture, m_dst_texture);
                if (m_dst_texture != null)
                {
                    SaveTextureToFile(m_dst_texture, "sdf_result.png");
                }
            }
        }
    }

    public void SaveTextureToFile(Texture2D texture, string fileName)
    {
        var bytes = texture.EncodeToPNG();
        string dir = Application.dataPath + "/" + "Temp/";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        var file = File.Open(dir + fileName, FileMode.Create);
        var binary = new BinaryWriter(file);
        binary.Write(bytes);
        file.Close();
    }

    struct Pixel
    {
        public bool isIn;
        public float distance;
    }

    /*
    扫一遍源纹理中所有的像素，将像素分为in和out两种，
    对于目标纹理的每个像素，找到源纹理中对应的像素，对这个像素求最近的和自己不是同一种像素之间的距离。
    最后将所有结果clamp到 0和1 之间.
     */
    public void GenerateSDF(Texture2D source, Texture2D destination)
    {
        int sourceWidth = source.width;
        int sourceHeight = source.height;
        int targetWidth = destination.width;
        int targetHeight = destination.height;

        Pixel[][] sourcePixels = new Pixel[sourceWidth][];
        for (int i = 0; i < sourcePixels.Length; ++i)
        {
            sourcePixels[i] = new Pixel[sourceHeight];
        }
        Pixel[][] targetPixels = new Pixel[targetWidth][];
        for (int i = 0; i < targetPixels.Length; ++i)
        {
            targetPixels[i] = new Pixel[targetHeight];
        }

        Debug.Log("sourceWidth" + sourceWidth);
        Debug.Log("sourceHeight" + sourceHeight);

        int x, y;
        Color targetColor = Color.white;
        for (y = 0; y < sourceWidth; ++y)
        {
            for (x = 0; x < sourceHeight; ++x)
            {
                sourcePixels[x][y] = new Pixel();
                if (source.GetPixel(x, y) == Color.white)
                    sourcePixels[x][y].isIn = true;
                else
                    sourcePixels[x][y].isIn = false;
            }
        }

        int gapX = sourceWidth / targetWidth;
        int gapY = sourceHeight / targetHeight;
        int MAX_SEARCH_DIST = 512;
        int minx, maxx, miny, maxy;
        float max_distance = -MAX_SEARCH_DIST;
        float min_distance = MAX_SEARCH_DIST;

        for (x = 0; x < targetWidth; ++x)
        {
            for (y = 0; y < targetHeight; ++y)
            {
                targetPixels[x][y] = new Pixel();
                int sourceX = x * gapX;
                int sourceY = y * gapY;
                int min = MAX_SEARCH_DIST;

                // 遍历范围
                minx = sourceX - MAX_SEARCH_DIST;
                if (minx < 0)
                {
                    minx = 0;
                }
                miny = sourceY - MAX_SEARCH_DIST;
                if (miny < 0)
                {
                    miny = 0;
                }
                maxx = sourceX + MAX_SEARCH_DIST;
                if (maxx > (int)sourceWidth)
                {
                    maxx = sourceWidth;
                }
                maxy = sourceY + MAX_SEARCH_DIST;
                if (maxy > (int)sourceHeight)
                {
                    maxy = sourceHeight;
                }

                int dx, dy, iy, ix, distance;
                bool sourceIsInside = sourcePixels[sourceX][sourceY].isIn;
                if (sourceIsInside)
                {
                    for (iy = miny; iy < maxy; ++iy)
                    {
                        dy = iy - sourceY;
                        dy *= dy;
                        for (ix = minx; ix < maxx; ++ix)
                        {
                            bool targetIsInside = sourcePixels[ix][iy].isIn;
                            if (targetIsInside)
                            {
                                continue;
                            }
                            dx = ix - sourceX;
                            distance = (int)Mathf.Sqrt(dx * dx + dy);
                            if (distance < min)
                            {
                                min = distance;
                            }
                        }
                    }

                    if (min > max_distance)
                    {
                        max_distance = min;
                    }
                    targetPixels[x][y].distance = min;
                }
                else
                {
                    for (iy = miny; iy < maxy; iy++)
                    {
                        dy = iy - sourceY;
                        dy *= dy;
                        for (ix = minx; ix < maxx; ix++)
                        {
                            bool targetIsInside = sourcePixels[ix][iy].isIn;
                            if (!targetIsInside)
                            {
                                continue;
                            }
                            dx = ix - sourceX;
                            distance = (int)Mathf.Sqrt(dx * dx + dy);
                            if (distance < min)
                            {
                                min = distance;
                            }
                        }
                    }

                    if (-min < min_distance)
                    {
                        min_distance = -min;
                    }
                    targetPixels[x][y].distance = -min;
                }
            }
        }

        float clampDist = max_distance - min_distance;
        for (x = 0; x < targetWidth; x++)
        {
            for (y = 0; y < targetHeight; y++)
            {
                targetPixels[x][y].distance -= min_distance;
                float value = targetPixels[x][y].distance / clampDist;
                destination.SetPixel(x, y, new Color(1, 1, 1, value));
            }
        }
    }
}
