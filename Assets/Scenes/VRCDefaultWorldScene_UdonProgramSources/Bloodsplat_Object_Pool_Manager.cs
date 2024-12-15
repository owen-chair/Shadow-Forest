using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Bloodsplat_Object_Pool_Manager : UdonSharpBehaviour
{
    public GameObject[] m_BloodSplats;
    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;
    private int currentIndex = 0;

    void Start()
    {
        // Initialize the arrays to store original positions and rotations
        originalPositions = new Vector3[m_BloodSplats.Length];
        originalRotations = new Quaternion[m_BloodSplats.Length];

        // Store the original positions and rotations
        for (int i = 0; i < m_BloodSplats.Length; i++)
        {
            originalPositions[i] = m_BloodSplats[i].transform.position;
            originalRotations[i] = m_BloodSplats[i].transform.rotation;
            ResetBloodsplat(m_BloodSplats[i], i);
        }
    }

    public void CreateBloodsplat(Vector3 position, Quaternion rotation)
    {
        GameObject bloodsplat = m_BloodSplats[currentIndex];
        bloodsplat.transform.position = position;

        // Ensure the bloodsplat is flat by setting the rotation's X and Z components to zero
        bloodsplat.transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);

        bloodsplat.SetActive(true);

        currentIndex = (currentIndex + 1) % m_BloodSplats.Length;
    }

    public void ResetBloodsplat(GameObject bloodsplat, int index)
    {
        bloodsplat.SetActive(false);
        bloodsplat.transform.position = originalPositions[index];
        bloodsplat.transform.rotation = originalRotations[index];
    }

    public void ResetAllBloodsplats()
    {
        for (int i = 0; i < m_BloodSplats.Length; i++)
        {
            ResetBloodsplat(m_BloodSplats[i], i);
        }
    }
}