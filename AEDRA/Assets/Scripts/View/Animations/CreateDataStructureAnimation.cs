using DG.Tweening;
using SideCar.DTOs;
using UnityEngine;
using Utils;
using View.Animations;
using View.GUI;

namespace View.Animations
{
    internal class CreateDataStructureAnimation : IAnimationStrategy
    {
        public void Animate()
        {
            Sequence animationList = DOTween.Sequence();
            StructureProjection structureProjection = GameObject.FindObjectOfType<StructureProjection>();
            foreach (ElementDTO dto in structureProjection.DTOs){
                Debug.Log(dto.Id);
                ProjectedObject obj = structureProjection.CreateObject(dto);
                Vector3 coordinates = new Vector3(dto.Coordinates.X, dto.Coordinates.Y,dto.Coordinates.Z);
                obj.Move(coordinates);
                 obj.AnimationTime = 0;
                animationList.Append(obj.Animations[dto.Operation]());
                obj.AnimationTime = Constants.AnimationTime;
            }
        }
    }
}