using Utils.Enums;
using SideCar.DTOs;
using Model.Common;
using Utils;
using UnityEngine;// REMOVEEEEEEEEEEEEEEEEEEEEEEEEE

namespace Model.TreeModel
{
    public class BinarySearchTreeNode
    {
        /// <summary>
        /// Id node
        /// </summary>
        public int Id{get;set;}

        /// <summary>
        /// Node value
        /// </summary>
        public int Value {get; set;}

        /// <summary>
        /// Left node child
        /// </summary>
        /// <value>Null when node is created</value>
        public BinarySearchTreeNode LeftChild {get; set;}

        /// <summary>
        /// Right node child
        /// </summary>
        /// <value>Null when node is created</value>
        public BinarySearchTreeNode RightChild {get; set;}

        /// <summary>
        /// Node coordinates on view
        /// </summary>
        /// <value></value>
        public Point Coordinates {get; set;}

        /// <summary>
        /// Node coordinates on view
        /// </summary>
        /// <value></value>
        public int Level {get; set;}

        public BinarySearchTreeNode(int id, int value, Point point, int Level){
            this.Id = id;
            this.Value = value;
            this.Coordinates = point;
            this.Level = Level;
        }

        /// <summary>
        /// Method that indicates if node is Leaf
        /// </summary>
        /// <returns>True if node have child, false if both are null</returns>
        public bool IsLeaf(){
            return this.LeftChild == null && this.RightChild == null;
        }

        /// <summary>
        /// Method to add a node on the tree recursively
        /// </summary>
        /// <param name="id">Node id to add</param>
        /// <param name="value">Valu that new node will contain</param>
        public void AddElement(int id, int value, Point point){
            if(value > this.Value){
                if(this.RightChild!=null){
                    NotifyEdge(this, this.RightChild, AnimationEnum.PaintAnimation);
                    NotifyNode(this, this.RightChild, AnimationEnum.PaintAnimation);
                    this.RightChild.AddElement(id,value, point);
                }
                else{
                    if(this.Level < Constants.MaxTreeLevel){
                        this.RightChild = new BinarySearchTreeNode(id, value, point, this.Level+1);
                        NotifyNode(null, this, AnimationEnum.UpdateAnimation);
                        NotifyNode(this,this.RightChild, AnimationEnum.CreateAnimation);
                        NotifyEdge(this, this.RightChild, AnimationEnum.CreateAnimation);
                    }
                    else{
                        DataStructure.ShowNotification("El nodo supera el nivel máximo permitido");
                    }
                    
                }
            }
            else if(value < this.Value){
                if(this.LeftChild!=null){
                    NotifyEdge(this, this.LeftChild, AnimationEnum.PaintAnimation);
                    NotifyNode(this, this.LeftChild, AnimationEnum.PaintAnimation);
                    this.LeftChild.AddElement(id,value, point);
                }
                else{
                    if(this.Level < Constants.MaxTreeLevel){
                        this.LeftChild = new BinarySearchTreeNode(id, value, point, this.Level+1);
                        NotifyNode(null, this, AnimationEnum.UpdateAnimation);
                        NotifyNode(this,this.LeftChild, AnimationEnum.CreateAnimation);
                        NotifyEdge(this, this.LeftChild, AnimationEnum.CreateAnimation);
                    }
                    else{
                        DataStructure.ShowNotification("El nodo supera el nivel máximo permitido");
                    }
                }
            }
        }

        /// <summary>
        /// Method to delete a node of the tree recursively
        /// </summary>
        /// <param name="value">Value of the node to delete</param>
        public void DeleteElement(int value){
            if(value > this.Value){
                if(this.RightChild != null){
                    if(value == this.RightChild.Value && this.RightChild.IsLeaf()){
                        NotifyNode(null, this, AnimationEnum.UpdateAnimation);
                        NotifyEdge(this, this.RightChild, AnimationEnum.DeleteAnimation);
                        NotifyNode(this, this.RightChild, AnimationEnum.DeleteAnimation);
                        this.RightChild = null;
                    }
                    else{
                        NotifyEdge(this, this.RightChild, AnimationEnum.PaintAnimation);
                        NotifyNode(this, this.RightChild, AnimationEnum.PaintAnimation);
                        this.RightChild.DeleteElement(value);
                    }
                }
            }
            else if(value < this.Value){
                if(this.LeftChild != null){
                    if(value == this.LeftChild.Value && this.LeftChild.IsLeaf()){
                        NotifyNode(null, this, AnimationEnum.UpdateAnimation);
                        NotifyEdge(this, this.LeftChild, AnimationEnum.DeleteAnimation);
                        NotifyNode(this, this.LeftChild, AnimationEnum.DeleteAnimation);
                        this.LeftChild = null;
                    }
                    else{
                        NotifyEdge(this, this.LeftChild, AnimationEnum.PaintAnimation);
                        NotifyNode(this, this.LeftChild, AnimationEnum.PaintAnimation);
                        this.LeftChild.DeleteElement(value);
                    }
                }
            }
        }

        /// <summary>
        /// Method that notifies to view when a edge is modified
        /// </summary>
        /// <param name="parent">Node parent</param>
        /// <param name="node">Node</param>
        /// <param name="operation">Operation applied to edge</param>
        public void NotifyEdge(BinarySearchTreeNode parent, BinarySearchTreeNode node, AnimationEnum operation){
            EdgeDTO dto = new EdgeDTO(0, "", parent.Id, node.Id)
            {
                Operation = operation
            };
            DataStructure.Notify(dto);
        }

        /// <summary>
        /// Method that notifies to view when a node is modified
        /// </summary>
        /// <param name="parent">Node parent</param>
        /// <param name="node">Modified node</param>
        /// <param name="operation">Operation applied to node</param>
        public void NotifyNode(BinarySearchTreeNode parent, BinarySearchTreeNode node, AnimationEnum operation, int step = -1){
            if(node != null){
                int? parentId = null;
                bool isLeft = false;
                if(parent != null){
                    parentId = parent.Id;
                    if(parent.LeftChild != null && parent.LeftChild.Value == node.Value){
                        isLeft = true;
                    }
                }
                BinarySearchNodeDTO dto = new BinarySearchNodeDTO(node.Id, node.Value, parentId, isLeft, node.LeftChild?.Id, node.RightChild?.Id){
                    Operation = operation,
                    Coordinates = new Point(this.Coordinates.X, this.Coordinates.Y, this.Coordinates.Z),
                    Step = step
                };
                DataStructure.Notify(dto);
            }
        }
    }
}