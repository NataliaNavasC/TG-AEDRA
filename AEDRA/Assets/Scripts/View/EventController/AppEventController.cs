using System;
using System.Collections.Generic;
using Controller;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;
using Utils.Enums;
using Utils.Parameters;
using View.GUI;
using View.GUI.ProjectedObjects;

namespace View.EventController
{ 
    /// <summary>
    /// Class to manage the principal event system
    /// </summary>
    public class AppEventController: MonoBehaviour
    {
        /// <summary>
        /// Dictionary that contains the actual data structure menus
        /// </summary>
        public Dictionary<MenuEnum, GameObject> _menus {get; set;}

        /// <summary>
        /// Id of the actual sub menu that is visible to user
        /// </summary>
        public MenuEnum _activeSubMenu {get; set;}

        /// <summary>
        /// Id of the previous sub menu that was visible to user
        /// </summary>
        protected MenuEnum _previousActiveSubMenu;

        /// <summary>
        /// Game Object with the actual structure menu
        /// </summary>
        private GameObject _activeMenu;

        /// <summary>
        /// Id of the actual projected data structure
        /// </summary>
        private StructureEnum _activeStructure;

        /// <summary>
        /// Indicates if an structure is projected
        /// </summary>
        private bool _hasProjectedStructure;

        private GameObject _structureProjection;

        public bool IsAnimationControlEnable;

        public static event Action<string> NotifyNotification;

        private GameObject _targetProjectionInformation;

        private TargetTypeEnum _actualTargetType;

        private bool _isAlgorithmProjected;
        private bool _isStructureProjected;
        private GameObject _algorithmMenu;

        private void Awake(){
            _actualTargetType = TargetTypeEnum.None;
            _hasProjectedStructure = false;
        }

        public void ShowNotification(string notification){
            NotifyNotification?.Invoke(notification);
        }

        /// <summary>
        /// Method that executes when a target is detected by the camera
        /// </summary>
        public void OnTargetDetected(TargetParameter targetParameter){
            if(!_hasProjectedStructure){
                if(_activeStructure != targetParameter.GetStructure()){
                    LoadTarget(targetParameter);
                }else if(_actualTargetType != targetParameter.GetTargetType()){
                    LoadTarget(targetParameter);
                }else{
                    _hasProjectedStructure = true;
                    _activeMenu?.SetActive(true);
                }
            }
            else{
                
            }
        }

        private void LoadTarget(TargetParameter targetParameter){
            if(targetParameter.GetTargetType() == TargetTypeEnum.DataStructure){
                _isStructureProjected = true;
            }
            else{
                _isAlgorithmProjected = true;
            }
            _targetProjectionInformation = targetParameter.GetTargetProjectionInformation();
            //_activeStructure is 'None' by default
            if(_structureProjection != null ){
                //2. We are changing target - If there is an existing structure we destroy it
                _structureProjection.SetActive(false);
                Destroy(_structureProjection);
                _structureProjection = null;
            }
            //If the is no structure we create a new
            LoadDataStructure(targetParameter.GetReferencePoint().transform);
            //Load target
            _activeStructure = targetParameter.GetStructure();
            _actualTargetType = targetParameter.GetTargetType();
            Command command = new LoadCommand(_activeStructure, targetParameter.GetDataFilePath());
            CommandController.GetInstance().Invoke(command);
            //Change to respective menu
            LoadStructureMenu( targetParameter.GetPrefabMenu() );
        }

        /// <summary>
        /// Method that executes when a target is lost by the camera
        /// </summary>
        public void OnTargetLost(TargetParameter targetParameter){
            if(_activeStructure == targetParameter.GetStructure()){
                GameObject structureProjection = GameObject.Find(Constants.ObjectsParentName);
                _activeMenu?.SetActive(false);
                _hasProjectedStructure = false;
                _isStructureProjected = false;
            }
        }

        /// <summary>
        /// Method to change the actual sub menu
        /// </summary>
        /// <param name="menu">Information about the next menu</param>
        public void ChangeToMenu(MenuEnumParameter menu){
            GameObject activeMenu = _menus[_activeSubMenu];
            activeMenu?.SetActive(false);
            GameObject newMenu = _menus[menu.GetMenu()];
            newMenu?.SetActive(true);
            _previousActiveSubMenu = _activeSubMenu;
            _activeSubMenu = menu.GetMenu();
            if(_activeSubMenu.ToString().Contains("Input")){
                ClearInputTextField();
            }
            if(menu.GetMenu() == MenuEnum.AnimationControlMenu){
                GameObject backButtonMenu = GameObject.Find(Constants.BackOptionsMenuParent).gameObject;
                backButtonMenu.SetActive(false);
                GameObject hamburger = GameObject.Find(Constants.HamburgerButtonName).gameObject;
                hamburger.SetActive(false);
                _targetProjectionInformation?.SetActive(true);
            }else{
                _targetProjectionInformation?.SetActive(false);
            }
        }

        /// <summary>
        /// Method that create the information for the next menu
        /// </summary>
        /// <param name="menu">Id of the new menu</param>
        public void ChangeToMenu(MenuEnum menu){
            if(!IsAnimationControlEnable){
                MenuEnumParameter menuEnumParameter = gameObject.GetComponent<MenuEnumParameter>();
                menuEnumParameter.SetMenu(menu);
                ChangeToMenu(menuEnumParameter);
            }
        }


