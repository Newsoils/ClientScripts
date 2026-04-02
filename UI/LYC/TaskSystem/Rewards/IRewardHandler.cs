using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CLIP.Project_Mouse.LYC.TaskSystem
{
    public interface IRewardHandler
    {
        public void GrantReward(PlayerRewardData playerData);
    }
}
