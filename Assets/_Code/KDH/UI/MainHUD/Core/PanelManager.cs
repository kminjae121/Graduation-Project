using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    private Dictionary<string, Panel> panels = new Dictionary<string, Panel>();
    private static PanelManager singleton = null;
    
    public static PanelManager Singleton
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindFirstObjectByType<PanelManager>();
                if (singleton == null)
                {
                    singleton = new GameObject("PanelManager").AddComponent<PanelManager>();
                }
            }
            return singleton; 
        }
    }

    public static void Register(Panel panel)
    {
        if (panel == null || string.IsNullOrEmpty(panel.ID)) return;

        if (!Singleton.panels.ContainsKey(panel.ID))
        {
            Singleton.panels.Add(panel.ID, panel);
        }
        else
        {
            Singleton.panels[panel.ID] = panel;
        }
    }

    public static void Unregister(Panel panel)
    {
        if (panel == null || string.IsNullOrEmpty(panel.ID)) return;

        if (singleton != null && singleton.panels.ContainsKey(panel.ID))
        {
            singleton.panels.Remove(panel.ID);
        }
    }
    
    private void OnDestroy()
    {
        if (singleton == this)
        {
            singleton = null;
        }
    }
    
    public static Panel GetSingleton(string id)
    {
        if (Singleton.panels.ContainsKey(id))
        {
            return Singleton.panels[id];
        }
        
        Debug.LogWarning($"패널 매니저에 '{id}' ID를 가진 패널이 등록되어 있지 않습니다.");
        return null;
    }
    
    public static void Open(string id)
    {
        var panel = GetSingleton(id);
        if (panel != null)
        {
            panel.Open();
        }
    }
    
    public static void Close(string id)
    {
        var panel = GetSingleton(id);
        if (panel != null)
        {
            panel.Close();
        }
    }
    
    public static bool IsOpen(string id)
    {
        if (Singleton.panels.ContainsKey(id))
        {
            return Singleton.panels[id].IsOpen;
        }
        return false;
    }
    
    public static void CloseAll()
    {
        foreach (var panel in Singleton.panels)
        {
            if (panel.Value != null)
            {
                panel.Value.Close();
            }
        }
    }
}