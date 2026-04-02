using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace CLIP
{
    namespace Project_Mouse
    {
        namespace UI
        {
            public class Input_Locker : MonoBehaviour
            {
                public float _locking_time = 0.25f;
                [Header("Button")]
                public Button _button;
                public UnityEvent _on_btn_click=new UnityEvent();
                public bool _on_locked = false;
                private void Awake()
                {
                    _button.onClick.AddListener(start_locking);
                }
                private void OnDisable()
                {
                    _button.onClick.RemoveListener(start_locking);
                }
         
                public void start_locking()
                {
                    if (_on_locked == true) return;
                    
                  
                    //Debug.Log(this.gameObject.name + "_#_start_locking()");
                    StartCoroutine(start_locking_co());
                  
                }

                public IEnumerator start_locking_co()
                {
                    _on_locked = true;
                    if (_button != null) {
                        _button.interactable = false;
                    }
                 
              
                    if (_button != null) _on_btn_click.Invoke();
                     
                    yield return new WaitForSecondsRealtime(_locking_time);

                    if (_button != null) {
                        _button.interactable = true;
                    }
                    _on_locked = false;
                }
                

            }
        }
    }
}