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
        
        public string Name(){return definition.Name();}
        public string Description(){return definition.Description();}
        public int Cost(){return definition.Cost();}
        public Sprite Graphic(){return definition.Graphic();}
        public TroopData[] Troops(){return definition.Troops();}
        public PieceData[] Buildings(){return definition.Buildings();}
        public TechNode[] Next(){return following_nodes;}
        public bool HasTroops(){return definition.Troops().Length > 0;}
        public bool HasBuildings(){return definition.Buildings().Length > 0;}
    }

    public class AbstractTechManager{

        List<AbstractTechnology> unlocked_odd_tech = new List<AbstractTechnology>();

        public AbstractTechManager(){unlocked_odd_tech = new List<AbstractTechnology>();}

        public bool Unlocked(AbstractTechnology tech){return unlocked_odd_tech.Contains(tech);}
        public bool Unlocked(string tech_name){
            
            if(unlocked_odd_tech.Count == 0)
                return false;

            tech_name = tech_name.ToUpper();
            foreach(AbstractTechnology abstr in unlocked_odd_tech){
                if(abstr.Reference().ToUpper() == tech_name)
                    return true;
            }
        }

        public void Unlock(AbstractTechnology tech){
            if(!unlocked_odd_tech.Contains(tech))
                unlocked_odd_tech.Add(tech);
        }
    }
}