using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class AniControl : MonoBehaviour
{
    private Animator anim;
    private float inputV;
    private float inputH;

    void Start()
    {
        Component[] components = GetComponentsInChildren(typeof(Component));
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        inputH = Input.GetAxis("Horizontal");
        inputV = Input.GetAxis("Vertical");

        anim.SetFloat("InputH", inputH);
        anim.SetFloat("InputV", inputV);

        float moveX = inputH * 0.2f * Time.deltaTime;
        float moveZ = inputV * 0.5f * Time.deltaTime;

        transform.Translate(moveX, 0, moveZ);
    }
}
