using System.Collections.Generic;
using System;
using Utils.Enums;
using Model.Common;
using SideCar.Converters;
using SideCar.DTOs;
using Newtonsoft.Json;
using Repository;
using Utils;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace Model.GraphModel
{
    /// <summary>
    /// Class to manage operations and data realted to a Graph
    /// </summary>
    public class Graph : DataStructure
    {
        /// <summary>
        /// Autogenerated Node Id
        /// </summary>
        [JsonProperty]
        public static int NodesId{get; set;}
        /// <summary>
        /// Autogenerated Edge id
        /// </summary>
        /// <value></value>
        [JsonProperty]
        public static int EdgesId{get; set;}

        /// <summary>
        /// List to store nodes of the graph
        /// </summary>
        public Dictionary<int,GraphNode> Nodes {get; set;}

        /// <summary>
        /// Adjacent matrix of the graph
        /// </summary>
        public Dictionary<int, Dictionary<int, object>> AdjacentMtx { get; set; }
        /// <summary>
        /// Class to convert between NodeDTO and GraphNode
        /// </summary>
        private GraphNodeConverter _nodeConverter;

        private Dictionary<TraversalEnum, Action<ElementDTO>> _traversals;

        public Graph(){
            NodesId = 0;
            EdgesId = 0;
            Nodes = new Dictionary<int, GraphNode>();
            AdjacentMtx = new Dictionary<int, Dictionary<int, object>>();
            _nodeConverter = new GraphNodeConverter();
            _traversals = new Dictionary<TraversalEnum, Action<ElementDTO>>() {
                {TraversalEnum.GraphBFS, BFSTraversal},
            };
        }

        /// <summary>
        /// Method to add a node on the graph
        /// </summary>
        /// <param name="element"> Node that will be added to the graph </param>
        public override void AddElement(ElementDTO element)
        {
            GraphNode node = _nodeConverter.ToEntity((GraphNodeDTO)element);
            node.Id = NodesId++;
            Nodes.Add(node.Id,node);
            AdjacentMtx.Add(node.Id, new Dictionary<int, object>());
            //return DTO updated
            node.Coordinates = Utilities.GenerateRandomPoint();
            element = _nodeConverter.ToDto(node);
            element.Operation = AnimationEnum.CreateAnimation;
            base.Notify(element);
        }

        /// <summary>
        /// Method to remove a node of the graph
        /// </summary>
        /// <param name="element"> Node that will be removed</param>
        public override void DeleteElement(ElementDTO element)
        {
            DeleteEdges(element.Id);
            this.Nodes.Remove( element.Id );
            element.Operation = AnimationEnum.DeleteAnimation;
            base.Notify(element);
        }

        /// <summary>
        /// Method to do a traversal on the graph
        /// </summary>
        /// <param name="traversalName"> Name of the traversal to execute</param>
        public override void DoTraversal(TraversalEnum traversalName, ElementDTO startNode)
        {
            this._traversals[traversalName](startNode);
        }

        /// <summary>
        /// Method to perform a Breath First Search (BFS) traversal in the graph
        /// </summary>
        /// <param name="startNode">Node to start BFS</param>
        private void BFSTraversal(ElementDTO startNode){
            Dictionary<int, bool> visitedMap = InitializeVisiteMap();
            // Item1 destino item2 origen
            Queue<Tuple<int, int> > q = new Queue<Tuple<int, int> >();
            q.Enqueue(new Tuple<int, int>(startNode.Id, startNode.Id));
            while(q.Count > 0){
                Tuple<int, int> actualNode = q.Dequeue();
                visitedMap[actualNode.Item1] = true;
                if(actualNode.Item1 != actualNode.Item2){
                    NotifyEdge(actualNode.Item2,actualNode.Item1,AnimationEnum.PaintAnimation);
                }
                NotifyNode(actualNode.Item1,AnimationEnum.PaintAnimation);
                foreach (int key in AdjacentMtx[actualNode.Item1].Keys)
                {
                    GraphNode neighboorNode = this.Nodes[key];
                    if(!visitedMap[neighboorNode.Id]){
                        q.Enqueue(new Tuple<int, int>(neighboorNode.Id, actualNode.Item1));
                    }
                }
            }
        }

        /// <summary>
        /// Method to initialize the visited map for traversals
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, bool> InitializeVisiteMap(){
            Dictionary<int, bool> visitedMap = new Dictionary<int, bool>();
            foreach (int id in Nodes.Keys)
            {
                visitedMap.Add(id, false);
            }
            return visitedMap;
        }

        /// <summary>
        /// Method to connect two nodes bidirectionally
        /// </summary>
        /// <param name="element"></param>
        public void ConnectElements(ElementDTO graphEdgeDTO)
        {
            GraphEdgeDTO edgeDTO = (GraphEdgeDTO) graphEdgeDTO;
            edgeDTO.Id = EdgesId++;
            // TODO: validar aristas
            bool edgeStartToEnd = AdjacentMtx[edgeDTO.IdStartNode].ContainsKey(edgeDTO.IdEndNode);
            bool edgeEndToStart = AdjacentMtx[edgeDTO.IdEndNode].ContainsKey(edgeDTO.IdStartNode);
            if(!edgeStartToEnd && !edgeEndToStart){
                AdjacentMtx[edgeDTO.IdStartNode].Add(edgeDTO.IdEndNode, edgeDTO.Value);
                AdjacentMtx[edgeDTO.IdEndNode].Add(edgeDTO.IdStartNode, edgeDTO.Value);
                NotifyEdge(edgeDTO.IdStartNode,edgeDTO.IdEndNode,AnimationEnum.CreateAnimation);
            }
            else{
                Debug.Log("Ya existe la arista");
            }
        }
        /// <summary>
        /// Method to obtain list of neighbors of a given node
        /// </summary>
        /// <param name="nodeId">Id of node to search</param>
        /// <returns>List of ids representing the neighbors of the node</returns>
        public List<int> GetNeighbors(int nodeId){
            List<int> neighbors = new List<int>();
            foreach (int neighbor in AdjacentMtx[nodeId].Keys)
            {
                neighbors.Add(neighbor);
            }
            return neighbors;
        }

        //TODO: rename method or create graph from this 
        public override void CreateDataStructure()
        {
            Dictionary<int,bool> visited = new Dictionary<int, bool>();
            foreach (GraphNode node in this.Nodes.Values)
            {
                visited.Add(node.Id,false);
                NotifyNode(node.Id, AnimationEnum.CreateAnimation);
            }
            foreach (GraphNode node in this.Nodes.Values)
            {
                visited[node.Id] = true;
                foreach (int key in this.AdjacentMtx[node.Id].Keys)
                {
                    if(!visited[key]){
                        NotifyEdge(node.Id, key,AnimationEnum.CreateAnimation);
                    }
                }
            }
        }
        /// <summary>
        /// Method to obtain list of neighbors of a given node
        /// </summary>
        /// <param name="nodeId">Id of node to search</param>
        /// <returns>List of ids representing the neighbors of the node</returns>
        public void DeleteEdges(int nodeId){
            if(AdjacentMtx.ContainsKey(nodeId))
            {
                foreach (int key in AdjacentMtx.Keys)
                {
                    bool existsStartToEnd = AdjacentMtx[key].Remove(nodeId);
                    bool existsEndToStart = AdjacentMtx[nodeId].Remove(key);
                    if(existsStartToEnd || existsEndToStart){
                       //TODO: Revisar el warning de andres cuando se eliminan nodos
                       NotifyEdge(key,nodeId,AnimationEnum.DeleteAnimation);
                    }
                }
            }
            AdjacentMtx.Remove(nodeId);
        }

        //TODO: This method needs to take into account that a GraphNode may have been deleted
        private void NotifyNode(int id, AnimationEnum operation){
            GraphNode node = this.Nodes[id];
            GraphNodeDTO dto = _nodeConverter.ToDto(node);
            dto.Operation = operation;
            base.Notify(dto);
        }

        private void NotifyEdge(int start, int end, AnimationEnum operation){
            object value = null;
            if(AdjacentMtx[start].ContainsKey(end)){
                value = AdjacentMtx[start][end];
            }
            GraphEdgeDTO edge = new GraphEdgeDTO(0, value, start, end)
            {
                Operation = operation
            };
            NotifyNode(start, AnimationEnum.UpdateAnimation);
            NotifyNode(end, AnimationEnum.UpdateAnimation);
            base.Notify(edge);
        }
    }
}