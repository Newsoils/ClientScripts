using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GuideDefinition_SO", menuName = "Project_Mouse/Guide/GuideDefinition_SO")]
public class GuideDefinition_SO : ScriptableObject
{
    [Title("新手引导数据", Bold = true)]
    [ListDrawerSettings(
       ShowFoldout = true,              // 每个引导可折叠
       ListElementLabelName = nameof(GuideDefinition.Id), // 折叠栏显示 GuideId
       DraggableItems = true,           // 可拖拽排序
       ShowIndexLabels = false
   )]
    public List<GuideDefinition> guideDefinitions = new List<GuideDefinition>();
}
