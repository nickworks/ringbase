using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LibraryStructures : MonoBehaviour
{
    [System.Serializable]
    public class RecipeStructure {
        public bool playerKnows = false;
        public string name = "Recipe";
        public Structure prefab;
    }
    public List<RecipeStructure> recipes = new List<RecipeStructure>();

    public static LibraryStructures singleton {
        get;
        private set;
    }
    void Start(){
        if(singleton != null){
            Destroy(gameObject);
            return;
        }
        singleton = this;
    }
    void OnDestroy(){
        if(singleton == this){
            singleton = null;
        }
    }
    public Structure GetPrefab(){
        if(recipes.Count == 0) return null;
        return recipes[0].prefab;
    }
}
