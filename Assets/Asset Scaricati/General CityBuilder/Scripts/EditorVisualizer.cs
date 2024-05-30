using UnityEngine;
using UnityEditor;
using System;

public enum Estado
{
    NoCity,
    GenRoads,
    DrawRoads,
    GenCityCells,
    GenBuildingLots,
    BuildingsCreated,
    GenAI
}

[CustomEditor(typeof(CityBuilder))]
public class EditorVisualizer : Editor
{
    Estado state = Estado.NoCity;

    public override void OnInspectorGUI(){
        base.OnInspectorGUI();

        CityBuilder cityBuilder = (CityBuilder)target;

        if(state == Estado.NoCity){
            if(GUILayout.Button("Generar Caminos")){
                cityBuilder.BuildCity();
                cityBuilder.RenderLines();
                state = Estado.GenRoads;
            }else if(GUILayout.Button("Generar Ciudad")){
                try{
                    cityBuilder.BuildCity();
                    cityBuilder.RenderLines();

                    cityBuilder.RenderRoads();

                    cityBuilder.DetectCityCells();

                    cityBuilder.GenerateBuildingLots();

                    if(cityBuilder.houseModels.Count > 0){
                        cityBuilder.GenerateBuildings();

                        if(cityBuilder.AI){
                            cityBuilder.GenerateAI();
                            state = Estado.GenAI;
                        }else{
                             state = Estado.BuildingsCreated;
                        }
                    }else{
                        Debug.Log("ERROR: Se necesita al menos un modelo de casa para poder edificar");
                    }
                }catch(Exception e){
                    cityBuilder.DestroyCity();
                    state = Estado.NoCity;
                    Debug.Log("Se ha reseteado la cuidad por este error: " + e.Message);
                }
            }
        }else if(state == Estado.GenRoads){
            if(GUILayout.Button("Dibujar Caminos")){
                cityBuilder.RenderRoads();
                state = Estado.DrawRoads;
            }else if(GUILayout.Button("Generar Caminos")){
                cityBuilder.BuildCity();
                cityBuilder.RenderLines();
            }else if(GUILayout.Button("Resetear CityBuilder")){
                cityBuilder.DestroyCity();
                state = Estado.NoCity;
            }
        }else if(state == Estado.DrawRoads){
            if(GUILayout.Button("Dibujar Caminos")){
                cityBuilder.RenderRoads();
            }else if(GUILayout.Button("Detectar City Cells")){
                try{
                    cityBuilder.DetectCityCells();
                    state = Estado.GenCityCells;
                }catch(Exception e){
                    cityBuilder.DestroyCity();
                    state = Estado.NoCity;
                    Debug.Log("Se ha reseteado la cuidad por este error: " + e.Message);
                }
            }else if (GUILayout.Button("Paso Anterior")){
                cityBuilder.RenderLines();
                state = Estado.GenRoads;
            }else if(GUILayout.Button("Resetear CityBuilder")){
                cityBuilder.DestroyCity();
                state = Estado.NoCity;
            }
        }else if(state == Estado.GenCityCells){
            if (GUILayout.Button("Generar Building Lots")){
                cityBuilder.GenerateBuildingLots();
                state = Estado.GenBuildingLots;
            }else if (GUILayout.Button("Paso Anterior")){
                cityBuilder.DestroyCityCells();
                state = Estado.DrawRoads;
            }else if(GUILayout.Button("Resetear CityBuilder")){
                cityBuilder.DestroyCity();
                state = Estado.NoCity;
            }
        }else if(state == Estado.GenBuildingLots){
            if (GUILayout.Button("Edificar Ciudad")){
                if(cityBuilder.houseModels.Count > 0){
                    cityBuilder.GenerateBuildings();
                    state = Estado.BuildingsCreated;
                }else{
                    Debug.Log("ERROR: Se necesita al menos un modelo de casa para poder edificar");
                }
            }else if (GUILayout.Button("Paso Anterior")){
                cityBuilder.DestroyBuildingLots();
                state = Estado.GenCityCells;
            }else if(GUILayout.Button("Resetear CityBuilder")){
                cityBuilder.DestroyCity();
                state = Estado.NoCity;
            }
        }else if(state == Estado.BuildingsCreated && cityBuilder.AI){
            if (GUILayout.Button("Generar IA")){
                cityBuilder.GenerateAI();
                state = Estado.GenAI;
            }else if (GUILayout.Button("Paso Anterior")){
                cityBuilder.DestroyBuildings();
                state = Estado.GenBuildingLots;
            }else if(GUILayout.Button("Resetear CityBuilder")){
                cityBuilder.DestroyCity();
                state = Estado.NoCity;
            }
        }else{
            if(cityBuilder.defaultPlayerSys){
                if(GUILayout.Button("Colocar Default Player System")){
                    cityBuilder.SetUpDeafultPlayerSys();
                }else if (GUILayout.Button("Paso Anterior")){
                    if(cityBuilder.city.transform.Find("IA") != null){
                        cityBuilder.DestroyAI();
                        state = Estado.BuildingsCreated;
                    }else{
                        cityBuilder.DestroyBuildings();
                        state = Estado.GenBuildingLots;
                    }
                }else if(GUILayout.Button("Resetear CityBuilder")){
                    cityBuilder.DestroyCity();
                    state = Estado.NoCity;
                }
            }else{
                if (GUILayout.Button("Paso Anterior")){
                    if(cityBuilder.city.transform.Find("IA") != null){
                        cityBuilder.DestroyAI();
                        state = Estado.BuildingsCreated;
                    }else{
                        cityBuilder.DestroyBuildings();
                        state = Estado.GenBuildingLots;
                    }
                }else if(GUILayout.Button("Resetear CityBuilder")){
                    cityBuilder.DestroyCity();
                    state = Estado.NoCity;
                }
            }
        }
    }
}
