using UnityEngine;
using UnityEditor;

public class ShaderTypePrinter : EditorWindow
{
    [MenuItem("Tools/Print Shader Types")]
    public static void ShowWindow()
    {
        GetWindow<ShaderTypePrinter>("Shader Type Printer");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Print Shader Types"))
        {
            PrintShaderTypes();
        }
    }

    private void PrintShaderTypes()
    {
        // Find all renderers in the scene
        Renderer[] renderers = FindObjectsOfType<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            // Get all materials of the renderer
            Material[] materials = renderer.sharedMaterials;

            foreach (Material material in materials)
            {
                if (material != null)
                {
                    // Print the shader name
                    Debug.Log($"GameObject: {renderer.gameObject.name}, Shader: {material.shader.name}");
                }
            }
        }

        Debug.Log("Shader type printing complete.");
    }
}