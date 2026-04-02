using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Graphics {

            public class Change_Image_Material_Value : MonoBehaviour
            {
                public float _Saturation=0.5f;
                public Image _image;
                public Material _local_material;
                void Start()
                {
                    //StartCoroutine(set_val());
                    // Visual Studio 允许将 C++ 源代码保存在任意几个字符编码中。 有关源字符集和执行字符集的信息，请参阅语言文档中
                }

                public IEnumerator set_val()
                {
                    yield return new WaitForSecondsRealtime(1f);
                    if (_image.material != null)
                    {
                        set_saturation(_Saturation);
                    }
                }
                public void set_saturation(float _val)
                {
                    //must do this
                    _Saturation = _val;
                    _local_material = new Material(_image.material);

                    _image.material = null;
                    _local_material.SetFloat("_Saturation", _Saturation);

                    _image.material = _local_material;

                }


                void Update()
                {

                }
            }
        }
    }


}
