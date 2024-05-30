using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodoCopia : Nodo
{
    Nodo nodoOriginal;
    
    public NodoCopia(Nodo original) : base(original.getPosicion()){
        nodoOriginal = original;
    }

    public Nodo getNodoOriginal(){
        return nodoOriginal;
    }
}
