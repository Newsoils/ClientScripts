using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CLIP
{
    namespace Project_Mouse
    {
        namespace Game_Play_System
        {
            public class CharacterClothes : MonoBehaviour
            {
                public List<GameObject> sources;
                public GameObject modelTarget;
                public GameObject boneParent;
                public LayerMask layer = 1 << 9;
                

                private Dictionary<string, Dictionary<string, List<(string, SkinnedMeshRenderer)>>> clothesData;//服装-部位-(部件名,部件skm)
                private Dictionary<string, List<SkinnedMeshRenderer>> characterRenderers;//主角的各部件的信息
                private Dictionary<string, Transform> characterBones;
                private void Start()
                {
                    InitData();
                    InitBones();
                    InitCharacter();
                }

                private void InitCharacter()
                {
                    characterRenderers = new Dictionary<string, List<SkinnedMeshRenderer>>
                    {
                        {"head", new List<SkinnedMeshRenderer>()},
                        {"body", new List<SkinnedMeshRenderer>()},
                        {"leg", new List<SkinnedMeshRenderer>() },
                        {"shoes", new List<SkinnedMeshRenderer>()},
                        {"decoration", new List<SkinnedMeshRenderer>()},
                    };
                    foreach(Transform child in modelTarget.transform)
                    {
                        Destroy(child.gameObject);
                    }
                    ChangeClothes("head", "default");
                    ChangeClothes("body", "default");
                    ChangeClothes("leg", "default");
                }
                private void InitData()
                {
                    sources = Resources.LoadAll<GameObject>("Models/Clothes").ToList();
                    clothesData = new Dictionary<string, Dictionary<string, List<(string, SkinnedMeshRenderer)>>> ();
                    foreach(var source in sources)
                    {
                        SkinnedMeshRenderer[] parts = source.GetComponentsInChildren<SkinnedMeshRenderer>();
                        foreach (var part in parts)
                        {
                            string[] names = part.name.Split('_');
                            if(names.Length < 3)//该部件命名不规范，或者不是服装
                            {
                                continue;
                            }
                            if (clothesData.ContainsKey(names[0]))
                            {
                                if (clothesData[names[0]].ContainsKey(names[2]))
                                {
                                    clothesData[names[0]][names[2]].Add((names[1], part));
                                }
                                else
                                {
                                    clothesData[names[0]].Add(names[2], new List<(string, SkinnedMeshRenderer)> { (names[1], part) });
                                }
                            }
                            else
                            {
                                clothesData.Add(names[0], new Dictionary<string, List<(string, SkinnedMeshRenderer)>> { { names[2], new List<(string, SkinnedMeshRenderer)> { (names[1], part) } } });
                            }

                        }
                    }
                    
                }
                private void InitBones()
                {
                    characterBones = boneParent.GetComponentsInChildren<Transform>().ToDictionary(property => property.name, property => property);
                }
                public void WearSuit(string clothesName)
                {
                    ChangeClothes("head", clothesName);
                    ChangeClothes("body", clothesName);
                    ChangeClothes("leg", clothesName);
                    ChangeClothes("shoes", clothesName);
                    ChangeClothes("decoration", clothesName);
                }
                public void ChangeClothes(string partName, string clothesName ,Material mat = null)
                {
                    //删除旧衣服
                    for (int i = characterRenderers[partName].Count-1 ; i >= 0 ; i--)
                    {
                        Destroy(characterRenderers[partName][i].gameObject);
                        characterRenderers[partName].RemoveAt(i);
                    }
                    //增加新衣服
                    if (clothesData[clothesName].TryGetValue(partName, out List<(string,SkinnedMeshRenderer)> parts))
                    {
                        foreach (var part in parts)
                        {
                            SkinnedMeshRenderer skm = SetSKM(part.Item2, clothesName + "_" + partName + "_" + parts.IndexOf(part));
                            characterRenderers[partName].Add(skm);

                            if (part.Item1.Contains("head")) continue;
                            if(mat != null) skm.material = mat;
                        }
                    }
                }
                private SkinnedMeshRenderer SetSKM(SkinnedMeshRenderer renderer, string name)
                {
                    List<Transform> bones = new List<Transform>();
                    foreach(var bone in renderer.bones)
                    {
                        bones.Add(characterBones[bone.name]);
                    }
                    GameObject skmObj = new GameObject();
                    skmObj.name = name;
                    skmObj.transform.parent = modelTarget.transform;
                    SkinnedMeshRenderer newRenderer = skmObj.AddComponent<SkinnedMeshRenderer>();
                    newRenderer.bones = bones.ToArray();
                    newRenderer.sharedMaterials = renderer.sharedMaterials;
                    newRenderer.sharedMesh = renderer.sharedMesh;
                    
                    newRenderer.gameObject.layer = (int)Mathf.Log(layer.value, 2);
                    return newRenderer;
                }
            }
        }
    }
}

