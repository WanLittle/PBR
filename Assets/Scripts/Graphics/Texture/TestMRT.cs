using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class TestMRT : PostEffectsBase
{
    private Material testMRTMaterial = null;
    public Material material
    {
        get
        {
            testMRTMaterial = CheckShaderAndCreateMaterial(Shader.Find("Little/TestMRT"), testMRTMaterial);
            return testMRTMaterial;
        }
    }
    private RenderTexture[] mrtTex = new RenderTexture[2];
    private RenderBuffer[] mrtRB = new RenderBuffer[2];

    new void Start()
    {
        CheckResources();
        mrtTex[0] = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        mrtTex[1] = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        mrtRB[0] = mrtTex[0].colorBuffer;
        mrtRB[1] = mrtTex[1].colorBuffer;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material != null)
        {
            RenderTexture oldRT = RenderTexture.active;

            Graphics.SetRenderTarget(mrtRB, mrtTex[0].depthBuffer);

            GL.Clear(false, true, Color.clear);

            GL.PushMatrix();
            GL.LoadOrtho();

            testMRTMaterial.SetPass(0);     //Pass 0 outputs 2 render textures.

            //Render the full screen quad manually.
            GL.Begin(GL.QUADS);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
            GL.End();

            GL.PopMatrix();

            RenderTexture.active = oldRT;

            //Show the result
            testMRTMaterial.SetTexture("_Tex0", mrtTex[0]);
            testMRTMaterial.SetTexture("_Tex1", mrtTex[1]);
            Graphics.Blit(source, destination, testMRTMaterial, 1);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}