using UnityEngine;
using Controller;
using SideCar.DTOs;
using View.GUI.ProjectedObjects;
using System.Collections.Generic;
using Utils.Enums;
using UnityEngine.UI;
using Utils;
using Utils.Parameters;

namespace View.EventController
{
    /// <summary>
    /// Class to manage events received from an action executed on a graph by the user
    /// </summary>
    public class GraphEventController : AppEventController
    {
        /// <summary>
        /// Instance of the application selection controller
        /// </summary>
        private SelectionController _selectionController;

        public void Start(){
            _selectionController = FindObjectOfType<SelectionController>();
            base._menus = new Dictionary<MenuEnum, GameObject>
            {
                { MenuEnum.MainMenu, gameObject.transform.Find("MainMenu").gameObject },
                { MenuEnum.TraversalMenu, gameObject.transform.Find("TraversalMenu").gameObject },
                { MenuEnum.NodeSelectionMenu, gameObject.transform.Find("NodeSelectionMenu").gameObject },
                { MenuEnum.NodeMultiSelectionMenu, gameObject.transform.Find("NodeMultiSelectionMenu").gameObject },
                { MenuEnum.AddElementInputMenu, gameObject.transform.Find("AddElementInputMenu").gameObject },
                { MenuEnum.AnimationControlMenu, gameObject.transform.Find("AnimationControlMenu").gameObject}
            };
            base._activeSubMenu = MenuEnum.MainMenu;
            base.gameObject.AddComponent<MenuEnumParameter>();
            base.ChangeToMenu(MenuEnum.MainMenu);
        }

        /// <summary>
        /// Graph controller subscribes to update menu event for updating UI
        /// </summary>
        public void OnEnable() {
            SelectionController.UpdateMenu += UpdateMenuOnSelection;
            SelectionController.OnEmptyTouch += OnEmptyTouch;
        }

        /// <summary>
        /// Projection unsubscribes to update menu event for updating UI
        /// </summary>
        public void OnDisable() {
            SelectionController.UpdateMenu -= UpdateMenuOnSelection;
            SelectionController.OnEmptyTouch -= OnEmptyTouch;
        }

        /// <summary>
        /// Hide menus when touching empty space
        /// </summary>
        public void OnEmptyTouch(){
            GameObject BackOptionsMenu = GameObject.Find("BackOptionsMenu");
            if(_activeSubMenu.ToString().Contains("Input")){
                ChangeToMenu(MenuEnum.MainMenu);
            }
            BackOptionsMenu?.SetActive(false);
        }

        /// <summary>
        /// Method to change the actual menu depending on user selection
        /// </summary>
        /// <param name="selectedObjects">List of the user selected objects</param>
        private void UpdateMenuOnSelection(List<ProjectedObject> selectedObjects){
            switch(selectedObjects.Count){
                case 0: base.ChangeToMenu(MenuEnum.MainMenu);
                break;
                case 1: base.ChangeToMenu(MenuEnum.NodeSelectionMenu);
                break;
                default: base.ChangeToMenu(MenuEnum.NodeMultiSelectionMenu);
                break;
            }
        }

        /// <summary>
        /// Method to detect when the user taps on add node button
        /// </summary>
        public void OnTouchAddNode()
        {
            List<ProjectedObject> objs = _selectionController.GetSelectedObjects();
            string value = FindObjectOfType<InputField>().text;
            List<int> neighbors = new List<int>();
            GraphNodeDTO nodeDTO = new GraphNodeDTO(0, value, neighbors);
            if (objs.Count == 1 && objs[0].GetType() == typeof(ProjectedNode))
            {
                nodeDTO.ElementToConnectID = objs[0].Dto.Id;
            }
            AddElementCommand addCommand = new AddElementCommand(nodeDTO);
            CommandController.GetInstance().Invoke(addCommand);
        }

        /// <summary>
        /// Method to detect when the user taps on delete node button
        /// </summary>
        public void OnTouchDeleteNode()
        {
            List<ProjectedObject> objs = new List<ProjectedObject>(_selectionController.GetSelectedObjects());
            foreach(ProjectedObject selectedObject in objs){
                if(selectedObject.GetType() == typeof(ProjectedNode)){
                    GraphNodeDTO nodeDTO = (GraphNodeDTO)selectedObject.Dto;
                    DeleteElementCommand deleteCommand = new DeleteElementCommand(nodeDTO);
                    CommandController.GetInstance().Invoke(deleteCommand);
                }
            }
        }

        /// <summary>
        /// Method to detect when the user taps the on connect node button
        /// </summary>
        public void OnTouchConnectNodes()
        {
            List<ProjectedObject> objs = _selectionController.GetSelectedObjects();
            if (objs.Count == 2)
            {
                if(objs[0].GetType() == typeof(ProjectedNode) && objs[1].GetType() == typeof(ProjectedNode)){
                    EdgeDTO edgeDTO = new EdgeDTO(0, Utilities.GenerateRandomDouble(), objs[0].Dto.Id, objs[1].Dto.Id);
                    ConnectElementsCommand connectCommand = new ConnectElementsCommand(edgeDTO);
                    CommandController.GetInstance().Invoke(connectCommand);
                }
            }
            else
            {
                //TODO: delete this
                Debug.Log("Numero de nodos seleccionados inválido");
            }
        }

        /// <summary>
        /// Method to detect when the user taps on BFS traversal button
        /// </summary>
        public void OnTouchBFSTraversal(){
            base.IsAnimationControlEnable = true;
            List<ProjectedObject> objs = _selectionController.GetSelectedObjects();
            if (objs.Count == 1 && objs[0].GetType() == typeof(ProjectedNode))
            {
                    GraphNodeDTO nodeDTO = (GraphNodeDTO)objs[0].Dto;
                    DoTraversalCommand traversalCommand = new DoTraversalCommand(TraversalEnum.GraphBFS,nodeDTO);
                    _selectionController.DeselectAllObjects();
                    CommandController.GetInstance().Invoke(traversalCommand);
            }
            else
            {
                //TODO: delete this
                Debug.Log("Numero de nodos seleccionados inválido");
            }
        }

        /// <summary>
        /// Method to detect when the user taps on DFS traversal button
        /// </summary>
        public void OnTouchDFSTraversal(){
            base.IsAnimationControlEnable = true;
            List<ProjectedObject> objs = _selectionController.GetSelectedObjects();
            if (objs.Count == 1 && objs[0].GetType() == typeof(ProjectedNode))
            {
                    GraphNodeDTO nodeDTO = (GraphNodeDTO)objs[0].Dto;
                    Command traversalCommand = new DoTraversalCommand(TraversalEnum.GraphDFS,nodeDTO);
                    _selectionController.DeselectAllObjects();
                    CommandController.GetInstance().Invoke(traversalCommand);
            }
            else
            {
                //TODO: delete this
                Debug.Log("Numero de nodos seleccionados inválido");
            }
        }

        public void OnTouchExecuteAlgorithm(){
            List<ProjectedObject> objs = _selectionController.GetSelectedObjects();
            if (objs.Count == 2)
            {
                if(objs[0].GetType() == typeof(ProjectedNode) && objs[1].GetType() == typeof(ProjectedNode)){
                    DoAlgorithmCommand doAlgorithmCommand = new DoAlgorithmCommand(AlgorithmEnum.Dijkstra,new List<ElementDTO>(){objs[0].Dto, objs[1].Dto});
                    CommandController.GetInstance().Invoke(doAlgorithmCommand);
                }
            }
            else
            {
                Debug.Log("Numero de nodos seleccionados inválido");
            }
        }
    }
}