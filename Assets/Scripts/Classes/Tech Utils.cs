using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechUtils{

    public class TechNode{

        public TechnologyDefinition definition {get; private set;}
        public bool unlocked {get; private set;}
        public TechNode parent_node {get; private set;}

        TechNode[] following_nodes;
        int width = 0;

        // CREATION //

        public TechNode(TechnologyDefinition _definition){
            definition = _definition;
            unlocked = false;
            CreateChildNodes();
        }

        void CreateChildNodes(){
            if(definition.NextTech().Length == 0)
                return;

            TechnologyDefinition[] next_tech = definition.NextTech();
            following_nodes = new TechNode[next_tech.Length];

            for(int i = 0; i < next_tech.Length; i++){
                following_nodes[i] = new TechNode(next_tech[i]);
                following_nodes[i].SetParent(this);
            }
        }

        public void SetParent(TechNode node){
            parent_node = node;
        }

        public int Width(){
            if(width != 0)
                return width;

            width = 0;
            if(HasChildren()){
                width = ChildrenCount() - 1;
                foreach(TechNode node in following_nodes){
                    width += node.Width();
                }
            }
            return width;
        }

        // SETTERS //

        public void Unlock(){
            if(!Available())
                return;
            unlocked = true;
        }

        // GETTERS //

        public bool Available(){
            if(parent_node != null)
                return parent_node.unlocked;
            return true;
        }

        public bool HasChildren(){
            if(following_nodes == null)
                return false;
            return following_nodes.Length > 0;
        }

        public int ChildrenCount(){
            if(!HasChildren())
                return 0;
            return following_nodes.Length;
        }

        public int ParentWidth(){
            if(parent_node == null)
                return 0;
            return parent_node.Width();
        }

        public int SiblingCount(){
            if(parent_node == null)
                return 0;
            return parent_node.ChildrenCount() - 1;
        }
        
        public TechNode[] Next(){return following_nodes;}

        public string Name(){return definition.Name();}
        public string Description(){return definition.Description();}
        public int Cost(){return definition.Cost();}
        public Sprite Graphic(){return definition.Graphic();}
        
        public TroopData[] Troops(){return definition.Troops();}
        public PieceData[] Buildings(){return definition.Buildings();}
        public string[] Abstracts(){return definition.Abstracts();}
       
        public bool HasArray<T>(T[] check_array){
            if(check_array == null)
                return false;
            return check_array.Length > 0;
        }

        public bool HasTroops(){return HasArray(Troops());}
        public bool HasBuildings(){return HasArray(Buildings());}
        public bool HasAbstracts(){return HasArray(Abstracts());}
    }
}