using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowHighlight : MonoBehaviour
{
    Dictionary<Renderer, Material[]> glowMaterialDictionary = new Dictionary<Renderer, Material[]>();
    Dictionary<Renderer, Material[]> originalMaterialDictionary = new Dictionary<Renderer, Material[]>();
    Dictionary<Color, Material> cachedGlowMaterials = new Dictionary<Color, Material>();

    public Material glowMaterial;
    public Material glowMaterialWhenInvalid;
    public Material greenGlowMaterial;
    public Material redGlowMaterial;
    public Material orangeGlowMaterial;

    private bool isGlowing = false;

    private Color validSpaceColor = Color.green;
    private Color OriginalGlowColor;


    private void Awake()
    {
        PrepareMaterialsDictionary();
        OriginalGlowColor = glowMaterial.GetColor("_GlowColor");
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
        }
    }

    internal void ResetGlowHighlight()
    {
        foreach (Renderer renderer in glowMaterialDictionary.Keys)
        {
            foreach(Material item in glowMaterialDictionary[renderer])
            {
                item.SetColor("_GlowColor", OriginalGlowColor);
            }
        }
    }

    public void ResetHighlight()
    {
        foreach (Renderer renderer in originalMaterialDictionary.Keys)
        {
            renderer.materials = originalMaterialDictionary[renderer];
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

    public void ToggleGlow()
    {
        if (!isGlowing)
        {
            ResetGlowHighlight();
            foreach (Renderer renderer in originalMaterialDictionary.Keys)
            {
                renderer.materials = glowMaterialDictionary[renderer];
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

    public void DisplayDefaultGlow()
    {
        foreach (Renderer renderer in originalMaterialDictionary.Keys)
        {
            renderer.materials = glowMaterialDictionary[renderer];
        }
    }

    public void DisplayAsPirateFiringArc()
    {
        foreach (Renderer renderer in originalMaterialDictionary.Keys)
        {
            renderer.materials = new Material[1] { redGlowMaterial };
        }
    }

    public void DisplayAsPlayerFiringArc()
    {
        foreach (Renderer renderer in originalMaterialDictionary.Keys)
        {
            renderer.materials = new Material[1] { greenGlowMaterial };
        }
    }

    public void DisplayAsQueueCard()
    {
        foreach (Renderer renderer in originalMaterialDictionary.Keys)
        {
            renderer.materials = new Material[1] { orangeGlowMaterial };
        }
    }
}
