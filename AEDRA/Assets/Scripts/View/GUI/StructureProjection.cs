using System.Collections.Generic;
using Model.Common;
using SideCar.DTOs;
using UnityEngine;
using Utils;
using Utils.Enums;
using View.Animations;

namespace View.GUI
{
    /// <summary>
    /// Class to update the UI projection of any data structure of the application
    /// </summary>
    public class StructureProjection : MonoBehaviour
    {
        /// <summary>
        /// Name of the structure projection
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of the structure projection
        /// </summary>
        public string Type { get; set; }
        public List<ElementDTO> DTOs {get; set;}
        public List<ProjectedObject> ProjectedObjects {get; set;}
        private Dictionary<OperationEnum, IAnimationStrategy> _animations;
        public void Awake()
        {
            DTOs = new List<ElementDTO>();
            //TODO: initialize already existing objects in list
            ProjectedObjects = new List<ProjectedObject>();
            _animations = new Dictionary<OperationEnum, IAnimationStrategy>
            {
                { OperationEnum.AddObject, new AddNodeAnimation() },
                { OperationEnum.DeleteObject, new DeleteNodeAnimation()},
                { OperationEnum.ConnectObjects, new ConnectNodesAnimation()}
            };
        }
        public void AddDto(ElementDTO dto)
        {
            DTOs.Add(dto);
            Debug.Log(dto.GetUnityId());
            GameObject obj = GameObject.Find(dto.GetUnityId());
            obj?.GetComponentInChildren<ProjectedObject>().SetDTO(dto);
        }

        public void Animate(OperationEnum operation){
            _animations[operation].Animate();
            DTOs.Clear();
        }

        public ProjectedObject CreateObject(ElementDTO dto){
            string prefabPath = Constants.PrefabPath + dto.Name;
            GameObject prefab = Resources.Load(prefabPath) as GameObject;
            prefab = Instantiate(prefab,new Vector3(0,0,0),Quaternion.identity,this.transform);
            prefab.name = dto.GetUnityId();
            ProjectedObject createdObject = prefab.GetComponentInChildren<ProjectedObject>();
            createdObject.SetDTO(dto);
            ProjectedObjects.Add(createdObject);
            return createdObject;
        }

        public void DeleteObject(List<ProjectedObject> objectsToBeDeleted){
            foreach (ProjectedObject dto in objectsToBeDeleted)
            {
                DeleteObject(dto);
            }
        }

        public void DeleteObject(ProjectedObject objectToBeDeleted){
            this.ProjectedObjects.Remove(objectToBeDeleted);
            Destroy(objectToBeDeleted.transform.parent.gameObject);
        }
    }
}