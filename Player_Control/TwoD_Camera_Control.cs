using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
using UnityEngine.UI;
public class TwoD_Camera_Control : MonoBehaviour
{
    public Button btn_0;
    public Button btn_1;
    public Button btn_2;
    public Button btn_3;

    public Button btn_zoom_up;
    public Button btn_zoom_down;
    public Camera _camera;

    public float move_scale = 4;

    public float zoom_scale = 0.5f;
    void Start()
    {
        //Debug.Log("Test_WX_On");

        //编译器生成通用对象文件格式 (COFF) 对象 (.obj) 文件。 链接器生成可执行文件 (.exe) 文件或动态链接库 (DLL)。


        btn_0.onClick.AddListener(() =>{
            move_cam(new Vector3(0, move_scale, 0));



        });
        btn_1.onClick.AddListener(() => {
            move_cam(new Vector3(0, move_scale*-1, 0));



        });

        btn_2.onClick.AddListener(() => {
            move_cam(new Vector3(move_scale*-1,0 , 0));



        });
        btn_3.onClick.AddListener(() => {
            move_cam(new Vector3(move_scale, 0, 0));



        });

        btn_zoom_up.onClick.AddListener(() => {
            zoom_cam(zoom_scale);



        });
        btn_zoom_down.onClick.AddListener(() => {
            zoom_cam(zoom_scale*-1.0f);



        });

    }


    void Update()
    {
        
    }

    public void move_cam(Vector3 pos)
    {
        _camera.transform.position=_camera.transform.position+ pos;
    }

    public void zoom_cam(float _val)
    {
        _camera.orthographicSize += _val;
    }
}
