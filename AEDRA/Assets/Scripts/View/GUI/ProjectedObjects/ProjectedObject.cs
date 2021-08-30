using SideCar.DTOs;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System;
using Utils.Enums;
using Utils;

namespace View.GUI.ProjectedObjects
{
    public class ProjectedObject : MonoBehaviour
    {
        [SerializeField]
        private bool _selectable;
        private bool _selected;
        public Dictionary<AnimationEnum, Func<Tween>> Animations { get; set; }
        public ElementDTO Dto { get; set; }
        public bool IsCreated { get; set; }
        public float AnimationTime{get; set;}
        virtual public void Awake()
        {
            AnimationTime = Constants.AnimationTime;
            IsCreated = false;
            Animations = new Dictionary<AnimationEnum, Func<Tween>> {
                {AnimationEnum.CreateAnimation, CreateAnimation},
                {AnimationEnum.DeleteAnimation, DeleteAnimation},
                {AnimationEnum.PaintAnimation, PaintAnimation},
                {AnimationEnum.UpdateAnimation, UpdateAnimation}
            };
        }

        virtual public Tween CreateAnimation()
        {
            return null;
        }

        virtual public Tween DeleteAnimation()
        {
            return null;
        }

        virtual public Tween PaintAnimation()
        {
            return null;
        }

        virtual public Tween UpdateAnimation()
        {
            return default;
        }

        virtual public void Move(Vector3 coordinates)
        {
        }

        virtual public void SetDTO(ElementDTO dto)
        {
            Dto = dto;
        }

        public override bool Equals(object other)
        {
            return Equals(other as ProjectedObject);
        }

        public bool Equals(ProjectedObject other)
        {
            return other != null &&
                Dto.Id == other.Dto.Id;
        }

        public override int GetHashCode()
        {
            //TODO: Look how to implement this method since library HashCode.Combine can't be used
            throw new NotImplementedException();
        }
        public bool IsSelectable()
        {
            return _selectable;
        }

        public void SetSelectable(bool selectable)
        {
            this._selectable = selectable;
        }
        public bool IsSelected()
        {
            return _selected;
        }

        public void SetSelected(bool selected)
        {
            this._selected = selected;
            MeshRenderer mesh = gameObject.GetComponentInChildren<MeshRenderer>();
            if (_selected)
            {
                mesh.material.DOColor(Color.red, 0);
            }
            else
            {
                mesh.material.DOColor(Color.white, 0);
            }
        }
    }
}