using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConectionType;

namespace ConectionType
{
    public enum TipoConexion
    {
        Interna,
        Externa,
        Filamento
    }
}

public class Nodo
{
    private Vector3 posicion;
    private List<Nodo> conexiones;
    private List<bool> conexionesVisitadas;
    private List<TipoConexion> tipoConexiones;

    public Nodo(Vector3 pos){
        posicion = pos;
        conexiones = new List<Nodo>();
        conexionesVisitadas = new List<bool>();
        tipoConexiones = new List<TipoConexion>();
    }

    public Nodo(Nodo nodoOriginal) : this(nodoOriginal.getPosicion()) { }// Solo copia el Vector3 de la posición

    public Vector3 getPosicion(){
        return posicion;
    }

    public void setPosicion(Vector3 pos){
        posicion = pos;
    }

    public List<Nodo> getConexiones(){
        return conexiones;
    }

    public Nodo getConexion(Vector3 v){
        foreach(Nodo n in conexiones){
            Vector3 currentDir = n.getPosicion() - posicion;

            if(currentDir == v){
                return n;
            }
        }

        return new Nodo(Vector3.up);
    }

    public void resetConexiones(){
        conexiones = new List<Nodo>();
        conexionesVisitadas = new List<bool>();
        tipoConexiones = new List<TipoConexion>();
    }

    public List<bool> getVisitados(){
        return conexionesVisitadas;
    }

    public int getNumVisitados(){
        int visitados = 0;

        foreach(bool b in conexionesVisitadas){
            if(b){
                visitados++;
            }
        }

        return visitados;
    }

    public bool isVisitadaConexion(Nodo conexion){
        int i = conexiones.IndexOf(conexion);
        return conexionesVisitadas[i];
    }

    public bool isVisited(Nodo conectionNotMentioned){
        int j = conexiones.IndexOf(conectionNotMentioned);

        for(int i = 0 ; i < conexionesVisitadas.Count ; i++){
            if(i != j && conexionesVisitadas[i]){
                return true;
            }
        }

        return false;
    }

    public bool allConectionsVisited(){
        for(int i = 0 ; i < conexionesVisitadas.Count ; i++){
            if(!conexionesVisitadas[i]){
                return false;
            }
        }

        return true;
    }

    public void setConexionVisitada(Nodo conexion){
        int i = conexiones.IndexOf(conexion);
        if(i != -1){
            conexionesVisitadas[i] = true;

            Nodo vecino = conexiones[i];
            int j = vecino.getConexiones().IndexOf(this);
            vecino.getVisitados()[j] = true;
        }
    }

    public TipoConexion getTipoConexion(Nodo conexion){
        int i = conexiones.IndexOf(conexion);
        return tipoConexiones[i];
    }

    public void setTipoConexion(int i, TipoConexion t){
        tipoConexiones[i] = t;
    }

    public void setTipoConexion(Nodo conexion, TipoConexion t){
        int i = conexiones.IndexOf(conexion);
        tipoConexiones[i] = t;
    }

    public int getIndexFirstFilament(){
        for(int i = 0 ; i < tipoConexiones.Count ; i++){
            if(tipoConexiones[i] == TipoConexion.Filamento){
                return i;
            }
        }

        return -1;
    }

    public void addConexion(Nodo newConexion){ 
        if(!existeConexion(newConexion)){
            conexiones.Add(newConexion);
            conexionesVisitadas.Add(false);
            tipoConexiones.Add(TipoConexion.Interna);
        }
    }

    public void removeConexion(Nodo delConexion){
        if(existeConexion(delConexion)){
            int i = conexiones.IndexOf(delConexion);
            conexionesVisitadas.RemoveAt(i);
            tipoConexiones.RemoveAt(i);

            conexiones.Remove(delConexion);
        }
    }

    public void resetVisitados(){
        for(int i = 0 ; i < conexionesVisitadas.Count ; i++){
            conexionesVisitadas[i] = false;
        }
    }

    private bool existeConexion(Nodo nodo){
        foreach(Nodo n in conexiones){
            if(n == nodo){
                return true;
            }
        }

        return false;
    }
}
