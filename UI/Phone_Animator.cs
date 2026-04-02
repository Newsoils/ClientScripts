using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace CLIP.Project_Mouse.UI
{
    public class Phone_Animator : MonoBehaviour
    {
        public Animator _animator;
        public float open_duration = 1f;

        public event Action OnPhoneOpened;
        public event Action OnPhoneClosed;

        public Transform _bottom_left;
        public Transform _bottom_right;
        public Transform _top_left;
        public Transform _top_right;
        public Camera _current_ui_camera;
        public RectTransform phonePanel_RT;
        public RectTransform buttons;

        public void Open_Phone()
        {
            //Debug.Log("点击手机模型，" + open_duration + "秒后打开手机界面");
            StartCoroutine(Open_Phone_Co());
        }

        public IEnumerator Open_Phone_Co()
        {
            _animator.enabled = true;
            yield return new WaitForSeconds(0.1f);
            _animator.CrossFade("Open_Phone", 0.05f);
            yield return new WaitForSeconds(open_duration);
            _animator.enabled = false;
            OnPhoneOpened?.Invoke();
            yield return new WaitForEndOfFrame();
            MatchUI();
        }

        public void Close_Phone()
        {
            StartCoroutine(Close_Phone_Co());
        }

        public IEnumerator Close_Phone_Co()
        {
            _animator.enabled = true;
            yield return new WaitForSeconds(0.1f);
            _animator.CrossFade("Close_Phone", 0.05f);
            yield return new WaitForSeconds(open_duration);
            _animator.enabled = false;
            OnPhoneClosed?.Invoke();
        }

        public void ListenPhoneClosedOnce(Action callback)
        {
            void Wrapper()
            {
                callback?.Invoke();
                OnPhoneClosed -= Wrapper; // ⭐ 自动解绑
            }

            OnPhoneClosed += Wrapper;
        }

        public void ListenPhoneOpenedOnce(Action callback)
        {
            void Wrapper()
            {
                callback?.Invoke();
                OnPhoneOpened -= Wrapper;
            }

            OnPhoneOpened += Wrapper;
        }

        public void MatchUI()
        {
            if (_current_ui_camera == null) return;
            var _buttom_left = _current_ui_camera.WorldToScreenPoint(this._bottom_left.position);
            //var screen_pos_buttom_right = _current_ui_camera.WorldToScreenPoint(_bottom_right.position);
            //var screen_pos_top_left = _current_ui_camera.WorldToScreenPoint(_top_left.position);
            var screen_pos_top_right = _current_ui_camera.WorldToScreenPoint(_top_right.position);

            // 将四个世界点的屏幕坐标转换为父物体下的本地坐标
            Vector2 localBottomLeft, localBottomRight, localTopLeft, localTopRight;

            // 获取父级RectTransform（假设phonePanel_RT的父物体就是Canvas）
            RectTransform parentRT = phonePanel_RT.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            if (parentRT == null) return;
     

            //var _center = (_bottom_left + screen_pos_buttom_right + screen_pos_top_left + screen_pos_top_right) * 0.25f;
            //var _size = screen_pos_top_right - _buttom_left;
            //var _size = localTopRight - localBottomLeft;

            //phonePanel_RT.pivot = new Vector2(0.5f, 0.5f);
            //phonePanel_RT.anchorMin = Vector2.zero;
            //phonePanel_RT.anchorMax = Vector2.zero;

            ////Vector2 size = new Vector2(_size.x, _size.y);
            ////Vector2 center = new Vector2(_center.x, _center.y);
            ////phonePanel_RT.anchoredPosition = center;
            //phonePanel_RT.sizeDelta = _size;
            //phonePanel_RT.offsetMin = localBottomLeft;
            //phonePanel_RT.offsetMax = localTopRight;

            // 将四个世界点通过_current_ui_camera转为屏幕像素坐标
            Vector3 screenBL = _current_ui_camera.WorldToScreenPoint(_bottom_left.position);
            Vector3 screenBR = _current_ui_camera.WorldToScreenPoint(_bottom_right.position);
            Vector3 screenTL = _current_ui_camera.WorldToScreenPoint(_top_left.position);
            Vector3 screenTR = _current_ui_camera.WorldToScreenPoint(_top_right.position);

            // 调试：打印屏幕坐标，检查是否合理（不应为(0,0)且z为正）
            Debug.Log($"屏幕坐标: BL={screenBL}, BR={screenBR}, TL={screenTL}, TR={screenTR}");

            // 将屏幕坐标转换为父物体下的本地坐标（因为UI是Overlay模式，相机传null）
            Vector2 localBL, localBR, localTL, localTR;
            bool allSuccess = true;

            allSuccess &= RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, screenBL, null, out localBL);
            allSuccess &= RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, screenBR, null, out localBR);
            allSuccess &= RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, screenTL, null, out localTL);
            allSuccess &= RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, screenTR, null, out localTR);

            if (!allSuccess)
            {
                Debug.LogError("部分屏幕点转换失败！请检查父物体RectTransform是否有效。");
                return;
            }

            // 调试：打印本地坐标，应大致对应Canvas参考分辨率内的值（如1080x1920内）
            Debug.Log($"本地坐标: BL={localBL}, BR={localBR}, TL={localTL}, TR={localTR}");

            // 计算四个本地点的轴对齐包围盒
            float minX = Mathf.Min(localBL.x, localBR.x, localTL.x, localTR.x);
            float minY = Mathf.Min(localBL.y, localBR.y, localTL.y, localTR.y);
            float maxX = Mathf.Max(localBL.x, localBR.x, localTL.x, localTR.x);
            float maxY = Mathf.Max(localBL.y, localBR.y, localTL.y, localTR.y);

            Vector2 center = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
            Vector2 size = new Vector2(maxX - minX, maxY - minY);



            // 设置phonePanel_RT（锚点置零，pivot居中，位置和大小由anchoredPosition/sizeDelta决定）
            phonePanel_RT.pivot = new Vector2(0.5f, 0.5f);

            var height = 1920f;
            var width = 1080f;

            var percentB = screenBL.x / width;
            var percentR = screenBL.y / height;
                 var percentL = screenTR.x / width;
            var percentT = screenTR.y / height;
           

            phonePanel_RT.anchorMin = new Vector2(percentB, percentR );
            phonePanel_RT.anchorMax = new Vector2(percentL, percentT);
            phonePanel_RT.sizeDelta = size;

            phonePanel_RT.offsetMin = Vector2.zero;
            phonePanel_RT.offsetMax = Vector2.zero;
            //phonePanel_RT.anchoredPosition =  vector3;

            if (buttons != null)
            {
                var grid = buttons.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    float availableWidth = phonePanel_RT.rect.width;
                    float availableHeight = phonePanel_RT.rect.height;

                    int columns = 3;
                    int rows = 3;
                    float paddingX = grid.padding.left + grid.padding.right;
                    float paddingY = grid.padding.top + grid.padding.bottom;

                    float cellHeight = (availableHeight - paddingY) / rows;
                    float imageHeight = Mathf.Min((availableWidth - paddingX) / columns, cellHeight - 50);
                    float finalCellWidth = imageHeight;
                    float finalCellHeight = imageHeight + 50;

                    float minSpacing = 10f;
                    float totalCellWidth = finalCellWidth * columns;
                    float remainWidth = availableWidth - paddingX - totalCellWidth;
                    float spacingX = Mathf.Max(remainWidth / (columns - 1), minSpacing);

                    float totalCellHeight = finalCellHeight * rows;
                    float remainHeight = availableHeight - paddingY - totalCellHeight;
                    float spacingY = Mathf.Max(remainHeight / (rows - 1), minSpacing);

                    grid.cellSize = new Vector2(finalCellWidth, finalCellHeight);
                    grid.spacing = new Vector2(spacingX, spacingY);
                }
            }
        }



    }

}
