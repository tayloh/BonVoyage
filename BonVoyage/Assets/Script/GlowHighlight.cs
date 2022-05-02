using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowHighlight : MonoBehaviour
{
    Dictionary<Renderer, Material[]> glowMaterialDictionary = new Dictionary<Renderer, Material[]>();
    Dictionary<Renderer, Material[]> shipGlowMaterialDictionary = new Dictionary<Renderer, Material[]>();//
    Dictionary<Renderer, Material[]> originalMaterialDictionary = new Dictionary<Renderer, Material[]>();
    Dictionary<Color, Material> cachedGlowMaterials = new Dictionary<Color, Material>();
    Dictionary<Color, Material> cachedShipGlowMaterials = new Dictionary<Color, Material>();

    public Material glowMaterial;
    public Material glowMaterialWhenInvalid;
    public Material shipGlowMaterial;// added martial for ship glowiG.

    [HideInInspector]
    public bool isGlowing = false;

    private Color validSpaceColor = Color.green;
    private Color OriginalGlowColor;
    private Color shipOriginalGlowColor;


    private void Awake()
    {
        PrepareMaterialsDictionary();
        OriginalGlowColor = glowMaterial.GetColor("_GlowColor");
        shipOriginalGlowColor = shipGlowMaterial.GetColor("_GlowColor");
    }

    private void PrepareMaterialsDictionary()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            Material[] originalMaterials = renderer.materials;
            originalMaterialDictionary.Add(renderer, originalMaterials);
            Material[] newMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                Material mat = null;
                if (cachedGlowMaterials.TryGetValue(originalMaterials[i].color, out mat) == false)
                {
                    mat = new Material(glowMaterial);

                    mat.color = originalMaterials[i].color;
                    cachedGlowMaterials[mat.color] = mat;
                }
                newMaterials[i] = mat;
            }
            glowMaterialDictionary.Add(renderer, newMaterials);



            // create new dicitnary for on ship hex highlight
            Material[] newShipMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                Material mat = null;
                if (cachedShipGlowMaterials.TryGetValue(originalMaterials[i].color, out mat) == false)
                {
                    mat = new Material(shipGlowMaterial);

                    mat.color = shipOriginalGlowColor;
                    cachedShipGlowMaterials[mat.color] = mat;
                }
                newShipMaterials[i] = mat;
            }
            shipGlowMaterialDictionary.Add(renderer, newShipMaterials);

        }
    }

    internal void ResetGlowHighlight()
    {
        foreach (Renderer renderer in glowMaterialDictionary.Keys)
        {
            foreach (Material item in glowMaterialDictionary[renderer])
            {
                item.SetColor("_GlowColor", OriginalGlowColor);
            }
        }

        foreach (Renderer renderer in shipGlowMaterialDictionary.Keys)
        {
            foreach (Material item in shipGlowMaterialDictionary[renderer])
            {
                item.SetColor("_GlowColor", shipOriginalGlowColor);
            }
        }
    }

    internal void HighlightValidPath()
    {
        if (isGlowing == false)
        {
            return;
        }
        foreach (Renderer renderer in glowMaterialDictionary.Keys)
        {
            foreach (Material item in glowMaterialDictionary[renderer])
            {
                item.SetColor("_GlowColor", validSpaceColor);
            }
        }
    }

    // Enable On Hex Ship highlight 
    public void EnableShipGlow()
    {
        ResetGlowHighlight();
        isGlowing = true;

        foreach (Renderer renderer in originalMaterialDictionary.Keys)
        {

            renderer.materials = shipGlowMaterialDictionary[renderer];
        }
    }


    public void EnableMouseGlow()
    {
        ResetGlowHighlight();


        foreach (Renderer renderer in originalMaterialDictionary.Keys)
        {

            renderer.materials = shipGlowMaterialDictionary[renderer];
        }
    }

    public void EnableGlow()
    {
        ResetGlowHighlight();
        foreach (Renderer renderer in originalMaterialDictionary.Keys)
        {
            renderer.materials = glowMaterialDictionary[renderer];
        }
    }

    public void DisableGlow()
    {
        foreach (Renderer renderer in originalMaterialDictionary.Keys)
        {
            renderer.materials = originalMaterialDictionary[renderer];
        }
    }

    public void ToggleGlow()
    {
        if (!isGlowing)
        {
            EnableGlow();
        }
        else
        {
            DisableGlow();
        }
        isGlowing = !isGlowing;
    }

    public void ToggleGlow(Material mat)
    {
        if (!isGlowing)
        {
            foreach (Renderer renderer in originalMaterialDictionary.Keys)
            {
                renderer.materials = new Material[1] { mat };
            }
        }
        else
        {
            foreach (Renderer renderer in originalMaterialDictionary.Keys)
            {
                renderer.materials = originalMaterialDictionary[renderer];
            }
        }
        isGlowing = !isGlowing;
    }

    public void ToggleGlowInvalid(bool state)
    {
        if (isGlowing == state) return;

        isGlowing = !state;
        ToggleGlow(glowMaterialWhenInvalid);
    }

    public void ToggleGlow(bool state)
    {
        if (isGlowing == state) return;

        isGlowing = !state;
        ToggleGlow();
    }
}
