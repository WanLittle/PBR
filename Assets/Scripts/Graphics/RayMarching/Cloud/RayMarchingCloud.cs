using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RayMarchingCloud : SceneViewFilter
{
    [SerializeField] private Shader _shader;

    private Material _raymarchMat;
    public Material m_raymarchMaterial
    {
        get
        {
            if (!_raymarchMat && _shader)
            {
                _raymarchMat = new Material(_shader);
                _raymarchMat.hideFlags = HideFlags.HideAndDontSave;
            }

            return _raymarchMat;
        }
    }

    private Camera _cam;

    public Camera m_camera
    {
        get
        {
            if (!_cam)
            {
                _cam = GetComponent<Camera>();
            }

            return _cam;
        }
    }


    public Transform m_light;
    public Texture2D m_noise_tex;

    public Transform[] m_clouds_transform;
    private Vector4[] m_clouds_pos;

    private void Start()
    {
        
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (!m_raymarchMaterial)
        {
            Graphics.Blit(src, dest);
            return;
        }

        for (int i = 0; i < m_clouds_transform.Length; ++i)
        {
            m_clouds_pos[i] = new Vector4 (
                    m_clouds_transform[i].position.x,
                    m_clouds_transform[i].position.y,
                    m_clouds_transform[i].position.z,
                    m_clouds_transform[i].localScale.x
                );
        }

        m_raymarchMaterial.SetMatrix("_CamFrustum", CamFrustum(m_camera));
        m_raymarchMaterial.SetMatrix("_CamToWorld", m_camera.cameraToWorldMatrix);

        RenderTexture.active = dest;
        m_raymarchMaterial.SetTexture("_MainTex", src);

        _raymarchMat.SetInt("_cloudNum", m_clouds_transform.Length);
        m_raymarchMaterial.SetVectorArray("_cloudPos", m_clouds_pos);

        m_raymarchMaterial.SetTexture("_NoiseTex", m_noise_tex);
        m_raymarchMaterial.SetVector("_LightDir", m_light ? m_light.forward : Vector3.down);
        //todo
        m_raymarchMaterial.SetVector("_CloudAndSphere", m_clouds_pos[0]);
        m_raymarchMaterial.SetVector("_Cloud2", m_clouds_pos[1]);

        GL.PushMatrix();
        GL.LoadOrtho();
        m_raymarchMaterial.SetPass(0);
        GL.Begin(GL.QUADS);

        //BL
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f); // 3.0f == 第三 row
        //BR
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);
        //TR
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);
        //TL
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.End();
        GL.PopMatrix();
    }

    private Matrix4x4 CamFrustum(Camera cam)
    {
        Matrix4x4 frustum = Matrix4x4.identity;
        float fov = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        Vector3 goUp = Vector3.up * fov;
        Vector3 goRight = Vector3.right * fov * cam.aspect;

        Vector3 TL = (-Vector3.forward - goRight + goUp);
        Vector3 TR = (-Vector3.forward + goRight + goUp);
        Vector3 BR = (-Vector3.forward + goRight - goUp);
        Vector3 BL = (-Vector3.forward - goRight - goUp);

        frustum.SetRow(0, TL);
        frustum.SetRow(1, TR);
        frustum.SetRow(2, BR);
        frustum.SetRow(3, BL);
        return frustum;
    }

    private void Update()
    {

    }
}