        /// <summary>
        /// Method to detect when the user taps on back button
        /// </summary>
        public void OnTouchBackButton(){
            GameObject backButtonMenu = GameObject.Find(Constants.BackOptionsMenuParent).transform.Find("BackOptionsMenu").gameObject;
            backButtonMenu.SetActive(true);
            backButtonMenu.transform.Find("CleanOption").gameObject.SetActive(_hasProjectedStructure);

        }

        /// <summary>
        /// Method to change to previous menu
        /// </summary>
        public void OnTouchBackToPreviousMenu(){
            if(_activeSubMenu == MenuEnum.AnimationControlMenu){
                GameObject backButtonMenu = GameObject.Find(Constants.MenusParentName).transform.Find(Constants.BackOptionsMenuParent).gameObject;
                backButtonMenu.SetActive(true);
                GameObject hamburger = GameObject.Find(_menus[_activeSubMenu].transform.parent.name).transform.Find(Constants.HamburgerButtonName).gameObject;
                hamburger.SetActive(true);
                IsAnimationControlEnable = false;
                ChangeToMenu(MenuEnum.MainMenu);
                _previousActiveSubMenu = MenuEnum.MainMenu;
            }
            else{
                ChangeToMenu(_previousActiveSubMenu);
            }
        }

        /// <summary>
        /// Method to detect when the user taps on clean structure button
        /// </summary>
        public void OnTouchCleanStructure(){
            if(_actualTargetType != TargetTypeEnum.Algorithm){
                Command command = new CleanStructureCommand();
                CommandController.GetInstance().Invoke(command);
            }else{
                GameObject BackOptionsMenu = GameObject.Find("BackOptionsMenu");
                //BackOptionsMenu?.SetActive(false);
                ShowNotification("No se puede eliminar la estructura de un algoritmo");
            }
        }

        /// <summary>
        /// Method to change between scenes
        /// </summary>
        /// <param name="nextPage">Id of the next scene</param>
        public void ChangeScene(int nextPage)
        {
            GameObject structureProjection = GameObject.Find(Constants.ObjectsParentName);
            /*if(structureProjection != null){
                Command command = new SaveCommand();
                CommandController.GetInstance().Invoke(command);
            }*/
            SceneManager.LoadScene(nextPage);
        }

        /// <summary>
        /// Method to clear input field
        /// </summary>
        public void ClearInputTextField(){
            FindObjectOfType<InputField>().text = "";
        }
        private void LoadStructureMenu(GameObject menu){
            //Destroy previous menu
            if(_activeMenu != null){
                Destroy(_activeMenu); //could be failing
            }
            _activeMenu = Instantiate(menu, new Vector3(0,0,0), Quaternion.identity, GameObject.Find(Constants.MenusParentName).transform);
            _activeMenu.name = menu.name;
            _activeMenu.transform.localPosition = Vector3.zero;
            _activeMenu.transform.SetAsFirstSibling();
        }

        private void LoadDataStructure(Transform unityParent){
            GameObject prefab = Resources.Load(Constants.PrefabPath + Constants.ObjectsParentName) as GameObject;
            _structureProjection = Instantiate(prefab);
            _structureProjection.name = Constants.ObjectsParentName;
            _structureProjection.transform.parent = unityParent;
            _structureProjection.transform.localPosition = new Vector3(0,10,0);
            _structureProjection.transform.localRotation = Quaternion.Euler(-50,0,0);
        }

        public void OnAlgorithmTargetDetected(TargetParameter targetParameter){
            if(_isStructureProjected){
                if(_activeStructure == targetParameter.GetStructure()){
                    Debug.Log("ASDASD");
                    _activeMenu.SetActive(false);
                    _targetProjectionInformation = targetParameter.GetTargetProjectionInformation();
                    _targetProjectionInformation.SetActive(true);
                    _algorithmMenu = Instantiate(targetParameter.GetPrefabMenu(), new Vector3(0,0,0), Quaternion.identity, GameObject.Find(Constants.MenusParentName).transform);
                    _algorithmMenu.name = targetParameter.GetPrefabMenu().name;
                    _algorithmMenu.transform.localPosition = Vector3.zero;
                    GameObject.Find("CancelButton")?.SetActive(false);
                }
                else{
                    ShowNotification("Este algoritmo no se puede aplicar a esta estructura");
                }
            }else{
                OnTargetDetected(targetParameter);
                GameObject.Find("CancelButton")?.SetActive(false);
                _targetProjectionInformation?.SetActive(true);
            }
        }

        public void OnAlgorithmTargetLost(TargetParameter targetParameter){
            if(!_isStructureProjected){
                OnTargetLost(targetParameter);
                _targetProjectionInformation?.SetActive(false);
            }
            else{
                if(_activeStructure == targetParameter.GetStructure()){
                    GameObject.Find("CancelButton")?.SetActive(true);
                    Destroy(_algorithmMenu);
                    _activeMenu.SetActive(true);
                }
            }
        }
    }
}