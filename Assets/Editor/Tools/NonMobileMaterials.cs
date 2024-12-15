using UnityEngine;
using UnityEditor;

public class NonMobileMaterials : EditorWindow
{
    [MenuItem("Tools/Non Mobile Materials")]
    public static void ShowWindow()
    {
        GetWindow<NonMobileMaterials>("Non Mobile Materials");
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
                    if (IsNonMobileShader(material.shader))
                    {
                        Debug.LogError($"GameObject: {renderer.gameObject.name}, Shader: {material.shader.name}");
                    }
                }
            }
        }

        Debug.Log("Shader type printing complete.");
    }

    private bool IsNonMobileShader(Shader shader)
    {
        // Check for known non-mobile shaders
        string shaderName = shader.name.ToLower();
        return shaderName != "vrchat/mobile/standard lite";
    }
}